using System;

public class UIData
{
    public class CharacterUIParam
    {
        public string Name = "";
        public int Hp = 0;
        public int Move = 0;
    }

    public event Action UICharacterParamChanged = default!;

    private CharacterUIParam _playerCharacter;
    private CharacterUIParam _enemyCharacter;

    public CharacterUIParam PlayerCharacter { get => _playerCharacter; }
    public CharacterUIParam EnemyCharacter { get => _enemyCharacter; }

    public UIData()
    {
        _playerCharacter = new CharacterUIParam();
        _enemyCharacter = new CharacterUIParam();
    }

    public void UpdateCharacterParam(Unit unit)
    {
        if (unit == null)
        {
            return;
        }

        if (unit.UnitTeam == Unit.Team.Player)
        {
            _playerCharacter.Name = unit.Name;
            _playerCharacter.Hp = (int)unit.CurrentHp;
            _playerCharacter.Move = unit.Move;
        }
        else
        {
            _enemyCharacter.Name = unit.Name;
            _enemyCharacter.Hp = (int)unit.CurrentHp;
            _enemyCharacter.Move = unit.Move;
        }

        UpdateCallback();
    }

    private void UpdateCallback()
    {
        UICharacterParamChanged?.Invoke();
    }
}
