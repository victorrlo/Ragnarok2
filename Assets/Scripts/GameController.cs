using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private PlayerStatsData _playerStatsData;
    public void OnApplicationQuit()
    {
        _playerStatsData.SaveToPrefs();
    }
}
