using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ユニットを操作する。Presenter(Controller)の役割
/// </summary>
public class UnitController
{
    private Battle _battle;

    private CharacterAI _characterAI = default!;

    public UnitController(Battle battle)
    {
        _battle = battle;
    }

    public void ActStart(ActionBase action, CharacterAI characterAI)
    {
        _characterAI = characterAI;
        action.Execute(this);
    }

    public void CallbackFinishAct(Unit unit)
    {
        _characterAI.FinishAct(unit);
    }

    public void CallbackFinishAttack()
    {
        _battle.FinishAttack();
    }

    /// <summary>
    /// 移動先のセルを指定して移動
    /// </summary>
    /// <param name="field"></param>
    /// <param name="unit"></param>
    /// <param name="destinationCell"></param>
    /// <param name="route"></param>
    /// <param name="isAnime"></param>
    /// <returns></returns>
    public void MoveUnit(GameFieldData field, Unit unit, FieldCell destinationCell, RouteSearchNode[][] route, bool isAnime = true)
    {
        if (destinationCell == null)
        {
            Debug.Log("destinationCell null:");
        }

        if (field.FieldCells.ContainsKey(destinationCell!.gameObject.transform.position))
        {
            List<FieldCell> routeList = NavigationAI.GetRoute(destinationCell, route);

            unit.MovePosition(destinationCell, routeList, isAnime);
        }
        else
        {
            Debug.Log("UnMovable position:" + destinationCell.transform.position);
        }
    }

    public void Battle(Unit attackCharacter, Unit defenseCharacter)
    {
        if (_battle.AssignBattleCharacter(attackCharacter, defenseCharacter))
        {
            _battle.StartBattle();
        }
        else
        {
            attackCharacter.NotifyFinishAction();
        }
    }
}
