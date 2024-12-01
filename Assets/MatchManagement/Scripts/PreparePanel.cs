using DevOpsGuy.GUI;
using System;
using System.Collections.Generic;
using TRPG.Unit;
using TRPG;
using UnityEngine;
using UnityEngine.UI;
using CoopPackage;
using DevOpsGuy;
using Unity.Services.Authentication;
using TMPro;
using System.Linq;

public class PreparePanel : Panel
{
    public Action<string> OnLeaveMatch;

    [SerializeField] private UnitConfigList configList;
    [SerializeField] private Button btnReady;
    [SerializeField] private Button btnCancelGame;
    [SerializeField] private List<UIUnitSlot> slotList;

    [SerializeField] private List<PlayerPrepareStatus> statusPlayerList = new List<PlayerPrepareStatus>();

    private void Start()
    {
        slotList.ForEach(slot => slot.Setup(slotList.IndexOf(slot), SelectSlot));
        //InMatchPlayerManager.OnPlayerJoinedCallback += ShowPlayerStatus;
    }    

    /// <summary>
    /// Load all units in team on UI and on scene.
    /// </summary>
    private void OnEnable()
    {
        btnReady.onClick.AddListener(Ready);
        btnCancelGame.onClick.AddListener(Cancel);

        NetworkPlayerManager.OnPlayerReadyCallback += ReadyPlayer;
    }

    /// <summary>
    /// Clear all UI cache.
    /// Clear all 3D models.
    /// </summary>
    private void OnDisable()
    {
        NetworkPlayerManager.OnPlayerReadyCallback -= ReadyPlayer;

        btnReady.onClick.RemoveListener(Ready);
        btnCancelGame.onClick.RemoveListener(Cancel);

        slotList.ForEach(slot => slot.ResetData());
    }

    public override void Hide()
    {
        base.Hide();
        StandSlotList.Singleton.DestroyAllModels();
    }

    /// <summary>
    /// Notify for the other that you're read for battle.
    /// </summary>
    public void Ready()
    {
        NetworkPlayerManager.Instance.ReadyForBattle(AuthenticationService.Instance.PlayerId);
    }

    /// <summary>
    /// Go back to home.
    /// Leave the lobby (If you're the host of room, the room shall be canceled).
    /// </summary>
    public void Cancel()
    {
        UIManager.HideAll();
        OnLeaveMatch?.Invoke(NetworkGameManager.Singleton.SDK.Authentication.PlayerId);
        manager.ShowUI<MenuPanel>();
    }

    public override void Show()
    {
        base.Show();
        SoundManager.Singleton.PlayBGMusic(MusicCategory.PrepareForBattle, true);
        StandSlotList.Singleton.ResetCameraToDefault();
        LoadInTeamUnitToUI();
    }

    private void LoadInTeamUnitToUI()
    {
        UserData userData = SaveLoadUtil.Load<UserData>(SaveKeys.UserData);

        if (userData != null)
        {
            if (userData.InTeamUnitIds != null && userData.InTeamUnitIds.Length > 0)
            {
                for (int i = 0; i < userData.InTeamUnitIds.Length; i++)
                {
                    if (!string.IsNullOrEmpty(userData.InTeamUnitIds[i]))
                    {
                        UnitProfile profile = configList.GetUnitProfileById(userData.InTeamUnitIds[i]);
                        StandSlotList.Singleton.SpawnUntiToSlot(profile, i);
                        slotList[i].SetUnit(profile.unitName);
                    }
                }
            }
        }
        else
        {
            Debug.Log("User data is null!");
        }
    }
    
    public void LoadPlayersStatus(List<InGamePlayerData> playerDataList)
    {
        for (int i = 0; i < playerDataList.Count; i++)
        {
            statusPlayerList[i].Setup(playerDataList[i].id, playerDataList[i].playerName, playerDataList[i].status);
        }
    }

    public void ShowPlayerStatus(string playerId, int index, bool ready)
    {
        statusPlayerList[index].Setup(playerId, ready);
    }

    public void ReadyPlayer(string playerId)
    {
        PlayerPrepareStatus status = statusPlayerList.FirstOrDefault(player => player.Id.Equals(playerId));

        if (status != null) status.ToggleReady();

        else Debug.LogError($"Player: {playerId} does not have status UI.");
    }

    public void SelectSlot(int slotIndex)
    {
        string occupiedId = StandSlotList.Singleton.GetSlotByIndex(slotIndex).OccupiedUnitId;

        manager.HideUI(this);

        manager.ShowUI<UnitSelectionPopup>().LoadSelectedUnitOnUI(occupiedId, slotIndex);
    }
}
