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

    public Dictionary<Vector3, FieldCell> FieldCells { get; private set; } = default!;
    public List<Unit> PlayerUnits { get; private set; } = default!;
    public List<Unit> EnemyUnits { get; private set; } = default!;
    public List<Unit> RemoveUnits { get; private set; } = default!;
    public Turn CurrentTurn { get; set; }

    public Mode CurrentMode { get; set; }

    public Unit CurrentTarget { get; set; } = default!;
    private void Start()
    {
        /* 10×10生成 */
        FieldCells = new Dictionary<Vector3, FieldCell>();
        PlayerUnits = new List<Unit>();
        EnemyUnits = new List<Unit>();
        RemoveUnits = new List<Unit>();

        CurrentTurn = Turn.EnemyTurn;

        CurrentMode = Mode.Game;
    }

    public void DeathUnits()
    {
        foreach (var unit in RemoveUnits)
        {
            if (unit.UnitTeam == Unit.Team.Player)
            {
                if (PlayerUnits.Contains(unit))
                {
                    PlayerUnits.Remove(unit);
                }
            }
            else
            {
                if (EnemyUnits.Contains(unit))
                {
                    EnemyUnits.Remove(unit);
                }
            }
        }

        RemoveUnits.Clear();
    }
}
