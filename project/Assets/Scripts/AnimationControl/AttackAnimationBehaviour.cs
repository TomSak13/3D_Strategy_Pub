using UnityEngine;

public class AttackAnimationBehaviour : StateMachineBehaviour
{
    public bool IsInAnim { get; private set; }
    public Unit _unit = default!;

    public void Initialize(Unit unit)
    {
        IsInAnim = false;
        _unit = unit;
    }

    public void StartAnimation()
    {
        IsInAnim = true;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        IsInAnim = false;
        _unit.NotifyFinishAttack();
    }
}
