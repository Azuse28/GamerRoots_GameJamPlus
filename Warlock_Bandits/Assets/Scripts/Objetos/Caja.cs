
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Caja : MonoBehaviour
{
    public TipoPersonaje quienEmpuja = TipoPersonaje.Bandit;
    public TipoPersonaje quienMuere = TipoPersonaje.Warlock;
    public Key teclaEmpujar = Key.X;

    [SerializeField] private float velocidadCaja = 8f;

    private Rigidbody2D rb;
    private Personaje banditTocando;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        Bloquear();
    }

    void FixedUpdate()
    {
        if (Keyboard.current == null)
        {
            Bloquear();
            return;
        }

        bool teclaPulsada =
            Keyboard.current[teclaEmpujar].isPressed;

        bool puedeMover =
            banditTocando != null && teclaPulsada;

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
            Desbloquear();

            rb.MovePosition(
                rb.position + new Vector2(
                    movimientoX * velocidadCaja *
                    Time.fixedDeltaTime,
                    0f
                )
            );
        }
        else
        {
            Bloquear();
        }
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
        if (rb.bodyType != RigidbodyType2D.Dynamic)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        rb.constraints =
            RigidbodyConstraints2D.FreezePositionX |
            RigidbodyConstraints2D.FreezeRotation;

        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );
    }

    void Desbloquear()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;

        rb.useFullKinematicContacts = true;

        // Importante: liberar X para permitir MovePosition
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        rb.linearVelocity = Vector2.zero;
    }
}
