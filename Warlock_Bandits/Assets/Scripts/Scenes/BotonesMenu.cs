using UnityEngine;
using UnityEngine.SceneManagement;

public class BotonesMenu : MonoBehaviour
{
    // Se conecta a BotonNewGame
    public void BotonJugar()
    {
        if (DirectorEscenas.Instancia != null)
            DirectorEscenas.Instancia.IrAJugar();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Main");// Por si pruebas sin abrir Director
            UnityEngine.SceneManagement.SceneManager.LoadScene("Level0");
    }

    // Se conecta a BotonCredits
    public void BotonCreditos()
    {
        if (DirectorEscenas.Instancia != null)
            DirectorEscenas.Instancia.IrACreditos();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("CreditScene");
    }

    // Se conecta a BotonExit
    public void BotonSalir()
    {
        if (DirectorEscenas.Instancia != null)
            DirectorEscenas.Instancia.SalirDelJuego();
        else
        {
            Debug.Log("Saliendo del juego...");
            Application.Quit();
        }
    }
}