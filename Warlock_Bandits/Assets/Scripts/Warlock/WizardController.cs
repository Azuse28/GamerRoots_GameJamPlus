
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class WizardController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 8f;
    [SerializeField] private float velocidadEscalera = 5f;
    [SerializeField] private float alturaSalidaEscalera = 4f;

    [Header("Salto")]
    [SerializeField] private float alturaSalto = 11f;
    [SerializeField] private float tiempoHastaCima = 0.38f;
    [SerializeField] private float multiplicadorCaida = 1.8f;
    [SerializeField] private float multiplicadorSaltoCorto = 2.5f;
    [SerializeField] private float velocidadMaxCaida = 65f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBuffer = 0.12f;

    [Header("Rebote en paredes")]
    [SerializeField] private bool permitirWallJump = true;
    [SerializeField] private string tagParedRebote = "bound";
    [SerializeField] private string tagParedReboteExclusivo = "bound_warlock";
    [Min(0.01f)] [SerializeField] private float distanciaDeteccionPared = 0.12f;
    [Min(0f)] [SerializeField] private float velocidadReboteHorizontal = 35f;
    [Min(0f)] [SerializeField] private float velocidadReboteVertical = 50f;
    [Min(0f)] [SerializeField] private float impulsoVerticalExtraRebote = 12f;
    [Min(0f)] [SerializeField] private float tiempoBloqueoRebote = 0.12f;

    [Header("Suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask capaSuelo = ~0;
    [SerializeField] private float altoCheckSuelo = 0.3f;
    [SerializeField] private string[] tagsSuelo = { "Ground", "box" };

    [Header("Animaciones")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Hechizo")]
    [SerializeField] private GameObject prefabHechizo;
    [SerializeField] private Transform puntoLanzamiento;

    [Header("Sonidos")]
    [SerializeField] private AudioClip sonidoSalto;

    [Range(0f, 1f)]
    [SerializeField] private float volumenSalto = 0.15f;

    // NUEVO: SONIDO DE CAMINAR
    [SerializeField] private AudioClip sonidoCaminar;

    [Range(0f, 1f)]
    [SerializeField] private float volumenCaminar = 0.2f;

    private AudioSource audioSource;
    private AudioSource audioPasos;

    private const string ANIM_IDLE = "Warlock_idle";
    private const string ANIM_WALK = "Warlock_walk";
    private const string ANIM_CAST = "Warlock_cast";

    private Rigidbody2D rb;
    private Collider2D col;

    private float movimientoX;
    private float movimientoY;

    private bool mantieneSalto;
    private float contadorCoyote;
    private float contadorBuffer;
    private float contadorSaltoReciente;
    private float contadorBloqueoRebote;
    private float direccionRebote;
    private bool enSuelo = false;
    private bool enEscalera = false;
    private int escalerasTocando = 0;

    // Control de animaciones
    private bool estaCasteando = false;
    private bool muriendoPorLiana;
    private string animacionActual = "";

    public static event Action<WizardController> MagiaUsada;

    public void MorirPorLiana()
    {
        if (muriendoPorLiana) return;
        muriendoPorLiana = true;

        Personaje personaje = GetComponentInParent<Personaje>();
        if (personaje != null)
        {
            personaje.Morir();
            return;
        }

        Destroy(gameObject);
        if (DirectorEscenas.Instancia != null)
            DirectorEscenas.Instancia.IrAGameOver();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // AudioSource original para salto
        audioSource = GetComponent<AudioSource>();

        // NUEVO: AudioSource independiente para caminar
        audioPasos = gameObject.AddComponent<AudioSource>();

        audioPasos.clip = sonidoCaminar;
        audioPasos.loop = true;
        audioPasos.playOnAwake = false;
        audioPasos.volume = volumenCaminar;
        audioPasos.spatialBlend = 0f;
    }

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
            DetenerSonidoCaminar();
            return;
        }

        // MOVIMIENTO HORIZONTAL

        if (Keyboard.current.leftArrowKey.isPressed)
            movimientoX = -1f;

        if (Keyboard.current.rightArrowKey.isPressed)
            movimientoX = 1f;

        // VOLTEAR SPRITE SEGUN DIRECCION

        if (spriteRenderer != null)
        {
            if (movimientoX < 0f)
                spriteRenderer.flipX = true;
            else if (movimientoX > 0f)
                spriteRenderer.flipX = false;
        }

        // ESCALERAS Y SALTO

        if (enEscalera)
        {
            if (Keyboard.current.upArrowKey.isPressed)
                movimientoY = 1f;

            if (Keyboard.current.downArrowKey.isPressed)
                movimientoY = -1f;
        }
        else
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                contadorBuffer = jumpBuffer;

            mantieneSalto =
                Keyboard.current.upArrowKey.isPressed;
        }

        // INICIAR ANIMACION DE CAST

        if (Keyboard.current.iKey.wasPressedThisFrame &&
            !estaCasteando)
        {
            MagiaUsada?.Invoke(this);
            estaCasteando = true;
            CambiarAnimacion(ANIM_CAST);
        }

        if (Keyboard.current.oKey.wasPressedThisFrame &&
            !estaCasteando)
        {
            MagiaUsada?.Invoke(this);
            estaCasteando = true;
            CambiarAnimacion(ANIM_CAST);
            LanzarHechizo();
        }

        // CONTROLAR ANIMACIONES

        ActualizarAnimaciones();

        // Detener los pasos al soltar las teclas
        if (Mathf.Abs(movimientoX) <= 0.01f)
        {
            DetenerSonidoCaminar();
        }
    }

    // CAMBIAR ANIMACION SIN REINICIARLA

    void CambiarAnimacion(string nombre)
    {
        if (animator == null)
            return;

        if (animacionActual == nombre)
            return;

        animacionActual = nombre;
        animator.Play(nombre, 0, 0f);
    }

    // ACTUALIZAR IDLE, WALK Y CAST

    void ActualizarAnimaciones()
    {
        if (animator == null)
            return;

        if (estaCasteando)
        {
            AnimatorStateInfo estado =
                animator.GetCurrentAnimatorStateInfo(0);

            // Esperar a que Cast termine
            if (estado.IsName(ANIM_CAST) &&
                estado.normalizedTime >= 1f)
            {
                estaCasteando = false;
                animacionActual = "";
            }
            else
            {
                return;
            }
        }

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

    void LanzarHechizo()
    {
        if (prefabHechizo == null) return;

        Vector3 origen = puntoLanzamiento != null
            ? puntoLanzamiento.position
            : transform.position;
        float direccion = spriteRenderer != null && spriteRenderer.flipX
            ? -1f : 1f;

        GameObject objeto = Instantiate(prefabHechizo, origen, Quaternion.identity);
        HechizoWarlock hechizo = objeto.GetComponent<HechizoWarlock>();
        if (hechizo != null)
            hechizo.Inicializar(direccion, gameObject);
    }

    // CALCULO DE GRAVEDAD

    float CalcularGravedad()
    {
        return 2f * alturaSalto /
            (tiempoHastaCima * tiempoHastaCima);
    }

    // MOVIMIENTO FISICO

    void FixedUpdate()
    {
        float gravedad = CalcularGravedad();

        float escalaBase =
            gravedad / Mathf.Abs(Physics2D.gravity.y);

        // MOVIMIENTO EN ESCALERA

        if (enEscalera)
        {
            contadorBloqueoRebote = 0f;
            rb.gravityScale = 0f;

            rb.linearVelocity = new Vector2(
                movimientoX * velocidad,
                movimientoY * velocidadEscalera
            );

            // NUEVO: Detener pasos en escaleras
            DetenerSonidoCaminar();

            return;
        }

        contadorBloqueoRebote = Mathf.Max(0f,
            contadorBloqueoRebote - Time.fixedDeltaTime);

        if (contadorSaltoReciente > 0f)
            contadorSaltoReciente -= Time.fixedDeltaTime;

        enSuelo = DetectarSuelo();

        if (enSuelo)
            contadorCoyote = coyoteTime;
        else
            contadorCoyote -= Time.fixedDeltaTime;

        if (enSuelo)
            contadorBloqueoRebote = 0f;

        float velocidadY = rb.linearVelocity.y;
        float velocidadX = movimientoX * velocidad;
        float direccionSalida = 0f;
        bool puedeRebotar = permitirWallJump && !enSuelo &&
            DetectarParedRebote(out direccionSalida);

        // EJECUTAR SALTO

        if (contadorBuffer > 0f && puedeRebotar)
        {
            direccionRebote = direccionSalida;
            velocidadX = direccionRebote * velocidadReboteHorizontal;
            velocidadY = velocidadReboteVertical + impulsoVerticalExtraRebote;
            contadorBloqueoRebote = tiempoBloqueoRebote;
            contadorBuffer = 0f;
            contadorCoyote = 0f;
            contadorSaltoReciente = 0.1f;
            enSuelo = false;
            DetenerSonidoCaminar();

            if (audioSource != null && sonidoSalto != null)
                audioSource.PlayOneShot(sonidoSalto, volumenSalto);
        }
        else if (contadorBuffer > 0f && contadorCoyote > 0f)
        {
            velocidadY =
                2f * alturaSalto / tiempoHastaCima;

            contadorBuffer = 0f;
            contadorCoyote = 0f;
            contadorSaltoReciente = 0.1f;

            // NUEVO: Detener sonido de caminar al saltar
            enSuelo = false;
            DetenerSonidoCaminar();

            // SONIDO DE SALTO
            if (audioSource != null && sonidoSalto != null)
            {
                audioSource.PlayOneShot(
                    sonidoSalto,
                    volumenSalto
                );
            }
        }

        if (contadorBloqueoRebote > 0f)
            velocidadX = direccionRebote * velocidadReboteHorizontal;

        // CAIDA Y SALTO CORTO

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
            velocidadX,
            Mathf.Max(velocidadY, -velocidadMaxCaida)
        );

        // NUEVO: CONTROLAR SONIDO DE CAMINAR
        ReproducirSonidoCaminar();
    }

    // NUEVO: REPRODUCIR SONIDO DE CAMINAR

    void ReproducirSonidoCaminar()
    {
        bool estaCaminando =
            enSuelo &&
            !enEscalera &&
            Mathf.Abs(movimientoX) > 0.01f &&
            Mathf.Abs(rb.linearVelocity.x) > 0.1f;

        if (estaCaminando)
        {
            if (audioPasos != null &&
                audioPasos.clip != null &&
                !audioPasos.isPlaying)
            {
                audioPasos.Play();
            }
        }
        else
        {
            DetenerSonidoCaminar();
        }
    }

    // NUEVO: DETENER SONIDO DE CAMINAR

    void DetenerSonidoCaminar()
    {
        if (audioPasos != null && audioPasos.isPlaying)
        {
            audioPasos.Stop();
        }
    }

    void OnDisable()
    {
        DetenerSonidoCaminar();
    }

    // DETECCION DE SUELO

    bool DetectarSuelo()
    {
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

    bool EsSuelo(Collider2D toque)
    {
        foreach (string tagSuelo in tagsSuelo)
        {
            if (toque.CompareTag(tagSuelo))
                return true;
        }

        return false;
    }

    bool DetectarParedRebote(out float direccionSalida)
    {
        direccionSalida = 0f;
        Bounds limites = col.bounds;
        float anchoSonda = distanciaDeteccionPared;
        Vector2 tamanoSonda = new Vector2(anchoSonda, limites.size.y * 0.8f);
        Vector2 centroIzquierda = new Vector2(
            limites.min.x - anchoSonda * 0.5f + 0.02f,
            limites.center.y);
        Vector2 centroDerecha = new Vector2(
            limites.max.x + anchoSonda * 0.5f - 0.02f,
            limites.center.y);

        bool paredIzquierda = HayParedRebote(centroIzquierda, tamanoSonda);
        bool paredDerecha = HayParedRebote(centroDerecha, tamanoSonda);
        if (paredIzquierda == paredDerecha)
            return false;

        direccionSalida = paredIzquierda ? 1f : -1f;
        return true;
    }

    bool HayParedRebote(Vector2 centro, Vector2 tamano)
    {
        Collider2D[] cercanos = Physics2D.OverlapBoxAll(
            centro, tamano, 0f, ~0);

        foreach (Collider2D cercano in cercanos)
        {
            if (cercano == col || cercano.isTrigger ||
                cercano.attachedRigidbody == rb ||
                cercano.GetComponentInParent<Personaje>() != null ||
                cercano.GetComponentInParent<waterscripts>() != null ||
                !TieneTagParedRebote(cercano))
                continue;

            return true;
        }

        return false;
    }

    bool TieneTagParedRebote(Collider2D superficie)
    {
        for (Transform actual = superficie.transform; actual != null; actual = actual.parent)
        {
            if (actual.CompareTag(tagParedRebote) ||
                (!string.IsNullOrEmpty(tagParedReboteExclusivo) &&
                actual.CompareTag(tagParedReboteExclusivo)))
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

            // NUEVO: Detener pasos al entrar en escalera
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
