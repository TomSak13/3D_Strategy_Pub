using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum Team
    {
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

    private float _maxHp;

    private Animator _animator = default!;
    private AttackAnimationBehaviour _attackAnimationBehaviour = default!;

    private UnitController _unitController = default!;

    public string Name { get; set; } = default!;
    public float CurrentHp { get; set; }
    public float Attack { get; private set; }
    public float Defense { get; private set; }
    public int Move { get; private set; }
    public int AttackRange { get; private set; }
    public FieldCell OnCell { get; private set; } = default!;
    public Team UnitTeam { get; private set; }
    public ActionState State { get; set; }
    public ActControlState ControlState { get; set; }
    public bool IsInRunAnim { get; private set; }
    public bool IsTurnFinished { get; private set; }

    public float GetCurrentHpRatio()
    {
        return (CurrentHp / _maxHp);
    }

    public void EraseGameobj()
    {
        gameObject.SetActive(false);
        OnCell.RemoveUnit();
        Destroy(this);
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
        if (State != ActionState.Defense)
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

        IsInRunAnim = true;
        _animator.SetBool(_runAnimatorHash, true);
    }

    private void StopRunAnimation()
    {
        if (_animator == null)
        {
            return;
        }

        IsInRunAnim = false;
        _animator.SetBool(_runAnimatorHash, false);
    }

    public void InitializeTurn()
    {
        if (State == ActionState.Defense) // 防御戦略時は次のターン開始時にHP５％回復
        {
            int recoverHp = (int)(_maxHp * 0.05);
            CurrentHp += recoverHp;
            if (CurrentHp >= _maxHp)
            {
                CurrentHp = _maxHp;
            }
        }

        State = ActionState.None;
        IsTurnFinished = false;
    }

    public void EndTurn()
    {
        IsTurnFinished = true;
    }

    public void InDefense()
    {
        State = ActionState.Defense;
        NotifyFinishAction();
    }

    public void Initialize(string name, float maxHp, float attack, float defense, int move, int attackRange, Team team, FieldCell fieldCell, UnitController unitController)
    {
        Name = name;
        _maxHp = maxHp;
        CurrentHp = _maxHp;
        Attack = attack;
        Defense = defense;
        Move = move;
        AttackRange = attackRange;

        State = ActionState.None;
        ControlState = ActControlState.None;
        IsTurnFinished = false;

        _unitController = unitController;

        UnitTeam = team;

        OnCell = fieldCell;
        if (OnCell != null)
        {
            OnCell.SetOnUnit(this);
        }

        _animator = GetComponent<Animator>();
        if (_animator != null)
        {
            _attackAnimationBehaviour = _animator.GetBehaviour<AttackAnimationBehaviour>();
            _attackAnimationBehaviour.Initialize(this);
        }

        IsInRunAnim = false;
    }

    /* 移動アニメーション */
    IEnumerator MovePositionAnim(List<FieldCell> routeList)
    {
        Vector3 delta;

        StartRunAnimation();

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
        NotifyFinishAction();

        yield return null;
    }

    public void MovePosition(FieldCell destinationCell, List<FieldCell> routeList, bool isAnim)
    {
        /* 移動アニメーション */
        if (isAnim)
        {
            StartCoroutine(MovePositionAnim(routeList));
        }
        else
        {
            transform.position = destinationCell.transform.position + (Vector3.up * GameFieldData.CharacterDiff);
            NotifyFinishAction();
        }
        /* 配置セル削除 */
        OnCell.RemoveUnit();

        /* セル移動 */
        OnCell = destinationCell;
        OnCell.SetOnUnit(this);
    }

    public void NotifyFinishAction()
    {
        _unitController.CallbackFinishAct(this);
    }

    public void NotifyFinishAttack()
    {
        _unitController.CallbackFinishAttack();
    }
}
