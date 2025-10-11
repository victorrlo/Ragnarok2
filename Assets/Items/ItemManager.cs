using System.Collections;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    private Vector3Int _currentPosition;
    private PlayerEventBus _eventBus;
    private PlayerContext _context;
    private Coroutine _getItemCoroutine;
    private GameObject _player;

    public static ItemManager Instance {get; private set;}
    public int applesObtained = 0;

    private bool _canGetItem = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);



        if (_context == null)
            TryGetComponent<PlayerContext>(out _context);
            
        if (_eventBus == null)
            _context.TryGetComponent<PlayerEventBus>(out _eventBus);
    }

    private void OnEnable()
    {
        _eventBus.OnGetItem += IsPlayerNearIt;
    }

    private void OnDisable()
    {
        _eventBus.OnGetItem -= IsPlayerNearIt;
        StopGettingItem();
    }

    private void IsPlayerNearIt(GameObject item)
    {
        // Debug.Log($"IsPlayerNearIt? {item.GetComponent<ItemDataLoader>().Name}");
        StopGettingItem();
        _getItemCoroutine = StartCoroutine(TryGetItem(item.transform));
    }

    private IEnumerator TryGetItem(Transform target)
    {
        while(true)
        {
            // Debug.Log("Try get item");
            Vector3Int playerPos = GridManager.Instance.WorldToCell(transform.position);
            Vector3Int itemPos = GridManager.Instance.WorldToCell(target.position);

            if (DistanceHelper.IsInAttackRange(playerPos, itemPos, _context.Stats.AttackRange))
            {
                // Debug.Log("Can get item!");
                _canGetItem = true;
                PickItem(target.gameObject);
                yield break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private void PickItem(GameObject item)
    {
        if (!_canGetItem) return;
        
        var itemName = item.GetComponent<ItemDataLoader>().Name;
        
        if (itemName == ItemName.Apple)
        {
            applesObtained++;
        }

        Destroy(item);
    }

    private void StopGettingItem()
    {
        if (_getItemCoroutine != null)
        {
            StopCoroutine(_getItemCoroutine);
            _getItemCoroutine = null;
        }

        _canGetItem = false;
    }
}
