using UnityEngine;
using UnityEngine.InputSystem;

// La caja reacciona distinto según el personaje que la toca:
// uno la puede empujar (manteniendo una tecla) y al otro lo mata
[RequireComponent(typeof(Rigidbody2D))]
public class Caja : MonoBehaviour
{
    public TipoPersonaje quienEmpuja = TipoPersonaje.Bandit;
    public TipoPersonaje quienMuere = TipoPersonaje.Warlock;
    public Key teclaEmpujar = Key.X;

    private Rigidbody2D rb;
    private int empujadoresTocando = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        Bloquear();
    }

    void Update()
    {
        bool teclaPulsada = Keyboard.current != null && Keyboard.current[teclaEmpujar].isPressed;

        if (empujadoresTocando > 0 && teclaPulsada)
        {
            Desbloquear();
        }
        else
        {
            Bloquear();
        }
    }

    void OnCollisionEnter2D(Collision2D colision)
    {
        Personaje personaje = colision.collider.GetComponentInParent<Personaje>();
        if (personaje == null)
        {
            return;
        }

        if (personaje.tipo == quienMuere)
        {
            personaje.Morir();
        }
        else if (personaje.tipo == quienEmpuja)
        {
            empujadoresTocando++;
        }
    }

    void OnCollisionExit2D(Collision2D colision)
    {
        Personaje personaje = colision.collider.GetComponentInParent<Personaje>();
        if (personaje == null || personaje.tipo != quienEmpuja)
        {
            return;
        }

        empujadoresTocando = Mathf.Max(0, empujadoresTocando - 1);
    }

    // Quieta en X para que nadie mas la mueva; la gravedad sigue actuando en Y
    void Bloquear()
    {
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void Desbloquear()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}
