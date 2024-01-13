using UnityEngine;

public class AttackAnimationBehaviour : StateMachineBehaviour
{
    private bool _isInAnim;
    public bool IsInAnim { get => _isInAnim;}

    public void Initialize()
    {
        _isInAnim = false;
    }

    public void StartAnimation()
    {
        _isInAnim = true;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _isInAnim = false;
    }
}
