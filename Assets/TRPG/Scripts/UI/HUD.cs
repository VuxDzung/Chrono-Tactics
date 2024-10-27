using DevOpsGuy.GUI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TRPG.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TRPG
{
    public class HUD : Panel
    {
        public static Action OnEndTurn;
        public static Action OnNextUnit;
        public static Action OnPrevUnit;

        [SerializeField] private TextMeshProUGUI tmpUnitName;
        [SerializeField] private UIUnitAbility uiAbility;
        [SerializeField] private Transform uiParent;
        [SerializeField] private Button btnEndTurn;
        [SerializeField] private Button btnNextUnit;
        [SerializeField] private Button btnPrevUnit;

        private List<UIUnitAbility> uiAbilityList = new List<UIUnitAbility>();

        private void OnEnable()
        {
            btnEndTurn.onClick.AddListener(EndTurn);
            btnNextUnit.onClick.AddListener(ChangeNextUnit);
            btnPrevUnit.onClick.AddListener(OnChangePrevUnit);
        }

        private void OnDisable()
        {
            btnEndTurn.onClick.RemoveListener(EndTurn);
            btnNextUnit.onClick.RemoveListener(ChangeNextUnit);
            btnPrevUnit.onClick.RemoveListener(OnChangePrevUnit);
        }

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

        private void EndTurn()
        {
            OnEndTurn?.Invoke();
        }

        private void ChangeNextUnit()
        {
            OnNextUnit?.Invoke();
        }

        private void OnChangePrevUnit()
        {
            OnPrevUnit?.Invoke();
        }
    }
}