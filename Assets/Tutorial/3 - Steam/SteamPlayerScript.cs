using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class SteamPlayerScript : NetworkBehaviour
{
    [SyncVar(hook = nameof(HandleSteamIdUpdated))]
    ulong steamId;

    public void SetSteamId(ulong SteamId)
    {
        this.steamId = SteamId;
    }
    private void HandleSteamIdUpdated(ulong oldSteamId, ulong newSteamId)
    {
        var cSteamId = new CSteamID(newSteamId);

        Debug.Log(" + " + SteamFriends.GetFriendPersonaName(cSteamId));
    }
}
