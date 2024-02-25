
public abstract class ActionBase
{
    protected Unit _actionUnit = default!;

    /// <summary>
    /// 行動の実行
    /// </summary>
    /// <param name="unitController"></param>
    public abstract void Execute(UnitController unitController);
}
