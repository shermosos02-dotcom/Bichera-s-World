
using UnityEngine;
using UnityEngine.InputSystem;

public class ScriptSalto : MonoBehaviour
{
    public float fuerzaSalto = 7f;
    public float fuerzaSaltoPared = 7f;
    public float impulsoPared = 6f;

    public GameObject suelo;
    public GameObject pared;

    private Rigidbody rb;
    private bool enSuelo;
    private bool enPared;
    private Vector3 normalPared;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Keyboard.current == null || rb == null)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (enSuelo)
            {
                Vector3 velocidad = rb.linearVelocity;
                velocidad.y = 0f;
                rb.linearVelocity = velocidad;

                rb.AddForce(
                    Vector3.up * fuerzaSalto,
                    ForceMode.Impulse
                );

                enSuelo = false;
            }
            else if (enPared)
            {
                rb.linearVelocity = Vector3.zero;

                rb.AddForce(
                    Vector3.up * fuerzaSaltoPared +
                    normalPared * impulsoPared,
                    ForceMode.Impulse
                );
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ComprobarColision(collision, true);
    }

    private void OnCollisionStay(Collision collision)
    {
        ComprobarColision(collision, true);
    }

    private void OnCollisionExit(Collision collision)
    {
        ComprobarColision(collision, false);
    }

    void ComprobarColision(Collision collision, bool tocando)
    {
        if (suelo != null && collision.gameObject == suelo)
        {
            enSuelo = tocando;
        }

        if (pared != null && collision.gameObject == pared)
        {
            enPared = tocando;

            if (tocando && collision.contactCount > 0)
            {
                normalPared = collision.GetContact(0).normal;
            }
        }
    }
}
