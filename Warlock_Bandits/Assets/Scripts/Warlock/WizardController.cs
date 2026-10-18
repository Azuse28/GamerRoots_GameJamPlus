
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

    [Header("Suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask capaSuelo = ~0;
    [SerializeField] private float altoCheckSuelo = 0.3f;
    [SerializeField] private string[] tagsSuelo = { "Ground", "box" };

    [Header("Animaciones")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

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
    private bool enSuelo = false;
    private bool enEscalera = false;
    private int escalerasTocando = 0;

    // Control de animaciones
    private bool estaCasteando = false;
    private string animacionActual = "";

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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
            return;

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
            estaCasteando = true;
            CambiarAnimacion(ANIM_CAST);
        }

        // CONTROLAR ANIMACIONES

        ActualizarAnimaciones();
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

        if (enEscalera)
        {
            rb.gravityScale = 0f;

            rb.linearVelocity = new Vector2(
                movimientoX * velocidad,
                movimientoY * velocidadEscalera
            );

            return;
        }

        if (contadorSaltoReciente > 0f)
            contadorSaltoReciente -= Time.fixedDeltaTime;

        enSuelo = DetectarSuelo();

        if (enSuelo)
            contadorCoyote = coyoteTime;
        else
            contadorCoyote -= Time.fixedDeltaTime;

        float velocidadY = rb.linearVelocity.y;

        if (contadorBuffer > 0f && contadorCoyote > 0f)
        {
            velocidadY =
                2f * alturaSalto / tiempoHastaCima;

            contadorBuffer = 0f;
            contadorCoyote = 0f;
            contadorSaltoReciente = 0.1f;
        }

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
            movimientoX * velocidad,
            Mathf.Max(velocidadY, -velocidadMaxCaida)
        );
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

    // DETECCION DE ESCALERAS

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            escalerasTocando++;
            enEscalera = true;
            contadorBuffer = 0f;
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