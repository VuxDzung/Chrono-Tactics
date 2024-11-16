using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using DevOpsGuy;
using FishNet;
using DevOpsGuy.GUI;
using FishNet.Transporting.UTP;
using Unity.Networking.Transport.Relay;
using Utils;
using System;
using System.Threading.Tasks;

namespace CoopPackage
{
    public class CoopController : M_Singleton<CoopController>
    {
        public static Action OnPlayerJoined;

        [Header("Development-LOCAL")]
        [SerializeField] private bool switchLocalProfile = false;
        [SerializeField] private LocalProfileSettings localProfile;
        private const bool SERVER = true;
        private const bool CLIENT = false;

        private NetworkManager networkManager;
        private FishyUnityTransport fishyUnityTransport;

        private Lobby currentLobby;

        private InRoomPanel roomPanel;

        private async void Start()
        {
            await UnityServices.InitializeAsync();
            if (switchLocalProfile)
            {
                AuthenticationService.Instance.SwitchProfile(localProfile.localProfile);
            }
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log($"Player Signed in: {AuthenticationService.Instance.PlayerId}");

            // Subscribe to UI events (e.g. button click)
            CreateGamePanel.OnCreateRoom += CreateLobby;
            JoinGamePanel.OnJoinRoom += JoinLobbyRelay;
            networkManager = InstanceFinder.NetworkManager;
            fishyUnityTransport = networkManager.TransportManager.GetTransport<FishyUnityTransport>();

            roomPanel = UIManager.GetUI<InRoomPanel>();
        }

        private void OnDestroy()
        {
            CreateGamePanel.OnCreateRoom -= CreateLobby;
            JoinGamePanel.OnJoinRoom -= JoinLobbyRelay;
        }

        // Create Lobby and set up Relay for the host
        public virtual async void CreateLobby(string lobbyName, int maxPlayer)
        {
            try
            {
                // Create Lobby with the specified name and player count
                CreateLobbyOptions options = new CreateLobbyOptions
                {
                    IsPrivate = false // Set to true if you want private lobbies
                };

                currentLobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayer, options);
                Debug.Log($"Lobby created: {currentLobby.LobbyCode}");

                // Call Relay Allocation to set up the game
                CreateGameRelay(maxPlayer);

                // Optionally update lobby with join code or other details
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Error creating lobby: {e.Message}");
            }
        }

        // Create Relay game and update the lobby with Relay info
        public virtual async void CreateGameRelay(int maxPlayer)
        {
            try
            {
                // Create Relay allocation for the host
                Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(maxPlayer);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);
                Debug.Log($"Server\nIP={hostAllocation.RelayServer.IpV4}\nPort={hostAllocation.RelayServer.Port}");

                fishyUnityTransport.SetRelayServerData(new RelayServerData(hostAllocation, "dtls"));

                // Start host
                if (networkManager.ServerManager.StartConnection()) // Server is successfully started.
                {
                    networkManager.ClientManager.StartConnection();

                    Debug.Log($"Relay Server started. JoinCode: {joinCode}");

                    SessionManager.SetAttribute("worldId", currentLobby.Id);
                    SessionManager.SetAttribute("worldName", currentLobby.Name);
                    SessionManager.SetAttribute("joinCode", currentLobby.LobbyCode);

                    // Update the lobby with the Relay join code
                    await Lobbies.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
                    {
                        Data = new Dictionary<string, DataObject>
                            {
                                { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                            }
                    });

                    UIManager.ShowUI<InRoomPanel>();
                }

            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"Error setting up Relay: {e.Message}");
            }
        }

        // Join Lobby and Relay as a client
        public virtual async void JoinLobbyRelay(string lobblyCode)
        {
            try
            {
                // Join the lobby using the lobbyId
                currentLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobblyCode);
                Debug.Log($"Joined Lobby: {currentLobby.Id}");

                // Extract Relay join code from lobby data
                if (currentLobby.Data.TryGetValue("joinCode", out var joinCodeData))
                {
                    string joinCode = joinCodeData.Value;
                    Debug.Log($"Joining Relay with JoinCode: {joinCode}");

                    // Join Relay using the extracted join code
                    JoinGameRelay(joinCode);
                }
                else
                {
                    Debug.LogError("Join code not found in lobby data.");
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Error joining lobby: {e.Message}");
            }
        }

        // Join Relay server using the join code
        public virtual async void JoinGameRelay(string joinCode)
        {
            try
            {
                // Join allocation
                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
                // Configure transport
                var unityTransport = networkManager.TransportManager.GetTransport<FishyUnityTransport>();
                unityTransport.SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

                if (!string.IsNullOrEmpty(joinCode))
                {
                    OnPlayerJoined?.Invoke();
                    //SceneLoader.Singleton.LoadScene("Demo", () => {
                    //    // Start client
                    //    if (networkManager.ClientManager.StartConnection())
                    //    {
                    //        SessionManager.SetAttribute("worldId", currentLobby.Id);
                    //        SessionManager.SetAttribute("worldName", currentLobby.Name);
                    //        SessionManager.SetAttribute("joinCode", currentLobby.LobbyCode);

                    //        //networkManager.ClientManager.StartConnection();
                    //        Debug.Log($"Connected to Relay server with JoinCode: {joinCode}");
                    //    }
                    //});
                }
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"Error joining Relay: {e.Message}");
            }
        }

        public async Task<List<Player>> GetPlayers()
        {
            await Task.Delay(3000);
            if (currentLobby == null)
            {
                Debug.LogError("Current Lobby is NULL");
                return null;
            }
            Debug.Log($"Lobby: Name={currentLobby.Name}, MaxPlayers={currentLobby.MaxPlayers}, PlayerCount={currentLobby.Players.Count}");
            return currentLobby.Players;
        }

        private async Task PollLobby()
        {
            while (true)
            {
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
                Debug.Log($"Current lobby player count: {lobby.Players.Count}");
                // Handle lobby players update here

                await Task.Delay(5000); // Poll every 5 seconds
            }
        }
    }
}