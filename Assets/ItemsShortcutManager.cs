using UnityEngine;

public class ItemsShortcutManager : MonoBehaviour
{
    private int _itemsObtained = 0;
    [SerializeField] private GameObject _baseShortcutPrefab;
    [SerializeField] private Sprite _numberOne;
    [SerializeField] private Sprite _numberTwo;
    [SerializeField] private Sprite _appleItem;


    private void Awake()
    {
        if (_itemsObtained < 1)
        {
            SetShortcutStructure(_itemsObtained);
        }
    }

    private void SetShortcutStructure(int numberOfItemsObtained)
    { 
        // number of shortcuts should be 1 if 0 items were obtained
        //  and the limit is 10. (buttons 1,2,3,4,5,6,7,8,9,0 in the keyboard).
        
        int numberOfShortcuts = 0;


        if (numberOfItemsObtained <= 1)
        {
            numberOfShortcuts = 1;
        }

        var parentObject = this.transform;
        GameObject newObject = Instantiate(_baseShortcutPrefab, parentObject);
    }
}
