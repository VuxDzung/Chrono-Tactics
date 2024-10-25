using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TRPG.Unit;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmpUnitName;
    [SerializeField] private UIUnitAbility uiAbility;
    [SerializeField] private Transform uiParent;

    private List<UIUnitAbility> uiAbilityList = new List<UIUnitAbility>();

    public void SetUnitName(string unitName)
    {
        tmpUnitName.text = unitName;    
    }

    public void AssignUIAbility(string aId, Sprite thumbail, Action<string> OnSelectAbility)
    {
        ClearUIAbilities();
        UIUnitAbility uiUnitAbility = Instantiate(uiAbility, uiParent);
        uiAbility.Setup(aId, OnSelectAbility);
    }

    public void ClearUIAbilities()
    {
        uiAbilityList.ForEach(ui => Destroy(ui.gameObject));
        uiAbilityList.Clear();
    }
}
