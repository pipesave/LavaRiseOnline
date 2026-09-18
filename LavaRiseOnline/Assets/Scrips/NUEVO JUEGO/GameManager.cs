using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    [Header("Jugador")]
    public string rutaPrefabJugador = "Prefabs/Player";
    public Transform[] puntosSpawn;

    [Header("Lava")]
    public LavaRise lava;

    [Header("UI")]
    public TextMeshProUGUI textoEstado;

    [Header("Panel Final")]
    public GameObject panelFinal;
    public TextMeshProUGUI textoResultado;
    public TextMeshProUGUI textoGanador;
    public Button botonSalir;

    [Header("Panel Muerte")]
    public GameObject panelMuerte;

    [Header("Espectador")]
    public EspectadorBombas espectadorBombas;

    [Header("Escenas")]
    public string escenaMenu = "Menu";

    private bool juegoIniciado;
    private PlayerController miPlayerController;
    private bool juegoTerminado;
    private bool yoListo;
    private int jugadoresListos;

    void Awake() => Instance = this;

    void Start()
    {
        if (panelFinal) panelFinal.SetActive(false);
        if (panelMuerte) panelMuerte.SetActive(false);
        if (textoEstado) textoEstado.text = "Esperando jugadores...";
        if (botonSalir) botonSalir.onClick.AddListener(Salir);
        StartCoroutine(AvisarAlMaster());
    }

    //esperar medio segundo para avisar al master que entro un jugador, para que este lo spawnee
    IEnumerator AvisarAlMaster()
    {
        yield return new WaitForSeconds(0.5f);
        photonView.RPC("RPC_JugadorEntro", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    void RPC_JugadorEntro(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int indice = (actorNumber - 1) % puntosSpawn.Length;
        Vector3 pos = puntosSpawn[indice].position;

        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (p.ActorNumber == actorNumber)
            {
                photonView.RPC("RPC_Spawnear", p, pos.x, pos.y);
                break;
            }
        }
    }

    [PunRPC]
    void RPC_Spawnear(float x, float y)
    {
        GameObject player = PhotonNetwork.Instantiate(rutaPrefabJugador, new Vector3(x, y, 0), Quaternion.identity);

        miPlayerController = player.GetComponent<PlayerController>();
        miPlayerController.enabled = false;

        string nombre = PhotonNetwork.NickName;
        player.GetComponent<PhotonView>().RPC("RPC_SetNombre", RpcTarget.AllBuffered, nombre);

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "vivo", true } });
        if (textoEstado) textoEstado.text = "Presiona E para estar listo!";
    }

    void Update()
    {
        if (!yoListo && Input.GetKeyDown(KeyCode.E))
        {
            if (PhotonNetwork.PlayerList.Length < 2)
            {
                if (textoEstado) textoEstado.text = "Se necesitan al menos 2 jugadores!";
                return;
            }

            yoListo = true;
            if (textoEstado) textoEstado.text = "Esperando jugadores...";
            photonView.RPC("RPC_JugadorListo", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_JugadorListo()
    {
        jugadoresListos++;
        if (jugadoresListos >= PhotonNetwork.PlayerList.Length)
        {
            if (textoEstado) textoEstado.text = "la lava esta subiendo!";
            juegoIniciado = true;
            lava?.StartRising();
            if (miPlayerController != null)
                miPlayerController.enabled = true;

            CerrarSala();
        }
    }

    // llamado cuando un nuevo jugador entra a la sala, para cerrar la sala si ya esta llena
    public override void OnPlayerEnteredRoom(Player nuevoJugador)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (PhotonNetwork.CurrentRoom.PlayerCount >= PhotonNetwork.CurrentRoom.MaxPlayers)
            CerrarSala();
    }

  
    private void CerrarSala()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!PhotonNetwork.CurrentRoom.IsOpen) return; 

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
    }

    public void YoMori()
    {
        if (juegoTerminado) return;

        if (panelMuerte) panelMuerte.SetActive(true);

        CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
        if (cam) cam.ActivarModoEspectador();

        if (espectadorBombas) espectadorBombas.Activar();

        photonView.RPC("RPC_ReportarMuerte", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    void RPC_ReportarMuerte(int actorMuerto)
    {
        if (!PhotonNetwork.IsMasterClient || juegoTerminado) return;

        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (p.ActorNumber == actorMuerto)
            {
                p.SetCustomProperties(new Hashtable { { "vivo", false } });
                break;
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PhotonNetwork.IsMasterClient || juegoTerminado || !juegoIniciado) return;
        if (changedProps.ContainsKey("vivo"))
            VerificarGanador();
    }

    public override void OnPlayerLeftRoom(Player jugador)
    {
        if (PhotonNetwork.IsMasterClient && juegoIniciado && !juegoTerminado) VerificarGanador();
    }

    private void VerificarGanador()
    {
        var lista = PhotonNetwork.PlayerList;
        if (lista.Length == 0) return;

        Player ganador = null;
        int vivos = 0;
        foreach (var p in lista)
        {
            if (p.CustomProperties.TryGetValue("vivo", out var v) && v is bool b && b)
            {
                vivos++;
                ganador = p;
            }
        }

        if (vivos == 1 && ganador != null)
        {
            juegoTerminado = true;
            photonView.RPC("RPC_FinJuego", RpcTarget.All, ganador.ActorNumber);
        }
        else if (vivos == 0)
        {
            juegoTerminado = true;
            photonView.RPC("RPC_FinJuego", RpcTarget.All, -1);
        }
    }

    [PunRPC]
    void RPC_FinJuego(int actorGanador)
    {
        juegoTerminado = true;
        lava?.gameObject.SetActive(false);

        if (panelMuerte) panelMuerte.SetActive(false);
        if (panelFinal) panelFinal.SetActive(true);

        if (textoResultado)
        {
            bool gane = PhotonNetwork.LocalPlayer.ActorNumber == actorGanador;
            textoResultado.text = gane ? "Ganaste!" : "Perdiste";
            textoResultado.color = gane ? Color.yellow : Color.red;
        }

        if (textoGanador)
        {
            string nombreGanador = "Jugador";
            foreach (var p in PhotonNetwork.PlayerList)
            {
                if (p.ActorNumber == actorGanador)
                {
                    nombreGanador = p.NickName;
                    break;
                }
            }
            textoGanador.text = "Gano: " + nombreGanador;
        }
    }

    public void Salir()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadScene(escenaMenu);
    }
}