using UnityEngine;
using UnityEngine.InputSystem;

public class WizardController : MonoBehaviour
{

    public float velocidad = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float velocidadX = 0f;

        if (Keyboard.current.aKey.isPressed)
        {
            velocidadX = -velocidad;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            velocidadX = velocidad;
        }

        Vector3 posicion = transform.position;

        transform.position = new Vector3(
            posicion.x + velocidadX * Time.deltaTime,
            posicion.y,
            posicion.z
        );

    }
}
