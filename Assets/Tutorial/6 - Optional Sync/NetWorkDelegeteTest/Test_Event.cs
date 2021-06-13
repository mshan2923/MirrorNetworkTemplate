using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEditor;

public class Test_Event : NetworkBehaviour// �̰͵� NetworkBehaviour ���� �Ű� Ŭ������?
{
    public Test_Delegate DelegateTarget;

    private void Start()
    {
        DelegateTarget = new Test_Delegate(gameObject, ReturnMessage, IsVaild, IsVisible);

        if (isServer)
            Debug.Log("Setup : Server is" + isServer);
        else if (isClient)
            Debug.Log("Setup : Client is" + isClient);
        else
            Debug.Log("Setup : Not Active");
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public void SendMessage()
    {
        DelegateTarget.Set(Random.value.ToString());
    }

    public bool IsVaild(GameObject ClientCaller, string Data)
    {
        return true;
    }
    public bool IsVisible(GameObject Target)
    {
        return true;
    }
    public void ReturnMessage(string data)
    {
        Debug.Log("Result : " + data);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Test_Event))]
public class Test_EventEditor : Editor
{
    Test_Event Onwer;
    private void OnEnable()
    {
        Onwer = target as Test_Event;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Send Message"))
        {
            if (Onwer.DelegateTarget == null)
            {
                Onwer.DelegateTarget = new Test_Delegate(Onwer.gameObject, Onwer.ReturnMessage, Onwer.IsVaild, Onwer.IsVisible);
            }
            Onwer.SendMessage();
        }
    }
}
#endif