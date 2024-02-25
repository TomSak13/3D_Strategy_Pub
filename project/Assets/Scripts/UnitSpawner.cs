using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner
{
    public enum UnitType
    {
        Male,
        Female,
        Boss
    }

    private Dictionary<UnitType, Unit> _unitDict = new Dictionary<UnitType, Unit>();

    public Dictionary<UnitType, Unit> UnitDict { get => _unitDict; }

    public Unit SpawnUnit(Vector3 coordinate, Quaternion quaternion, UnitType type)
    {
        if (_unitDict.ContainsKey(type) == false)
        {
            throw new System.InvalidOperationException("not found Type:"+type);
        }

        Unit unit = GameObject.Instantiate<Unit>(_unitDict[type], coordinate, quaternion);

        return unit;
    }
}
