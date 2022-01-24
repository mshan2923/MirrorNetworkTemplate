using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StudyNetworkManager : NetworkRoomManager
{
    bool showStartButton;
    GameObject SpawnTestObj;
    [Scene] public string SecenceSlect;


    public override void OnRoomServerPlayersReady()
    {
        // calling the base method calls ServerChangeScene as soon as all players are in Ready state.

#if UNITY_SERVER // �������忡���� �߰�
            base.OnRoomServerPlayersReady();
#else
        showStartButton = true; //#if !UNITY_SERVER
#endif
    }//If Server Show StartButton

    public override void OnGUI()
    {
        base.OnGUI();

        if (allPlayersReady && showStartButton && GUI.Button(new Rect(150, 300, 120, 20), "START GAME"))
        {
            // set to false to hide it in the game scene
            showStartButton = false;

            ServerChangeScene(GameplayScene);
        }
    }//GameStartButton

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        if (GameplayScene == sceneName)
        {
            SpawnTest();
        }
    }

    //[Server]//���������� ���� 
    [ServerCallback]//���������� ���� + Ŭ���̾�Ʈ ���ٽ� ����X
    public void SpawnTest()
    {
        // Registered Spawnable Prefabs�� Ŭ���̾�Ʈ�� Ư���� ������Ʈ�� ���������� ���

        SpawnTestObj = GameObject.Instantiate(spawnPrefabs[0]);
        //SpawnTestObj.transform.position = FindObjectOfType<Camera>().transform.position;
        var Lcamera = FindObjectOfType<Camera>().transform;
        SpawnTestObj.transform.position = Lcamera.position + Lcamera.forward * 100;

        NetworkServer.Spawn(SpawnTestObj);
    }    //���������� ������ ȣ���ϸ� Ŭ���̾�Ʈ�� �ڵ����� (NetworkServer.Spawn) // �������� �����Ǽ� �������� ���� , Ŭ���̾�Ʈ�� ������ ����ȭX

    #region RegisterHandler / Send
    public override void OnRoomClientConnect()
    {
        base.OnRoomClientConnect();

        StartCoroutine(delay());
    }
    IEnumerator delay()
    {
        yield return new WaitForSeconds(1f);

        SendNotification();
    }

    [ContextMenu("Send Notification")]
    private void SendNotification()
    {
        NetworkServer.SendToAll(new Notification { data = "Send Message" });
        //Receive to StudyRoomPlayer
    }
    #endregion
}
