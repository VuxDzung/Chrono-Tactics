using CoopPackage;
using DevOpsGuy.GUI;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TRPG.Unit;
using Unity.Services.Authentication;
using UnityEngine;

namespace TRPG
{
    /// <summary>
    /// When server start, init this object.
    /// When server stop, delete this object.
    /// </summary>
    
    public enum ChangeSceneAction
    {
        SpawnPlayers,
        Disconnect,
    }


    public class NetworkPlayerManager : CoreNetworkBehaviour
    {
        private const string PLAYER_OBJ_NAME_FORMAT = "Player [{0}]";
        private readonly bool NOT_READY = false;
        //private readonly bool READY = true;

        public static NetworkPlayerManager Instance;

        public static Action<string> OnPlayerReadyCallback;

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private bool addToDefaultScene = true;

        private readonly SyncList<InGamePlayerData> ingamePlayerList = new SyncList<InGamePlayerData>();
        private readonly SyncList<NetworkPlayer> nwPlayerObjectList = new SyncList<NetworkPlayer>(new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));
        private readonly SyncVar<int> currentPlayerIndex = new SyncVar<int>();
        private readonly SyncVar<bool> IsClientConnected = new SyncVar<bool>();

        private NetworkManager _networkManager;

        private PreparePanel _preparePanel;

        public readonly SyncVar<ChangeSceneAction> networkSceneAction = new SyncVar<ChangeSceneAction>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            gameObject.name = "NetworkPlayerManager";
            _networkManager = InstanceFinder.NetworkManager;

            _networkManager.SceneManager.OnClientLoadedStartScenes += OnClientConnected;
            ingamePlayerList.OnChange += OnPlayerListChange;

            NetworkSceneLoader.Singleton.OnClientPresenceChangeEnd += InitPlayerNetworkObjectWhenSceneLoaded;
            NetworkSceneLoader.Singleton.OnLoadSceneCompleteClient += LeaveMatch;

