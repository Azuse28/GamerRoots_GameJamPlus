using UnityEngine;
using UnityEngine.InputSystem;

public class LianaCortable : MonoBehaviour
{
    [Header("Corte")]
    [Min(0.1f)] [SerializeField] private float radioCorte = 1.25f;
    [SerializeField] private LayerMask capasInteraccion = ~0;

    [Header("Peligro para Warlock")]
    [Min(0.1f)] [SerializeField] private float radioPeligroWarlock = 2f;

    [Header("Objeto de la liana")]
    [SerializeField] private GameObject raizLiana;

    private bool cortada;
    private Collider2D[] colisionadoresLiana;
    private Renderer[] renderersLiana;

    private GameObject ObjetivoLiana => raizLiana != null
        ? raizLiana
        : gameObject;

    void Awake()
    {
        colisionadoresLiana = ObjetivoLiana.GetComponentsInChildren<Collider2D>(true);
        renderersLiana = ObjetivoLiana.GetComponentsInChildren<Renderer>(true);
    }

    void OnEnable()
    {
        WizardController.MagiaUsada += PenalizarMagiaCercana;
    }

    void OnDisable()
    {
        WizardController.MagiaUsada -= PenalizarMagiaCercana;
    }

    void Update()
    {
        if (cortada) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame &&
            BanditEnRango())
        {
            Cortar();
            return;
        }

        PenalizarWarlockCercano();
    }

    bool BanditEnRango()
    {
        Collider2D[] cercanos = Physics2D.OverlapCircleAll(
            ObjetivoLiana.transform.position, radioCorte, capasInteraccion);

        foreach (Collider2D cercano in cercanos)
        {
            if (cercano.GetComponentInParent<BanditController>() != null)
                return true;
        }

        return false;
    }

    void PenalizarWarlockCercano()
    {
        Collider2D[] cercanos = Physics2D.OverlapCircleAll(
            ObjetivoLiana.transform.position, radioPeligroWarlock, capasInteraccion);

        foreach (Collider2D cercano in cercanos)
        {
            WizardController wizard = cercano.GetComponentInParent<WizardController>();
            if (wizard != null)
            {
                wizard.MorirPorLiana();
                return;
            }
        }
    }

    void PenalizarMagiaCercana(WizardController wizard)
    {
        if (cortada || wizard == null) return;

        float distanciaMaxima = radioPeligroWarlock * radioPeligroWarlock;
        Vector2 diferencia = (Vector2)(wizard.transform.position - ObjetivoLiana.transform.position);
        if (diferencia.sqrMagnitude <= distanciaMaxima)
            wizard.MorirPorLiana();
    }

    void Cortar()
    {
        cortada = true;

        foreach (Collider2D colisionador in colisionadoresLiana)
        {
            if (colisionador != null)
                colisionador.enabled = false;
        }

        foreach (Renderer rendererLiana in renderersLiana)
        {
            if (rendererLiana != null)
                rendererLiana.enabled = false;
        }

        GameObject objetivo = ObjetivoLiana;
        objetivo.SetActive(false);
        Destroy(objetivo);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 centro = raizLiana != null ? raizLiana.transform.position : transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(centro, radioCorte);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centro, radioPeligroWarlock);
    }
}