using FishNet;
using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalRunner : MonoBehaviour
{
    [Command]
    public void StartServer()
    {
        InstanceFinder.ServerManager.StartConnection();
    }
    [Command]
    public void StopServer()
    {
        InstanceFinder.ServerManager.StopConnection(true);
    }
}
