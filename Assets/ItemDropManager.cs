using UnityEngine;

public class ItemDropManager : MonoBehaviour
{
    [SerializeField] private GameObject _itemDropPrefab;

    private static ItemDropManager _instance;
    public static ItemDropManager Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        DontDestroyOnLoad(this);
    }

    public void DropItems(MonsterData monsterData, Vector3 monsterPosition)
    {
        if (monsterData.PossibleDrops.Count == 0) return;

        Vector3Int monsterCell = GridManager.Instance.WorldToCell(monsterPosition);
        Vector3 cellCenter = GridManager.Instance.GetCellCenterWorld(monsterCell);

        foreach (var dropData in monsterData.PossibleDrops)
        {
            if (ShouldDropItem(dropData.dropChance))
            {
                int quantity = dropData.maxQuantity;

                for (int i = 0; i < quantity; i++)
                {
                    SpawnItemDrop(dropData.item, monsterCell, cellCenter);
                }
            }
        }
    }

    private bool ShouldDropItem(float dropChance)
    {
        return Random.Range(0f, 1f) <= dropChance;
    }

    private void SpawnItemDrop(Item item, Vector3Int cell, Vector3 cellCenter)
    {
        if (_itemDropPrefab == null || item == null) return;

        Vector3 randomPositionInCell = GetRandomPositionInCell(cellCenter);

        // create the drop slightly above the monster for the jump effect
        Vector3 spawnPosition = cellCenter + Vector3.up * 0.5f;

        GameObject dropObject = Instantiate(_itemDropPrefab, spawnPosition, _itemDropPrefab.transform.rotation);

        ItemDrop itemDrop = dropObject.GetComponent<ItemDrop>();

        if (itemDrop != null)
        {
            itemDrop.InitializeDrop(spawnPosition, randomPositionInCell);
        }
        else
        {
            dropObject.transform.position = randomPositionInCell;
        }
    }

    private Vector3 GetRandomPositionInCell(Vector3 cellCenter)
    {
        float cellHalfSize = 0.4f;

        float randomX = Random.Range(-cellHalfSize, cellHalfSize);
        float randomY = Random.Range(-cellHalfSize, cellHalfSize);

        return cellCenter + new Vector3(randomX, 0, randomY);
    }
}
