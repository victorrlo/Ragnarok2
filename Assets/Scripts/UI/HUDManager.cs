using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance {get; private set;}
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _spText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }   
    }

    public void UpdateHUD(int currentHp, int maxHp, int currentSp, int maxSp)
    {
        _hpText.text = $"HP: {currentHp}/{maxHp}";
        _spText.text = $"SP: {currentSp}/{maxSp}";
    }
}
