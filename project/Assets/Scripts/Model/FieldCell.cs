using System.Collections.Generic;
using UnityEngine;

public class FieldCell : MonoBehaviour
{
    public class FieldEffect
    {
        public float AttackEffect;
        public float DefenseEffect;
        public int MoveEffect;
    }
    public static readonly string ObjTag = "cell";

    public Unit OnUnitData { get; private set; } = default!;
    public FieldEffect Effect { get; private set; } = default!;
    public bool Movable { get; private set; } = default!;
    public List<FieldCell> NeighborCells { get; private set; } = default!;
    public void Initialize(bool movable, float attackEffect, float defenseEffect, int moveEffect)
    {
        Movable = movable;
        Effect = new FieldEffect
        {
            AttackEffect = attackEffect,
            DefenseEffect = defenseEffect,
            MoveEffect = moveEffect
        };

        NeighborCells = new List<FieldCell>();
    }

    public void AddNeighborCell(FieldCell neighborCell)
    {
        if (neighborCell == null)
        {
            return;
        }

        NeighborCells.Add(neighborCell);
    }

    public void SetOnUnit(Unit unit)
    {
        if (unit != null)
        {
            OnUnitData = unit;
            Movable = false;
        }
    }

    public void RemoveUnit()
    {
        OnUnitData = null!;
        Movable = true;
    }
}
