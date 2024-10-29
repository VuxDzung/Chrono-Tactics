using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TRPG.Unit
{
    public class TRPGGameManager : CoreNetworkBehaviour
    {
        private const string PLAYER_OBJ_NAME_FORMAT = "Player [{0}]";
        public static TRPGGameManager Instance;

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private bool addToDefaultScene = true;

        private readonly SyncList<NetworkPlayer> nwPlayerList = new SyncList<NetworkPlayer>(new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));
        private readonly SyncVar<int> currentPlayerIndex = new SyncVar<int>();
        private NetworkManager networkManager;

        private void Start()
        {
            Instance = this;
            Initialize();
            networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
        }

        private void Initialize()
        {
            // Get the NetworkManager instance
            networkManager = InstanceFinder.NetworkManager;
            if (networkManager == null)
            {
                Debug.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
                return;
            }
            // Add this event so we can spawn the player
            //InvokeRepeating("CheckNetworkConnection", 2, 7);
        }

        private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
        {
            Debug.Log("OnClientLoadedStartScenes called");
            if (!asServer)
                return;

            // So the scene has loaded and this call is made on the server.
            Debug.Log("OnClientLoadedStartScenes running asServer");

            // Check if we gave a player prefab we want to spawn in the game:
            if (playerPrefab == null)
            {
                Debug.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
                return;
            }
            Debug.Log("PlayerPrefab found");

            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;

            NetworkObject nob = networkManager.GetPooledInstantiated(playerPrefab, position, rotation, true);

            networkManager.ServerManager.Spawn(nob, conn);
            nob.gameObject.name = string.Format(PLAYER_OBJ_NAME_FORMAT, nwPlayerList.Count);

            NetworkPlayer player = nob.GetComponent<NetworkPlayer>();

            RegisterPlayer(player);

            //If there are no global scenes 
            if (addToDefaultScene)
                networkManager.SceneManager.AddOwnerToDefaultScene(nob);

            player.Initialized(SceneSpawnAreaManager.S.GetArea(), conn);
            SceneSpawnAreaManager.S.IncreaseAreaIndex();

            StartFirstPlayerTurn(player);
        }

        [Server]
        private void StartFirstPlayerTurn(NetworkPlayer player)
        {
            int playerIndex = nwPlayerList.IndexOf(player); 
            if (playerIndex == 0)
            {
                nwPlayerList[playerIndex].StartOwnerTurn();
            }
            else
            {
                nwPlayerList[playerIndex].StopOwnerTurn();
            }
        }

        [Server]
        public void ChangeNextPlayerTurn()
        {
            nwPlayerList[currentPlayerIndex.Value].StopOwnerTurn();
            currentPlayerIndex.Value++;
            if (currentPlayerIndex.Value >= nwPlayerList.Count)
                currentPlayerIndex.Value = 0;

            nwPlayerList[currentPlayerIndex.Value].StartOwnerTurn();
        }

        [Server]
        public bool RegisterPlayer(NetworkPlayer player)
        {
            if (nwPlayerList.Contains(player))
                return false;
            nwPlayerList.Add(player);
            return true;
        }

        [Server]
        public bool UnregisterPlayer(NetworkPlayer player)
        {
            return nwPlayerList.Remove(player);
        }

        public NetworkPlayer GetPlayer(NetworkConnection owner)
        {
            return nwPlayerList.FirstOrDefault(player => player.OwnerId.Equals(owner.ClientId));
        }
    }
}