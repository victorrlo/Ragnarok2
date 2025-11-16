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

    public void PickItem(GameObject item)
    {
        
        var itemName = item.GetComponent<ItemDataLoader>().Name;
        
        if (itemName == ItemName.Apple)
        {
            if (ItemController.Instance.MaxApplesObtained < ItemController.Instance.MaxApples) 
            {
                ItemController.Instance.MaxApplesObtained++;
            }

            if (ItemController.Instance.Apples < ItemController.Instance.MaxApples)
            {
                ItemController.Instance.Apples++;
            }
        }

        Destroy(item);
        _context.Control.ChangeState(new IdleState());
    }
}
