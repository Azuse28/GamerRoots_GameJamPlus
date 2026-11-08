using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
public class HechizoWarlock : MonoBehaviour
{
    [SerializeField] private float velocidad = 10f;
    [SerializeField] private float distanciaMaxima = 8f;
    [SerializeField] private float tiempoMaximo = 3f;
    [SerializeField] private int danio = 1;

    [Header("Visual")]
    [SerializeField] private Color colorLinea = Color.red;
    [SerializeField] private float anchoLinea = 0.15f;

    private LineRenderer lineaVisual;
    private float direccion;
    private Vector3 posicionOrigen;
    private float tiempoVivido;

    public void Inicializar(float dir, GameObject caster)
    {
        direccion = dir;
        posicionOrigen = transform.position;

        CapsuleCollider2D miCol = GetComponent<CapsuleCollider2D>();
        foreach (Collider2D colisionador in caster.GetComponentsInChildren<Collider2D>())
            Physics2D.IgnoreCollision(miCol, colisionador);
    }

    void Awake()
    {
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        col.isTrigger = true;
        col.direction = CapsuleDirection2D.Horizontal;
        col.size = new Vector2(1.2f, 0.8f);

        lineaVisual = gameObject.AddComponent<LineRenderer>();
        lineaVisual.positionCount = 2;
        lineaVisual.startWidth = anchoLinea;
        lineaVisual.endWidth = anchoLinea;
        lineaVisual.startColor = colorLinea;
        lineaVisual.endColor = colorLinea;
        lineaVisual.useWorldSpace = true;
        lineaVisual.sortingOrder = 1;
    }

    void Update()
    {
        tiempoVivido += Time.deltaTime;
        transform.position += new Vector3(
            direccion * velocidad * Time.deltaTime, 0f, 0f);

        lineaVisual.SetPosition(0, posicionOrigen);
        lineaVisual.SetPosition(1, transform.position);

        float distancia = Mathf.Abs(transform.position.x - posicionOrigen.x);
        if (distancia >= distanciaMaxima || tiempoVivido >= tiempoMaximo)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Enemigo enemigo = other.GetComponentInParent<Enemigo>();
        if (enemigo == null) return;

        if (enemigo.PuedeDeflectar())
        {
            Destroy(gameObject);
            return;
        }

        enemigo.RecibirDanio(danio);
        Destroy(gameObject);
    }
}