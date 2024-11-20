using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleGridPosition : MonoBehaviour
{
    public bool HasBeenOccupied { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        HasBeenOccupied = true;
    }

    private void OnTriggerExit(Collider other)
    {
        HasBeenOccupied = false;
    }

    private void OnTriggerStay(Collider other)
    {
        
    }
}
