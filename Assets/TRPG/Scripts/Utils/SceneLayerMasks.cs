using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SceneLayerMasks : MonoBehaviour
{
    [SerializeField] private List<LayerMaskData> layerMaskList = new List<LayerMaskData>();

    public static SceneLayerMasks Instance { get; private set; }

    public List<LayerMaskData> LayerMaskList => layerMaskList;

    private void Awake()
    {
        Instance = this;
    }

    public static LayerMask GetLayerMaskByCategory(MaskCategory category)
    {
        LayerMaskData data = Instance.LayerMaskList.FirstOrDefault(x => x.category == category);
        return data.layerMask;
    }

    public static float GetLayerMaskValue(LayerMask layerMask)
    {
        return Mathf.Log(layerMask.value, 2);
    }
}

[Serializable]
public struct LayerMaskData
{
    public MaskCategory category;
    public LayerMask layerMask;
}

public enum MaskCategory
{
    None,
    Unit,
    Obstacle,
    Ground,
    GridObstacle,
    Enemy,
}