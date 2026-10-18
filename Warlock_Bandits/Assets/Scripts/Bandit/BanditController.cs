
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BanditController : MonoBehaviour
{
    [SerializeField] private float velocidad = 8f;
    [SerializeField] private float velocidadEscalera = 5f;
    // Al salir por arriba de la escalera sube esta altura extra para alcanzar la plataforma
    [SerializeField] private float alturaSalidaEscalera = 4f;

    [Header("Salto")]
    // La gravedad y la fuerza del salto se calculan a partir de estos dos valores
    [SerializeField] private float alturaSalto = 11f;
    [SerializeField] private float tiempoHastaCima = 0.38f;
    [SerializeField] private float multiplicadorCaida = 1.8f;
    [SerializeField] private float multiplicadorSaltoCorto = 2.5f;
    [SerializeField] private float velocidadMaxCaida = 65f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBuffer = 0.12f;

    [Header("Suelo")]
    // Opcional: si no se asigna, se usa la base del collider
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask capaSuelo = ~0;
    [SerializeField] private float altoCheckSuelo = 0.3f;
    // Tags sobre los que se puede estar de pie (el suelo y las cajas)
    [SerializeField] private string[] tagsSuelo = { "Ground", "box" };

    [Header("Sonidos")]
    [SerializeField] private AudioClip sonidoSalto;

    private AudioSource audioSource;

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
    // Cuenta los triggers de escalera tocados, por si hay varios solapados
    private int escalerasTocando = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        audioSource = GetComponent<AudioSource>();
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

        if (Keyboard.current.aKey.isPressed)
            movimientoX = -1f;

        if (Keyboard.current.dKey.isPressed)
            movimientoX = 1f;

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

            mantieneSalto = Keyboard.current.wKey.isPressed;
        }
    }

    float CalcularGravedad()
    {
        return 2f * alturaSalto / (tiempoHastaCima * tiempoHastaCima);
    }


    void FixedUpdate()
    {
        float gravedad = CalcularGravedad();
        float escalaBase = gravedad / Mathf.Abs(Physics2D.gravity.y);

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
            velocidadY = 2f * alturaSalto / tiempoHastaCima;
            contadorBuffer = 0f;
            contadorCoyote = 0f;
            contadorSaltoReciente = 0.1f;

            // ÚNICO CAMBIO: reproducir sonido al ejecutar el salto
            if (audioSource != null && sonidoSalto != null)
            {
                audioSource.PlayOneShot(sonidoSalto, 0.15f);
            }
        }

        // Cae más rápido de lo que sube, y si suelta la tecla el salto se corta
        float escala = escalaBase;

        if (velocidadY < 0f)
            escala *= multiplicadorCaida;
        else if (velocidadY > 0f && !mantieneSalto)
            escala *= multiplicadorSaltoCorto;

        rb.gravityScale = escala;

        rb.linearVelocity = new Vector2(
            movimientoX * velocidad,
            Mathf.Max(velocidadY, -velocidadMaxCaida)
        );
    }


    bool DetectarSuelo()
    {
        // Justo después de saltar no cuenta como suelo, para no repetir el salto
        if (contadorSaltoReciente > 0f)
            return false;

        Bounds limites = col.bounds;

        Vector2 centro = groundCheck != null
            ? (Vector2)groundCheck.position
            : new Vector2(limites.center.x, limites.min.y);

        Vector2 tamano = new Vector2(
            limites.size.x * 0.9f,
            altoCheckSuelo
        );

        Collider2D[] toques =
            Physics2D.OverlapBoxAll(centro, tamano, 0f, capaSuelo);

        foreach (Collider2D toque in toques)
        {
            if (toque != col && !toque.isTrigger && EsSuelo(toque))
                return true;
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
            escalerasTocando = Mathf.Max(0, escalerasTocando - 1);

            if (escalerasTocando > 0)
                return;

            enEscalera = false;

            // Si sale subiendo, impulso para llegar a la plataforma de arriba
            if (movimientoY > 0f)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    Mathf.Sqrt(2f * CalcularGravedad() * alturaSalidaEscalera)
                );
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Collider2D colisionador = GetComponent<Collider2D>();

        if (colisionador == null)
            return;

        Bounds limites = colisionador.bounds;

        Vector3 centro = groundCheck != null
            ? groundCheck.position
            : new Vector3(limites.center.x, limites.min.y, 0f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            centro,
            new Vector3(limites.size.x * 0.9f, altoCheckSuelo, 0f)
        );
    }

    void prueba()
    {
        Console.WriteLine("Game over");
    }
}
