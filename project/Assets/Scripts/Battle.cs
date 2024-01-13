using UnityEngine;

public class Battle : MonoBehaviour
{
    public enum BattleState
    {
        None,
        Start,
        Attack,
        CounterAttack,
        Finished
    }

    [SerializeField] private UIPresenter _uiPresenter;
    [SerializeField] private GameFieldData _field;
    [SerializeField] private MainCamera _camera;

    public const float RandomRate = 0.25f;
    private Unit _attackCharacter;
    private Unit _defenseCharacter;

    private BattleState _state;

    public BattleState State { get => _state; }

    private void Start()
    {
        _attackCharacter = null;
        _defenseCharacter = null;

        _state = BattleState.None;
    }

    private void Update()
    {
        if (_uiPresenter == null)
        {
            Debug.Log("attach objs are none:" + System.Reflection.MethodBase.GetCurrentMethod().Name);
            return;
        }
        if (_attackCharacter == null || _defenseCharacter == null)
        {
            return;
        }

        if (_state == BattleState.None)
        {
            return; /* 開始まで待つ */
        }
        if (_attackCharacter.IsInAttackAnim() || _defenseCharacter.IsInAttackAnim())
        {
            return; /* アニメーション中は制御を止める */
        }

        _uiPresenter.UpdateBattleResult(_attackCharacter, _defenseCharacter);

        switch (_state)
        {
            case BattleState.Start:

                _state = BattleState.Attack;
                break;
            case BattleState.Attack:
                if (_camera != null)
                {
                    _camera.MoveCursor(_attackCharacter);
                }
                _attackCharacter.StartAttackAnimation(_defenseCharacter);
                
                _defenseCharacter.CurrentHp -= CalcDamage(_attackCharacter, _defenseCharacter);

                if (_defenseCharacter.CurrentHp > 0 && IsInAttackRange(_field, _defenseCharacter, _attackCharacter) && _defenseCharacter.State != Unit.ActionState.Defense)
                {
                    _state = BattleState.CounterAttack; /* 攻撃範囲内にいれば反撃 */
                }
                else
                {
                    _state = BattleState.Finished;
                }
                _defenseCharacter.StartDamagedAnimation(_attackCharacter);
                break;
            case BattleState.CounterAttack:
                if (_camera != null)
                {
                    _camera.MoveCursor(_defenseCharacter);
                }
                _defenseCharacter.StartAttackAnimation(_attackCharacter);
                
                _attackCharacter.CurrentHp -= CalcDamage(_defenseCharacter, _attackCharacter);
                

                _state = BattleState.Finished;

                _attackCharacter.StartDamagedAnimation(_defenseCharacter);
                break;
            case BattleState.Finished:
                if (_attackCharacter.CurrentHp <= 0)
                {
                    DefeatUnit(_field, _attackCharacter);
                }
                if (_defenseCharacter.CurrentHp <= 0)
                {
                    DefeatUnit(_field, _defenseCharacter);
                }

                _attackCharacter = null;
                _defenseCharacter = null;

                _state = BattleState.None;
                break;
            default:
                break;
        }
    }

    public void DefeatUnit(GameFieldData field, Unit unit)
    {
        if (field == null || unit == null)
        {
            return;
        }
        field.RemoveUnits.Add(unit);
        unit.EraseGameobj();
    }

    public bool IsInAttackRange(GameFieldData field, Unit attackCharacter, Unit defenseCharacter)
    {
        if (field == null || attackCharacter == null || defenseCharacter == null)
        {
            return false;
        }

        for (int i = -attackCharacter.AttackRange; i <= attackCharacter.AttackRange; i++)
        {
            for (int j = -attackCharacter.AttackRange; j <= attackCharacter.AttackRange; j++)
            {
                if (i == 0 && j == 0) 
                {
                    continue;
                }
                if ((Mathf.Abs(i) + Mathf.Abs(j)) > attackCharacter.AttackRange)
                {
                    continue;
                }
                Vector3 targetPosition = attackCharacter.OnCell.transform.position + (Vector3.forward * j) + (Vector3.right * i);
                
                if (targetPosition == defenseCharacter.OnCell.transform.position)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool AssignBattleCharacter(Unit attackCharacter, Unit defenseCharacter)
    {
        if (attackCharacter == null || defenseCharacter == null)
        {
            Debug.Log("battle unit or field is null");
            return false;
        }

        if (attackCharacter.CurrentHp <= 0 || defenseCharacter.CurrentHp <= 0)
        {
            Debug.Log("battle unit HP is 0");
            return false;
        }

        if (!IsInAttackRange(_field ,attackCharacter, defenseCharacter))
        {
            Debug.Log("not under AttackRange");
            return false;
        }

        _attackCharacter = attackCharacter;
        _defenseCharacter = defenseCharacter;

        return true;
    }

    private int CalcDamage(Unit attackCharacter, Unit defenseCharacter)
    {
        if (attackCharacter == null || defenseCharacter == null)
        {
            Debug.Log("battle unit is null");
            return 0;
        }

        float realAttackVal;
        realAttackVal = (attackCharacter.Attack + attackCharacter.OnCell.Effect.AttackEffect) -
                         (defenseCharacter.Defense + defenseCharacter.OnCell.Effect.DefenseEffect);

        if (realAttackVal <= 0)
        {
            realAttackVal = 5f;
        }

        float randVal = Random.Range((-1 * RandomRate * realAttackVal), (RandomRate * realAttackVal));

        int realDamage = (int)(realAttackVal + randVal);

        if (defenseCharacter.State == Unit.ActionState.Defense)
        {
            realDamage = realDamage / 2; /* 防御時はダメージを1/2にする */
        }

        return realDamage;
    }

    public void StartBattle()
    {
        _state = BattleState.Start;
    }
}
