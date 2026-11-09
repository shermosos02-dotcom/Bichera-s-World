using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GestorEnemigos : MonoBehaviour
{
    [Header("Referencias")]
    public Camera cam;                        
    public GameObject enemigoPrefab;          

    [Header("Aparición")]
    public int cantidad = 2;
    public float radioMaximo = 20f;           // Distancia máxima a la que pueden aparecer
    public float distanciaMinimaJugador = 8f; // Distancia mínima al jugador

    [Header("Persecución")]
    public float velocidad = 3.5f;
    public float distanciaDeteccion = 15f;    // A partir de esta distancia te ve
    public float distanciaParada = 1.1f;      // Se detiene a esta distancia
    public float velocidadGiro = 8f;

    [Header("Contacto")]
    public bool reiniciarAlTocar = true;      // Reinicia la escena si te alcanzan
    public float distanciaContacto = 1.3f;

    [Header("Apuntado")]
    public float distanciaApuntado = 40f;
    public Color colorResaltado = new Color(1f, 1f, 0f, 0.4f); // Amarillo transparente
    public float escalaResaltado = 1.1f;      // Cuánto más grande es que el enemigo

    class Enemigo
    {
        public GameObject cuerpo;
        public GameObject resaltado;
    }

    private readonly List<Enemigo> enemigos = new List<Enemigo>();
    private Enemigo objetivoActual;
    private Material materialResaltado;

    void Start()
    {
        if (cam == null) cam = Camera.main;

        if (enemigoPrefab == null)
        {
            Debug.LogError("GestorEnemigos: falta asignar el enemigo en 'Enemigo Prefab'.");
            return;
        }

        // Material compartido para el resaltado amarillo transparente
        materialResaltado = new Material(Shader.Find("Sprites/Default"));
        materialResaltado.color = colorResaltado;

        for (int i = 0; i < cantidad; i++)
            CrearEnemigo(PosicionAleatoria());
    }

    void Update()
    {
        Perseguir();
        Apuntar();
    }

    // ---------- APARICIÓN ----------

    Vector3 PosicionAleatoria()
    {
        float angulo = Random.Range(0f, Mathf.PI * 2f);
        float distancia = Random.Range(distanciaMinimaJugador, radioMaximo);

        return transform.position + new Vector3(
            Mathf.Cos(angulo) * distancia,
            0f,
            Mathf.Sin(angulo) * distancia);
    }

    // Busca la altura del suelo bajo una posición (si no hay, usa la altura del jugador - 1)
    float BuscarSuelo(Vector3 posicion)
    {
        Vector3 origen = new Vector3(posicion.x, transform.position.y + 5f, posicion.z);
        if (Physics.Raycast(origen, Vector3.down, out RaycastHit hit, 100f))
            return hit.point.y;

        return transform.position.y - 1f;
    }

    Bounds CalcularBounds(GameObject obj)
    {
        Renderer[] rs = obj.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0)
            return new Bounds(obj.transform.position, Vector3.one);

        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++)
            b.Encapsulate(rs[i].bounds);
        return b;
    }

    void CrearEnemigo(Vector3 posicion)
    {
        float sueloY = BuscarSuelo(posicion);

        GameObject cuerpo = Instantiate(enemigoPrefab, posicion, Quaternion.identity);
        cuerpo.name = "Enemigo";

        // Apoyar el enemigo sobre el suelo, sea cual sea su punto de pivote
        Bounds b = CalcularBounds(cuerpo);
        float desplazamiento = sueloY - b.min.y;
        cuerpo.transform.position += Vector3.up * desplazamiento;
        b.center += Vector3.up * desplazamiento;

        // Lo movemos por código: si tiene Rigidbody, lo dejamos kinemático
        Rigidbody rb = cuerpo.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // Necesita un collider para poder apuntarlo; si no tiene, se le añade uno
        if (cuerpo.GetComponentInChildren<Collider>() == null)
        {
            BoxCollider bc = cuerpo.AddComponent<BoxCollider>();
            Vector3 s = cuerpo.transform.lossyScale;
            bc.center = cuerpo.transform.InverseTransformPoint(b.center);
            bc.size = new Vector3(b.size.x / s.x, b.size.y / s.y, b.size.z / s.z);
        }

        // Caja amarilla transparente que envuelve al enemigo (oculta hasta apuntarlo)
        GameObject resaltado = GameObject.CreatePrimitive(PrimitiveType.Cube);
        resaltado.name = "Resaltado";
        Destroy(resaltado.GetComponent<Collider>());
        resaltado.transform.SetParent(cuerpo.transform, false);

        Vector3 escalaPadre = cuerpo.transform.lossyScale;
        resaltado.transform.localPosition = cuerpo.transform.InverseTransformPoint(b.center);
        resaltado.transform.localRotation = Quaternion.identity;
        resaltado.transform.localScale = new Vector3(
            b.size.x * escalaResaltado / escalaPadre.x,
            b.size.y * escalaResaltado / escalaPadre.y,
            b.size.z * escalaResaltado / escalaPadre.z);

        MeshRenderer mr = resaltado.GetComponent<MeshRenderer>();
        mr.sharedMaterial = materialResaltado;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        resaltado.SetActive(false);

        enemigos.Add(new Enemigo { cuerpo = cuerpo, resaltado = resaltado });
    }

    // ---------- PERSECUCIÓN ----------

    void Perseguir()
    {
        foreach (Enemigo e in enemigos)
        {
            Transform t = e.cuerpo.transform;

            // Dirección horizontal hacia el jugador
            Vector3 direccion = transform.position - t.position;
            direccion.y = 0f;
            float distancia = direccion.magnitude;

            // Si te alcanza, reinicia la escena
            if (reiniciarAlTocar && distancia <= distanciaContacto)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }

            // Fuera del alcance de visión: no hace nada
            if (distancia > distanciaDeteccion) continue;

            // Mirar al jugador suavemente
            if (direccion.sqrMagnitude > 0.001f)
            {
                Quaternion giro = Quaternion.LookRotation(direccion);
                t.rotation = Quaternion.Slerp(t.rotation, giro, velocidadGiro * Time.deltaTime);
            }

            // Avanzar hasta la distancia de parada
            if (distancia > distanciaParada)
                t.position += direccion.normalized * velocidad * Time.deltaTime;
        }
    }
    

    void Apuntar()
    {
        if (cam == null) return;
        
        Enemigo nuevo = null;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward,
                out RaycastHit hit, distanciaApuntado))
        {
            Transform golpeado = hit.collider.transform;
            nuevo = enemigos.Find(e => golpeado.IsChildOf(e.cuerpo.transform));
        }

        // Cambiar el resaltado si apuntamos a otro (o a ninguno)
        if (nuevo != objetivoActual)
        {
            if (objetivoActual != null && objetivoActual.resaltado != null)
                objetivoActual.resaltado.SetActive(false);

            if (nuevo != null)
                nuevo.resaltado.SetActive(true);

            objetivoActual = nuevo;
        }

        // Pulsar U: eliminar el enemigo apuntado
        if (objetivoActual != null &&
            Keyboard.current != null &&
            Keyboard.current.uKey.wasPressedThisFrame)
        {
            Destroy(objetivoActual.cuerpo);
            enemigos.Remove(objetivoActual);
            objetivoActual = null;
        }
    }
}