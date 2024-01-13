using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum Team {
        Player,
        Enemy
    }

    public enum ActionState
    {
        None,
        Defense,
    }

    public enum ActControlState
    {
        None,
        ThinkStrategy,
        StartAct,
        IsInAct,
        Finished
    }

    private static readonly int _runAnimatorHash = Animator.StringToHash("isRun");
    private static readonly int _attackAnimatorHash = Animator.StringToHash("Attack");
    private static readonly int _damagedAnimatorHash = Animator.StringToHash("Damaged");
    private static readonly int _defenseAnimatorHash = Animator.StringToHash("Defense");

    private string _name;
    private float _maxHp;
    private float _currentHp;
    private float _attack;
    private float _defense;
    private int _move;
    private int _attackRange;

    private bool _isFinishedTurn;
    private FieldCell _onCell;
    private Team _unitTeam;
    private ActionState _state;
    private ActControlState _controlState;

    private Animator _animator;
    private AttackAnimationBehaviour _attackAnimationBehaviour;
    private bool _isinRunAnim;

    public string Name { get => _name; set => _name = value; }
    public float CurrentHp { get => _currentHp; set => _currentHp = value; }
    public float Attack { get => _attack; }
    public float Defense { get => _defense; }
    public int Move { get => _move; }
    public int AttackRange { get => _attackRange; }
    public FieldCell OnCell { get => _onCell; }
    public Team UnitTeam { get => _unitTeam; }
    public ActionState State { get => _state; set => _state = value; }

    public ActControlState ControlState { get => _controlState; set => _controlState = value; }

    public bool IsInRunAnim { get => _isinRunAnim; }
    public bool IsTurnFinished { get => _isFinishedTurn; }

    public float GetCurrentHpRatio()
    {
        return (_currentHp/ _maxHp);
    }

    public void EraseGameobj()
    {
        gameObject.SetActive(false);
        _onCell.RemoveUnit();
    }
    public bool IsInAttackAnim()
    {
        return _attackAnimationBehaviour.IsInAnim;
    }

    private void RotateToOpponent(Unit opponent)
    {
        if (opponent == null)
        {
            return;
        }
        
        transform.rotation = Quaternion.LookRotation(opponent.transform.position - transform.position, Vector3.up);
    }

    /// <summary>
    /// ダメージを受けた際のアニメーション(防御時はアニメーションなし)。こちらは攻撃アニメーションより短いので同期は取らない
    /// </summary>
    public void StartDamagedAnimation(Unit opponent)
    {
        if (_state != ActionState.Defense)
        {
            _animator.SetTrigger(_damagedAnimatorHash);
        }
        else
        {
            RotateToOpponent(opponent);
            _animator.SetTrigger(_defenseAnimatorHash);
        }
    }

    public void StartAttackAnimation(Unit opponent)
    {
        if (_attackAnimationBehaviour == null || _animator == null)
        {
            return;
        }
        RotateToOpponent(opponent);


        _attackAnimationBehaviour.StartAnimation();
        _animator.SetTrigger(_attackAnimatorHash);
    }

    private void StartRunAnimation()
    {
        if (_animator == null)
        {
            return;
        }
        _isinRunAnim = true;
        _animator.SetBool(_runAnimatorHash,true);
    }

    private void StopRunAnimation()
    {
        if (_animator == null)
        {
            return;
        }
        _isinRunAnim = false;
        _animator.SetBool(_runAnimatorHash, false);
    }

    public void InitializeTurn()
    {
        if (_state == ActionState.Defense) // 防御戦略時は次のターン開始時にHP５％回復
        {
            int recoverHp = (int)(_maxHp * 0.05);
            _currentHp += recoverHp;
            if (_currentHp >= _maxHp)
            {
                _currentHp = _maxHp;
            }
        }
        _state = ActionState.None;
        _isFinishedTurn = false;
    }

    public void EndTurn()
    {
        _isFinishedTurn = true;
    }

    public void InDefense()
    {
        _state = ActionState.Defense;
    }

    public void Initialize(string name, float maxHp, float attack, float defense, int move, int attackRange, Team team, FieldCell fieldCell)
    {
        _name = name;
        _maxHp = maxHp;
        _currentHp = _maxHp;
        _attack = attack;
        _defense = defense;
        _move = move;
        _attackRange = attackRange;

        _state = ActionState.None;
        _controlState = ActControlState.None;
        _isFinishedTurn = false;

        _unitTeam = team;

        _onCell = fieldCell;
        if (_onCell != null)
        {
            _onCell.SetOnUnit(this);
        }
        _animator = GetComponent<Animator>();
        if (_animator != null) {
            _attackAnimationBehaviour = _animator.GetBehaviour<AttackAnimationBehaviour>();
            _attackAnimationBehaviour.Initialize();
        }

        _isinRunAnim = false;
    }

    /* 移動アニメーション */
    IEnumerator MovePositionAnim(List<FieldCell> routeList)
    {
        Vector3 delta;

        foreach (FieldCell route in routeList)
        {
            if (route.transform.position.z == (int)transform.position.z)
            {
                while (Mathf.Abs(route.transform.position.x - transform.position.x) >= Mathf.Epsilon)
                {
                    transform.position = Vector3.MoveTowards(transform.position,
                                                             new Vector3(route.transform.position.x, transform.position.y, transform.position.z),
                                                             10f * Time.deltaTime); // (現在地, 目標地, 速度)
                    /* 進行方向を向く */
                    delta = new Vector3(route.transform.position.x - transform.position.x, 0, 0);
                    if (delta.magnitude != 0)
                    {
                        transform.rotation = Quaternion.LookRotation(delta, Vector3.up);
                    }
                    yield return null;
                }
            }
            else
            {
                while (Mathf.Abs(route.transform.position.z - transform.position.z) >= Mathf.Epsilon)
                {
                    transform.position = Vector3.MoveTowards(transform.position,
                                                             new Vector3(transform.position.x, transform.position.y, route.transform.position.z),
                                                             10f * Time.deltaTime); // (現在地, 目標地, 速度)

                    /* 進行方向を向く */
                    delta = new Vector3(0, 0, route.transform.position.z - transform.position.z);
                    if (delta.magnitude != 0)
                    {
                        transform.rotation = Quaternion.LookRotation(delta, Vector3.up);
                    }
                    yield return null;
                }
            }
        }

        StopRunAnimation();

        yield return null;
    }

    public void MovePosition(FieldCell destinationCell, GameFieldData field, List<FieldCell> routeList, bool isAnim)
    {
        /* モック用:ナビゲーションAIは後々実装のため、目的地のセルへ移動させるだけ */
        /* 移動アニメーション */
        if (isAnim)
        {
            StartRunAnimation();
            StartCoroutine(MovePositionAnim(routeList));
        }
        else
        {
            transform.position = destinationCell.transform.position + (Vector3.up * GameFieldData.CharacterDiff);
        }
        /* 配置セル削除 */
        _onCell.RemoveUnit();

        /* セル移動 */
        _onCell = destinationCell;
        _onCell.SetOnUnit(this);
    }
}
