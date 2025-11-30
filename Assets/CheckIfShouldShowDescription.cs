using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CheckIfShouldShowDescription : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _skillDescriptionObject;
    [SerializeField] private Skill _skill;
    [SerializeField] private TextMeshProUGUI _description;
    [SerializeField] private TextMeshProUGUI _spCost;
    private void Awake()
    {
        _skillDescriptionObject.gameObject.SetActive(false);
    }

    private void Start()
    {
        _description.text = _skill.Description;
        _spCost.text = $"Custa {_skill.SpCost.ToString()} de SP.";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _skillDescriptionObject.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _skillDescriptionObject.gameObject.SetActive(false);
    }
}
