using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class SteamNetworkManager : NetworkManager
{
    #region SteamLobby_Disable
    /*
    [Header("Steam Lobby")]
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    public const string HostAddressKey = "HostAddress";

    //private NetworkManager networkManager;

    public static CSteamID LobbyId { get; private set; }

        public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        CSteamID steamID = SteamMatchmaking.GetLobbyMemberByIndex(LobbyId, numPlayers - 1);

        var playerInfoDisplay = conn.identity.GetComponent<SteamPlayerScript>();
        //SteamPlayer에게 steamID.m_SteamID 전달 >> SyncVar으로 HandleSteamIdUpdated >> 데이터출력
        playerInfoDisplay.SetSteamId(steamID.m_SteamID);
    }

    private void Start()
    {
        base.Start();

        //networkManager = GetComponent<NetworkManager>();

        if (!SteamManager.Initialized) { return; }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);


        HostLobby();
    }

    [ContextMenu("Host")]
    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, maxConnections);
    }
    void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            return;
        }

        LobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        //networkManager.StartHost();
        StartHost();

        SteamMatchmaking.SetLobbyData
            (
            LobbyId,
            HostAddressKey,
            SteamUser.GetSteamID().ToString());
    }
    void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        Debug.Log(" Join : " + SteamFriends.GetFriendPersonaName(callback.m_steamIDFriend));
    }
    void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) { return; }

        string hostAdress = SteamMatchmaking.GetLobbyData
            (
            new CSteamID(callback.m_ulSteamIDLobby),
            HostAddressKey);

        networkAddress = hostAdress;
        StartClient();
    }
    */
    #endregion

    new private void Start()
    {
        base.Start();

        StartHost();//이게 없으니 Mirror 작동X
        //SteamAPI work After Host();
    }

}
