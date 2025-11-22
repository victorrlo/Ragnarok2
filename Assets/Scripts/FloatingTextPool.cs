using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FloatingTextPool : MonoBehaviour
{
    public static FloatingTextPool Instance {get; private set;}

    [SerializeField] private GameObject _damageTextPrefab;
    [SerializeField] private GameObject _healTextPrefab;
    [SerializeField] private int _poolSize = 30;

    private Queue<FloatingDamageText> _damageTextPool = new();
    private Queue<FloatingHealText> _healTextPool = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        for (int i = 0; i < _poolSize; i++)
        {
            CreateNewDamageText();
            CreateNewHealText();
        }
    }

    private FloatingDamageText CreateNewDamageText()
    {
        GameObject go = Instantiate(_damageTextPrefab, transform);
        go.SetActive(false);
        var text = go.GetComponent<FloatingDamageText>();
        _damageTextPool.Enqueue(text);
        return text;
    }

    private FloatingHealText CreateNewHealText()
    {
        GameObject go = Instantiate(_healTextPrefab, transform);
        go.SetActive(false);
        var text = go.GetComponent<FloatingHealText>();
        _healTextPool.Enqueue(text);
        return text;
    }

    public void ShowDamage(Vector3 worldPos, int amount, Color color)
    {
        if (_damageTextPool.Count == 0)
            CreateNewDamageText();

        var text = _damageTextPool.Dequeue();
        text.transform.position = worldPos;
        text.Initialize(amount, color, 1.2f, 0.5f, 0.6f, ReturnDamageTextToPool);
    }

    public void ShowHeal(Vector3 worldPos, float amount)
    {
        if (_damageTextPool.Count == 0)
            CreateNewHealText();

        var text = _healTextPool.Dequeue();
        text.transform.position = worldPos;
        text.Initialize(amount, 1.2f, 0.5f, ReturnHealTextToPool, Color.green);
    }

    public void ShowSPRecovery(Vector3 worldPos, float amount)
    {
        if (_damageTextPool.Count == 0)
            CreateNewHealText();

        var text = _healTextPool.Dequeue();
        text.transform.position = worldPos;
        text.Initialize(amount, 1.2f, 0.5f, ReturnHealTextToPool, Color.blue);
    }

    private void ReturnDamageTextToPool(FloatingDamageText text)
    {
        text.gameObject.SetActive(false);
        _damageTextPool.Enqueue(text);
    }

    private void ReturnHealTextToPool(FloatingHealText text)
    {
        text.gameObject.SetActive(false);
        _healTextPool.Enqueue(text);
    }
}
