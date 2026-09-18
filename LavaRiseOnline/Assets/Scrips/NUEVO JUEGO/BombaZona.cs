using UnityEngine;
using Photon.Pun;

public class BombaZona : MonoBehaviourPun
{
    public float radio = 2f;
    public float delayExplosion = 1f;
    public float duracionExplosion = 0.25f;
    public float fuerzaImpacto = 20f;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (photonView.IsMine)
            Invoke(nameof(Explotar), delayExplosion);
    }

    void Explotar()
    {
        photonView.RPC("RPC_MostrarExplosion", RpcTarget.All);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radio);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;
            PhotonView pvOther = hit.GetComponent<PhotonView>();
            if (pvOther == null) continue;

            Vector2 dir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
            if (dir == Vector2.zero) dir = Vector2.up;

            pvOther.RPC("RPC_RecibirProyectil", RpcTarget.All,
                dir.x * fuerzaImpacto, dir.y * fuerzaImpacto);
        }

        Invoke(nameof(DestruirMe), duracionExplosion);
    }

    [PunRPC]
    void RPC_MostrarExplosion()
    {
        if (sr) sr.color = new Color(1f, 0.4f, 0f, 0.9f);
    }

    void DestruirMe()
    {
        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}