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
    

    public void CastDamageCellsOnGround(GameObject caster, Skill skill)
    {
        CastDamageCellsOnGround(caster, skill, false);
    }

    public async void CastDamageCellsOnGround(GameObject caster, Skill skill, bool charged)
    {
        CastDamageCellsOnGround(caster, skill, charged, null);
    }

    public async void CastDamageCellsOnGround(GameObject caster, Skill skill, bool charged, Func<bool> shouldSuppressDamage)
    {
        var damageCell = _monsterDamageCell;
        
        if (caster.tag == "Player")
            damageCell = _playerDamageCell;

        var cellsAffected = DefineRangeOfCells(caster, skill, charged ? 1 : 0);
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

        await RemoveDamageCells(caster, skill, cellsAffected, spawnedCells, charged, shouldSuppressDamage);
    }

    private async UniTask RemoveDamageCells(
        GameObject caster,
        Skill skill,
        List<Vector3Int> cellsAffected,
        List<GameObject> spawnedCells,
        bool charged,
        Func<bool> shouldSuppressDamage)
    {
        var castingTime = skill.CastingTime;

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(castingTime));

            if (caster != null && (shouldSuppressDamage == null || !shouldSuppressDamage()))
            {
                ApplySkillEffect(caster, skill, cellsAffected, charged);
            }

            foreach (var cell in spawnedCells)
            {
                if (cell != null)
                    Destroy(cell);
            }

            if (caster != null && _activeDamageCells.TryGetValue(caster, out List<GameObject> activeCells))
            {
                foreach (var cell in spawnedCells)
                    activeCells.Remove(cell);

                if (activeCells.Count == 0)
                    _activeDamageCells.Remove(caster);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error removing damage cells: {e.Message}");
        }
    }

    private void ApplySkillEffect(GameObject caster, Skill skill, List<Vector3Int> cellsAffected, bool charged)
    {

        if (caster == null) return;

        if (caster.CompareTag("Player"))
        {
            GameObject[] monsters = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (GameObject monster in monsters)
            {
                Vector3Int monsterCell = GridManager.Instance.WorldToCell(monster.transform.position);

                if (cellsAffected.Contains(monsterCell))
                {
                    var monsterCombat = monster.GetComponent<EnemyCombat>();
                    var playerContext = caster.GetComponent<PlayerContext>();

                    if (monsterCombat == null)
                    {
                        Debug.LogError("Couldn't find [EnemyCombat] component in monster...");
                        return;
                    }

                    if (playerContext == null)
                    {
                        Debug.LogError("Couldn't find [PlayerContext] component in player...");
                        return;
                    }

                    ApplyPlayerAreaDamage(caster, skill, charged, monster, monsterCombat, playerContext);
                }
            }
        }

        else if (caster.CompareTag("Enemy"))
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                Vector3Int playerCell = GridManager.Instance.WorldToCell(player.transform.position);

                if (cellsAffected.Contains(playerCell))
                {
                    var playerCombat = player.GetComponent<PlayerCombat>();
                    var enemyContext = caster.GetComponent<EnemyContext>();

                    if (playerCombat != null && enemyContext != null)
                        ApplyEnemyAreaDamage(caster, skill, charged, player, playerCombat, enemyContext);
                }
            }
        }
    }

    private void ApplyPlayerAreaDamage(
        GameObject caster,
        Skill skill,
        bool charged,
        GameObject monster,
        EnemyCombat monsterCombat,
        PlayerContext playerContext)
    {
        var playerStats = playerContext.StatsManager.RunTimeStats;
        var baseDamage = Mathf.RoundToInt(skill.Multiplier * playerStats.Attack);

        if (!charged)
        {
            var normalDamage = DamageCalculator.Roll(
                baseDamage,
                playerStats.CriticalChance,
                playerStats.CriticalDamageMultiplier);

            monsterCombat.TakeDamage(normalDamage);
            return;
        }

        Vector3Int casterCell = GridManager.Instance.WorldToCell(caster.transform.position);
        Vector3Int monsterCell = GridManager.Instance.WorldToCell(monster.transform.position);
        bool isOriginalCell = IsInsideSquareRange(casterCell, monsterCell, skill.Range);
        int chargedBaseDamage = baseDamage * 2;

        if (!isOriginalCell)
        {
            var borderDamage = DamageCalculator.Roll(
                chargedBaseDamage,
                playerStats.CriticalChance,
                playerStats.CriticalDamageMultiplier);

            monsterCombat.TakeDamage(borderDamage);
            return;
        }

        List<int> accumulatedDamageSteps = new List<int>();
        int accumulatedDamage = 0;

        for (int hit = 0; hit < 2; hit++)
        {
            if (monsterCombat == null || monsterCombat.IsDead)
                break;

            var innerDamage = DamageCalculator.Roll(
                chargedBaseDamage,
                playerStats.CriticalChance,
                playerStats.CriticalDamageMultiplier);

            monsterCombat.TakeDamage(innerDamage);
            accumulatedDamage += innerDamage.Amount;
            accumulatedDamageSteps.Add(accumulatedDamage);
        }

        if (accumulatedDamageSteps.Count > 0)
            FloatingTextPool.Instance.ShowAccumulatingDamage(monster.transform.position, accumulatedDamageSteps, Color.yellow);
    }

    private void ApplyEnemyAreaDamage(
        GameObject caster,
        Skill skill,
        bool charged,
        GameObject player,
        PlayerCombat playerCombat,
        EnemyContext enemyContext)
    {
        int baseDamage = Mathf.RoundToInt(skill.Multiplier * enemyContext.Stats.Attack);

        if (!charged)
        {
            var normalDamage = DamageCalculator.Roll(
                baseDamage,
                enemyContext.Stats.CriticalChance,
                enemyContext.Stats.CriticalDamageMultiplier);

            playerCombat.TakeDamage(normalDamage);
            return;
        }

        Vector3Int casterCell = GridManager.Instance.WorldToCell(caster.transform.position);
        Vector3Int playerCell = GridManager.Instance.WorldToCell(player.transform.position);
        bool isOriginalCell = IsInsideSquareRange(casterCell, playerCell, skill.Range);
        int chargedBaseDamage = baseDamage * 2;

        if (!isOriginalCell)
        {
            var borderDamage = DamageCalculator.Roll(
                chargedBaseDamage,
                enemyContext.Stats.CriticalChance,
                enemyContext.Stats.CriticalDamageMultiplier);

            playerCombat.TakeDamage(borderDamage);
            return;
        }

        List<int> accumulatedDamageSteps = new List<int>();
        int accumulatedDamage = 0;

        for (int hit = 0; hit < 2; hit++)
        {
            var innerDamage = DamageCalculator.Roll(
                chargedBaseDamage,
                enemyContext.Stats.CriticalChance,
                enemyContext.Stats.CriticalDamageMultiplier);

            playerCombat.TakeDamage(innerDamage);
            accumulatedDamage += innerDamage.Amount;
            accumulatedDamageSteps.Add(accumulatedDamage);
        }

        if (accumulatedDamageSteps.Count > 0)
            FloatingTextPool.Instance.ShowAccumulatingDamage(player.transform.position, accumulatedDamageSteps, Color.yellow);
    }

    private List<Vector3Int> DefineRangeOfCells(GameObject caster, Skill skill, int rangeBonus = 0)
    {
        var startingPosition = GridManager.Instance.WorldToCell(caster.transform.position);
        List<Vector3Int> cellsAffected = new List<Vector3Int>();
        int range = skill.Range + rangeBonus;
        
        if (skill.SkillType == Skill.Type.AreaOfEffect)
        {
            cellsAffected.Add(startingPosition);

            for (int x = -range; x <= range; x++)
            {
                for (int y = -range; y <= range; y++)
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

    private bool IsInsideSquareRange(Vector3Int center, Vector3Int cell, int range)
    {
        return Mathf.Abs(cell.x - center.x) <= range &&
               Mathf.Abs(cell.y - center.y) <= range;
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
