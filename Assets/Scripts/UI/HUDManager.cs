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

        PlayerStatsManager.Instance.OnHPChanged += UpdateHP;
        PlayerStatsManager.Instance.OnSPChanged += UpdateSP;
    }

    private void OnDestroy()
    {
        PlayerStatsManager.Instance.OnHPChanged -= UpdateHP;
        PlayerStatsManager.Instance.OnSPChanged -= UpdateSP;
    }

    public void UpdateHP(float current, float max)
    {
        _hpText.text = $"HP: {current}/{max}";
    }

    public void UpdateSP(float current, float max)
    {
        _spText.text = $"SP: {current}/{max}";
    }
}
