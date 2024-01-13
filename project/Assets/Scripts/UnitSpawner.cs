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

    private Dictionary<UnitType, GameObject> _unitDict;

    public Dictionary<UnitType, GameObject> UnitDict { get => _unitDict; }

    public UnitSpawner()
    {
        _unitDict = new Dictionary<UnitType, GameObject>();
    }

    public Unit SpawnUnit(Vector3 cordinate, Quaternion quaternion, UnitType type)
    {
        GameObject gameObj;

        if (_unitDict.ContainsKey(type) == false)
        {
            return null;
        }

        gameObj = GameObject.Instantiate(_unitDict[type], cordinate, quaternion);
        Unit cell = gameObj.GetComponent<Unit>();

        return cell;
    }
}