            _preparePanel = UIManager.GetUIStatic<PreparePanel>();
            _preparePanel.OnLeaveMatch += LeaveMatch;
        }

        public void OnClientConnected(NetworkConnection playerConnection, bool asServer)
        {
            if (asServer)
            {
                IsClientConnected.Value = true;
            }
        }

        #region Room

        public async Task<bool> JoinMatch(string playerID, string playerName, string[] playerUnitIDArr)
        {
            while (!IsClientConnected.Value)
            {
                await Task.Delay(500);
            }
            JoinMatch_RPC_Server(playerID, playerName, playerUnitIDArr);

            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        private void JoinMatch_RPC_Server(string playerId, string playerName, string[] playerUnitIDArr)
        {
            if (ContainPlayer(playerId)) return;

            InGamePlayerData data = new InGamePlayerData();
            data.id = playerId;
            data.playerName = playerName;
            data.selectedUnitIDArr = playerUnitIDArr;
            data.status = NOT_READY;

            ingamePlayerList.Add(data);
        }

        public void LeaveMatch()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == SceneConfig.SCENE_MENU)
            {
                LeaveMatch(AuthenticationService.Instance.PlayerId);
            }
        }

        private void LeaveMatch(string playerID)
        {
            NetworkGameManager.Singleton.SDK.Lobby.LeaveLobby((string)Session.GetAttribute("lobbyID"), playerID);
            LeaveMatch_RPC_Server(playerID);
        }

        [ServerRpc(RequireOwnership = false)]
        private void LeaveMatch_RPC_Server(string playerId)
        {
            Debug.Log("LeaveMatch_RPC_Server");
            if (ContainPlayer(playerId))
            {
                if (RemovePlayer(playerId))
                {
                    Debug.Log($"Player Leave: {playerId}");
                }
            }
            else
            {
                Debug.LogError($"PlayerID: {playerId} does not exist");
            }
        }

        /// <summary>
        /// When both players are ready, load players to the Gameplay scene.
        /// When they're in Gameplay scene, initialize Network Player Object for each player.
        /// </summary>
        public void ReadyForBattle(string playerId)
        {
            ReadyForBattle_RPC_Server(playerId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ReadyForBattle_RPC_Server(string playerId)
        {
            InGamePlayerData playerData = GetIngamePlayerData(playerId);
            if (playerData != null)
            {
                int dataIndex = ingamePlayerList.IndexOf(playerData);
                ingamePlayerList[dataIndex].status = !ingamePlayerList[dataIndex].status;
                ingamePlayerList.Dirty(dataIndex);
            }

            bool allPlayersAreReady = true;
            foreach (InGamePlayerData player in ingamePlayerList)
            {
                if (!player.status)
                {
                    allPlayersAreReady = false;
                    break;
                }
            }

            if (allPlayersAreReady)
            {
                networkSceneAction.Value = ChangeSceneAction.SpawnPlayers;
                NetworkSceneLoader.Singleton.LoadScene(SceneConfig.SCENE_MENU, SceneConfig.SCENE_DESERT_MAP);
            }

            ReadyForBattleCallback(playerId, allPlayersAreReady);
        }

        [ObserversRpc]
        private void ReadyForBattleCallback(string playerId, bool allPlayerReady)
        {
            if (OnPlayerReadyCallback != null)
            {
                OnPlayerReadyCallback(playerId);
            }
        }

        private void OnPlayerListChange(SyncListOperation op, int index, InGamePlayerData oldItem, InGamePlayerData newItem, bool asServer)
        {
            switch (op)
            {
                case SyncListOperation.Add:
                    List<InGamePlayerData> _callbackPlayerList = new List<InGamePlayerData>();

                    for (int i = 0; i < ingamePlayerList.Count; i++)
                        _callbackPlayerList.Add(ingamePlayerList[i]);

                    _preparePanel.LoadPlayersStatus(_callbackPlayerList);
                    break;
                case SyncListOperation.RemoveAt:
                    if (ingamePlayerList.Count == 0)
                    {
                        if (asServer)
                        {
                            nwPlayerObjectList.Clear();

                            if (_networkManager.ServerManager.StopConnection(true))
                                Debug.Log("Server is shutdown");
                        }
                        else
                        {
                            _networkManager.ClientManager.StopConnection();
                        }
                    }
                    break;
                case SyncListOperation.Insert:
                    break;
                case SyncListOperation.Set:
                    break;
                case SyncListOperation.Clear:
                    break;
                case SyncListOperation.Complete:
                    break;
            }
        }

        #endregion

        #region Gameplay
        private void InitPlayerNetworkObjectWhenSceneLoaded(ClientPresenceChangeEventArgs args)
        {
            switch(networkSceneAction.Value)
            {
                case ChangeSceneAction.SpawnPlayers:
                    Debug.Log($"InitPlayerNetworkObjectWhenSceneLoaded | NetworkScene: {args.Scene.name} | CurrentScene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
                    SpawnNetworkPlayerObject(args.Connection, NetworkGameManager.Singleton.SDK.Authentication.PlayerId);
                    break;
                case ChangeSceneAction.Disconnect:
                    break;
            }
        }

        private void SpawnNetworkPlayerObject(NetworkConnection player, string playerID)
        {
            // Check if we gave a player prefab we want to spawn in the game:
            if (playerPrefab == null)
            {
                Debug.LogWarning($"Player prefab is empty and cannot be spawned for connection {player.ClientId}.");
                return;
            }
            Debug.Log("PlayerPrefab found");

            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;

            NetworkObject nob = _networkManager.GetPooledInstantiated(playerPrefab, position, rotation, true);

            _networkManager.ServerManager.Spawn(nob, player);
            nob.gameObject.name = string.Format(PLAYER_OBJ_NAME_FORMAT, nwPlayerObjectList.Count);

            NetworkPlayer networkPlayerObject = nob.GetComponent<NetworkPlayer>();

            RegisterPlayerObject(networkPlayerObject);

            //If there are no global scenes 
            if (addToDefaultScene)
                _networkManager.SceneManager.AddOwnerToDefaultScene(nob);

            string[] _playerUnitIDArr = GetIngamePlayerData(playerID)?.selectedUnitIDArr;

            networkPlayerObject.Initialized(SceneSpawnAreaManager.S.GetArea(), player, _playerUnitIDArr);
            SceneSpawnAreaManager.S.IncreaseAreaIndex();
            StartFirstPlayerTurn(networkPlayerObject);
        }

        [Server]
        public void StartFirstPlayerTurn(NetworkPlayer player)
        {
            int playerIndex = nwPlayerObjectList.IndexOf(player);
            if (playerIndex == 0)
            {
                nwPlayerObjectList[playerIndex].StartOwnerTurn();
            }
            else
            {
                nwPlayerObjectList[playerIndex].StopOwnerTurn();
            }
        }

        [Server]
        public void StartFirstPlayerTurn()
        {
            currentPlayerIndex.Value = 0;
            nwPlayerObjectList[0].StartOwnerTurn();
        }

        [Server]
        public void ChangeNextPlayerTurn()
        {
            nwPlayerObjectList[currentPlayerIndex.Value].StopOwnerTurn();
            currentPlayerIndex.Value++;
            if (currentPlayerIndex.Value >= nwPlayerObjectList.Count) // When all players has make their move, the final turn shall belong to the AI.
            {
                currentPlayerIndex.Value = 0;
            }
            nwPlayerObjectList[currentPlayerIndex.Value].StartOwnerTurn();
        }

        [Server]
        public bool RegisterPlayerObject(NetworkPlayer player)
        {
            if (nwPlayerObjectList.Contains(player))
                return false;
            nwPlayerObjectList.Add(player);
            return true;
        }

        [Server]
        public bool UnregisterPlayerObject(NetworkPlayer player)
        {
            return nwPlayerObjectList.Remove(player);
        }

        public NetworkPlayer GetPlayer(NetworkConnection owner)
        {
            return nwPlayerObjectList.FirstOrDefault(player => player.OwnerId.Equals(owner.ClientId));
        }

        public NetworkPlayer GetTheOtherPlayer(NetworkPlayer networkPlayerObject)
        {
            NetworkPlayer otherPlayer = nwPlayerObjectList.FirstOrDefault(player => player.OwnerId != networkPlayerObject.OwnerId);
            return otherPlayer;
        }

        public List<UnitController> GetAllPlayersUnits()
        {
            List<UnitController> unitList = new List<UnitController>();
            foreach (NetworkPlayer player in nwPlayerObjectList)
            {
                player.ActiveUnitList.ForEach(activeUnit => unitList.Add(activeUnit));
            }
            return unitList;
        }
        #endregion

        private InGamePlayerData GetIngamePlayerData(string playerId)
        {
            foreach (InGamePlayerData data in ingamePlayerList)
            {
                if (data.id.Equals(playerId))
                {
                    return data;
                }
            }
            return null;
        }

        private bool ContainPlayer(string playerId)
        {
            foreach (InGamePlayerData data in ingamePlayerList)
            {
                if (data.id.Equals(playerId))
                {
                    return true;
                }
            }
            return false;
        }

        [Server]
        private bool RemovePlayer(string playerID)
        {
            InGamePlayerData data = GetIngamePlayerData(playerID);
            if (data != null)
            {
                return ingamePlayerList.Remove(data);
            }
            else
            {
                Debug.LogWarning($"Player {playerID} does not exist");
                return false;
            }
        }
    }
}