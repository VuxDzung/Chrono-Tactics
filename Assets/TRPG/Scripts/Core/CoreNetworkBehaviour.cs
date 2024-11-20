using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreNetworkBehaviour : NetworkBehaviour
{
    public enum TextColor
    {
        Red,
        Green,
        Yellow,
    }
    public bool IsServerMachine => OwnerId == -1;

    public virtual void Update()
    {
        if (IsServerInitialized)
            OnServerUpdate();
        if (IsClientInitialized)
            OnClientUpdate();
    }

    [Server]
    public virtual void OnServerUpdate() { }
    [Client]
    public virtual void OnClientUpdate() { }

    public virtual void Log(string label, string message, TextColor labelColor)
    {
        string color = "red";

        switch (labelColor)
        {
            case TextColor.Red:
                color = "red";
                break;
            case TextColor.Green:
                color = "green";
                break;
            case TextColor.Yellow:
                color = "yellow";
                break;
        }

        Debug.Log($"<color={color}>{label}:</color> {message}");
    }
}
