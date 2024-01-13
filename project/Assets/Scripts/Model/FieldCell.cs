using System.Collections.Generic;
using UnityEngine;

public class FieldCell : MonoBehaviour
{
    public class FieldEffect{
        public float AttackEffect;
        public float DefenseEffect;
        public int MoveEffect;
    }
    public static string ObjTag = "cell";

    private bool _isMovable;
    private Unit _onUnitData;
    private FieldEffect _fieldEffect;
    private List<FieldCell> _neighborCells;

    public Unit OnUnitData { get => _onUnitData; }
    public FieldEffect Effect { get => _fieldEffect; }
    public bool Movable { get => _isMovable; }
    public List<FieldCell> NeighborCells { get => _neighborCells; }
    public void initialize(bool movable, float attackEffect, float defenseEffect, int moveEffect)
    {
        _isMovable = movable;
        _fieldEffect = new FieldEffect 
        { 
            AttackEffect = attackEffect, 
            DefenseEffect = defenseEffect, 
            MoveEffect = moveEffect 
        };

        _neighborCells = new List<FieldCell>();
    }

    public void AddNeighborCell(FieldCell neighborCell)
    {
        if (neighborCell == null)
        {
            return;
        }

        _neighborCells.Add(neighborCell);
    }

    public void SetOnUnit(Unit unit)
    {
        if (unit != null)
        {
            _onUnitData = unit;
            _isMovable = false;
        }
    }

    public void RemoveUnit()
    {
        _onUnitData = null;
        _isMovable = true;
    }

    public void BreakCell()
    {
        /* セルが無くなった場合の処理(必要になった際に実装) */
    }
}
