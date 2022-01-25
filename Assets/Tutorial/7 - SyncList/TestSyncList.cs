using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEditor;

public class TestSyncList : NetworkBehaviour
{
    public TextMesh textMesh;

    //SyncSet
    //SyncDictionary
    SyncList<string> list = new SyncList<string>();
    //https://github.com/vis2k/Mirror/blob/master/Assets/Mirror/Runtime/SyncList.cs/#L27

    void Start()
    {
        list.Callback += listCallBack;

        //도중에 클라 들어왔을때 이미 있는 데이터를 TextMesh에 띄우는건 귀찮아서...
        //그래서 아래있는 코드(list.Add)는 서버가 시작될때 1번 실행

        list.OnDirty += new System.Action(OnDirty);

        if ( isServer)//isServerOnly - 서버도 실행 안되었음
            list.Add("Test " + list.Count);

    }

    private void Update()
    {
        if (list.IsRecording())//always true
        {

        }
    }

    void OnDirty()
    {
        if (isServer)
        {
            DebugLogToClient("Added" + list.Count);
        }
    }

    public void listCallBack(SyncList<string>.Operation op, int itemIndex, string oldItem, string newItem)
    {
        switch (op)
        {
            case SyncList<string>.Operation.OP_ADD:
                Debug.Log("Added : " + newItem + " : " + itemIndex);
                break;
            case SyncList<string>.Operation.OP_CLEAR:
                break;
            case SyncList<string>.Operation.OP_INSERT:
                break;
            case SyncList<string>.Operation.OP_REMOVEAT:
                break;
            case SyncList<string>.Operation.OP_SET:
                break;
        }
    }

    [ClientRpc]
    void DebugLogToClient(string data)
    {
        Debug.Log("Client Recieve : " + data);
        textMesh.text += "\n" + data;
    }

    [Command(requiresAuthority = false)]
    public void AddList()
    {
        list.Add("Test " + list.Count);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TestSyncList))]//커스텀 에디터 쓰니깐 SyncSettings 사라짐
public class TestSyncListEditor : Editor
{
    TestSyncList Onwer;
    private void OnEnable()
    {
        Onwer = target as TestSyncList;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Add"))
        {
            Onwer.AddList();
        }
        EditorGUILayout.LabelField("커스텀 에디터 쓰니깐 SyncSettings 사라짐");
    }
}
#endif