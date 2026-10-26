using UnityEngine;
using UnityEngine.InputSystem;

public class Agacharse : MonoBehaviour
{
    private bool agachado = false;
    public float alturaAgachado = 0.4f;

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
            agachado = true;

        if (Keyboard.current.leftCtrlKey.wasReleasedThisFrame)
            agachado = false;

        transform.localScale = agachado
            ? Vector3.one * alturaAgachado
            : Vector3.one;
    }
}