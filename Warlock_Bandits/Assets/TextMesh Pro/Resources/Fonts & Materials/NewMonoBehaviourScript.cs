
using UnityEngine;

public class ControlMusicaCreditos : MonoBehaviour
{
    public AudioSource musica;

    public void TerminarCreditos()
    {
        if (musica != null)
        {
            musica.Stop();
        }
    }
}
