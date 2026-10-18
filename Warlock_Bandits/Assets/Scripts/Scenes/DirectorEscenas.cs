using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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

    [Header("Configuración de Tiempos (Segundos)")]
    [SerializeField] private float duracionWin = 5f; // 5 segundos en Win antes de pasar a Créditos
    [SerializeField] private float duracionCreditos = 15f; 

    private float temporizador = 0f;
    private bool mostrandoWin = false;
    private bool reproduciendoCreditos = false;

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
        // 1. TEMPORIZADOR DE WIN (5 segundos y pasa solo a Créditos)
        if (mostrandoWin)
        {
            temporizador += Time.unscaledDeltaTime;
            if (temporizador >= duracionWin)
            {
                mostrandoWin = false;
                IrACreditos(); // Pasa automáticamente a los créditos
                return;
            }
        }

        // 2. CONTROL DE CRÉDITOS (Tecla ENTER o Temporizador para volver al Menú)
        if (reproduciendoCreditos)
        {
            temporizador += Time.unscaledDeltaTime;

            if (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
            {
                reproduciendoCreditos = false;
                IrAlMenu();
                return;
            }

            if (temporizador >= duracionCreditos)
            {
                reproduciendoCreditos = false;
                IrAlMenu();
                return;
            }
        }

        // 3. CONTROL DE PAUSA CON TECLA ESC
        if (estamosJugando && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (juegoPausado)
                ReanudarJuego();
            else
                PausarJuego();
        }
    }

    private void DescargarEscenasActivas()
    {
        Time.timeScale = 1f;

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

    public void IrAJugar()
    {
        mostrandoWin = false;
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
        mostrandoWin = false;
        estamosJugando = false;
        juegoPausado = false;
        DescargarEscenasActivas();
        CargarAditiva(escenaCreditos);

        temporizador = 0f;
        reproduciendoCreditos = true;
    }

    public void IrAlMenu()
    {
        mostrandoWin = false;
        reproduciendoCreditos = false;
        estamosJugando = false;
        juegoPausado = false;
        Time.timeScale = 1f;

        DescargarEscenasActivas();
        CargarAditiva(escenaMenu);
    }

    public void PausarJuego()
    {
        if (!estamosJugando || juegoPausado) return;

        juegoPausado = true;
        Time.timeScale = 0f;
        CargarAditiva(escenaPausa);
    }

    public void ReanudarJuego()
    {
        if (!juegoPausado) return;

        juegoPausado = false;
        Time.timeScale = 1f;

        if (SceneManager.GetSceneByName(escenaPausa).isLoaded)
        {
            SceneManager.UnloadSceneAsync(escenaPausa);
        }
        escenasActivas.Remove(escenaPausa);
    }

    public void IrAGameOver()
    {
        mostrandoWin = false;
        reproduciendoCreditos = false;
        estamosJugando = false;
        juegoPausado = false;
        DescargarEscenasActivas();
        CargarAditiva(escenaGameOver);
    }

    public void IrAWin()
    {
        reproduciendoCreditos = false;
        estamosJugando = false;
        juegoPausado = false;
        DescargarEscenasActivas();
        CargarAditiva(escenaWin);

        // Inicia la cuenta de 5 segundos para saltar a Créditos
        temporizador = 0f;
        mostrandoWin = true;
    }

    public void SalirDelJuego()
    {
        Time.timeScale = 1f;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}