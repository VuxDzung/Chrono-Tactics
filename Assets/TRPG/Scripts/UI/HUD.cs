using DevOpsGuy.GUI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TRPG.Unit;
using UnityEngine;

namespace TRPG
{
    public class HUD : Panel
    {
        [SerializeField] private TextMeshProUGUI tmpUnitName;
        [SerializeField] private UIUnitAbility uiAbility;
        [SerializeField] private Transform uiParent;

        private List<UIUnitAbility> uiAbilityList = new List<UIUnitAbility>();

        public void SetUnitName(string unitName)
        {
            tmpUnitName.text = unitName;
        }

        public void AssignUIAbility(AbilityType type, Sprite thumbail, Action<AbilityType> OnSelectAbility)
        {
            ClearUIAbilities();
            UIUnitAbility uiUnitAbility = Instantiate(uiAbility, uiParent);
            uiAbility.Setup(type, OnSelectAbility);
            uiAbility.Activate();
            uiAbilityList.Add(uiUnitAbility);
        }

        public void ClearUIAbilities()
        {
            uiAbilityList.ForEach(ui => Destroy(ui.gameObject));
            uiAbilityList.Clear();
        }
    }
}