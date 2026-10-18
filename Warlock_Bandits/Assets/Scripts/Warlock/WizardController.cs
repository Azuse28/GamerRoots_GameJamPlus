
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class WizardController : MonoBehaviour
{
    [SerializeField] private float velocidad = 8f;
    [SerializeField] private float fuerzaSalto = 12f;
    [SerializeField] private float velocidadEscalera = 5f;

    private Rigidbody2D rb;
    private float movimientoX;
    private float movimientoY;
    private float gravedadOriginal;

    private bool saltar;
    private bool enSuelo = false;
    private bool enEscalera = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        gravedadOriginal = rb.gravityScale;
    }

    void Update()
    {
        movimientoX = 0f;
        movimientoY = 0f;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.leftArrowKey.isPressed)
            movimientoX = -1f;

        if (Keyboard.current.rightArrowKey.isPressed)
            movimientoX = 1f;

        if (enEscalera)
        {
            if (Keyboard.current.upArrowKey.isPressed)
                movimientoY = 1f;

            if (Keyboard.current.downArrowKey.isPressed)
                movimientoY = -1f;
        }
        else
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame
                && enSuelo)
            {
                saltar = true;
                enSuelo = false;
            }
        }
    }

    void FixedUpdate()
    {
        if (enEscalera)
        {
            rb.gravityScale = 0f;

            rb.linearVelocity = new Vector2(
                movimientoX * velocidad,
                movimientoY * velocidadEscalera
            );
        }
        else
        {
            rb.gravityScale = gravedadOriginal;

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
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            enEscalera = true;
            saltar = false;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            enEscalera = false;
            rb.gravityScale = gravedadOriginal;
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

    void prueba()
    {
        Console.WriteLine("Game over");
    }
}
