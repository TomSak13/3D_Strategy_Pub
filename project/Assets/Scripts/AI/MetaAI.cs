using UnityEngine;
using UnityEngine.SceneManagement;

public class MetaAI : MonoBehaviour
{
    public const int UnitMaxNum = 3;
    public const int FieldWidth = 10;
    public const int FieldLength = 10;

    public const float RandomVal = 5;

    public const int PlayerUnitAttackVal = 30;
    public const int PlayerUnitDefenseVal = 5;

    public const int EnemyUnitDefenseVal = 5;

    public enum InvokeName
    {
        RestartScene,
    }

    [SerializeField] private GameObject _fieldPrefab;
    [SerializeField] private GameObject _maleUnitPrefab;
    [SerializeField] private GameObject _femaleUnitPrefab;
    [SerializeField] private GameFieldData _gameField;
    [SerializeField] private StrategyAI _strategyAI;
    [SerializeField] private TargetSelecter _targetSelecter;
    [SerializeField] private TurnChanger _turnChanger;
    [SerializeField] private InputStrategyShaper _input;

    [SerializeField] private UIPresenter _uiPresenter;

    [SerializeField] private CommonParam _commonParam;

    private CellSpawner _cellSpawner;
    private UnitSpawner _unitSpawner;

    private int _enemyAttackNum;

    private void Start()
    {
        _cellSpawner = new CellSpawner(_fieldPrefab);
        _unitSpawner = new UnitSpawner();

        _unitSpawner.UnitDict.Add(UnitSpawner.UnitType.Male, _maleUnitPrefab);
        _unitSpawner.UnitDict.Add(UnitSpawner.UnitType.Female, _femaleUnitPrefab);

        _enemyAttackNum = 10;
        if (_commonParam != null)
        {
            switch (_commonParam.GameDifficulty)
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
                default:
                    break;
            }
        }

        initializeField();
        initializeUnit();

        _strategyAI.Initialize(Unit.Team.Enemy, _gameField);

        _input.Initialize(_gameField, GameFieldData.Turn.PlayerTurn);

        _targetSelecter.Initialize(_gameField);

        _turnChanger.Initialize(_gameField);

        _turnChanger.SendStartTurn();

        
    }

    // Update is called once per frame
    private void Update()
    {
        /* 勝敗確認 */
        if (_gameField == null)
        {
            return;
        }

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

    public void initializeField()
    {
        if (_fieldPrefab != null)
        {
            for (int i = 0; i < FieldWidth; i++)
            {
                for (int j = 0; j < FieldLength; j++)
                {
                    Vector3 cordinate = new Vector3(i, 0.0f, j);
                    FieldCell cell = _cellSpawner.SpawnCell(cordinate);
                    cell.initialize(true, i, j, i - j); /* モック用:パラメータ設定 */
                    _gameField.FieldCells.Add(cordinate, cell);
                }
            }

            foreach (var fieldCell in _gameField.FieldCells.Values)
            {
                Vector3 cellCordinate = fieldCell.transform.position;
                if (_gameField.FieldCells.ContainsKey(cellCordinate + Vector3.right))
                {
                    fieldCell.AddNeighborCell(_gameField.FieldCells[cellCordinate + Vector3.right]);
                }
                if (_gameField.FieldCells.ContainsKey(cellCordinate + Vector3.left))
                {
                    fieldCell.AddNeighborCell(_gameField.FieldCells[cellCordinate + Vector3.left]);
                }
                if (_gameField.FieldCells.ContainsKey(cellCordinate + Vector3.forward))
                {
                    fieldCell.AddNeighborCell(_gameField.FieldCells[cellCordinate + Vector3.forward]);
                }
                if (_gameField.FieldCells.ContainsKey(cellCordinate + Vector3.back))
                {
                    fieldCell.AddNeighborCell(_gameField.FieldCells[cellCordinate + Vector3.back]);
                }
            }
        }
    }

    public void initializeUnit()
    {
        if (_maleUnitPrefab != null)
        {
            Vector3 cordinate;
            Vector3 temp = new Vector3(0f, GameFieldData.CharacterDiff, 0f);

            for (int i = 0; i < _commonParam.UnitNum; i++)
            {
                cordinate = new Vector3(i, GameFieldData.CharacterDiff, 0f);
                Unit playerUnit = _unitSpawner.SpawnUnit(cordinate, Quaternion.identity, UnitSpawner.UnitType.Male);
                if (_gameField.FieldCells.ContainsKey(cordinate - temp)) {
                    FieldCell cell = _gameField.FieldCells[cordinate - temp];
                    
                    int realAttackVal = PlayerUnitAttackVal + (int)Random.Range((-1 * RandomVal), (RandomVal)); /* -5～5の間にしたい */
                    int  realDefenseVal = PlayerUnitDefenseVal + (int)Random.Range((-1 * RandomVal), (RandomVal));

                    playerUnit.Initialize("Player"+i.ToString(), 100f, realAttackVal, realDefenseVal, 5, 1, Unit.Team.Player, cell);
                    _gameField.PlayerUnits.Add(playerUnit);
                }
            }
            for (int i = 0; i < _commonParam.UnitNum; i++)
            {
                cordinate = new Vector3(FieldWidth - 1 - i, GameFieldData.CharacterDiff, FieldLength - 1);
                Unit enemyUnit = _unitSpawner.SpawnUnit(cordinate, new Quaternion(0,180,0,0), UnitSpawner.UnitType.Female);
                if (_gameField.FieldCells.ContainsKey(cordinate - temp))
                {
                    FieldCell cell = _gameField.FieldCells[cordinate - temp];

                    int realAttackVal = _enemyAttackNum + (int)Random.Range((-1 * RandomVal), (RandomVal));
                    int realDefenseVal = EnemyUnitDefenseVal + (int)Random.Range((-1 * RandomVal), (RandomVal));
                    
                    enemyUnit.Initialize("Enemy" + i.ToString(), 100f, realAttackVal, realDefenseVal, 5, 1, Unit.Team.Enemy, cell);
                    _gameField.EnemyUnits.Add(enemyUnit);
                }
            }
        }
    }

    public void FinishGame(Unit.Team winTeam)
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
