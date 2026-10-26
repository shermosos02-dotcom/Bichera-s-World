using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public GameObject panelAjustes;
    public GameObject botonJugar;
    public GameObject botonAjustes;

    public void Jugar()
    {
        SceneManager.LoadScene("NombreDeTuEscena");
    }

    public void AbrirAjustes()
    {
        panelAjustes.SetActive(true);
        botonJugar.SetActive(false);
        botonAjustes.SetActive(false);
    }

    public void CerrarAjustes()
    {
        panelAjustes.SetActive(false);
        botonJugar.SetActive(true);
        botonAjustes.SetActive(true);
    }
}