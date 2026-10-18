
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapCollider2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class waterscripts : MonoBehaviour
{
    public enum EstadoAgua
    {
        Liquida,
        Congelada
    }

    [Header("Configuración del agua")]
    [SerializeField] private EstadoAgua estadoActual = EstadoAgua.Liquida;
    [SerializeField] private float duracionCongelada = 10f;

    [Header("Evento de muerte")]
    [SerializeField] private UnityEvent alMorirPersonaje;

    private const string TAG_AGUA = "BadWater";

    private TilemapCollider2D colliderAgua;
    private CapsuleCollider2D rangoHabilidad;
    private Tilemap tilemap;
    private Coroutine temporizador;

    private readonly HashSet<GameObject> personajesEliminados =
        new HashSet<GameObject>();

    private readonly HashSet<WizardController> magosEnRango =
        new HashSet<WizardController>();

    private void Awake()
    {
        if (!CompareTag(TAG_AGUA))
        {
            Debug.LogError(
                "El objeto Water debe tener el Tag: " + TAG_AGUA
            );

            enabled = false;
            return;
        }

        colliderAgua = GetComponent<TilemapCollider2D>();
        rangoHabilidad = GetComponent<CapsuleCollider2D>();
        tilemap = GetComponent<Tilemap>();

        // El Capsule siempre detecta proximidad
        rangoHabilidad.isTrigger = true;

        // Liquida = atravesable
        // Congelada = solida
        colliderAgua.isTrigger =
            estadoActual == EstadoAgua.Liquida;

        if (GetComponent<BoxCollider2D>() != null)
        {
            Debug.LogError(
                "Water todavía tiene un Box Collider 2D. " +
                "Elimínalo."
            );

            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.iKey.wasPressedThisFrame)
            return;

        foreach (WizardController wizard in magosEnRango)
        {
            if (wizard != null)
            {
                Congelar();
                break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        VerificarContacto(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        VerificarContacto(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        WizardController wizard =
            other.GetComponentInParent<WizardController>();

        if (wizard != null)
        {
            if (!rangoHabilidad.IsTouching(other))
            {
                magosEnRango.Remove(wizard);
            }
        }
    }

    private void VerificarContacto(Collider2D other)
    {
        WizardController wizard =
            other.GetComponentInParent<WizardController>();

        // Detectar si Warlock está cerca del agua
        if (wizard != null)
        {
            if (rangoHabilidad.IsTouching(other))
            {
                magosEnRango.Add(wizard);
            }
        }

        // La muerte solo ocurre al tocar el Tilemap
        if (!colliderAgua.IsTouching(other))
            return;

        VerificarPersonaje(other);
    }

    private void VerificarPersonaje(Collider2D other)
    {
        BanditController bandit =
            other.GetComponentInParent<BanditController>();

        if (bandit != null)
        {
            // Bandit solo muere con agua líquida
            if (estadoActual == EstadoAgua.Liquida)
            {
                EliminarPersonaje(bandit.gameObject);
            }

            return;
        }

        WizardController wizard =
            other.GetComponentInParent<WizardController>();

        if (wizard != null)
        {
            EliminarPersonaje(wizard.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        VerificarColision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        VerificarColision(collision);
    }

    private void VerificarColision(Collision2D collision)
    {
        // Warlock sigue muriendo si toca el agua congelada
        if (estadoActual != EstadoAgua.Congelada)
            return;

        WizardController wizard =
            collision.collider.GetComponentInParent<WizardController>();

        if (wizard != null)
        {
            EliminarPersonaje(wizard.gameObject);
        }
    }

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

    [ContextMenu("Congelar agua")]
    public void Congelar()
    {
        if (temporizador != null)
        {
            StopCoroutine(temporizador);
        }

        temporizador =
            StartCoroutine(TemporizadorCongelacion());
    }

    private IEnumerator TemporizadorCongelacion()
    {
        estadoActual = EstadoAgua.Congelada;

        // El agua se vuelve sólida
        colliderAgua.isTrigger = false;

        // Color del agua congelada
        tilemap.color = new Color(1f, 1f, 1f, 1f);

        Debug.Log("Agua congelada - superficie sólida");

        yield return new WaitForSeconds(duracionCongelada);

        estadoActual = EstadoAgua.Liquida;

        // El agua vuelve a ser atravesable
        colliderAgua.isTrigger = true;

        // Color original del agua líquida
        tilemap.color = new Color(
            0.212f,
            0.898f,
            0.2f,
            1f
        );

        Debug.Log("Agua líquida nuevamente");

        temporizador = null;
    }
}
