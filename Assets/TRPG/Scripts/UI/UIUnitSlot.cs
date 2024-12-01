using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitSlot : MonoBehaviour
{
    protected Action<int> OnSelectSlot;
    protected int index;

    [SerializeField] private Button button;
    [SerializeField] private Image nonSelectImage;
    [SerializeField] private GameObject profileObject;
    [SerializeField] private TextMeshProUGUI tmpUnitName;

    public bool IsOccupied { get; set; }

    private void Start()
    {
        if (button == null) button = GetComponent<Button>();
        button.onClick.AddListener(OnSelect);
    }

    public void Setup(int index, Action<int> selectSlotAction)
    {
        this.index = index;
        OnSelectSlot = selectSlotAction;
    }

    public void SetUnit(string unitName)
    {
        IsOccupied = true;
        tmpUnitName.text = unitName;

        if (IsOccupied)
        {
            nonSelectImage.enabled = false;
            profileObject.SetActive(true);
        }
        else
        {
            nonSelectImage.enabled=true;
            profileObject.SetActive(false);
        }
    }

    public void ResetData()
    {
        IsOccupied = false;
        nonSelectImage.enabled = true;
        profileObject.SetActive(false);
    }

    public void OnSelect()
    {
        if (OnSelectSlot != null) OnSelectSlot(index);
    }
}
