using UnityEngine;
using UnityEngine.InputSystem;

public class PrimeraPersona : MonoBehaviour
{
    public Transform camara;
    public float sensibilidad = 0.2f;

    private float rotacionX = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Mouse.current == null)
            return;

        camara.position = transform.position;

        float ratonX = Mouse.current.delta.x.ReadValue() * sensibilidad;
        float ratonY = Mouse.current.delta.y.ReadValue() * sensibilidad;

        transform.Rotate(Vector3.up * ratonX);

        rotacionX -= ratonY;
        rotacionX = Mathf.Clamp(rotacionX, -90f, 90f);

        camara.rotation = Quaternion.Euler(rotacionX, transform.eulerAngles.y, 0f);
    }
}