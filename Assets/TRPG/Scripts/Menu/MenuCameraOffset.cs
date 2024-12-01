using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct CustomTransform
{
    public Vector3 position;
    public Vector3 rotation; 
}

[CreateAssetMenu]
public class MenuCameraOffset : ScriptableObject
{
    public CustomTransform defaultTransform;
    public List<CustomTransform> slotTransformList;
}
