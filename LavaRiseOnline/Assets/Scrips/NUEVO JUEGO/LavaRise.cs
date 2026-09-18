using Photon.Pun;
using UnityEngine;

public class LavaRise : MonoBehaviour
{
    [SerializeField] private float velocidadInicial = 0.2f;
    [SerializeField] private float aceleracion = 0.05f;
    [SerializeField] private float velocidadMaxima = 10f;

    private float velocidadActual;
    public bool IsRising { get; private set; }

    public void StartRising()
    {
        velocidadActual = velocidadInicial;
        IsRising = true;
    }

    void Update()
    {
        if (!IsRising) return;

        velocidadActual = Mathf.Min(velocidadActual + aceleracion * Time.deltaTime, velocidadMaxima);
        transform.Translate(Vector3.up * velocidadActual * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var pv = other.GetComponent<PhotonView>();
        if (pv == null || !pv.IsMine) return;
        GameManager.Instance.YoMori();
    }
}