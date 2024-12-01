using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using TRPG;
using TRPG.Unit;
using UnityEngine;
using Utils;

public class StandSlotList : M_Singleton<StandSlotList>
{
    [SerializeField] private MenuCameraOffset cameraOffsetConfig;
    [SerializeField] private List<SceneSelectSlot> slotList;


    public SceneSelectSlot GetSlotByIndex(int index)
    {
        return slotList[index];
    }

    public void LookToSlot(int slotIndex)
    {

    }

    public void SpawnUntiToSlot(UnitProfile profile, int slotIndex)
    {
        SceneSelectSlot slot = GetSlotByIndex(slotIndex);
        if (slot != null)
        {
            RemoveOldModel(slot);
            slot.OccupiedUnitId = profile.id;
            slot.model = Instantiate(profile.model, slot.StandPosition.position, Quaternion.Euler(0, 180, 0));
            //MoveCameraToSlot(slotIndex);
        }
    }

    public void ResetCameraToDefault()
    {
        SceneCamera.Singleton.MoveTo(cameraOffsetConfig.defaultTransform.position, Quaternion.Euler(cameraOffsetConfig.defaultTransform.rotation.x, cameraOffsetConfig.defaultTransform.rotation.y, cameraOffsetConfig.defaultTransform.rotation.z));
    }

    public void DestroyAllModels()
    {
        slotList.ForEach(slot => { if (slot.model) Destroy(slot.model); });
    }

    private void RemoveOldModel(SceneSelectSlot slot)
    {
        if (slot.model != null) Destroy(slot.model);
    }

    public void MoveCameraToSlot(int slotIndex)
    {
        SceneCamera.Singleton.MoveTo(cameraOffsetConfig.slotTransformList[slotIndex].position, Quaternion.Euler(cameraOffsetConfig.slotTransformList[slotIndex].rotation.x, cameraOffsetConfig.slotTransformList[slotIndex].rotation.y, cameraOffsetConfig.slotTransformList[slotIndex].rotation.z));
    }
}
