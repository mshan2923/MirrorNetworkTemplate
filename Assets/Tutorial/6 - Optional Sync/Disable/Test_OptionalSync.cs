using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Mirror;

public class Test_OptionalSync : MonoBehaviour
{
    public OptionalSync optionalSync;
    public string Message = "Test";
    void Start()
    {
        //optionalSync.SetServer<string>(Message);
        StartCoroutine(TesTDelay());
        optionalSync.SetUp(ValidEvent, ConditionEvent, SyncEvent);
    }
    IEnumerator TesTDelay()
    {
        yield return new WaitForSeconds(0.1f);
        if (NetworkClient.active)
            optionalSync.SetServer(optionalSync.Convert(Message));
        else
            StartCoroutine(TesTDelay());
    }

    bool ValidEvent(OptionalSync.OptionalMessage Data)
    {
        return true;
    }
    bool ConditionEvent(int TargetConnID)
    {
        //NetworkServer.connections[TargetConnID].identity
        
        return (Random.value >= 0.5f);
    }
    void SyncEvent(bool Updated)
    {

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Test_OptionalSync))]
public class Test_OptionalSyncEditor : Editor
{
    Test_OptionalSync Onwer;
    private void OnEnable()
    {
        Onwer = target as Test_OptionalSync;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Test"))
        {
            Onwer.optionalSync.Set(Onwer.optionalSync.Convert(Onwer.Message));
        }
    }
}
#endif
