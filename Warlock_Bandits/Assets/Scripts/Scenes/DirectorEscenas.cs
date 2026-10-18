using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Obligatorio para leer el teclado en Unity 6

public class DirectorEscenas : MonoBehaviour
{
    public static DirectorEscenas Instancia;

    [Header("Nombres exactos de las escenas")]
    [SerializeField] private string escenaMenu = "MainMenu";
    [SerializeField] private string escenaMain = "Main";
    [SerializeField] private string escenaNivel = "Level0";
    [SerializeField] private string escenaCreditos = "CreditScene";
    [SerializeField] private string escenaPausa = "PauseScene";
    [SerializeField] private string escenaGameOver = "GameOverScene";
    [SerializeField] private string escenaWin = "WinScene";

    [Header("Configuración de Créditos")]
    [Tooltip("¡OJO! Cambia este número desde el Inspector de Unity en la escena Director")]
    [SerializeField] private float duracionCreditos = 15f; 
    private float temporizadorCreditos = 0f;
    private bool reproduciendoCreditos = false;

    // Estados para controlar la Pausa
    private bool estamosJugando = false;
    private bool juegoPausado = false;

    private List<string> escenasActivas = new List<string>();

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        IrAlMenu();
    }

    private void Update()
    {
        // 1. CONTROL DE CRÉDITOS: Tecla ENTER o Temporizador
        if (reproduciendoCreditos)
        {
            temporizadorCreditos += Time.unscaledDeltaTime;

            // Si toca ENTER o INTRO numérico, salta los créditos de inmediato
            if (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
            {
                Debug.Log("Créditos saltados con ENTER");
                reproduciendoCreditos = false;
                IrAlMenu();
                return;
            }

            // Si se cumple el tiempo configurado en el Inspector, vuelve al menú solo
            if (temporizadorCreditos >= duracionCreditos)
            {
                Debug.Log("Créditos terminados por tiempo: " + duracionCreditos + " seg.");
                reproduciendoCreditos = false;
                IrAlMenu();
                return;
            }
        }

        // 2. CONTROL DE PAUSA: Tecla ESC (solo funciona cuando estás dentro del nivel)
        if (estamosJugando && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (juegoPausado)
            {
                ReanudarJuego();
            }
            else
            {
                PausarJuego();
            }
        }
    }

    private void DescargarEscenasActivas()
    {
        Time.timeScale = 1f; // Asegura que el tiempo del juego corra normal

        foreach (string nombreEscena in escenasActivas)
        {
            if (SceneManager.GetSceneByName(nombreEscena).isLoaded)
            {
                SceneManager.UnloadSceneAsync(nombreEscena);
            }
        }
        escenasActivas.Clear();

        string[] todas = { escenaMenu, escenaMain, escenaNivel, escenaCreditos, escenaPausa, escenaGameOver, escenaWin };
        foreach (string esc in todas)
        {
            if (SceneManager.GetSceneByName(esc).isLoaded)
            {
                SceneManager.UnloadSceneAsync(esc);
            }
        }
    }

    private void CargarAditiva(string nombreEscena)
    {
        if (!SceneManager.GetSceneByName(nombreEscena).isLoaded)
        {
            SceneManager.LoadSceneAsync(nombreEscena, LoadSceneMode.Additive);
        }
        if (!escenasActivas.Contains(nombreEscena))
        {
            escenasActivas.Add(nombreEscena);
        }
    }

    // --- NAVEGACIÓN PRINCIPAL ---

    public void IrAJugar()
    {
        reproduciendoCreditos = false;
        juegoPausado = false;
        estamosJugando = true;
        Time.timeScale = 1f;

        DescargarEscenasActivas();
        CargarAditiva(escenaMain);
        CargarAditiva(escenaNivel);
    }

    public void IrACreditos()
    {
        estamosJugando = false;
        juegoPausado = false;
        DescargarEscenasActivas();
        CargarAditiva(escenaCreditos);

        temporizadorCreditos = 0f;
        reproduciendoCreditos = true;
    }

    public void IrAlMenu()
    {
        reproduciendoCreditos = false;
        estamosJugando = false;
        juegoPausado = false;
        Time.timeScale = 1f;

        DescargarEscenasActivas();
        CargarAditiva(escenaMenu);
    }

    // --- SISTEMA DE PAUSA ---

    public void PausarJuego()
    {
        if (!estamosJugando || juegoPausado) return;

        juegoPausado = true;
        Time.timeScale = 0f; // Congela el nivel (personajes, física y enemigos)
        CargarAditiva(escenaPausa); // Abre la escena de pausa encima del juego
    }

    public void ReanudarJuego()
    {
        if (!juegoPausado) return;

        juegoPausado = false;
        Time.timeScale = 1f; // Descongela el nivel

        if (SceneManager.GetSceneByName(escenaPausa).isLoaded)
        {
            SceneManager.UnloadSceneAsync(escenaPausa);
        }
        escenasActivas.Remove(escenaPausa);
    }

    // --- FUNCIONES DE FIN DEL JUEGO (Game Over y Win) ---
    // Tus compañeros pueden llamar a DirectorEscenas.Instancia.IrAGameOver() o IrAWin()

    public void IrAGameOver()
    {
        // Si los dos personajes mueren a la vez, solo se carga una vez
        if (!estamosJugando) return;

        estamosJugando = false;
        juegoPausado = false;
        DescargarEscenasActivas();
        CargarAditiva(escenaGameOver);
    }

    public void IrAWin()
    {
        estamosJugando = false;
        juegoPausado = false;
        DescargarEscenasActivas();
        CargarAditiva(escenaWin);
    }

    public void SalirDelJuego()
    {
        Time.timeScale = 1f;
        Debug.Log("Cerrando Warlock & Bandit...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}