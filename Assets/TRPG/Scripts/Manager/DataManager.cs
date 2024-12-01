using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class DataManager : M_Singleton<DataManager>
{
    [SerializeField] private UnitConfigList unitConfig;

    public UnitConfigList UnitConfig => unitConfig;
}
