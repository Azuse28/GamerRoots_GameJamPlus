using UnityEngine;
using UnityEngine.InputSystem;

public class WizardController : MonoBehaviour
{
    [SerializeField] private float velocidad = 8f;

    private Rigidbody2D rb;
    private float movimientoX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movimientoX = 0f;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.jKey.isPressed)
        {
            movimientoX = -1f;
            Debug.Log("Presionando J");
        }

        if (Keyboard.current.lKey.isPressed)
        {
            movimientoX = 1f;
            Debug.Log("Presionando L");
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            movimientoX * velocidad,
            rb.linearVelocity.y
        );
    }
}