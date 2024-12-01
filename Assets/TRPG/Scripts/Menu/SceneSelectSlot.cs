using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSelectSlot : MonoBehaviour
{
    [SerializeField] private Transform standPosition;
    [SerializeField] public GameObject model;

    public string OccupiedUnitId { get; set; }
    public Transform StandPosition => standPosition;
}
