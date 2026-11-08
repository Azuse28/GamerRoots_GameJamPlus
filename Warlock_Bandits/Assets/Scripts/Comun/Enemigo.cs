using UnityEngine;

public class Enemigo : MonoBehaviour
{
    [SerializeField] protected int vidaMaxima = 1;
    protected int vidaActual;

    protected virtual void Awake()
    {
        vidaActual = vidaMaxima;
    }

    public virtual bool PuedeDeflectar() => false;

    public virtual void RecibirDanio(int cantidad)
    {
        vidaActual -= cantidad;
        if (vidaActual <= 0)
            Morir();
    }

    protected virtual void Morir()
    {
        Debug.Log("[Enemigo] " + gameObject.name + " murio!");
        Destroy(gameObject);
    }
}