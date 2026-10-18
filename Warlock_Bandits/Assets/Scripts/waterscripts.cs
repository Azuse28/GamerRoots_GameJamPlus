
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

        // Ambos detectan contacto sin bloquear
        colliderAgua.isTrigger = true;
        rangoHabilidad.isTrigger = true;

        if (GetComponent<BoxCollider2D>() != null)
        {
            Debug.LogError(
                "Water todavía tiene un Box Collider 2D. " +
                "Elimínalo."
            );

            enabled = false;
        }
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // Solo activar cuando se presiona I
        if (!Keyboard.current.iKey.wasPressedThisFrame)
            return;

        // Comprobar si hay un Warlock dentro del rango
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
            // Solo quitarlo si ya no está dentro del Capsule
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

        // Detectar proximidad al agua
        if (wizard != null)
        {
            if (rangoHabilidad.IsTouching(other))
            {
                magosEnRango.Add(wizard);
            }
        }

        // Solo el Tilemap activa la muerte
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

        Debug.Log("Agua congelada");

        yield return new WaitForSeconds(duracionCongelada);

        estadoActual = EstadoAgua.Liquida;

        Debug.Log("Agua líquida nuevamente");

        temporizador = null;
    }
}
