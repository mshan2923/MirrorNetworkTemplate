using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FixProximityChecker : InterestManagement
{
    //클라이언트에서  범위 밖으로 나가진 경우 , 오브젝트 삭제됨
    //      =>Render컴포넌트 비활성화 , SyncVar는 계속 작동
    //InterestManagement는 추상 클래스이니깐 , DistanceInterestManagement 참고해서 개선

    //예외 오브젝트추가 (특히 플레이어) => NetworkIdentity.Visible - 강제 보이기로 하면됨

    [Tooltip("The maximum range that objects will be visible at.")]
    public int visRange = 10;

    [Tooltip("Rebuild all every 'rebuildInterval' seconds.")]
    public float rebuildInterval = 1;
    double lastRebuildTime;

    public GenericVar Test = new GenericVar(typeof(string), "Testing");

    public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnection newObserver)
    {
        return CalculateInRange(identity, newObserver.identity, visRange);

    }//서버에선 작동x, false이면 제거되는거 같음 , true이면... 다를게 없는데?

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
