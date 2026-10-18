using UnityEngine;
using UnityEngine.SceneManagement;

public class DirectorEscenas : MonoBehaviour
{
    public static DirectorEscenas Instancia;

    [Header("Nombres exactos de las escenas en tu carpeta")]
    [SerializeField] private string escenaMenu = "MainMenu";
    [SerializeField] private string escenaJuego = "Level0";
    [SerializeField] private string escenaCreditos = "CreditScene";

    private string escenaActualCargada = "";

    private void Awake()
    {
        Instancia = this;
    }

    private void Start()
    {
        // Al iniciar desde Director, prende el MainMenu automáticamente
        CambiarAEscena(escenaMenu);
    }

    // Desactiva la escena anterior y activa la nueva encima de Director
    public void CambiarAEscena(string nuevaEscena)
    {
        // 1. Si hay una escena activa (ej. MainMenu), la desactiva/descarga
        if (!string.IsNullOrEmpty(escenaActualCargada) && SceneManager.GetSceneByName(escenaActualCargada).isLoaded)
        {
            SceneManager.UnloadSceneAsync(escenaActualCargada);
        }
        else if (SceneManager.GetSceneByName(escenaMenu).isLoaded && nuevaEscena != escenaMenu)
        {
            SceneManager.UnloadSceneAsync(escenaMenu);
        }

        // 2. Activa la escena nueva en modo Aditivo (sin cerrar Director)
        if (!SceneManager.GetSceneByName(nuevaEscena).isLoaded)
        {
            SceneManager.LoadSceneAsync(nuevaEscena, LoadSceneMode.Additive);
        }

        escenaActualCargada = nuevaEscena;
    }

    public void IrAJugar() => CambiarAEscena(escenaJuego);
    public void IrACreditos() => CambiarAEscena(escenaCreditos);
    public void IrAlMenu() => CambiarAEscena(escenaMenu);

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}