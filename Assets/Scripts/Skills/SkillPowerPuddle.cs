using System.Collections.Generic;
using UnityEngine;

public class SkillPowerPuddle : MonoBehaviour
{
    private static readonly Dictionary<Vector3Int, SkillPowerPuddle> PuddlesByCell = new();
    private static readonly Dictionary<GameObject, HashSet<SkillPowerPuddle>> PuddlesByOccupant = new();
    private static readonly List<SkillPowerPuddle> ActivePuddles = new();

    [SerializeField, Min(1)] private int _maxSizeStage = 3;
    [SerializeField, Min(1)] private int _startingSizeStage = 1;
    [SerializeField] private float _secondsToGrowOneStage = 8f;
    [SerializeField] private float _skillDamageMultiplier = 2f;

    private Vector3Int _cell;
    private int _currentSizeStage;
    private float _growthTimer;
    private Vector3 _fullScale;
    private bool _isRegistered;
    private Collider _triggerCollider;

    public int CurrentSizeStage => _currentSizeStage;
    public float SkillDamageMultiplier => _skillDamageMultiplier;
    public Vector3Int Cell => _cell;

    private void Awake()
    {
        ConfigureTriggerCollider();
        ConfigureTriggerRigidbody();
        _fullScale = transform.localScale;
        _currentSizeStage = Mathf.Clamp(_startingSizeStage, 1, _maxSizeStage);
    }

    private void OnEnable()
    {
        if (!ActivePuddles.Contains(this))
            ActivePuddles.Add(this);

        ConfigureTriggerCollider();
        ConfigureTriggerRigidbody();
        RegisterCurrentCell();
        RefreshVisualSize();
    }

    private void Start()
    {
        if (!_isRegistered)
            RegisterCurrentCell();
    }

    private void Update()
    {
        if (_currentSizeStage >= _maxSizeStage)
            return;

        _growthTimer += Time.deltaTime;

        if (_growthTimer < _secondsToGrowOneStage)
            return;

        _growthTimer = 0f;
        _currentSizeStage = Mathf.Min(_currentSizeStage + 1, _maxSizeStage);
        RefreshVisualSize();
    }

    private void OnDisable()
    {
        if (PuddlesByCell.TryGetValue(_cell, out SkillPowerPuddle puddle) && puddle == this)
            PuddlesByCell.Remove(_cell);

        ActivePuddles.Remove(this);
        UnregisterAllOccupants();
    }

    public static bool TryConsumeAt(GameObject caster, out float damageMultiplier)
    {
        damageMultiplier = 1f;

        if (caster == null)
            return false;

        if (!TryGetBestPuddleFor(caster, out SkillPowerPuddle puddle))
            return false;

        damageMultiplier = puddle.SkillDamageMultiplier;
        puddle.ConsumeOneStage();
        return true;
    }

    public static bool HasPuddleAt(Vector3Int cell)
    {
        return PuddlesByCell.TryGetValue(cell, out SkillPowerPuddle puddle) && puddle != null;
    }

    public static bool TryFindNearest(Vector3 worldPosition, float maxDistance, out SkillPowerPuddle nearestPuddle)
    {
        nearestPuddle = null;
        float maxDistanceSqr = maxDistance * maxDistance;
        float closestDistanceSqr = float.MaxValue;
        List<SkillPowerPuddle> activePuddleSnapshot = new(ActivePuddles);

        foreach (SkillPowerPuddle puddle in activePuddleSnapshot)
        {
            if (puddle == null)
                continue;

            float distanceSqr = (puddle.transform.position - worldPosition).sqrMagnitude;

            if (distanceSqr > maxDistanceSqr || distanceSqr >= closestDistanceSqr)
                continue;

            closestDistanceSqr = distanceSqr;
            nearestPuddle = puddle;
        }

        return nearestPuddle != null;
    }

    private void OnTriggerEnter(Collider other)
    {
        RegisterOccupant(other);
    }

    private void OnTriggerStay(Collider other)
    {
        RegisterOccupant(other);
    }

    private void OnTriggerExit(Collider other)
    {
        UnregisterOccupant(other);
    }

    private static bool TryGetBestPuddleFor(GameObject occupant, out SkillPowerPuddle bestPuddle)
    {
        bestPuddle = null;

        if (PuddlesByOccupant.TryGetValue(occupant, out HashSet<SkillPowerPuddle> puddles))
        {
            List<SkillPowerPuddle> puddleSnapshot = new(puddles);

            foreach (SkillPowerPuddle puddle in puddleSnapshot)
            {
                if (puddle == null)
                    continue;

                if (bestPuddle == null || puddle.CurrentSizeStage > bestPuddle.CurrentSizeStage)
                    bestPuddle = puddle;
            }
        }

        if (bestPuddle != null)
            return true;

        return TryFindOverlappingPuddle(occupant, out bestPuddle);
    }

