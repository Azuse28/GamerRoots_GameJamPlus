
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Caja : MonoBehaviour
{
    public TipoPersonaje quienEmpuja = TipoPersonaje.Bandit;
    public TipoPersonaje quienMuere = TipoPersonaje.Warlock;
    public Key teclaEmpujar = Key.R;

    [SerializeField] private float velocidadCaja = 8f;

    [Header("Deteccion de paredes")]
    [SerializeField] private LayerMask capaWall;
    [SerializeField] private float margenPared = 0.02f;

    [Header("Deteccion de suelo")]
    // Mientras esta apoyada se congela Y, para que los personajes no la arrastren al saltar
    [SerializeField] private LayerMask capaSuelo = ~0;
    [SerializeField] private float margenSuelo = 0.05f;

    [Header("Agua")]
    // Una caja hundida en el agua hace de puente: no se empuja y las demas pasan por encima
    [SerializeField] private string tagAgua = "BadWater";
    // Cuanto puede subir una caja para pasar sobre otra hundida (fraccion de su altura)
    [SerializeField] private float escalonMaximo = 0.4f;

    [Header("Evento de muerte")]
    [SerializeField] private UnityEvent alMorirPersonaje;

    private Rigidbody2D rb;
    private BoxCollider2D colliderCaja;
    private CircleCollider2D rangoMuerte;
    private Personaje banditTocando;
    private bool apoyada;
    // Ultimo paso de fisica en el que otra caja empujo a esta
    private float tiempoEmpujada = -1f;
    private bool enAgua;

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

        // Sin friccion, para que los personajes no la arrastren hacia arriba al saltar a su lado.
        // Minimum hace que el 0 gane al combinarse con el material de cualquier otro collider
        colliderCaja.sharedMaterial = new PhysicsMaterial2D("CajaSinFriccion")
        {
            friction = 0f,
            bounciness = 0f,
            frictionCombine = PhysicsMaterialCombine2D.Minimum
        };

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
        apoyada = HaySueloDebajo();

        // Si otra caja ya la empujo en este paso, no se bloquea
        if (tiempoEmpujada == Time.fixedTime)
            return;

        if (Keyboard.current == null)
        {
            Bloquear();
            return;
        }

        bool puedeMover =
            !enAgua &&
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

        if (puedeMover && movimientoX != 0f && PuedeAvanzar(movimientoX))
        {
            Mover(movimientoX);
        }
        else
        {
            Bloquear();
        }
    }

    // Solo avanza si ni esta caja ni las que tiene delante chocan con una pared
    bool PuedeAvanzar(float direccion)
    {
        if (HayPared(direccion))
            return false;

        Caja cajaDelante = CajaDelante(direccion);

        return cajaDelante == null || cajaDelante.PuedeAvanzar(direccion);
    }

    void Mover(float direccion)
    {
        SubirSobreCajaEnAgua(direccion);

        rb.constraints =
            RigidbodyConstraints2D.FreezeRotation |
            BloqueoY();

        rb.linearVelocity = new Vector2(
            direccion * velocidadCaja,
            rb.linearVelocity.y
        );

        // Empuja en cadena a la caja que tenga delante
        Caja cajaDelante = CajaDelante(direccion);

        if (cajaDelante != null)
        {
            cajaDelante.tiempoEmpujada = Time.fixedTime;
            cajaDelante.Mover(direccion);
        }
    }

    Caja CajaDelante(float direccion)
    {
        ContactFilter2D filtro = new ContactFilter2D();

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

        for (int i = 0; i < cantidad; i++)
        {
            Caja otra = resultados[i].collider.GetComponentInParent<Caja>();

            // Solo cuenta si de verdad esta delante, para no volver a la caja de atras.
            // Las hundidas en el agua no se empujan
            if (otra != null && otra != this && !otra.enAgua &&
                (otra.transform.position.x - transform.position.x) * direccion > 0f)
                return otra;
        }

        return null;
    }

    // Si delante hay una caja hundida poco mas alta que la base de esta, la sube encima
    void SubirSobreCajaEnAgua(float direccion)
    {
        ContactFilter2D filtro = new ContactFilter2D();

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

        Bounds limites = colliderCaja.bounds;
        float subidaMaxima = limites.size.y * escalonMaximo;

        for (int i = 0; i < cantidad; i++)
        {
            Caja otra = resultados[i].collider.GetComponentInParent<Caja>();

            if (otra == null || otra == this || !otra.enAgua)
                continue;

            float subida = otra.colliderCaja.bounds.max.y - limites.min.y;

            if (subida > 0f && subida <= subidaMaxima)
            {
                rb.position += new Vector2(0f, subida + margenPared);
                return;
            }
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

    bool HaySueloDebajo()
    {
        ContactFilter2D filtro = new ContactFilter2D();

        filtro.SetLayerMask(capaSuelo);
        filtro.useTriggers = false;

        int cantidad = colliderCaja.Cast(
            Vector2.down,
            filtro,
            resultados,
            margenSuelo
        );

        for (int i = 0; i < cantidad; i++)
        {
            // Los personajes no cuentan como suelo
            if (resultados[i].collider.GetComponentInParent<Personaje>() == null)
                return true;
        }

        return false;
    }

    RigidbodyConstraints2D BloqueoY()
    {
        return apoyada
            ? RigidbodyConstraints2D.FreezePositionY
            : RigidbodyConstraints2D.None;
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
        DetectarAgua(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        DetectarWarlock(other);
        DetectarAgua(other);
    }

    // Esta en el agua cuando su centro queda dentro, asi rozar la superficie no cuenta
    void DetectarAgua(Collider2D other)
    {
        if (EsAgua(other))
            enAgua = other.OverlapPoint(colliderCaja.bounds.center);
    }

    // Solo el tilemap es el agua; la capsula del mismo objeto es el rango de la habilidad
    bool EsAgua(Collider2D other)
    {
        return other is TilemapCollider2D && other.CompareTag(tagAgua);
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
        if (EsAgua(other) &&
            !other.OverlapPoint(colliderCaja.bounds.center))
            enAgua = false;

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

        if (DirectorEscenas.Instancia != null)
            DirectorEscenas.Instancia.IrAGameOver();
    }

    void Bloquear()
    {
        rb.constraints =
            RigidbodyConstraints2D.FreezePositionX |
            RigidbodyConstraints2D.FreezeRotation |
            BloqueoY();

        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );
    }
}
