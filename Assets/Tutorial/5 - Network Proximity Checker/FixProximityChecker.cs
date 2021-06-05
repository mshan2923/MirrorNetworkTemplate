using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FixProximityChecker : InterestManagement
{
    //Ŭ���̾�Ʈ����  ���� ������ ������ ��� , ������Ʈ ������
    //      =>Render������Ʈ ��Ȱ��ȭ , SyncVar�� ��� �۵�
    //InterestManagement�� �߻� Ŭ�����̴ϱ� , DistanceInterestManagement �����ؼ� ����

    //���� ������Ʈ�߰� (Ư�� �÷��̾�) => NetworkIdentity.Visible - ���� ���̱�� �ϸ��

    [Tooltip("The maximum range that objects will be visible at.")]
    public int visRange = 10;

    [Tooltip("Rebuild all every 'rebuildInterval' seconds.")]
    public float rebuildInterval = 1;
    double lastRebuildTime;

    public GenericVar Test = new GenericVar(typeof(string), "Testing");

    public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnection newObserver)
    {
        return CalculateInRange(identity, newObserver.identity, visRange);

    }//�������� �۵�x, false�̸� ���ŵǴ°� ���� , true�̸�... �ٸ��� ���µ�?

    public override void OnRebuildObservers(NetworkIdentity identity, HashSet<NetworkConnection> newObservers, bool initialize)
    {
        //Vector3 position = identity.transform.position;

        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            // authenticated and joined world with a player?
            if (conn != null && conn.isAuthenticated && conn.identity != null)
            {
                // check distance
                bool active = CalculateInRange(conn.identity, identity, visRange);
                if (CalculateInRange(conn.identity, identity, visRange))
                {
                    newObservers.Add(conn);
                }
            }
        }
        //Same DistanceInterestManager
    }
    bool CalculateInRange(NetworkIdentity ConnId, NetworkIdentity Id, float Range)
    {
        return (ConnId.transform.position - Id.transform.position).sqrMagnitude < (Range * Range);
    }
    void Update()
    {
        // only on server
        if (!NetworkServer.active) return;

        // rebuild all spawned NetworkIdentity's observers every interval
        if (NetworkTime.time >= lastRebuildTime + rebuildInterval)
        {
            RebuildAll();
            lastRebuildTime = NetworkTime.time;
        }
    }

}
