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

        if (Keyboard.current.aKey.isPressed)
        {
            movimientoX = -1f;
            Debug.Log("Presionando A");
        }

        if (Keyboard.current.dKey.isPressed)
        {
            movimientoX = 1f;
            Debug.Log("Presionando D");
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