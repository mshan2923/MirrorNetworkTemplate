using UnityEngine;
using Mirror;
using System.Collections.Generic;
using Steamworks;

/*
	Documentation: https://mirror-networking.com/docs/Guides/NetworkBehaviour.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class SteamConnecter : NetworkBehaviour
{
    NetworkManager networkManager;

    public override void OnStartServer()
    {
        base.OnStartServer();
        networkManager = GetComponent<NetworkManager>();

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);//델리게이트임


        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, networkManager.maxConnections);
        //SteamAPI.RunCallbacks();
    }
    /*
    public override void OnStopServer() { }
    public override void OnStartClient() { }
    public override void OnStopClient() { }
    public override void OnStartLocalPlayer() { }
    public override void OnStartAuthority() { }
    public override void OnStopAuthority() { }
    */

    #region SteamLoddy

    [Header("Steam Lobby")]
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    public static CSteamID LobbyId { get; private set; }

    [ClientRpc]
    void RPCGetLobbyId(CSteamID Id)
    {
        LobbyId = Id;
        Debug.Log("Lobby ID : " + LobbyId + " | Client : " + isClient);

        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        SteamMatchmaking.RequestLobbyData(LobbyId);
        SteamMatchmaking.JoinLobby(LobbyId);
    }//서버에서 OnLobbyCreated으로 LobbyID 전달

    #region CallBack

    void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult == EResult.k_EResultOK)
        {
            Debug.Log("Lobby Created");


            LobbyId = (CSteamID)callback.m_ulSteamIDLobby;
            RPCGetLobbyId(LobbyId);

            SteamMatchmaking.SetLobbyData(LobbyId, "name", SteamFriends.GetPersonaName());
        }
        else
            Debug.Log("Not Lobby Create");
    }
    void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        string user = SteamFriends.GetFriendPersonaName(callback.m_steamIDFriend);
        Debug.Log("Already Join : " + user);
    }//이미 있는경우
    void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (callback.m_EChatRoomEnterResponse == 1)
            Debug.Log("Lobby Joined | " + SteamFriends.GetPersonaName());
        else
            Debug.Log("Failed to join lobby");

        int numPlayers = SteamMatchmaking.GetNumLobbyMembers(LobbyId);
        CSteamID steamID = SteamMatchmaking.GetLobbyMemberByIndex(LobbyId, numPlayers - 1);
        Debug.Log("Lobby Size : " + numPlayers + " , Last User : " + SteamFriends.GetFriendPersonaName(steamID));
    }

    #endregion

    //참고 https://github.com/famishedmammal/Steamworks.NET-matchmaking-lobbies-example/blob/master/lobbyserverTEST.cs
    //참고2 - SteamWork Documentation https://partner.steamgames.com/doc/api/ISteamMatchmaking#LobbyChatUpdate_t
    #endregion
}
