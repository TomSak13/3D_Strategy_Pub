using System.Collections;
using UnityEngine;

public class Battle : MonoBehaviour
{
    private const float RandomRate = 0.25f;
    private const float MinAttackVal = 5f;
    public enum BattleState
    {
        None,
        Attack,
        CounterAttack
    }

    [SerializeField] private UIPresenter _uiPresenter = default!;
    [SerializeField] private GameFieldData _field = default!;
    [SerializeField] private MainCamera _camera = default!;

    private Unit _attackCharacter = default!;
    private Unit _defenseCharacter = default!;

    public BattleState State { get; private set; }


    private void Start()
    {
        State = BattleState.None;
    }

    private void DefeatUnit(GameFieldData field, Unit unit)
    {
        if (field == null || unit == null)
        {
            return;
        }
        field.RemoveUnits.Add(unit);
        unit.EraseGameobj();
    }

    private bool IsInAttackRange(GameFieldData field, Unit attackCharacter, Unit defenseCharacter)
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

        if (!IsInAttackRange(_field, attackCharacter, defenseCharacter))
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

        float realAttackVal = (attackCharacter.Attack + attackCharacter.OnCell.Effect.AttackEffect) -
                         (defenseCharacter.Defense + defenseCharacter.OnCell.Effect.DefenseEffect);

        if (realAttackVal <= 0)
        {
            realAttackVal = MinAttackVal;
        }

        float randVal = Random.Range((-1 * RandomRate * realAttackVal), (RandomRate * realAttackVal));

        int realDamage = (int)(realAttackVal + randVal);

        if (defenseCharacter.State == Unit.ActionState.Defense)
        {
            realDamage /= 2; /* 防御時はダメージを1/2にする */
        }

        return realDamage;
    }

    public void StartBattle()
    {
        State = BattleState.Attack;
        StartAttack(_attackCharacter, _defenseCharacter);
        _uiPresenter.UpdateBattleResult(_attackCharacter, _defenseCharacter);
    }

    private void StartAttack(Unit attackUnit, Unit defenseUnit)
    {
        if (_camera != null)
        {
            _camera.MoveCursor(attackUnit);
        }
        attackUnit.StartAttackAnimation(defenseUnit);

        defenseUnit.StartDamagedAnimation(attackUnit);
    }

    public void FinishAttack()
    {
        switch (State)
        {
            case BattleState.Attack:
                _defenseCharacter.CurrentHp -= CalcDamage(_attackCharacter, _defenseCharacter);
                if (_defenseCharacter.CurrentHp > 0 && IsInAttackRange(_field, _defenseCharacter, _attackCharacter) && _defenseCharacter.State != Unit.ActionState.Defense)
                {
                    State = BattleState.CounterAttack; /* 攻撃範囲内にいれば反撃 */
                    StartAttack(_defenseCharacter, _attackCharacter);
                }
                else
                {
                    FinishBattle();
                }
                break;
            case BattleState.CounterAttack:
                _attackCharacter.CurrentHp -= CalcDamage(_defenseCharacter, _attackCharacter);
                FinishBattle();
                break;
            default:
                break;
        }

        _uiPresenter.UpdateBattleResult(_attackCharacter, _defenseCharacter);
    }

    private void FinishBattle()
    {
        if (_attackCharacter.CurrentHp <= 0)
        {
            DefeatUnit(_field, _attackCharacter);
        }
        if (_defenseCharacter.CurrentHp <= 0)
        {
            DefeatUnit(_field, _defenseCharacter);
        }

        State = BattleState.None;
        StartCoroutine(NotifyFinishBattle());
    }

    IEnumerator NotifyFinishBattle()
    {
        yield return new WaitForSeconds(0.5f); // 結果をユーザーに見せるため待つ

        _attackCharacter.NotifyFinishAction();

        yield return null;
    }
}
