using UnityEngine;

public class CellSpawner
{
    private FieldCell _fieldPrefab;

    public CellSpawner(FieldCell fieldPrefab)
    {
        _fieldPrefab = fieldPrefab;
    }

    public FieldCell SpawnCell(Vector3 coordinate)
    {
        FieldCell cell;
        cell = GameObject.Instantiate<FieldCell>(_fieldPrefab, coordinate, Quaternion.identity);

        return cell;
    }
}
