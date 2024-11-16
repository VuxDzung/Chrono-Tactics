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
        [SerializeField] private Image icon;
        [SerializeField] private AbilityType abilityType;

        public void Setup(AbilityType type, Sprite thumbnail, Action<AbilityType> onSelectAbility)
        {
            abilityType = type;
            OnSelectAbility = onSelectAbility;
            icon.sprite = thumbnail;
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