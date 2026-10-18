using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DirectorEscenas : MonoBehaviour
{
    public static DirectorEscenas Instancia;

    [Header("Nombres exactos de las escenas")]
    [SerializeField] private string escenaMenu = "MainMenu";
    [SerializeField] private string escenaMain = "Main";
    [SerializeField] private string escenaNivel = "Level0";
    [SerializeField] private string escenaCreditos = "CreditScene";

    [Header("Configuración de Créditos")]
    [SerializeField] private float duracionCreditos = 10f; // Tiempo en segundos antes de volver solo
    private float temporizadorCreditos = 0f;
    private bool reproduciendoCreditos = false;

    private List<string> escenasActivas = new List<string>();

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
        }
    }

    private void Start()
    {
        IrAlMenu();
    }

    private void Update()
    {
        // Si estamos reproduciendo los créditos, cuenta el tiempo automáticamente
        if (reproduciendoCreditos)
        {
            temporizadorCreditos += Time.deltaTime;
            if (temporizadorCreditos >= duracionCreditos)
            {
                reproduciendoCreditos = false;
                IrAlMenu(); // Vuelve solo al terminar el tiempo
            }
        }
    }

    private void DescargarEscenasActivas()
    {
        foreach (string nombreEscena in escenasActivas)
        {
            if (SceneManager.GetSceneByName(nombreEscena).isLoaded)
            {
                SceneManager.UnloadSceneAsync(nombreEscena);
            }
        }
        escenasActivas.Clear();

        string[] todas = { escenaMenu, escenaMain, escenaNivel, escenaCreditos };
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
        reproduciendoCreditos = false;
        DescargarEscenasActivas();
        CargarAditiva(escenaMain);
        CargarAditiva(escenaNivel);
    }

    public void IrACreditos()
    {
        DescargarEscenasActivas();
        CargarAditiva(escenaCreditos);
        
        // Inicia el contador automático para los créditos
        temporizadorCreditos = 0f;
        reproduciendoCreditos = true;
    }

    public void IrAlMenu()
    {
        reproduciendoCreditos = false;
        DescargarEscenasActivas();
        CargarAditiva(escenaMenu);
    }

    public void SalirDelJuego()
    {
        Debug.Log("Cerrando Warlock & Bandit...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}