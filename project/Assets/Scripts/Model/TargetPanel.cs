using System.Collections.Generic;
using UnityEngine;

public class TargetPanel : MonoBehaviour
{
    private FieldCell _focusCell = default!;

    public FieldCell FocusCell { get => _focusCell; }

    private void Update()
    {
        if (_focusCell == null)
        {
            return;
        }

        transform.position = _focusCell.transform.position + (Vector3.up * 1.25f);
    }

    public void MoveFocusCell(FieldCell cell)
    {
        if (cell == null)
        {
            return;
        }

        _focusCell = cell;
    }

    public void MoveNeighborFocusCell(KeyCode keycode)
    {
        List<FieldCell> neighbor = _focusCell.NeighborCells;
        Vector3 nextCellPos = _focusCell.transform.position;
        if (keycode == KeyCode.UpArrow)
        {
            nextCellPos += Vector3.forward;
        }
        if (keycode == KeyCode.DownArrow)
        {
            nextCellPos += Vector3.back;
        }
        if (keycode == KeyCode.LeftArrow)
        {
            nextCellPos += Vector3.left;
        }
        if (keycode == KeyCode.RightArrow)
        {
            nextCellPos += Vector3.right;
        }

        if (nextCellPos.x < 0 || nextCellPos.x >= GameFieldData.GameFieldWidth ||
            nextCellPos.z < 0 || nextCellPos.z >= GameFieldData.GameFieldLength)
        {
            return; /* フィールド外なので移動させない */
        }

        float distance = float.MaxValue;
        FieldCell nextCell = _focusCell;
        foreach (var cell in neighbor)
        {
            if (distance > Vector3.SqrMagnitude(cell.transform.position - nextCellPos))
            {
                nextCell = cell;
                distance = Vector3.SqrMagnitude(cell.transform.position - nextCellPos);
            }
        }

        _focusCell = nextCell;
    }
}
