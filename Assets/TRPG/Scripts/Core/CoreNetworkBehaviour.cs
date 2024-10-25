using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreNetworkBehaviour : NetworkBehaviour
{
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
}
