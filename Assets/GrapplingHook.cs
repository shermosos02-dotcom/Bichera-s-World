using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gancho 3D que TIRA del jugador hacia el punto (nuevo Input System).
/// Click izquierdo: lanza y tira. Soltar click: suelta.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class GrapplingHook : MonoBehaviour
{
    [Header("Referencias")]
    public Camera cam;
    public Transform ropeOrigin;
    public LineRenderer lineRenderer;

    [Header("Ajustes")]
    public float maxDistance = 40f;
    public LayerMask grappleLayers = ~0;
    public float pullAcceleration = 100f;   // Fuerza del tirón (sube si es lento)
    public float maxSpeed = 45f;           // Velocidad máxima mientras tira
    public float stopDistance = 2f;        // Distancia a la que deja de tirar
    public float liftOnStart = 4f;         // Pequeño impulso hacia arriba para despegar del suelo

    [Header("Cuerda")]
    public float ropeWidth = 0.1f;
    public Color ropeColor = Color.white;

    private Rigidbody rb;
    private bool grappling;
    private Vector3 grapplePoint;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Configuración del LineRenderer para que SIEMPRE se vea
        lineRenderer.useWorldSpace = true;          // Imprescindible: posiciones en el mundo
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = ropeWidth;
        lineRenderer.endWidth = ropeWidth;
        lineRenderer.startColor = ropeColor;
        lineRenderer.endColor = ropeColor;

        // Si no hay material asignado, crea uno simple (evita el rosa/invisible)
        if (lineRenderer.sharedMaterial == null || lineRenderer.sharedMaterial.name.Contains("Default-Line"))
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame) StartGrapple();
        if (mouse.leftButton.wasReleasedThisFrame) StopGrapple();
    }

    void FixedUpdate()
    {
        if (!grappling) return;

        Vector3 toPoint = grapplePoint - transform.position;
        float distance = toPoint.magnitude;

        // Si ya llegó, deja de tirar (se queda colgado/cerca)
        if (distance <= stopDistance) return;

        // Tirón hacia el punto
        rb.AddForce(toPoint.normalized * pullAcceleration, ForceMode.Acceleration);

        // Limitar velocidad
        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
    }

    void LateUpdate()
    {
        if (!grappling) return;
        lineRenderer.SetPosition(0, ropeOrigin.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }

    void StartGrapple()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward,
                out RaycastHit hit, maxDistance, grappleLayers))
        {
            grapplePoint = hit.point;
            grappling = true;

            // Impulso inicial para despegar del suelo y vencer la fricción
            rb.AddForce(Vector3.up * liftOnStart, ForceMode.VelocityChange);

            lineRenderer.positionCount = 2;
        }
    }

    void StopGrapple()
    {
        grappling = false;
        lineRenderer.positionCount = 0;
    }
}