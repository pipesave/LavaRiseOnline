using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerController : MonoBehaviourPun
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 70f;
    public LayerMask capaPiso;

    [Header("Proyectil")]
    public string rutaPrefabProyectil = "Prefabs/Proyectil";
    public float cooldownDisparo = 1.5f;
    public Transform puntoDisparo;

    [Header("Escudo")]
    public GameObject escudo;
    public float duracionEscudo = 0.5f;
    public float cooldownEscudo = 3f;

    [Header("Nametag")]
    public TextMeshPro textoNombre;

    private Rigidbody2D rb;
    private bool enPiso;
    private float ultimoDisparo = -99f;
    private float ultimoEscudo = -99f;
    private bool escudoActivo = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (escudo) escudo.SetActive(false);
        
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        float h = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(h * velocidad, rb.velocity.y);

        if (Input.GetKeyDown(KeyCode.Space) && enPiso)
            rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);

        if (Input.GetMouseButtonDown(0) && Time.time - ultimoDisparo >= cooldownDisparo)
        {
            ultimoDisparo = Time.time;
            Disparar();
        }

        if (Input.GetMouseButtonDown(1) && !escudoActivo && Time.time - ultimoEscudo >= cooldownEscudo)
        {
            ultimoEscudo = Time.time;
            photonView.RPC("RPC_ActivarEscudo", RpcTarget.All);
        }
    }

    void LateUpdate()
    {
        if (textoNombre != null)
            textoNombre.transform.rotation = Quaternion.identity;
    }

    void Disparar()
    {
        Vector3 posicionMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direccion = (posicionMouse - transform.position).normalized;
        Vector3 spawnPos = puntoDisparo ? puntoDisparo.position : transform.position;
        GameObject obj = PhotonNetwork.Instantiate(rutaPrefabProyectil, spawnPos, Quaternion.identity);
        obj.GetComponent<Proyectil>().Inicializar(direccion);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & capaPiso) != 0)
            enPiso = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & capaPiso) != 0)
            enPiso = false;
    }

    [PunRPC]
    public void RPC_SetNombre(string nombre)
    {
        if (textoNombre != null)
            textoNombre.text = nombre;
    }

    [PunRPC]
    public void RPC_RecibirProyectil(float vx, float vy)
    {
        if (!photonView.IsMine) return;
        rb.velocity = Vector2.zero;
        rb.AddForce(new Vector2(vx, vy), ForceMode2D.Impulse);
    }

    [PunRPC]
    void RPC_ActivarEscudo()
    {
        escudoActivo = true;
        if (escudo) escudo.SetActive(true);
        Invoke(nameof(DesactivarEscudo), duracionEscudo);
    }

    void DesactivarEscudo()
    {
        escudoActivo = false;
        if (escudo) escudo.SetActive(false);
        if (photonView.IsMine)
            photonView.RPC("RPC_DesactivarEscudo", RpcTarget.Others);
    }

    [PunRPC]
    void RPC_DesactivarEscudo()
    {
        escudoActivo = false;
        if (escudo) escudo.SetActive(false);
        CancelInvoke(nameof(DesactivarEscudo));
    }
}