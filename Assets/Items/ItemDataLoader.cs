using Unity.VisualScripting;
using UnityEngine;

public class ItemDataLoader : MonoBehaviour
{
    [SerializeField] private Item _itemData;
    private SpriteRenderer _spriteRenderer;
    public string Name => _itemData.Name;
    
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        _spriteRenderer.sprite = _itemData.Sprite;

        if (_itemData.Type == ItemType.Consumable)
        {
            Consumable consumable = _itemData as Consumable;
        }
    }
    
    void Update()
    {
        
    }
}
