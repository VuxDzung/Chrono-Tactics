using DevOpsGuy.GUI;
using System.Collections;
using System.Collections.Generic;
using TRPG;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using System;

public class RelayController
{
    public static readonly string RELAY_JOIN_CODE = "joinCode";
    public Action OnRelayCreated;
    public Action OnJoinedRelay;
    public Action OnServerStarted;

    private UnitySDK context;

    public RelayController(UnitySDK context)
    {
        this.context = context;
    }


    // Create Relay game and update the lobby with Relay info
    public virtual async void CreateGameRelay(string lobbyId, int maxPlayer)
    {
        try
        {
            Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(maxPlayer);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);
            Debug.Log($"Server\nIP={hostAllocation.RelayServer.IpV4}\nPort={hostAllocation.RelayServer.Port}");

            context.FishyTransport.SetRelayServerData(new RelayServerData(hostAllocation, "dtls"));
            Session.SetAttribute("lobbyID", lobbyId);
            if (context.NetworkManager.ServerManager.StartConnection()) // Server is successfully started.
            {
                OnServerStarted?.Invoke();
                if (context.NetworkManager.ClientManager.StartConnection())
                {
                    Debug.Log($"Relay Server started. JoinCode: {joinCode}");

                    // Update the lobby with the Relay join code
                    await Lobbies.Instance.UpdateLobbyAsync(lobbyId, new UpdateLobbyOptions
                    {
                        Data = new Dictionary<string, DataObject>
                            {
                                { RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                            }
                    });

                    Debug.Log("Relay Created");
                    OnRelayCreated?.Invoke();
                }
            }
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Error setting up Relay: {e.Message}");
        }
    }

    public virtual async void JoinGameRelay(string joinCode)
    {
        try
        {
            // Join allocation
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
            // Configure transport
            context.FishyTransport.SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            if (!string.IsNullOrEmpty(joinCode) && joinAllocation != null)
            {
                if (context.NetworkManager.ClientManager.StartConnection())
                {
                    OnJoinedRelay?.Invoke();
                }
            }
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Error joining Relay: {e.Message}");
        }
    }

}
