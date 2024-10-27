using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevOpsGuy.GUI;
using UnityEngine.UI;
using System;

namespace TRPG.Unit
{
    public class UIUnitAbility : UIContent
    {
        private const string UI_CONTENT_FORMAT = "UI-Ability [{0}]";
        public Action<AbilityType> OnSelectAbility;

        [SerializeField] private Button button;

        private AbilityType abilityType;

        public void Setup(AbilityType type, Action<AbilityType> onSelectAbility)
        {
            abilityType = type;
            OnSelectAbility = onSelectAbility;
            gameObject.name = string.Format(UI_CONTENT_FORMAT, type.ToString());
        }

        private void Start()
        {
            button.onClick.AddListener(Trigger);
        }

        public void Trigger()
        {
            OnSelectAbility?.Invoke(abilityType);
        }

        public void Activate()
        {
            gameObject.SetActive(true);
        }
    }
}