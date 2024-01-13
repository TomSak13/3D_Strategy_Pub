using UnityEngine;

public class CellSpawner
{
    private GameObject _fieldPrefab;

    public CellSpawner(GameObject fieldPrefab)
    {
        _fieldPrefab = fieldPrefab;
    }

    public FieldCell SpawnCell(Vector3 cordinate)
    {
        GameObject gameObj;
        gameObj = GameObject.Instantiate(_fieldPrefab, cordinate, Quaternion.identity);
        FieldCell cell = gameObj.GetComponent<FieldCell>();

        return cell;
    }
}
