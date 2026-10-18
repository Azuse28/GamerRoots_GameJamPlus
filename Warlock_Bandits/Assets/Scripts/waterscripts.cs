
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

    // Permite consultar el estado desde BanditController
    public bool EstaCongelada =>
        estadoActual == EstadoAgua.Congelada;

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

        if (tilemap == null)
        {
            Debug.LogError("El objeto necesita un Tilemap.");
            enabled = false;
            return;
        }

        if (GetComponent<BoxCollider2D>() != null)
        {
            Debug.LogError(
                "Elimina el BoxCollider2D del agua."
            );

            enabled = false;
            return;
        }

        rangoHabilidad.isTrigger = true;

        // Liquida: se atraviesa
        // Congelada: superficie solida
        colliderAgua.isTrigger = !EstaCongelada;
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

        if (wizard != null &&
            !rangoHabilidad.IsTouching(other))
        {
            magosEnRango.Remove(wizard);
        }
    }

    private void VerificarContacto(Collider2D other)
    {
        WizardController wizard =
            other.GetComponentInParent<WizardController>();

        // Detectar al mago dentro del rango
        if (wizard != null)
        {
            if (rangoHabilidad.IsTouching(other))
            {
                magosEnRango.Add(wizard);
            }
        }

        // El Capsule no mata.
        // Solo el Tilemap activa la muerte.
        if (!colliderAgua.IsTouching(other))
            return;

        if (wizard != null)
        {
            EliminarPersonaje(wizard.gameObject);
            return;
        }

        BanditController bandit =
            other.GetComponentInParent<BanditController>();

        // Bandit solamente muere si el agua es liquida
        if (bandit != null && !EstaCongelada)
        {
            EliminarPersonaje(bandit.gameObject);
        }
    }

    // COLISIONES CON AGUA CONGELADA

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
        if (!EstaCongelada)
            return;

        // Comprobar que la colision sea con el hielo
        if (collision.otherCollider != colliderAgua)
            return;

        WizardController wizard =
            collision.collider.GetComponentInParent<WizardController>();

        // Warlock no puede tocar el hielo
        if (wizard != null)
        {
            EliminarPersonaje(wizard.gameObject);
        }

        // Bandit y las cajas pueden apoyarse
        // gracias al TilemapCollider2D solido.
    }

    private void EliminarPersonaje(GameObject personaje)
    {
        if (!personajesEliminados.Add(personaje))
            return;

        Debug.Log("Game Over: " + personaje.name);

        alMorirPersonaje?.Invoke();

        Destroy(personaje);
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

        // Convertir el agua en una superficie solida
        colliderAgua.isTrigger = false;

        // Color del hielo
        tilemap.color = new Color(1f, 1f, 1f, 1f);

        Debug.Log("Agua congelada: Bandit puede caminar sobre ella");

        yield return new WaitForSeconds(duracionCongelada);

        estadoActual = EstadoAgua.Liquida;

        // El agua vuelve a ser atravesable
        colliderAgua.isTrigger = true;

        // Color del agua liquida
        tilemap.color = new Color(
            0.212f,
            0.898f,
            0.2f,
            1f
        );

        Debug.Log("Agua liquida nuevamente");

        temporizador = null;
    }
}
