using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public struct Notification : NetworkMessage
{
    public string data;
}
public class StudyRoomPlayer : NetworkRoomPlayer
{
    private void Start()
    {
        base.Start();

        //if (!NetworkClient.active) { return; }

        NetworkClient.RegisterHandler<Notification>(OnNotification);
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    void OnNotification(Notification msg)
    {
        Debug.Log(msg.data);
        var textCom = FindObjectOfType<UnityEngine.UI.Text>();
        textCom.text = msg.data;
    }//Send from StudyNetManager . SendNotification
}
