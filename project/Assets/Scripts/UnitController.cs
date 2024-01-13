using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ユニットを操作する。Presenter(Controller)の役割
/// </summary>
public class UnitController : MonoBehaviour
{
    [SerializeField] private Battle _battle;

    private void Update()
    {
        if (_battle == null)
        {
            Debug.Log("attach objs are none:" + System.Reflection.MethodBase.GetCurrentMethod().Name);
            return;
        }
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
    public bool MoveUnit(GameFieldData field, Unit unit, FieldCell destinationCell, List<RouteSearchNode>[] route,  bool isAnime = true)
    {
        bool ret = false;
        if (destinationCell == null)
        {
            Debug.Log("destinationCell null:");
        }

        if (field.FieldCells.ContainsValue(destinationCell))
        {
            List<FieldCell> routeList = NavigationAI.GetRoute(destinationCell, route);
            
            unit.MovePosition(destinationCell, field, routeList, isAnime);
            ret = true;
        }
        else
        {
            Debug.Log("UnMovable position:" + destinationCell.transform.position);
        }

        return ret;
    }


    public void Battle(Unit attackCharacter, Unit defenseCharacter)
    {
        if (_battle.AssignBattleCharacter(attackCharacter, defenseCharacter))
        {
            _battle.StartBattle();
        }
    }

    public Battle.BattleState GetBattleState()
    {
        return _battle.State;
    }
}
