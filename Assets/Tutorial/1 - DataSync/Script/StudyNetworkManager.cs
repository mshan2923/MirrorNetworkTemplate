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

#if UNITY_SERVER // 서버빌드에서만 추가
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

    //[Server]//서버에서만 실행 
    [ServerCallback]//서버에서만 실행 + 클라이언트 접근시 에러X
    public void SpawnTest()
    {
        // Registered Spawnable Prefabs은 클라이언트가 특정한 오브젝트를 생성가능한 목록

        SpawnTestObj = GameObject.Instantiate(spawnPrefabs[0]);
        //SpawnTestObj.transform.position = FindObjectOfType<Camera>().transform.position;
        var Lcamera = FindObjectOfType<Camera>().transform;
        SpawnTestObj.transform.position = Lcamera.position + Lcamera.forward * 100;

        NetworkServer.Spawn(SpawnTestObj);
    }    //서버에서만 생성후 호출하면 클라이언트도 자동생성 (NetworkServer.Spawn) // 서버에서 스폰되서 소유권은 서버 , 클라이언트가 변경이 동기화X

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
