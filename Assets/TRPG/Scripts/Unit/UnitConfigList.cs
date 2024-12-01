using System.Collections.Generic;
using System.Linq;
using TRPG.Unit;
using UnityEngine;

[CreateAssetMenu]
public class UnitConfigList : ScriptableObject
{
    [SerializeField] private List<UnitProfile> unitProfileList;

    public List<UnitProfile> UnitControllerList => unitProfileList;

    public UnitProfile GetUnitProfileById(string unitId)
    {
        UnitProfile profile = unitProfileList.FirstOrDefault(unit => unit.id.Equals(unitId));
        return profile;
    }

    //public UnitController[] GetUnitsById(string[] idArray)
    //{
    //    List<UnitController> selectedUnitList = new List<UnitController>();
    //    foreach (string unitId in idArray)
    //    {
    //        UnitController unitController = GetUnitById(unitId);

    //        if (unitController != null) selectedUnitList.Add(unitController);
    //    }
    //    return selectedUnitList.ToArray();
    //}
}