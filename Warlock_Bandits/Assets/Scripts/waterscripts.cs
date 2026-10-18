using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapCollider2D))]
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
    private Coroutine temporizador;

    private HashSet<GameObject> personajesEliminados =
        new HashSet<GameObject>();

    private void Awake()
    {
        // Verificar que el objeto sea el agua
        if (gameObject.tag != TAG_AGUA)
        {
            Debug.LogError(
                "El objeto Water debe tener el Tag: " + TAG_AGUA
            );

            enabled = false;
            return;
        }

        // Obtener el collider del Tilemap
        colliderAgua = GetComponent<TilemapCollider2D>();

        // El agua debe detectar contacto sin bloquear
        colliderAgua.isTrigger = true;

        // Evitar que otro collider genere zonas de muerte
        if (GetComponent<BoxCollider2D>() != null)
        {
            Debug.LogError(
                "Water todavía tiene un Box Collider 2D. " +
                "Elimínalo y utiliza solamente Tilemap Collider 2D."
            );

            enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        VerificarPersonaje(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        VerificarPersonaje(other);
    }

    private void VerificarPersonaje(Collider2D other)
    {
        // Detectar a Bandit
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

        // Detectar a Warlock
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