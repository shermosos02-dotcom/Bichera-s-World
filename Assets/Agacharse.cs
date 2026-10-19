using UnityEngine;
using UnityEngine.InputSystem;

public class Agacharse : MonoBehaviour
{
    public float alturaAgachado = 0.5f;

    private bool agachado = false;

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame && !agachado)
        {
            transform.Translate(Vector3.down * alturaAgachado, Space.Self);
            agachado = true;
        }

        if (Keyboard.current.leftCtrlKey.wasReleasedThisFrame && agachado)
        {
            transform.Translate(Vector3.up * alturaAgachado, Space.Self);
            agachado = false;
        }
    }
}