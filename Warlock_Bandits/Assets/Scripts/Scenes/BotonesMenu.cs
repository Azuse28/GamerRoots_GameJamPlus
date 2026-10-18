using UnityEngine;

public class BotonesMenu : MonoBehaviour
{
    // Para NEW GAME o REINTENTAR
    public void BotonJugar()
    {
        if (DirectorEscenas.Instancia != null)
            DirectorEscenas.Instancia.IrAJugar();
    }

    // Para CONTINUE dentro de la escena de Pausa
    public void BotonReanudarPausa()
    {
        if (DirectorEscenas.Instancia != null)
            DirectorEscenas.Instancia.ReanudarJuego();
    }

    // Para CREDITS
    public void BotonCreditos()
    {
        if (DirectorEscenas.Instancia != null)
            DirectorEscenas.Instancia.IrACreditos();
    }

    // Para ir al Menú Principal (desde Pausa, Game Over o Win)
    public void BotonVolverAlMenu()
    {
        if (DirectorEscenas.Instancia != null)
            DirectorEscenas.Instancia.IrAlMenu();
    }

    // Para EXIT (Cerrar el juego)
    public void BotonSalir()
    {
        if (DirectorEscenas.Instancia != null)
            DirectorEscenas.Instancia.SalirDelJuego();
    }
}