
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Caja : MonoBehaviour
{
    public TipoPersonaje quienEmpuja = TipoPersonaje.Bandit;
    public TipoPersonaje quienMuere = TipoPersonaje.Warlock;
    public Key teclaEmpujar = Key.X;

    [SerializeField] private float velocidadCaja = 8f;

    [Header("Deteccion de paredes")]
    [SerializeField] private LayerMask capaWall;
    [SerializeField] private float margenPared = 0.02f;

    [Header("Evento de muerte")]
    [SerializeField] private UnityEvent alMorirPersonaje;

    private Rigidbody2D rb;
    private BoxCollider2D colliderCaja;
    private CircleCollider2D rangoMuerte;
    private Personaje banditTocando;

    private readonly RaycastHit2D[] resultados =
        new RaycastHit2D[16];

    // Magos dentro del rango de la caja
    private HashSet<WizardController> magosEnRango =
        new HashSet<WizardController>();

    // Evitar eliminar al mismo personaje varias veces
    private HashSet<GameObject> personajesEliminados =
        new HashSet<GameObject>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        colliderCaja = GetComponent<BoxCollider2D>();
        rangoMuerte = GetComponent<CircleCollider2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3f;

        // La caja tiene colision fisica
        colliderCaja.isTrigger = false;

        // El circulo solamente detecta personajes
        rangoMuerte.isTrigger = true;

        Bloquear();
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        // Detectar las habilidades del Warlock
        bool habilidadUsada =
            Keyboard.current.iKey.wasPressedThisFrame ||
            Keyboard.current.oKey.wasPressedThisFrame ||
            Keyboard.current.pKey.wasPressedThisFrame;

        if (!habilidadUsada)
            return;

        // Eliminar magos que esten dentro del rango
        foreach (WizardController wizard in magosEnRango)
        {
            if (wizard != null)
            {
                EliminarPersonaje(wizard.gameObject);
            }
        }
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

    // COLISIONES FISICAS DE BANDIT

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

    // DETECCION DEL WARLOCK EN EL CIRCULO

    void OnTriggerEnter2D(Collider2D other)
    {
        DetectarWarlock(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        DetectarWarlock(other);
    }

    void DetectarWarlock(Collider2D other)
    {
        WizardController wizard =
            other.GetComponentInParent<WizardController>();

        if (wizard != null)
        {
            magosEnRango.Add(wizard);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        WizardController wizard =
            other.GetComponentInParent<WizardController>();

        if (wizard != null)
        {
            magosEnRango.Remove(wizard);
        }
    }

    // MISMA FUNCION DE MUERTE DEL AGUA

    private void EliminarPersonaje(GameObject personaje)
    {
        if (!personajesEliminados.Add(personaje))
            return;

        Debug.Log("Game Over: " + personaje.name);

        alMorirPersonaje?.Invoke();

        Destroy(personaje);
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
