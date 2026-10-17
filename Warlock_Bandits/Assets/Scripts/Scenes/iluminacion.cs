using UnityEngine;
using UnityEngine.UI;

public class Iluminación : MonoBehaviour
{
    [Header("Iluminación inicial")]
    [SerializeField] private float brilloInicial = 0.20f;
    [SerializeField] private float brilloFinal = 0.85f;
    [SerializeField] private float duracionAclarado = 3f;

    [Header("Efecto antorcha")]
    [SerializeField] private float brilloMinimo = 0.70f;
    [SerializeField] private float brilloMaximo = 1f;
    [SerializeField] private float velocidad = 2f;

    [SerializeField] private float duracionTransicion = 1f;

    private Image imagen;
    private Color colorOriginal;
    private float tiempo;

    void Awake()
    {
        imagen = GetComponent<Image>();
        colorOriginal = imagen.color;

        // La imagen comienza oscurecida
        AplicarBrillo(brilloInicial);
    }

    void Update()
    {
        tiempo += Time.deltaTime;

        float brillo;

        if (tiempo < duracionAclarado)
        {
            // FASE 1: Aclarar progresivamente
            float progreso = Mathf.Clamp01(
                tiempo / Mathf.Max(0.01f, duracionAclarado)
            );

            brillo = Mathf.SmoothStep(
                brilloInicial,
                brilloFinal,
                progreso
            );
        }
        else
        {
            // FASE 2: Efecto de antorcha
            float tiempoAntorcha = tiempo - duracionAclarado;

            float ruido = Mathf.PerlinNoise(
                tiempoAntorcha * velocidad,
                0.5f
            );

            float brilloAntorcha = Mathf.Lerp(
                brilloMinimo,
                brilloMaximo,
                ruido
            );

            // Transición suave hacia el efecto antorcha
            float transicion = Mathf.Clamp01(
                tiempoAntorcha / Mathf.Max(0.01f, duracionTransicion)
            );

            brillo = Mathf.SmoothStep(
                brilloFinal,
                brilloAntorcha,
                transicion
            );
        }

        AplicarBrillo(brillo);
    }

    private void AplicarBrillo(float brillo)
    {
        imagen.color = new Color(
            colorOriginal.r * brillo,
            colorOriginal.g * brillo,
            colorOriginal.b * brillo,
            colorOriginal.a
        );
    }
}