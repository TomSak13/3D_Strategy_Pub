using System.Collections.Generic;
using UnityEngine;

public class GameFieldData : MonoBehaviour
{
    public const float CharacterDiff = 0.5f;
    public const int GameFieldWidth = 10;
    public const int GameFieldLength = 10;
    public enum Turn
    {
        PlayerTurn,
        EnemyTurn
    }

    public enum Mode
    {
        Game,
        View
    }

    private Dictionary<Vector3, FieldCell> _fieldCells;
    private List<Unit> _palyerUnits;
    private List<Unit> _enemyUnits;
    private List<Unit> _removeUnits;
    private Turn _currentTurn;
    private Mode _currentMode;

    private Unit _currentTarget;

    public Dictionary<Vector3, FieldCell>  FieldCells { get => _fieldCells; }
    public List<Unit> PlayerUnits { get => _palyerUnits; }
    public List<Unit> EnemyUnits { get => _enemyUnits; }

    public List<Unit> RemoveUnits { get => _removeUnits; }
    public Turn CurrentTurn { get => _currentTurn; set => _currentTurn = value; }
    public Mode CurrentMode { get => _currentMode; set => _currentMode = value; }

    public Unit CurrentTarget { get => _currentTarget; set => _currentTarget = value; }

    private void Start()
    {
        /* モック用:10×10生成 */
        _fieldCells = new Dictionary<Vector3, FieldCell>();
        _palyerUnits = new List<Unit>();
        _enemyUnits = new List<Unit>();
        _removeUnits = new List<Unit>();

        _currentTurn = Turn.EnemyTurn;

        _currentMode = Mode.Game;
    }

    public bool IsField(Vector3 targetPosition)
    {
        if (_fieldCells.Count == 0)
        {
            return false;
        }

        if (!_fieldCells.ContainsKey(targetPosition))
        {
            return false;
        }

        return true;
    }

    public void DeathUnits()
    {
        foreach (var unit in _removeUnits)
        {
            if (unit.UnitTeam == Unit.Team.Player)
            {
                if (_palyerUnits.Contains(unit))
                {
                    _palyerUnits.Remove(unit);
                    Destroy(unit);
                }
            }
            else
            {
                if (_enemyUnits.Contains(unit))
                {
                    _enemyUnits.Remove(unit);
                    Destroy(unit);
                }
            }
        }

        _removeUnits.Clear();
    }
}
