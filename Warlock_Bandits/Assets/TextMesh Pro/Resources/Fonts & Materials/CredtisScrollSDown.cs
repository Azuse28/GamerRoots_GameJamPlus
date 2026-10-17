
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;


public class CreditsScrollDown : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 130f;
    [SerializeField] private float startOffset = 120f;

    // Música de los créditos
    [SerializeField] private AudioSource creditsMusic;
    // Escena a la que saltara luego de terminar creditos
    [SerializeField] private string nextSceneName = "";

    private RectTransform creditsRect;
    private RectTransform canvasRect;
    private float endY;

    private bool creditsFinished = false;

    void Start()
    {
        creditsRect = GetComponent<RectTransform>();

        Canvas canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();

        TMP_Text creditsText = GetComponent<TMP_Text>();

        // Centrar el objeto respecto al Canvas
        creditsRect.anchorMin = new Vector2(0.5f, 0.5f);
        creditsRect.anchorMax = new Vector2(0.5f, 0.5f);

        // Pivote en la parte superior del texto
        creditsRect.pivot = new Vector2(0.5f, 1f);

        // El contenido comienza arriba, no abajo
        creditsText.alignment = TextAlignmentOptions.Top;
        creditsText.enableAutoSizing = false;

        // Calcular la altura total de los créditos
        creditsText.ForceMeshUpdate();

        float textHeight = creditsText.preferredHeight + 100f;

        creditsRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            textHeight
        );

        // Posición inicial
        float startY = -(canvasRect.rect.height / 2f)
                       + startOffset;

        // Posición final
        endY = (canvasRect.rect.height / 2f)
               + textHeight
               + 50f;

        creditsRect.anchoredPosition = new Vector2(0f, startY);
    }



    void Update()
    {
        if (creditsFinished) return;

        // Mover los créditos hacia arriba
        float newY = Mathf.MoveTowards(
            creditsRect.anchoredPosition.y,
            endY,
            scrollSpeed * Time.deltaTime
        );

        creditsRect.anchoredPosition = new Vector2(0f, newY);

        // Detectar cuando terminan los créditos
        if (newY >= endY - 0.01f)
        {
            creditsFinished = true;

            Debug.Log("LOS CRÉDITOS HAN TERMINADO");

            // Detener la música
            if (creditsMusic != null)
            {
                creditsMusic.Stop();
            }

            // Cambiar de escena si hay una configurada
            if (!string.IsNullOrWhiteSpace(nextSceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }
    }


}
