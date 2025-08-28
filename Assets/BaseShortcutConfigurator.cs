using System.Collections.Generic;
using UnityEngine;

public class BaseShortcutConfigurator : MonoBehaviour
{

    [SerializeField] private Sprite[] _numbersSprites;
    private Dictionary<int, Sprite> _numbersDictionary;

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
