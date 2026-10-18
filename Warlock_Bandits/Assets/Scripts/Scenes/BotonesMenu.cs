using UnityEngine;

public class BotonesMenu : MonoBehaviour
{
    // Se conecta a BotonNewGame
    public void BotonJugar()
    {
        if (DirectorEscenas.Instancia != null)
        {
            DirectorEscenas.Instancia.IrAJugar();
        }
        else
        {
            Debug.LogWarning("¡Ojo! Debes darle Play desde la escena 'Director' para cambiar de escenas.");
        }
    }

    // Se conecta a BotonCredits
    public void BotonCreditos()
    {
        if (DirectorEscenas.Instancia != null)
        {
            DirectorEscenas.Instancia.IrACreditos();
        }
        else
        {
            Debug.LogWarning("¡Ojo! Debes darle Play desde la escena 'Director' para ir a Créditos.");
        }
    }

    // Por si ponen un botón físico de volver o al terminar una escena
    public void BotonVolverAlMenu()
    {
        if (DirectorEscenas.Instancia != null)
        {
            DirectorEscenas.Instancia.IrAlMenu();
        }
    }

    // Se conecta a BotonExit
    public void BotonSalir()
    {
        if (DirectorEscenas.Instancia != null)
        {
            DirectorEscenas.Instancia.SalirDelJuego();
        }
    }
}