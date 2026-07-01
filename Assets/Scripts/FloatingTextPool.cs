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

    public void ShowAccumulatingDamage(Vector3 worldPos, IReadOnlyList<int> accumulatedAmounts, Color color)
    {
        if (accumulatedAmounts == null || accumulatedAmounts.Count == 0)
            return;

        if (_damageTextPool.Count == 0)
            CreateNewDamageText();

        var text = _damageTextPool.Dequeue();
        text.transform.position = worldPos + new Vector3(0f, 0.4f, 0f);
        text.Initialize(accumulatedAmounts[0], color, 2f, 1f, 0.8f, ReturnDamageTextToPool);

        StartCoroutine(UpdateAccumulatingDamageText(text, accumulatedAmounts));
    }

    private IEnumerator UpdateAccumulatingDamageText(FloatingDamageText text, IReadOnlyList<int> accumulatedAmounts)
    {
        const float updateDelay = 0.25f;

        for (int i = 1; i < accumulatedAmounts.Count; i++)
        {
            yield return new WaitForSeconds(updateDelay);

            if (text == null || !text.gameObject.activeInHierarchy)
                yield break;

            text.SetAmount(accumulatedAmounts[i]);
        }
    }

    public void ShowHeal(Vector3 worldPos, float amount)
    {
        if (_damageTextPool.Count == 0)
            CreateNewHealText();

        var text = _healTextPool.Dequeue();
        text.transform.position = worldPos;
        text.Initialize(1.2f, 0.5f, ReturnHealTextToPool, Color.green, amount: amount);
    }

    public void ShowFailMessage(Vector3 worldPos)
    {
        if (_damageTextPool.Count == 0)
            CreateNewHealText();

        var text = _healTextPool.Dequeue();
        text.transform.position = worldPos;
        text.Initialize(1.2f, 0.5f, ReturnHealTextToPool, Color.red, text: "FAIL");
    }

    public void ShowSPRecovery(Vector3 worldPos, float amount)
    {
        if (_damageTextPool.Count == 0)
            CreateNewHealText();

        var text = _healTextPool.Dequeue();
        text.transform.position = worldPos;
        text.Initialize(1.2f, 0.5f, ReturnHealTextToPool, Color.blue, amount: amount);
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
