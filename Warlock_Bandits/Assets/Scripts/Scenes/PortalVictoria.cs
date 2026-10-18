using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Obligatorio para detectar la tecla I en Unity 6

public class PortalVictoria : MonoBehaviour
{
    [Header("Configuración del Portal")]
    [SerializeField] private int personajesNecesarios = 2;
    [Tooltip("Si lo desmarcas, presionar la tecla I activará el final aunque no estén dentro del portal")]
    [SerializeField] private bool exigirAmbosEnElPortal = true;

    private HashSet<GameObject> personajesEnPortal = new HashSet<GameObject>();
    private bool victoriaActivada = false;

    private void Update()
    {
        if (victoriaActivada) return;

        // Detecta cuando se presiona la tecla "I" en el teclado
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            // Si ambos están en el portal (o si desactivaste el requisito para probar rápido)
            if (!exigirAmbosEnElPortal || personajesEnPortal.Count >= personajesNecesarios)
            {
                ActivarFinalDelJuego();
            }
            else
            {
                Debug.Log("Faltan personajes en el portal para usar la tecla I (" + personajesEnPortal.Count + "/" + personajesNecesarios + ")");
            }
        }
    }

    private void ActivarFinalDelJuego()
    {
        victoriaActivada = true;
        Debug.Log("¡Tecla I presionada en el portal! Activando escena de Victoria...");

        if (DirectorEscenas.Instancia != null)
        {
            DirectorEscenas.Instancia.IrAWin();
        }
        else
        {
            SceneManager.LoadScene("WinScene");
        }
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (victoriaActivada) return;

        if (EsUnPersonaje(otro.gameObject))
        {
            personajesEnPortal.Add(otro.gameObject);
            Debug.Log("Entró al portal: " + otro.gameObject.name + " (" + personajesEnPortal.Count + "/" + personajesNecesarios + ")");
        }
    }

    private void OnTriggerExit2D(Collider2D otro)
    {
        if (personajesEnPortal.Contains(otro.gameObject))
        {
            personajesEnPortal.Remove(otro.gameObject);
            Debug.Log("Salió del portal: " + otro.gameObject.name + " (" + personajesEnPortal.Count + "/" + personajesNecesarios + ")");
        }
    }

    private bool EsUnPersonaje(GameObject obj)
    {
        // Ignora el propio portal o el suelo/paredes sin movimiento
        if (obj == gameObject) return false;

        string nombre = obj.name.ToLower();
        return obj.CompareTag("Player") || 
               nombre.Contains("warlock") || 
               nombre.Contains("bandit") || 
               nombre.Contains("player") ||
               obj.GetComponent<Rigidbody2D>() != null;
    }
}