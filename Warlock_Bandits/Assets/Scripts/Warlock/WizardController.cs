
using UnityEngine;
using UnityEngine.InputSystem;

public class WizardController : MonoBehaviour
{
    [SerializeField] private float velocidad = 8f;
    [SerializeField] private float fuerzaSalto = 12f;

    private Rigidbody2D rb;
    private float movimientoX;
    private bool saltar;
    private bool enSuelo = false;

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
        }

        if (Keyboard.current.lKey.isPressed)
        {
            movimientoX = 1f;
        }

        if (Keyboard.current.iKey.wasPressedThisFrame && enSuelo)
        {
            saltar = true;
            enSuelo = false;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            movimientoX * velocidad,
            rb.linearVelocity.y
        );

        if (saltar)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                fuerzaSalto
            );

            saltar = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            enSuelo = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            enSuelo = false;
        }
    }
}
