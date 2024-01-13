
public abstract class ActionBase
{
    protected Unit _actionUnit;
    protected bool _isActionStart;

    public bool IsActionStart { get => _isActionStart; }

    /// <summary>
    /// 行動の実行
    /// </summary>
    public virtual void Execute(UnitController unitController)
    {

    }

    public virtual bool IsFinishedAction(UnitController unitController)
    {
        return true;
    }

}
