
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BanditController : MonoBehaviour
{
    [SerializeField] private float velocidad = 8f;
    [SerializeField] private float velocidadEscalera = 5f;

    // Altura extra al salir por arriba de la escalera
    [SerializeField] private float alturaSalidaEscalera = 4f;

    [Header("Salto")]
    [SerializeField] private float alturaSalto = 11f;
    [SerializeField] private float tiempoHastaCima = 0.38f;
    [SerializeField] private float multiplicadorCaida = 1.8f;
    [SerializeField] private float multiplicadorSaltoCorto = 2.5f;
    [SerializeField] private float velocidadMaxCaida = 65f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBuffer = 0.12f;

    [Header("Suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask capaSuelo = ~0;
    [SerializeField] private float altoCheckSuelo = 0.3f;

    // Suelo normal y cajas
    [SerializeField]
    private string[] tagsSuelo =
    {
        "Ground",
        "box"
    };

    [Header("Sonidos")]
    [SerializeField] private AudioClip sonidoSalto;

    // SONIDO DE CAMINAR
    [SerializeField] private AudioClip sonidoCaminar;
    [SerializeField] private float volumenCaminar = 0.2f;

    // NUEVO: ANIMACIONES
    [Header("Animaciones")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private const string ANIM_IDLE = "Bandit_idle";
    private const string ANIM_WALK = "Bandit_walk";

    private string animacionActual = "";

    private AudioSource audioSource;
    private AudioSource audioPasos;

    private Rigidbody2D rb;
    private Collider2D col;

    private float movimientoX;
    private float movimientoY;

    private bool mantieneSalto;
    private float contadorCoyote;
    private float contadorBuffer;
    private float contadorSaltoReciente;

    private bool enSuelo = false;
    private bool enEscalera = false;

    private int escalerasTocando = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // NUEVO: Obtener componentes de animacion
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // AudioSource original para el salto
        audioSource = GetComponent<AudioSource>();

        // AudioSource independiente para caminar
        audioPasos = gameObject.AddComponent<AudioSource>();

        audioPasos.clip = sonidoCaminar;
        audioPasos.loop = true;
        audioPasos.playOnAwake = false;
        audioPasos.volume = volumenCaminar;
        audioPasos.spatialBlend = 0f;
    }

    // NUEVO: Iniciar animacion Idle
    void Start()
    {
        CambiarAnimacion(ANIM_IDLE);
    }

    void Update()
    {
        movimientoX = 0f;
        movimientoY = 0f;
        mantieneSalto = false;

        if (contadorBuffer > 0f)
            contadorBuffer -= Time.deltaTime;

        if (Keyboard.current == null)
        {
            ActualizarAnimaciones();
            return;
        }

        // MOVIMIENTO HORIZONTAL

        if (Keyboard.current.aKey.isPressed)
            movimientoX = -1f;

        if (Keyboard.current.dKey.isPressed)
            movimientoX = 1f;

        // NUEVO: VOLTEAR SPRITE SEGUN DIRECCION

        if (spriteRenderer != null)
        {
            if (movimientoX < 0f)
            {
                // Mirar a la izquierda
                spriteRenderer.flipX = true;
            }
            else if (movimientoX > 0f)
            {
                // Mirar a la derecha
                spriteRenderer.flipX = false;
            }
        }

        // ESCALERAS Y SALTO

        if (enEscalera)
        {
            if (Keyboard.current.wKey.isPressed)
                movimientoY = 1f;

            if (Keyboard.current.sKey.isPressed)
                movimientoY = -1f;
        }
        else
        {
            if (Keyboard.current.wKey.wasPressedThisFrame)
                contadorBuffer = jumpBuffer;

            mantieneSalto =
                Keyboard.current.wKey.isPressed;
        }

        // NUEVO: CONTROLAR ANIMACIONES
        ActualizarAnimaciones();
    }

    // NUEVO: CAMBIAR ANIMACION SIN REINICIARLA

    void CambiarAnimacion(string nombre)
    {
        if (animator == null)
            return;

        if (animacionActual == nombre)
            return;

        animacionActual = nombre;

        animator.Play(nombre, 0, 0f);
    }

    // NUEVO: ACTUALIZAR IDLE Y WALK

    void ActualizarAnimaciones()
    {
        if (animator == null)
            return;

        bool caminando =
            movimientoX != 0f || movimientoY != 0f;

        if (caminando)
        {
            CambiarAnimacion(ANIM_WALK);
        }
        else
        {
            CambiarAnimacion(ANIM_IDLE);
        }
    }

    float CalcularGravedad()
    {
        return 2f * alturaSalto /
            (tiempoHastaCima * tiempoHastaCima);
    }

    void FixedUpdate()
    {
        float gravedad = CalcularGravedad();

        float escalaBase =
            gravedad / Mathf.Abs(Physics2D.gravity.y);

        // MOVIMIENTO EN ESCALERA

        if (enEscalera)
        {
            rb.gravityScale = 0f;

            rb.linearVelocity = new Vector2(
                movimientoX * velocidad,
                movimientoY * velocidadEscalera
            );

            // Detener pasos mientras esta en escalera
            DetenerSonidoCaminar();

            return;
        }

        if (contadorSaltoReciente > 0f)
            contadorSaltoReciente -= Time.fixedDeltaTime;

        // DETECTAR SUELO, CAJAS O HIELO

        enSuelo = DetectarSuelo();

        if (enSuelo)
            contadorCoyote = coyoteTime;
        else
            contadorCoyote -= Time.fixedDeltaTime;

        float velocidadY = rb.linearVelocity.y;

        // EJECUTAR SALTO

        if (contadorBuffer > 0f && contadorCoyote > 0f)
        {
            velocidadY =
                2f * alturaSalto / tiempoHastaCima;

            contadorBuffer = 0f;
            contadorCoyote = 0f;
            contadorSaltoReciente = 0.1f;

            // Detener los pasos al saltar
            enSuelo = false;
            DetenerSonidoCaminar();

            // SONIDO DE SALTO
            if (audioSource != null && sonidoSalto != null)
            {
                audioSource.PlayOneShot(
                    sonidoSalto,
                    1.0f
                );
            }
        }

        // GRAVEDAD Y SALTO CORTO

        float escala = escalaBase;

        if (velocidadY < 0f)
        {
            escala *= multiplicadorCaida;
        }
        else if (velocidadY > 0f && !mantieneSalto)
        {
            escala *= multiplicadorSaltoCorto;
        }

        rb.gravityScale = escala;

        rb.linearVelocity = new Vector2(
            movimientoX * velocidad,
            Mathf.Max(velocidadY, -velocidadMaxCaida)
        );

        // CONTROLAR SONIDO DE CAMINAR
        ReproducirSonidoCaminar();
    }

    // SONIDO DE CAMINAR

    void ReproducirSonidoCaminar()
    {
        bool estaCaminando =
            enSuelo &&
            !enEscalera &&
            Mathf.Abs(movimientoX) > 0.01f &&
            Mathf.Abs(rb.linearVelocity.x) > 0.1f;

        if (estaCaminando)
        {
            // Reproducir unicamente si no esta sonando
            if (audioPasos != null &&
                audioPasos.clip != null &&
                !audioPasos.isPlaying)
            {
                audioPasos.Play();
            }
        }
        else
        {
            // Detener inmediatamente cuando deja de caminar
            DetenerSonidoCaminar();
        }
    }

    // DETENER SONIDO DE CAMINAR

    void DetenerSonidoCaminar()
    {
        if (audioPasos != null && audioPasos.isPlaying)
        {
            audioPasos.Stop();
        }
    }

    // DETECTAR SUELO

    bool DetectarSuelo()
    {
        // Evitar detectar suelo inmediatamente
        // despues de saltar
        if (contadorSaltoReciente > 0f)
            return false;

        Bounds limites = col.bounds;

        Vector2 centro = groundCheck != null
            ? (Vector2)groundCheck.position
            : new Vector2(
                limites.center.x,
                limites.min.y
            );

        Vector2 tamano = new Vector2(
            limites.size.x * 0.9f,
            altoCheckSuelo
        );

        Collider2D[] toques =
            Physics2D.OverlapBoxAll(
                centro,
                tamano,
                0f,
                capaSuelo
            );

        foreach (Collider2D toque in toques)
        {
            if (toque != col &&
                !toque.isTrigger &&
                EsSuelo(toque))
            {
                return true;
            }
        }

        return false;
    }

    // VERIFICAR SUPERFICIES VALIDAS

    bool EsSuelo(Collider2D toque)
    {
        // 1. Comprobar suelo normal y cajas
        foreach (string tagSuelo in tagsSuelo)
        {
            if (toque.CompareTag(tagSuelo))
                return true;
        }

        // 2. Comprobar agua congelada
        waterscripts agua =
            toque.GetComponent<waterscripts>();

        if (agua != null && agua.EstaCongelada)
        {
            return true;
        }

        return false;
    }

    // DETECCION DE ESCALERAS

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            escalerasTocando++;
            enEscalera = true;
            contadorBuffer = 0f;

            // Detener sonido al entrar en escalera
            DetenerSonidoCaminar();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            escalerasTocando =
                Mathf.Max(0, escalerasTocando - 1);

            if (escalerasTocando > 0)
                return;

            enEscalera = false;

            // Impulso al salir por arriba
            if (movimientoY > 0f)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    Mathf.Sqrt(
                        2f * CalcularGravedad() *
                        alturaSalidaEscalera
                    )
                );
            }
        }
    }

    // VISUALIZAR DETECTOR DE SUELO

    void OnDrawGizmosSelected()
    {
        Collider2D colisionador =
            GetComponent<Collider2D>();

        if (colisionador == null)
            return;

        Bounds limites = colisionador.bounds;

        Vector3 centro = groundCheck != null
            ? groundCheck.position
            : new Vector3(
                limites.center.x,
                limites.min.y,
                0f
            );

        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(
            centro,
            new Vector3(
                limites.size.x * 0.9f,
                altoCheckSuelo,
                0f
            )
        );
    }

    void prueba()
    {
        Console.WriteLine("Game over");
    }
}
