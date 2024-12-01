using FishNet.Object;
using TRPG;
using UnityEngine;

public class NetworkStarter : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerManagerObject;
    [SerializeField] private NetworkObject networkSceneLoader;

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("NetworkStarter");
        if (NetworkPlayerManager.Instance == null) InitializeNetworkManager();

        if (NetworkSceneLoader.Singleton == null) InitializeSceneLoader();
    }

    [Server]
    private void InitializeNetworkManager()
    {
        playerManagerObject = Instantiate(playerManagerObject);
        ServerManager.Spawn(playerManagerObject, null);
    }

    [Server]
    private void InitializeSceneLoader()
    {
        NetworkObject sceneLoader = Instantiate(networkSceneLoader);
        ServerManager.Spawn(sceneLoader, null);
    }
}
