using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CheckIfPlayerHasApples : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Consumable _apple;
    [SerializeField] Image _appleSprite;
    [SerializeField] TextMeshProUGUI _quantity;
    [SerializeField] private Image _skillDescriptionObject;
    [SerializeField] private TextMeshProUGUI _description;
    [SerializeField] private TextMeshProUGUI _effectDescription;

    private void FixedUpdate()
    {
        _appleSprite.gameObject.SetActive(true);
        var quantity = ItemController.Instance.Apples;
        _quantity.text = quantity.ToString() + "x";
    }

    private void Awake()
    {
        _description.text = _apple.EnglishDescription;
        var percentage = _apple.healPercent * 100;
        _effectDescription.text = $"Recovers {percentage}% of HP.";
        _skillDescriptionObject.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ItemController.Instance.Apples > 0)
            _skillDescriptionObject.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_skillDescriptionObject.IsActive())
            _skillDescriptionObject.gameObject.SetActive(false);
    }
}
