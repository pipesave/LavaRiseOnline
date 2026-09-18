using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public TMP_InputField inputNombre;
    public TextMeshProUGUI textoEstado;
    public GameObject botonPlay;

    [Header("Escena de juego")]
    public string escenaJuego = "Game 1";

    void Start()
    {
        botonPlay.SetActive(false);
        if (textoEstado) textoEstado.text = "Conectando...";

        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        if (textoEstado) textoEstado.text = "";
        botonPlay.SetActive(true);
    }

    public void BotonPlay()
    {
        string nombre = inputNombre.text.Trim();
        if (string.IsNullOrEmpty(nombre))
        {
            if (textoEstado) textoEstado.text = "Ingresa tu nombre!";
            return;
        }

        PhotonNetwork.NickName = nombre;
        PlayerPrefs.SetString("playerNickname", nombre);

        botonPlay.SetActive(false);
        if (textoEstado) textoEstado.text = "Buscando partida...";

        var opciones = new RoomOptions { MaxPlayers = 4, IsVisible = true, IsOpen = true };
        PhotonNetwork.JoinOrCreateRoom("SalaGeneral", opciones, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel(escenaJuego);
    }

    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        botonPlay.SetActive(true);

        if (returnCode == ErrorCode.GameFull || returnCode == ErrorCode.GameClosed)
        {
            if (textoEstado) textoEstado.text = "Sala llena, espere";
        }
        else
        {
            if (textoEstado) textoEstado.text = "Error: " + message;
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if (textoEstado) textoEstado.text = "Error: " + message;
        botonPlay.SetActive(true);
    }

    public void BotonSalir()
    {
        Application.Quit();
    }
}