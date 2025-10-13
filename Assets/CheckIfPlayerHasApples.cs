using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CheckIfPlayerHasApples : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private const int APPLEMAXXING = 7;
    [SerializeField] Image _appleSprite;
    [SerializeField] TextMeshProUGUI _quantity;
    [SerializeField] private Image _skillDescription;

    private void FixedUpdate()
    {
        if (GameController.Instance.MaxApplesObtained > 0)
        {
            _appleSprite.gameObject.SetActive(true);
            var quantity = GameController.Instance.MaxApplesObtained > GameController.Instance.MaxApples ? GameController.Instance.MaxApples : GameController.Instance.Apples;
            _quantity.text = quantity.ToString() + "x";
        }
        else
        {
            _appleSprite.gameObject.SetActive(false);
            _quantity.text = "";
        }
    }

    private void Awake()
    {
        _skillDescription.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameController.Instance.MaxApplesObtained > 0)
            _skillDescription.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (GameController.Instance.MaxApplesObtained > 0)
            _skillDescription.gameObject.SetActive(false);
    }
}
