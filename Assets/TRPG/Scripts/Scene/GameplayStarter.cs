using System.Collections;
using System.Collections.Generic;
using TRPG;
using UnityEngine;

public class GameplayStarter : CoreNetworkBehaviour
{
    public override void OnStartServer()
    {
        base.OnStartServer();
        //NetworkPlayerManager.Instance.InitializeNetworkPlayers();
    }
}
