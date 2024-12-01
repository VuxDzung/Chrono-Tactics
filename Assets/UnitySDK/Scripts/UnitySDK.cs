using FishNet.Managing;
using FishNet.Transporting.UTP;
using FishNet;
using UnityEngine;

public class UnitySDK 
{
    private Authentication authentication;
    private RelayController relay;
    private LobbyController lobby;

    private NetworkManager _networkManager;

    public Authentication Authentication => authentication;
    public RelayController Relay => relay;
    public LobbyController Lobby => lobby;

    private FishyUnityTransport _fishyTransport;

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

    public UnitySDK(bool useLocalProfile, string profileId)
    {
        authentication = new Authentication(this, useLocalProfile, profileId);
        relay = new RelayController(this);
        lobby = new LobbyController(this);

        if (authentication != null) Debug.Log("Authentication Initialized");

        if (relay != null) Debug.Log("Relay Initialized");

        if (lobby != null) Debug.Log("Lobby Services Initialized");
    }
}
