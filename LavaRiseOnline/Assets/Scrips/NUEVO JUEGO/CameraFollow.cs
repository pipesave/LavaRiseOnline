using UnityEngine;
using Photon.Pun;

public class CameraFollow : MonoBehaviour
{
    public float velocidadSeguimiento = 5f;
    public Vector3 offset = new Vector3(0, 2f, -15f);

    private Transform objetivo;
    private bool modoEspectador = false;

    void Update()
    {
        // Buscar mi jugador si todavia no lo encontro
        if (objetivo == null && !modoEspectador)
        {
            foreach (var pv in FindObjectsOfType<PhotonView>())
            {
                if (pv.IsMine && pv.CompareTag("Player"))
                {
                    objetivo = pv.transform;
                    break;
                }
            }
        }

        // En modo espectador, si el objetivo desaparecio o murio buscar el siguiente
        if (modoEspectador && !ObjetivoSigueVivo())
            BuscarJugadorVivo();

        if (objetivo == null) return;

        Vector3 destino = objetivo.position + offset;
        transform.position = Vector3.Lerp(transform.position, destino, velocidadSeguimiento * Time.deltaTime);
    }

    public void ActivarModoEspectador()
    {
        modoEspectador = true;
        objetivo = null;
        BuscarJugadorVivo();
    }

    // Verifica si el objetivo actual sigue existiendo y sigue vivo segun Photon
    private bool ObjetivoSigueVivo()
    {
        if (objetivo == null || !objetivo.gameObject.activeInHierarchy) return false;

        var pv = objetivo.GetComponent<PhotonView>();
        if (pv == null) return false;

        // Revisar la propiedad "vivo" del jugador objetivo
        if (pv.Owner != null &&
            pv.Owner.CustomProperties.TryGetValue("vivo", out var v) &&
            v is bool b && !b)
            return false; // si ya morio, no sigue vivo 

        return true;
    }

    private void BuscarJugadorVivo()
    {
        Transform masCercano = null;
        float menorDistancia = Mathf.Infinity;

        foreach (var pv in FindObjectsOfType<PhotonView>())
        {
            if (!pv.CompareTag("Player")) continue;
            if (!pv.gameObject.activeInHierarchy) continue;

            // Verificar que este vivo segun Photon
            if (pv.Owner != null &&
                pv.Owner.CustomProperties.TryGetValue("vivo", out var v) &&
                v is bool b && !b) continue; // este ya murio, saltarlo

            float dist = Vector3.Distance(transform.position, pv.transform.position);
            if (dist < menorDistancia)
            {
                menorDistancia = dist;
                masCercano = pv.transform;
            }
        }

        objetivo = masCercano;
    }
}