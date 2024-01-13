using System;

public class UIData
{
    public class CharacterUIParam
    {
        public CharacterUIParam()
        {
            Name = "";
            Hp = 0;
            Move = 0;
        }
        public string Name;
        public int Hp;
        public int Move;
    }

    public event Action UICharacterParamChanged;

    private CharacterUIParam _playerCharacter;
    private CharacterUIParam _enemyCharacter;

    public CharacterUIParam PlayerCharacter { get => _playerCharacter; }
    public CharacterUIParam EnemyCharacter { get => _enemyCharacter; }

    public void Initialize()
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

    public void UpdateCallback()
    {
        UICharacterParamChanged.Invoke();
    }
}
