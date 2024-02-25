using UnityEngine;
using UnityEngine.SceneManagement;

public class MetaAI : MonoBehaviour
{
    private const float RandomVal = 5;

    private const float UnitHpVal = 100f;
    private const int UnitMoveVal = 5;
    private const int AttackRangeVal = 1;

    private const int PlayerUnitAttackVal = 30;
    private const int PlayerUnitDefenseVal = 5;

    private const int EnemyUnitDefenseVal = 5;

    public enum InvokeName
    {
        RestartScene,
    }

    [SerializeField] private FieldCell _fieldPrefab = default!;
    [SerializeField] private Unit _maleUnitPrefab = default!;
    [SerializeField] private Unit _femaleUnitPrefab = default!;
    [SerializeField] private GameFieldData _gameField = default!;
    [SerializeField] private TurnChanger _turnChanger = default!;
    [SerializeField] private Battle _battle = default!;
    [SerializeField] private TargetPanel _targetPanel = default!;
    [SerializeField] private InputReceiver _inputReceiver = default!;

    private UnitController _unitController = default!;

    [SerializeField] private UIPresenter _uiPresenter = default!;

    private TargetSelector _targetSelecter = default!;

    private CellSpawner _cellSpawner = default!;
    private UnitSpawner _unitSpawner = default!;

    private int _enemyAttackNum;

    private void Start()
    {
        _cellSpawner = new CellSpawner(_fieldPrefab);
        _unitSpawner = new UnitSpawner();
        _unitController = new UnitController(_battle);

        _unitSpawner.UnitDict.Add(UnitSpawner.UnitType.Male, _maleUnitPrefab);
        _unitSpawner.UnitDict.Add(UnitSpawner.UnitType.Female, _femaleUnitPrefab);

        _enemyAttackNum = 10;
        switch (GameSettingParam.GameDifficulty)
        {
            case TitleData.Difficulty.Easy:
                _enemyAttackNum = 10;
                break;
            case TitleData.Difficulty.Normal:
                _enemyAttackNum = PlayerUnitAttackVal;
                break;
            case TitleData.Difficulty.Difficult:
                _enemyAttackNum = 50;
                break;

        }

        InitializeField();
        InitializeUnits();

        _turnChanger.Initialize(_gameField);
        _targetSelecter = new TargetSelector(_turnChanger, _uiPresenter, _targetPanel, _inputReceiver, _gameField, _unitController);

        _turnChanger.SendStartTurn();
    }

    private void Update()
    {
        if (_gameField == null)
        {
            throw new System.InvalidOperationException("_gameField field is null.");
        }

        /* 勝敗確認 */
        if (_gameField.EnemyUnits.Count == 0)
        {
            /* 味方の勝利 */
            FinishGame(Unit.Team.Player);
        }
        else if (_gameField.PlayerUnits.Count == 0)
        {
            /* 敵の勝利 */
            FinishGame(Unit.Team.Enemy);
        }
    }

    public void OnDestroy()
    {
        _targetSelecter.OnDestroy();
    }

    private void InitializeField()
    {
        if (_fieldPrefab == null)
        {
            return;
        }

        for (int i = 0; i < GameFieldData.GameFieldWidth; i++)
        {
            for (int j = 0; j < GameFieldData.GameFieldLength; j++)
            {
                Vector3 coordinate = new Vector3(i, 0.0f, j);
                FieldCell cell = _cellSpawner.SpawnCell(coordinate);
                cell.Initialize(true, i, j, i - j);
                _gameField.FieldCells.Add(coordinate, cell);
            }
        }

        foreach (var fieldCell in _gameField.FieldCells.Values)
        {
            Vector3 cellCoordinate = fieldCell.transform.position;
            if (_gameField.FieldCells.ContainsKey(cellCoordinate + Vector3.right))
            {
                fieldCell.AddNeighborCell(_gameField.FieldCells[cellCoordinate + Vector3.right]);
            }
            if (_gameField.FieldCells.ContainsKey(cellCoordinate + Vector3.left))
            {
                fieldCell.AddNeighborCell(_gameField.FieldCells[cellCoordinate + Vector3.left]);
            }
            if (_gameField.FieldCells.ContainsKey(cellCoordinate + Vector3.forward))
            {
                fieldCell.AddNeighborCell(_gameField.FieldCells[cellCoordinate + Vector3.forward]);
            }
            if (_gameField.FieldCells.ContainsKey(cellCoordinate + Vector3.back))
            {
                fieldCell.AddNeighborCell(_gameField.FieldCells[cellCoordinate + Vector3.back]);
            }
        }
    }

    private void InitializeUnits()
    {
        if (_maleUnitPrefab != null)
        {
            Vector3 coordinate;
            Vector3 temp = new Vector3(0f, GameFieldData.CharacterDiff, 0f);

            for (int i = 0; i < GameSettingParam.UnitNum; i++)
            {
                coordinate = new Vector3(i, GameFieldData.CharacterDiff, 0f);
                Unit playerUnit = _unitSpawner.SpawnUnit(coordinate, Quaternion.identity, UnitSpawner.UnitType.Male);

                if (!_gameField.FieldCells.TryGetValue(coordinate - temp, out var cell))
                {
                    continue;
                }

                cell = _gameField.FieldCells[coordinate - temp];

                int realAttackVal = PlayerUnitAttackVal + (int)Random.Range((-1 * RandomVal), (RandomVal)); /* -5～5の間にしたい */
                int realDefenseVal = PlayerUnitDefenseVal + (int)Random.Range((-1 * RandomVal), (RandomVal));

                playerUnit.Initialize($"Player{i}", UnitHpVal, realAttackVal, realDefenseVal, UnitMoveVal, AttackRangeVal, Unit.Team.Player, cell, _unitController);
                _gameField.PlayerUnits.Add(playerUnit);

            }
            for (int i = 0; i < GameSettingParam.UnitNum; i++)
            {
                coordinate = new Vector3(GameFieldData.GameFieldWidth - 1 - i, GameFieldData.CharacterDiff, GameFieldData.GameFieldLength - 1);
                Unit enemyUnit = _unitSpawner.SpawnUnit(coordinate, new Quaternion(0, 180, 0, 0), UnitSpawner.UnitType.Female);

                if (!_gameField.FieldCells.TryGetValue(coordinate - temp, out var cell))
                {
                    continue;
                }

                cell = _gameField.FieldCells[coordinate - temp];

                int realAttackVal = _enemyAttackNum + (int)Random.Range((-1 * RandomVal), (RandomVal));
                int realDefenseVal = EnemyUnitDefenseVal + (int)Random.Range((-1 * RandomVal), (RandomVal));

                enemyUnit.Initialize($"Enemy{i}", UnitHpVal, realAttackVal, realDefenseVal, UnitMoveVal, AttackRangeVal, Unit.Team.Enemy, cell, _unitController);
                _gameField.EnemyUnits.Add(enemyUnit);
            }
        }
    }

    private void FinishGame(Unit.Team winTeam)
    {
        _uiPresenter.SetGameFinishText(winTeam);
        Invoke(nameof(InvokeName.RestartScene), 3.0f); // 1.5秒後にRestartSceneを実行
    }

    /// <summary>
    /// ゲームリスタート
    /// </summary>
    public void RestartScene()
    {
        Scene thisScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(thisScene.name); // 今のシーンをもう一度読み込むことでリスタートする
    }
}
