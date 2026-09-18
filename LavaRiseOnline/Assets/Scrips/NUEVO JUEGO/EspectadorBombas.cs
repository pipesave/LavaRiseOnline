using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using TMPro;

public class EspectadorBombas : MonoBehaviour
{
    public string rutaPrefabBomba = "Prefabs/BombaZona";
    public int bombasIniciales = 3;
    public TextMeshProUGUI textoBombas;
    private int bombasRestantes;
    private bool activo;

    public void Activar()
    {
        bombasRestantes = bombasIniciales;
        activo = true;
        ActualizarTexto();
        Debug.Log("EspectadorBombas.Activar() ejecutado. bombasRestantes=" + bombasRestantes);
    }

    void Update()
    {
        if (!activo || bombasRestantes <= 0) return;
        if (!Input.GetMouseButtonDown(0)) return;

        bool sobreUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        
        if (sobreUI) return;

        Vector3 posMundo = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        posMundo.z = 0f;

        Debug.Log($"Instanciando bomba en {posMundo}");
        PhotonNetwork.Instantiate(rutaPrefabBomba, posMundo, Quaternion.identity);

        bombasRestantes--;
        ActualizarTexto();
    }

    void ActualizarTexto()
    {
        if (textoBombas) textoBombas.text = "Bombas: " + bombasRestantes;
    }
}