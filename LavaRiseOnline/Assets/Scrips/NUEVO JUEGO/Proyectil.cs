using UnityEngine;
using Photon.Pun;

public class Proyectil : MonoBehaviourPun
{
    public float velocidad = 6f;
    public float fuerzaImpacto = 12f;
    public float tiempoVida = 5f;

    private Vector2 direccion;
    private Rigidbody2D rb;
    private Photon.Realtime.Player ownerInicial;
    private bool ignorarOwner = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Inicializar(Vector2 dir)
    {
        direccion = dir.normalized;
        rb.velocity = direccion * velocidad;
        ownerInicial = photonView.Owner;

        if (photonView.IsMine)
        {
            Invoke(nameof(DestruirMe), tiempoVida);
            // Despues de 0.5f el proyectil ya puede golpear al que lo disparo (bug arreglado) :)
            Invoke(nameof(DesactivarIgnorarOwner), 0.5f);
        }
    }

    void DesactivarIgnorarOwner()
    {
        ignorarOwner = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!photonView.IsMine) return;

        PhotonView pvOther = other.GetComponent<PhotonView>();

        // Ignorar al owner solo durante los primeros 0.5f
        if (ignorarOwner && pvOther != null && pvOther.Owner == ownerInicial) return;

        // Choco con un escudo — rebotar
        if (other.CompareTag("Escudo"))
        {
            direccion = new Vector2(-direccion.x, direccion.y);
            rb.velocity = direccion * velocidad;
            // Al rebotar, ya puede golpear a cualquiera incluyendo al owner
            ignorarOwner = false;
            return;
        }

        // Choco con un jugador
        if (pvOther != null && other.CompareTag("Player"))
        {
            pvOther.RPC("RPC_RecibirProyectil", RpcTarget.All,
                direccion.x * fuerzaImpacto * 2f,
                direccion.y * fuerzaImpacto * 0.3f);

            DestruirMe();
        }
    }

    void DestruirMe()
    {
        CancelInvoke();
        PhotonNetwork.Destroy(gameObject);
    }
}