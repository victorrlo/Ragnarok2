using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsData", menuName = "Scriptable Objects/PlayerStatsData")]
public class PlayerStatsData : CharacterStatsData
{
    [Header("Current Stats")]
    public int _currentHP;
    public int _currentSP;

    public void ResetStats()
    {
        _currentHP = _maxHP;
        _currentSP = _maxSP;
    }

    public int GetCurrentHP()
    {
        return _currentHP;
    }

    public int GetCurrentSP()
    {
        return _currentSP;
    }

    public void LoadFromPrefs()
    {
        _currentHP = PlayerPrefs.GetInt("PlayerHP", _currentHP);
        _currentSP = PlayerPrefs.GetInt("PlayerSP", _currentSP);
    }

    public void SaveToPrefs()
    {
        PlayerPrefs.SetInt("PlayerHP", _currentHP);
        PlayerPrefs.SetInt("PlayerSP", _currentSP);
        PlayerPrefs.Save();
    }
}
