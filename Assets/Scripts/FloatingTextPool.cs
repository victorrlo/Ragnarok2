using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTextPool : MonoBehaviour
{
    public static FloatingTextPool Instance {get; private set;}

    [SerializeField] private GameObject _damageTextPrefab;
    [SerializeField] private int _poolSize = 30;

    private Queue<FloatingDamageText> _pool = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        for (int i = 0; i < _poolSize; i++)
        {
            CreateNewText();
        }
    }

    private FloatingDamageText CreateNewText()
    {
        GameObject go = Instantiate(_damageTextPrefab, transform);
        go.SetActive(false);
        var text = go.GetComponent<FloatingDamageText>();
        _pool.Enqueue(text);
        return text;
    }

    public void ShowDamage(Vector3 worldPos, int amount, Color color)
    {
        if (_pool.Count == 0)
            CreateNewText();

        var text = _pool.Dequeue();
        text.transform.position = worldPos;
        text.Initialize(amount, color, 1.2f, 0.5f, 0.6f, ReturnToPool);
    }

    private void ReturnToPool(FloatingDamageText text)
    {
        text.gameObject.SetActive(false);
        _pool.Enqueue(text);
    }
}
