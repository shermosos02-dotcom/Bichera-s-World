
using UnityEngine;
using UnityEngine.InputSystem;

public class Script : MonoBehaviour
{
    public float velocidad = 5f;
    public float velocidadAcelerada = 10f;
    public float alturaAgachado = 1f;

    private CapsuleCollider capsula;
    private float alturaOriginal;
    private Vector3 centroOriginal;
    private Vector3 escalaOriginal;

    void Start()
    {
        capsula = GetComponent<CapsuleCollider>();

        if (capsula != null)
        {
            alturaOriginal = capsula.height;
            centroOriginal = capsula.center;
        }

        escalaOriginal = transform.localScale;
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        float x = 0f;
        float z = 0f;

        if (Keyboard.current.wKey.isPressed) z++;
        if (Keyboard.current.sKey.isPressed) z--;
        if (Keyboard.current.aKey.isPressed) x--;
        if (Keyboard.current.dKey.isPressed) x++;

        bool agachado = Keyboard.current.leftCtrlKey.isPressed;

        if (capsula != null)
        {
            float nuevaAltura = agachado
                ? Mathf.Clamp(alturaAgachado, capsula.radius * 2f, alturaOriginal)
                : alturaOriginal;

            capsula.height = nuevaAltura;
            capsula.center = new Vector3(
                centroOriginal.x,
                centroOriginal.y - (alturaOriginal - nuevaAltura) / 2f,
                centroOriginal.z
            );
        }

        transform.localScale = new Vector3(
            escalaOriginal.x,
            escalaOriginal.y * (agachado && alturaOriginal > 0f
                ? Mathf.Clamp(alturaAgachado / alturaOriginal, 0.1f, 1f)
                : 1f),
            escalaOriginal.z
        );

        float velocidadActual = Keyboard.current.leftShiftKey.isPressed
            ? velocidadAcelerada
            : velocidad;

        Vector3 movimiento = new Vector3(x, 0f, z).normalized;

        transform.Translate(
            movimiento * velocidadActual * Time.deltaTime,
            Space.Self
        );
    }
}
