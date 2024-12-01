using DevOpsGuy.GUI;
using TMPro;
using UnityEngine;

public class PlayerPrepareStatus : UIContent
{
    [SerializeField] private Color readyColor;
    [SerializeField] private Color notReadyColor;

    [SerializeField] private TextMeshProUGUI tmpPlayerName;
    [SerializeField] private TextMeshProUGUI tmpPlayerStatus;

    public bool IsReady { get; set; }

    public void Setup(string id)
    {
        this.id = id;
        tmpPlayerName.text = id;
        IsReady = false;
        SetReady();
    }

    public void Setup(string id, bool ready)
    {
        this.id = id;
        tmpPlayerName.text = id;
        IsReady = ready;
        SetReady();
    }

    public void Setup(string id, string playerName, bool status)
    {
        if (!string.IsNullOrEmpty(this.id)) return;

        this.id = id;
        tmpPlayerName.text = playerName;
        IsReady = status;
        SetReady();
    }

    public void ToggleReady()
    {
        IsReady = !IsReady;
        SetReady();
    }

    public void SetReady()
    {
        if (IsReady)
        {
            tmpPlayerStatus.color = readyColor;
            tmpPlayerStatus.text = "Ready";
        }
        else
        {
            tmpPlayerStatus.color = notReadyColor;
            tmpPlayerStatus.text = "Not Ready";
        }
    }
}