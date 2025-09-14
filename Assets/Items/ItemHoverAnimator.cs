using UnityEngine;

public class ItemHoverAnimator : MonoBehaviour
{
    [SerializeField] private Canvas _itemCanvas;
    private void Awake()
    {
        if (_itemCanvas == null)
            gameObject.TryGetComponent<Canvas>(out _itemCanvas);
            
        _itemCanvas.gameObject.SetActive(false);
    }

    void OnMouseEnter()
    {
        _itemCanvas.gameObject.SetActive(true);
    }

    void OnMouseExit()
    {
        _itemCanvas.gameObject.SetActive(false);
    }
}
