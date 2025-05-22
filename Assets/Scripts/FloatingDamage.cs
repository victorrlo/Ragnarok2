using TMPro;
using UnityEngine;

public class FloatingDamage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _damageText;

    public void ShowDamage(int damage)
    {
        _damageText.text = damage.ToString();
        _damageText.transform.position = transform.position;
    }
}
