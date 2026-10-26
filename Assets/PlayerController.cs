
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CapsuleCollider))]
public class Script : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float velocidadAcelerada = 10f;

    [Header("Agacharse")]
    public float alturaAgachado = 1f;

    [Header("Resistencia")]
    public float resistenciaMaxima = 100f;
    public float resistencia = 100f;
    public float consumoResistencia = 20f;
    public float recuperacionResistencia = 15f;

    [Header("Interfaz")]
    public Slider barraResistencia;
    public Image rellenoBarra;

    private CapsuleCollider capsula;
    private float alturaOriginal;
    private Vector3 centroOriginal;
    private bool agotado = false;

    void Start()
    {
        capsula = GetComponent<CapsuleCollider>();

        alturaOriginal = capsula.height;
        centroOriginal = capsula.center;

        resistencia = Mathf.Clamp(
            resistencia, 0f, resistenciaMaxima
        );

        if (barraResistencia != null)
        {
            barraResistencia.minValue = 0f;
            barraResistencia.maxValue = resistenciaMaxima;
            barraResistencia.value = resistencia;
        }

        ActualizarBarra();
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        // Leer movimiento WASD
        float x = 0f;
        float z = 0f;

        if (Keyboard.current.wKey.isPressed) z++;
        if (Keyboard.current.sKey.isPressed) z--;
        if (Keyboard.current.aKey.isPressed) x--;
        if (Keyboard.current.dKey.isPressed) x++;

        Vector3 movimiento =
            new Vector3(x, 0f, z).normalized;

        // Solo correr si te estás moviendo
        bool quiereCorrer =
            Keyboard.current.leftShiftKey.isPressed &&
            movimiento.sqrMagnitude > 0f;

        // Consumir o recuperar resistencia
        if (quiereCorrer && !agotado)
        {
            resistencia -=
                consumoResistencia * Time.deltaTime;
        }
        else
        {
            resistencia +=
                recuperacionResistencia * Time.deltaTime;
        }

        resistencia = Mathf.Clamp(
            resistencia, 0f, resistenciaMaxima
        );

        // Bloquear carrera al agotarse
        if (resistencia <= 0f)
            agotado = true;

        // Volver a permitir correr con 25 de resistencia
        if (agotado && resistencia >= 25f)
            agotado = false;

        bool corriendo = quiereCorrer && !agotado;

        float velocidadActual = corriendo
            ? velocidadAcelerada
            : velocidad;

        // Aplicar movimiento
        transform.Translate(
            movimiento * velocidadActual * Time.deltaTime,
            Space.Self
        );

        // Agacharse con Ctrl
        bool agachado =
            Keyboard.current.leftCtrlKey.isPressed;

        if (agachado)
        {
            capsula.height = Mathf.Clamp(
                alturaAgachado, 0.1f, alturaOriginal
            );

            capsula.center = new Vector3(
                centroOriginal.x,
                centroOriginal.y -
                    (alturaOriginal - capsula.height) / 2f,
                centroOriginal.z
            );
        }
        else
        {
            capsula.height = alturaOriginal;
            capsula.center = centroOriginal;
        }

        // Actualizar interfaz cada fotograma
        ActualizarBarra();
    }

    void ActualizarBarra()
    {
        float porcentaje = resistenciaMaxima > 0f
            ? resistencia / resistenciaMaxima
            : 0f;

        if (barraResistencia != null)
        {
            barraResistencia.maxValue = resistenciaMaxima;
            barraResistencia.value = resistencia;
        }

        if (rellenoBarra != null)
        {
            if (porcentaje > 0.5f)
                rellenoBarra.color = Color.green;
            else if (porcentaje > 0.25f)
                rellenoBarra.color = Color.yellow;
            else
                rellenoBarra.color = Color.red;
        }
    }
}
