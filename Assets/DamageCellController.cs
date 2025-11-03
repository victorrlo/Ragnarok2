using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DamageCellController : MonoBehaviour
{
    public static DamageCellController Instance {get; private set;}
    [SerializeField] GameObject _monsterDamageCell;
    [SerializeField] GameObject _playerDamageCell;
    [SerializeField] private Tilemap _walkableTilemap;
    private Dictionary<GameObject, List<GameObject>> _activeDamageCells = new Dictionary<GameObject, List<GameObject>>();
    private Grid _grid;
    private List<Vector3Int> _allWalkableTilePositions = new List<Vector3Int>();
    public Action<GameObject, Skill> InvokeDamageCells;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        AssertReferences();
        DefineAllDamageableCells();
    }

    private void Start()
    {
        InvokeDamageCells += CastDamageCellsOnGround;
    }

    private void OnDestroy()
    {
        InvokeDamageCells -= CastDamageCellsOnGround;
    }

    private async void CastDamageCellsOnGround(GameObject caster, Skill skill)
    {
        var damageCell = _monsterDamageCell;
        
        
        if (caster.tag == "Player")
            damageCell = _playerDamageCell;

        var cellsAffected = DefineRangeOfCells(caster, skill);
        var spawnedCells = new List<GameObject>();

        foreach (var cell in cellsAffected)
        {
            Vector3 worldPosition = _walkableTilemap.GetCellCenterWorld(cell);
            var newCell = Instantiate(damageCell, worldPosition, damageCell.transform.rotation);
            spawnedCells.Add(newCell);
        }


        if (_activeDamageCells.ContainsKey(caster))
            _activeDamageCells[caster].AddRange(spawnedCells);
        else
            _activeDamageCells[caster] = spawnedCells;

        await RemoveDamageCells(caster, skill.CastingTime);
    }

    private async UniTask RemoveDamageCells(GameObject caster, float castingTime)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(castingTime));
        
        foreach (var cell in _activeDamageCells[caster])
        {
            Destroy(cell);
        }
    }

    private List<Vector3Int> DefineRangeOfCells(GameObject caster, Skill skill)
    {
        var startingPosition = GridManager.Instance.WorldToCell(caster.transform.position);
        List<Vector3Int> cellsAffected = new List<Vector3Int>();
        
        if (skill.SkillType == Skill.Type.AreaOfEffect)
        {
            cellsAffected.Add(startingPosition);

            for (int x = -skill.Range; x <= skill.Range; x++)
            {
                for (int y = -skill.Range; y <= skill.Range; y++)
                {
                    Vector3Int potentialPosition = startingPosition + new Vector3Int(x, y, 0);

                    if (_allWalkableTilePositions.Contains(potentialPosition))
                    {
                        if (!cellsAffected.Contains(potentialPosition))
                            cellsAffected.Add(potentialPosition);
                    }
                }
            }       
        }
        return cellsAffected;
    }

    private void DefineAllDamageableCells()
    {
        _grid = GridManager.Instance;

        _allWalkableTilePositions.Clear();

        BoundsInt bounds = _walkableTilemap.cellBounds;

        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            if (_walkableTilemap.HasTile(position))
            {
                _allWalkableTilePositions.Add(position);
            }
        }
    }

    private void AssertReferences()
    {
        if (_walkableTilemap == null) 
        {
            Debug.LogError("Walkable Tilemap not found!");
            return;
        }

        if (_monsterDamageCell == null)
        {
            Debug.LogError("Damage Cell Prefab not found!");
            return;
        }

        if (_playerDamageCell == null)
        {
            Debug.LogError("Damage Cell Prefab not found!");
            return;
        }
    }
}
