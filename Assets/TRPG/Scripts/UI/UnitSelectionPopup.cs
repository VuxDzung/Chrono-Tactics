using DevOpsGuy.GUI;
using System.Collections;
using System.Collections.Generic;
using TRPG;
using TRPG.Unit;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectionPopup : Modal
{
    [SerializeField] private UnitConfigList unitConfigList;
    [SerializeField] private Transform contentParent;
    [SerializeField] private UIUnitIcon iconPrefab;
    [SerializeField] private Button btnSelect;
    [SerializeField] private Button btnBack;

    private List<UIUnitIcon> unitIconList = new List<UIUnitIcon>();
    private string selectedUnitID;
    private int currentSlot;

    private void OnEnable()
    {
        
        btnSelect.onClick.AddListener(SaveSelectOption);
        btnBack.onClick.AddListener(Back);
        
    }

    private void OnDisable()
    {
        ClearAllIcons();

        btnSelect.onClick.RemoveListener(SaveSelectOption);
        btnBack.onClick.RemoveListener(Back);
    }

    public void LoadSelectedUnitOnUI(string unitId, int slotIndex)
    {
        ClearAllIcons();
        LoadUnitsOnUI();
        currentSlot = slotIndex;
        if (!string.IsNullOrEmpty(unitId)) SelectUnit(unitId);

        StandSlotList.Singleton.MoveCameraToSlot(currentSlot);
    }

    /// <summary>
    /// Load all obtained unit id array on UI.
    /// </summary>
    private void LoadUnitsOnUI()
    {
        UserData userData = SaveLoadUtil.Load<UserData>(SaveKeys.UserData);

        if (userData != null)
        {
            if (userData.ObtainedUnitIds.Length > 0)
            {
                for (int i = 0; i < userData.ObtainedUnitIds.Length; i++)
                {
                    if (!string.IsNullOrEmpty(userData.ObtainedUnitIds[i]))
                    {
                        UnitProfile profile = unitConfigList.GetUnitProfileById(userData.ObtainedUnitIds[i]);
                        UIUnitIcon icon = Instantiate(iconPrefab, contentParent);
                        icon.Setup(profile.id, profile.thumbnail, SelectUnit);
                        icon.gameObject.SetActive(true);

                        if (HasOwned(userData.InTeamUnitIds, userData.ObtainedUnitIds[i]))
                            icon.IsClickable = false;
                        else 
                            icon.IsClickable = true;

                        unitIconList.Add(icon);
                    }
                }
            }
        }
        else
        {
            Debug.Log("User data is null!");
        }
    }

    public void DeselectAll()
    {
        unitIconList.ForEach(icon => icon.Deselect());
    }

    /// <summary>
    /// Load the selected model to UI.
    /// Assign current unit id.
    /// </summary>
    /// <param name="selectedUnitID"></param>
    private void SelectUnit(string selectedUnitID)
    {
        DeselectAll();
        this.selectedUnitID = selectedUnitID.Trim();
        UnitProfile profile = unitConfigList.GetUnitProfileById(selectedUnitID);
        StandSlotList.Singleton.SpawnUntiToSlot(profile, currentSlot);
    }

    /// <summary>
    /// Add selected unit to the user data.
    /// Hide current UI and show Prepare Panel.
    /// </summary>
    private void SaveSelectOption()
    {
        UserData userData = SaveLoadUtil.Load<UserData>(SaveKeys.UserData);

        if (userData != null)
        {
            if (userData.InTeamUnitIds.Length > 0)
            {
                // - Check if the current index greater/less than the array size.
                // - If greater, ensure the current slot does not exceed 4 and if it does, replace the saved data as the current selected unit id.
                // - If less, ensure the slot index does not fall below zero and if it does, replace the saved data as the current selected unit id.
                if (currentSlot >= 0 && currentSlot < 4)
                {
                    userData.InTeamUnitIds[currentSlot] = selectedUnitID;
                } 
            }
            SaveLoadUtil.Save(SaveKeys.UserData, userData);
        }
        manager.HideUI(this);
        manager.ShowUI<PreparePanel>();
    }

    private void ClearAllIcons()
    {
        unitIconList.ForEach(icon => Destroy(icon.gameObject));
        unitIconList.Clear();
    }

    private void Back()
    {
        manager.HideUI(this);
        manager.ShowUI<PreparePanel>();
        //StandSlotList.Singleton.ResetCameraToDefault();
    }

    public static bool HasOwned(string[] unitIdArr, string selectedUnitId)
    {
        foreach (var id in unitIdArr)
            if (id.Equals(selectedUnitId))
                return true;
        return false;
    }
}