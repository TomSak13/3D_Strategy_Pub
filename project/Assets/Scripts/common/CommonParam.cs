using UnityEngine;

[CreateAssetMenu(fileName = "CommonData", menuName = "ScriptableObjects/CommonParam", order = 1)]
public class CommonParam : ScriptableObject
{
    [SerializeField] private TitleData.Difficulty _gameDifficulty;
    [SerializeField] private int _unitNum;

    public TitleData.Difficulty GameDifficulty { get => _gameDifficulty; set => _gameDifficulty = value; }

    public int UnitNum { get => _unitNum; set => _unitNum = value; }
}
