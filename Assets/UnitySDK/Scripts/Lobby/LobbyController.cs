using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class LobbyController
{
    public Action<string, int> OnLobbyCreatedWithID;
    public Action OnLobbyCreated;
    public Action<Lobby> OnJoinedLobbyWithLobbyData;

    public Action OnJoinedLobby;

    private UnitySDK context;

    private Lobby currentLobby;

    public string CurrentLobbyId => currentLobby.Id;

    public LobbyController(UnitySDK context)
    {
        this.context = context;
    }

    // Create Lobby and set up Relay for the host
    public virtual async void CreateLobby(string lobbyName, int maxPlayer)
    {
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false
            };

            currentLobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayer, options);
            Debug.Log($"Lobby created: {currentLobby.LobbyCode}");

            // Call Relay Allocation to set up the game
            OnLobbyCreated?.Invoke();
            OnLobbyCreatedWithID?.Invoke(currentLobby.Id, maxPlayer);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error creating lobby: {e.Message}");
        }
    }

    public virtual async void JoinLobbyById(string lobbyId)
    {
        try
        {
            // Join the lobby using the lobbyId
            currentLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId);
            Debug.Log($"Joined Lobby: {currentLobby.Id}");
            Session.SetAttribute("lobbyID", lobbyId);
            OnJoinedLobbyWithLobbyData?.Invoke(currentLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error joining lobby: {e.Message}");
        }
    }

    public virtual async void LeaveLobby(string lobbyId, string playerId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, playerId);
            Session.RemoveAttribute("lobbyID");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async Task<Lobby> GetPlayers()
    {
        await Task.Delay(1000);
        if (currentLobby == null)
        {
            Debug.LogError("Current Lobby is NULL");
            return null;
        }
        Debug.Log($"Lobby: Name={currentLobby.Name}, MaxPlayers={currentLobby.MaxPlayers}, PlayerCount={currentLobby.Players.Count}");
        return currentLobby;
    }

    public async Task<List<Lobby>> GetLobbies()
    {
        QueryLobbiesOptions options = new QueryLobbiesOptions();
        options.Count = 30;

        // Filter for open lobbies only
        options.Filters = new List<QueryFilter>() {
                                new QueryFilter(
                                    field: QueryFilter.FieldOptions.AvailableSlots,
                                    op: QueryFilter.OpOptions.GT,
                                    value: "0"
                                    )};

        // Order by newest lobbies first
        options.Order = new List<QueryOrder>() {
                            new QueryOrder(
                                asc: false,
                                field: QueryOrder.FieldOptions.Created)};

        QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);
        return lobbies.Results;
    }
}
