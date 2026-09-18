using UnityEngine;
using Photon.Pun;

//color unico a cada jugador.
public class PlayerColor : MonoBehaviourPun
{
    private static readonly Color[] colores = {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.magenta,
        Color.cyan,
        Color.red + Color.yellow, // naranja :)
        Color.white
    };

    void Start()
    {
        //ActorNumber  elige color unico basado en el orden de entrada
        int indice = (photonView.Owner.ActorNumber - 1) % colores.Length;
        Color miColor = colores[indice];

        photonView.RPC("RPC_SetColor", RpcTarget.AllBuffered, miColor.r, miColor.g, miColor.b);
    }

    [PunRPC]
    void RPC_SetColor(float r, float g, float b)
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = new Color(r, g, b);
    }
}
