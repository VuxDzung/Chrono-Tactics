using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitIcon : MonoBehaviour
{
    public Action<string> OnSelectUnit;

    [SerializeField] private Button button;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color notOpenedColor;
    [SerializeField] private Image borderImg;
    [SerializeField] private Image avatarImg;

    public string UnitID { get; private set; }

    public bool IsClickable
    {
        get
        {
            return button.interactable;
        }
        set
        {
            button.interactable = value;
        }
    }

    public void Setup(string unitId, Sprite avatarThumbnail, Action<string> selectUnitAction)
    {
        UnitID = unitId;
        avatarImg.sprite = avatarThumbnail;

        borderImg.color = normalColor;
        avatarImg.color = normalColor;

        OnSelectUnit = selectUnitAction;
        button.onClick.AddListener(OnSelect);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnSelect);
    }

    public void OnSelect()
    {
        borderImg.color = selectedColor;

        if (OnSelectUnit != null) OnSelectUnit(UnitID);
    }

    public void Deselect()
    {
        borderImg.color = normalColor;
    }
}