    private static bool TryFindOverlappingPuddle(GameObject occupant, out SkillPowerPuddle foundPuddle)
    {
        foundPuddle = null;
        Collider[] occupantColliders = occupant.GetComponentsInChildren<Collider>();
        List<SkillPowerPuddle> activePuddleSnapshot = new(ActivePuddles);

        foreach (SkillPowerPuddle puddle in activePuddleSnapshot)
        {
            if (puddle == null || puddle._triggerCollider == null || !puddle._triggerCollider.enabled)
                continue;

            foreach (Collider occupantCollider in occupantColliders)
            {
                if (occupantCollider == null || !occupantCollider.enabled)
                    continue;

                if (!puddle._triggerCollider.bounds.Intersects(occupantCollider.bounds))
                    continue;

                foundPuddle = puddle;
                puddle.RegisterOccupant(occupant);
                return true;
            }
        }

        return false;
    }

    private void RegisterOccupant(Collider other)
    {
        if (other == null)
            return;

        RegisterOccupant(ResolveOccupantRoot(other));
    }

    private void RegisterOccupant(GameObject occupant)
    {
        if (occupant == null)
            return;

        if (!occupant.CompareTag("Player") && !occupant.CompareTag("Enemy"))
            return;

        if (!PuddlesByOccupant.TryGetValue(occupant, out HashSet<SkillPowerPuddle> puddles))
        {
            puddles = new HashSet<SkillPowerPuddle>();
            PuddlesByOccupant[occupant] = puddles;
        }

        if (puddles.Add(this))
            Debug.Log($"[{nameof(SkillPowerPuddle)}] {occupant.name} entered {name}. Occupants: {GetCurrentOccupantsDebugText()}");
    }

    private void UnregisterOccupant(Collider other)
    {
        if (other == null)
            return;

        UnregisterOccupant(ResolveOccupantRoot(other));
    }

    private void UnregisterOccupant(GameObject occupant)
    {
        if (occupant == null)
            return;

        if (!PuddlesByOccupant.TryGetValue(occupant, out HashSet<SkillPowerPuddle> puddles))
            return;

        if (puddles.Remove(this))
            Debug.Log($"[{nameof(SkillPowerPuddle)}] {occupant.name} exited {name}. Occupants: {GetCurrentOccupantsDebugText(occupant)}");

        if (puddles.Count == 0)
            PuddlesByOccupant.Remove(occupant);
    }

    private void UnregisterAllOccupants()
    {
        List<GameObject> occupants = new(PuddlesByOccupant.Keys);

        foreach (GameObject occupant in occupants)
            UnregisterOccupant(occupant);
    }

    private void ConfigureTriggerCollider()
    {
        if (_triggerCollider == null)
            TryGetComponent(out _triggerCollider);

        if (_triggerCollider == null)
        {
            Debug.LogError($"[{nameof(SkillPowerPuddle)}] Missing Collider on {name}.");
            return;
        }

        _triggerCollider.enabled = true;
        _triggerCollider.isTrigger = true;
    }

    private void ConfigureTriggerRigidbody()
    {
        if (!TryGetComponent(out Rigidbody rigidbody))
            rigidbody = gameObject.AddComponent<Rigidbody>();

        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
    }

    private string GetCurrentOccupantsDebugText(GameObject occupantToIgnore = null)
    {
        List<string> occupantNames = new();
        List<KeyValuePair<GameObject, HashSet<SkillPowerPuddle>>> occupantSnapshot = new(PuddlesByOccupant);

        foreach (KeyValuePair<GameObject, HashSet<SkillPowerPuddle>> entry in occupantSnapshot)
        {
            GameObject occupant = entry.Key;

            if (occupant == null || occupant == occupantToIgnore)
                continue;

            if (entry.Value.Contains(this))
                occupantNames.Add(occupant.name);
        }

        if (occupantNames.Count == 0)
            return "none";

        return string.Join(", ", occupantNames);
    }

    private GameObject ResolveOccupantRoot(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
            return other.gameObject;

        PlayerContext playerContext = other.GetComponentInParent<PlayerContext>();
        if (playerContext != null)
            return playerContext.gameObject;

        EnemyContext enemyContext = other.GetComponentInParent<EnemyContext>();
        if (enemyContext != null)
            return enemyContext.gameObject;

        return other.gameObject;
    }

    private void RegisterCurrentCell()
    {
        if (GridManager.Instance == null)
            return;

        _cell = GridManager.Instance.WorldToCell(transform.position);
        PuddlesByCell[_cell] = this;
        _isRegistered = true;
    }

    private void ConsumeOneStage()
    {
        _currentSizeStage--;

        if (_currentSizeStage <= 0)
        {
            Destroy(gameObject);
            return;
        }

        _growthTimer = 0f;
        RefreshVisualSize();
    }

    private void RefreshVisualSize()
    {
        float sizePercent = Mathf.Clamp01((float)_currentSizeStage / _maxSizeStage);
        transform.localScale = _fullScale * sizePercent;
    }
}
