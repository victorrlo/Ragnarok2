using System.Collections.Generic;
using UnityEngine;

public class ShortcutManager : MonoBehaviour
{
    private int _itemsObtained = 0;
    [SerializeField] private GameObject _itemShortcutPrefab;
    [SerializeField] private GameObject _skillShortcutPrefab;
    [SerializeField] private Sprite _appleItem;
    [SerializeField] private Sprite[] _numbersSprites;
    private Dictionary<int, Sprite> _numbersDictionary;


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
        GameObject itemShortcut = Instantiate(_itemShortcutPrefab, parentObject);

        GameObject skillShortcut = Instantiate(_skillShortcutPrefab, parentObject);
    }

    private Dictionary<int, Sprite> CreateIndexOfNumbers()
    {
        Dictionary<int, Sprite> dictionary = new Dictionary<int, Sprite>();
        
        int index = 1;
        foreach (var number in _numbersSprites)
        {
            dictionary.Add(index, number);

            index++;
        }

        return dictionary;
    }

    public void DefineShortcutNumber(int number)
    {
        _numbersDictionary = CreateIndexOfNumbers();

        foreach (var item in _numbersDictionary)
        {
            if (item.Key == number)
            {
                Debug.Log($"Number is {number} and key is {item.Key}");
            }
        }
    }
}
