using UnityEngine;
using Mirror;
using System.Collections.Generic;
using UnityEditor;

/*
	Documentation: https://mirror-networking.com/docs/Guides/NetworkBehaviour.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class NetworkConnectCheck : NetworkBehaviour
{
    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer() 
    {
        if (isServer)
            DebugServerState(" Start Server ");
        if (isClient)
            Debug.Log("?? Server함수에서 클라 접근 - StartServer");
    }

    /// <summary>
    /// Invoked on the server when the object is unspawned
    /// <para>Useful for saving object data in persistent storage</para>
    /// </summary>
    public override void OnStopServer() 
    {
        if (isServer)
            //DebugServerState(" Stop Server ");
        if (isClient)
            Debug.Log("?? Server함수에서 클라 접근 - StopServer");
    }

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() { Debug.Log("Start Client"); }

    /// <summary>
    /// This is invoked on clients when the server has caused this object to be destroyed.
    /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
    /// </summary>
    public override void OnStopClient() { Debug.Log("Stop Client"); }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer() { }

    /// <summary>
    /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
    /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
    /// <para>When <see cref="NetworkIdentity.AssignClientAuthority">AssignClientAuthority</see> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnection parameter included, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStartAuthority() { }

    /// <summary>
    /// This is invoked on behaviours when authority is removed.
    /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStopAuthority() { }

    [ClientRpc(includeOwner = true)]
    public void DebugServerState(string text)
    {
        Debug.Log(text);
    }
    [Command(requiresAuthority = false)]
    public void DebugClientState(string text)
    {
        Debug.Log(text);

        DebugServerState(text + "\n Server is Ready");
    }

    public void RecieveToManager()
    {
        if (isServer)
        {//클라들에게 전파
            Debug.Log("Call Server ");
            DebugServerState(" Server is Ready ");
        }
        if (isClient)
        {//서버에 들리고 다시 클라들에게 전파
            DebugClientState("Client is Ready");
        }
    }
    public void DebugMessage(string text)
    {
        if (isServer)
        {
            Debug.Log(text + "Send To Self");
            DebugServerState(text +  " | Send To Server");
        }
        if (isClient)
        {
            DebugClientState(text + " | Send To Client");
        }
    }

    #endregion
}


#if UNITY_EDITOR
[CustomEditor(typeof(NetworkConnectCheck))]
public class NetworkConnectCheckEditor : Editor
{
    NetworkConnectCheck onwer;
    private void OnEnable()
    {
        onwer = target as NetworkConnectCheck;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button(" Test "))
        {
            onwer.RecieveToManager();
        }
    }
}
#endif
