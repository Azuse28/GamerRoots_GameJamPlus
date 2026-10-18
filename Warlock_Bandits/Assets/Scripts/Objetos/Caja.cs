
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Caja : MonoBehaviour
{
    public TipoPersonaje quienEmpuja = TipoPersonaje.Bandit;
    public TipoPersonaje quienMuere = TipoPersonaje.Warlock;
    public Key teclaEmpujar = Key.X;

    [SerializeField] private float velocidadCaja = 8f;

    [Header("Deteccion de paredes")]
    [SerializeField] private LayerMask capaWall;
    [SerializeField] private float margenPared = 0.02f;

    private Rigidbody2D rb;
    private Collider2D colliderCaja;
    private Personaje banditTocando;

    private readonly RaycastHit2D[] resultados =
        new RaycastHit2D[16];

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        colliderCaja = GetComponent<Collider2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3f;

        Bloquear();
    }

    void FixedUpdate()
    {
        if (Keyboard.current == null)
        {
            Bloquear();
            return;
        }

        bool puedeMover =
            banditTocando != null &&
            Keyboard.current[teclaEmpujar].isPressed;

        float movimientoX = 0f;

        if (puedeMover)
        {
            if (Keyboard.current.aKey.isPressed)
                movimientoX = -1f;

            if (Keyboard.current.dKey.isPressed)
                movimientoX = 1f;
        }

        if (puedeMover && movimientoX != 0f)
        {
            if (!HayPared(movimientoX))
            {
                rb.constraints =
                    RigidbodyConstraints2D.FreezeRotation;

                rb.linearVelocity = new Vector2(
                    movimientoX * velocidadCaja,
                    rb.linearVelocity.y
                );
            }
            else
            {
                Bloquear();
            }
        }
        else
        {
            Bloquear();
        }
    }

    bool HayPared(float direccion)
    {
        ContactFilter2D filtro = new ContactFilter2D();

        filtro.SetLayerMask(capaWall);
        filtro.useTriggers = false;

        float distancia =
            velocidadCaja * Time.fixedDeltaTime +
            margenPared;

        int cantidad = colliderCaja.Cast(
            new Vector2(direccion, 0f),
            filtro,
            resultados,
            distancia
        );

        return cantidad > 0;
    }

    void OnCollisionEnter2D(Collision2D colision)
    {
        DetectarBandit(colision);
    }

    void OnCollisionStay2D(Collision2D colision)
    {
        DetectarBandit(colision);
    }

    void DetectarBandit(Collision2D colision)
    {
        Personaje personaje =
            colision.collider.GetComponentInParent<Personaje>();

        if (personaje == null)
            return;

        if (personaje.tipo == quienEmpuja)
        {
            banditTocando = personaje;
        }
    }

    void OnCollisionExit2D(Collision2D colision)
    {
        Personaje personaje =
            colision.collider.GetComponentInParent<Personaje>();

        if (personaje == banditTocando)
        {
            banditTocando = null;
        }
    }

    void Bloquear()
    {
        rb.constraints =
            RigidbodyConstraints2D.FreezePositionX |
            RigidbodyConstraints2D.FreezeRotation;

        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );
    }
}
