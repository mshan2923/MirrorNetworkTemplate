using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkDebug
{
    [Command(requiresAuthority = false)]
    public static void CMDLog(string text)
    {
        Debug.Log(text);
        RPCLog(text);
        Debug.Log("Receiver : " + NetworkServer.connections.Count);
    }//NotWork

    [ClientRpc(includeOwner = true)]
    public static void RPCLog(string text)
    {
        Debug.Log(text);
    }
}
