using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TargetRpcSample : NetworkBehaviour
{
    [SerializeField]
    float Vaule = -1;

    public TextMesh TextComponent;
    [Range(min: 0.1f, max: 10)]
    public float Delay = 1f;
    public float Distance = 2f;

    void Start()
    {
        
    }
    public override void OnStartServer()
    {
        base.OnStartServer();

        StartCoroutine(Loop());
    }

    IEnumerator Loop()
    {
        if (TextComponent != null)
        {
            Vaule = Random.Range(0, 10);
            TextComponent.text = Vaule.ToString("0.##");
        }

        NetworkConnectionToClient[] clients = new NetworkConnectionToClient[NetworkServer.connections.Count];
        NetworkServer.connections.Values.CopyTo(clients, 0);

        for(int i = 0; i < clients.Length; i++)
        {
            float sqrLength = (clients[i].identity.gameObject.transform.position - gameObject.transform.position).sqrMagnitude;

            if (sqrLength <= Distance)
            {
                SyncOptional(clients[i], Vaule);
            }
        }

        yield return new WaitForSeconds(Delay);
        StartCoroutine(Loop());
    }

    [TargetRpc]
    public void SyncOptional(NetworkConnection Target, float vaule)//타겟이 없으면 소유주에게
    {
        if (TextComponent != null)
        {
            TextComponent.text = vaule.ToString("0.##");
        }
    }
}
