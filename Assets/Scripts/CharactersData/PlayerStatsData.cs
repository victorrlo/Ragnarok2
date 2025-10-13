using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsData", menuName = "Scriptable Objects/PlayerStatsData")]
public class PlayerStatsData : CharacterStatsData
{
    [Header("Current Stats")]
    public float _currentHP;
    public float _currentSP;

    public void ResetStats()
    {
        _currentHP = MaxHP;
        _currentSP = MaxSP;
    }

    public float GetCurrentHP()
    {
        return _currentHP;
    }

    public float GetCurrentSP()
    {
        return _currentSP;
    }

    public void LoadFromPrefs()
    {
        _currentHP = PlayerPrefs.GetFloat("PlayerHP", _currentHP);
        _currentSP = PlayerPrefs.GetFloat("PlayerSP", _currentSP);
    }

    public void SaveToPrefs()
    {
        PlayerPrefs.SetFloat("PlayerHP", _currentHP);
        PlayerPrefs.SetFloat("PlayerSP", _currentSP);
        PlayerPrefs.Save();
    }
}
