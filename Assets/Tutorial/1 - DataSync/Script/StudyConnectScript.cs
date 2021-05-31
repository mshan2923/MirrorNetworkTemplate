using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StudyConnectScript : MonoBehaviour
{
    NetworkManager manager;

    // Start is called before the first frame update
    void Start()
    {
        manager = NetworkManager.singleton;//StudyNetworkManager.singleton;
    }

    public void CreateRoom()
    {
        //�漳��
        //
        //
        manager.StartHost();
    }
    public void JoinRoom()
    {
        manager.StartClient();
    }
}
