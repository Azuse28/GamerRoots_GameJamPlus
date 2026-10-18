using UnityEngine;

public enum TipoPersonaje
{
    Warlock,
    Bandit
}

// Se pone en cada personaje jugable para que los objetos sepan quién los toca
public class Personaje : MonoBehaviour
{
    public TipoPersonaje tipo;

    // Efecto opcional al morir (partículas, animación...)
    public GameObject efectoMuerte;

    public void Morir()
    {
        if (efectoMuerte != null)
        {
            Instantiate(efectoMuerte, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);

        if (DirectorEscenas.Instancia != null)
            DirectorEscenas.Instancia.IrAGameOver();
    }
}
