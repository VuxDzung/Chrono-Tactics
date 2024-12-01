using FishNet;
using FishNet.Managing;
using FishNet.Transporting.UTP;
using Utils;
using UnityEngine;
using CoopPackage;
using DevOpsGuy.GUI;
using TRPG;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using FishNet.Managing.Server;
using FishNet.Object;

public class NetworkGameManager : M_Singleton<NetworkGameManager>
{
    [SerializeField] private NetworkObject playerManagerObject;
    [SerializeField] private NetworkObject networkSceneLoader;


    [Tooltip("This is used in local testing only")]
    [SerializeField] private bool useLocalProfle = true;
    [SerializeField] private LocalProfileSettings localProfile;

    NetworkManager _networkManager;

    private FishyUnityTransport _fishyTransport;

    private UnitySDK sdk;

    public UnitySDK SDK => sdk;

    public NetworkManager NetworkManager
    {
        get
        {
            if (_networkManager == null)
                _networkManager = InstanceFinder.NetworkManager;
            return _networkManager;
        }
    }

    public FishyUnityTransport FishyTransport
    {
        get
        {
            if (_fishyTransport == null)
                _fishyTransport = NetworkManager.TransportManager.GetTransport<FishyUnityTransport>();
            return _fishyTransport;
        }
    }


    private void Start()
    {
        sdk = new UnitySDK(useLocalProfle, localProfile.localProfile);

        sdk.Lobby.OnLobbyCreatedWithID += sdk.Relay.CreateGameRelay;
        sdk.Lobby.OnJoinedLobbyWithLobbyData += JoinRelayWithCode;

        sdk.Relay.OnRelayCreated += JoinMatch;
        sdk.Relay.OnJoinedRelay += JoinMatch;

        sdk.Relay.OnServerStarted += InitServerComponents;
    }

    public void InitServerComponents()
    {
        if (NetworkPlayerManager.Instance == null) InitializeNetworkManager();

        if (NetworkSceneLoader.Singleton == null) InitializeSceneLoader();
    }

    private void InitializeNetworkManager()
    {
        NetworkObject _playerManagerObject = Instantiate(playerManagerObject);
        InstanceFinder.ServerManager.Spawn(_playerManagerObject, null);
    }

    private void InitializeSceneLoader()
    {
        NetworkObject sceneLoader = Instantiate(networkSceneLoader);
        InstanceFinder.ServerManager.Spawn(sceneLoader, null);
    }

    public void JoinRelayWithCode(Lobby joinedLobby)
    {
        if (joinedLobby.Data.TryGetValue(RelayController.RELAY_JOIN_CODE, out var joinCodeData))
        {
            string joinCode = joinCodeData.Value;
            Debug.Log($"Joining Relay with JoinCode: {joinCode}");

            // Join Relay using the extracted join code
            sdk.Relay.JoinGameRelay(joinCode);
        }
        else
        {
            Debug.LogError("Join code not found in lobby data.");
        }
    }

    /// <summary>
    /// Add player to network list in NetworkPlayerManager.
    /// Go to the prepare panel.
    /// </summary>
    public async void JoinMatch()
    {
        // In production, theses information shall be contained in real-time database. But not I keep them in Local (PlayerPrefs) [Dung].
        UserData data = SaveLoadUtil.Load<UserData>(SaveKeys.UserData);
        string playerId = sdk.Authentication.PlayerId;
        string playerName = data.UserName;
        string[] playerUnitArr = data.InTeamUnitIds;

        while (NetworkPlayerManager.Instance == null)
        {
            Debug.Log("Wait the NetworkPlayerManager init");
            await Task.Delay(500);
        }

        await NetworkPlayerManager.Instance.JoinMatch(playerId, playerName, playerUnitArr);

        GoToPreparePanel();
    }

    public void GoToPreparePanel()
    {
        UIManager.HideAll();
        UIManager.ShowUIStatic<PreparePanel>();
    }
}
