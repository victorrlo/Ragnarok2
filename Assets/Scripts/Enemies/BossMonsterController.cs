using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
[RequireComponent(typeof(EnemyContext))]
public class BossMonsterController : MonoBehaviour, IEnemySkillRuleProvider
{
    [Header("Summons")]
    [SerializeField] private GameObject _minionPrefab;
    [SerializeField, Min(0)] private int _maxActiveMinions = 4;
    [SerializeField, Min(0.1f)] private float _summonDelay = 8f;
    [SerializeField, Min(0)] private int _summonAmountPerWave = 4;
    [SerializeField, Min(1)] private int _minionFollowDistance = 2;
    [SerializeField, Min(0.1f)] private float _minionRepathInterval = 0.5f;

    [Header("HP Skill Rules")]
    [SerializeField] private List<BossHpSkillPhase> _skillPhases = new();
    [SerializeField] private bool _useBaseMonsterSkillsWhenNoPhaseMatches = true;

    [Header("Puddle Retreat")]
    [SerializeField] private bool _canRetreatToPuddle = true;
    [SerializeField, Range(0f, 1f)] private float _retreatBelowHpPercent = 0.5f;
    [SerializeField, Range(0f, 1f)] private float _stopRetreatAtHpPercent = 0.8f;
    [SerializeField, Min(0f)] private float _puddleSearchRange = 15f;
    [SerializeField, Min(0f)] private float _puddleHealPerSecond = 20f;
    [SerializeField, Min(0f)] private float _retreatCooldown = 8f;

    private readonly List<GameObject> _activeMinions = new();
    private EnemyAI _ai;
    private EnemyContext _context;
    private EnemyStatsManager _statsManager;
    private float _summonTimer;
    private float _nextRetreatTime;
    private bool _isRetreating;

    public float HealPerSecond => _puddleHealPerSecond;
    public float StopRetreatAtHpPercent => _stopRetreatAtHpPercent;
    public bool IsRetreating => _isRetreating;

    private void Awake()
    {
        _ai = GetComponent<EnemyAI>();
        _context = GetComponent<EnemyContext>();
        _statsManager = GetComponent<EnemyStatsManager>();
        _summonTimer = _summonDelay;
    }

    private void Update()
    {
        RemoveDeadMinions();
        UpdateSummoning();
        TryStartPuddleRetreat();
    }

    public List<SkillCastingData> GetSkillRules()
    {
        float hpPercent = GetHpPercent();

        foreach (BossHpSkillPhase phase in _skillPhases)
        {
            if (!phase.Contains(hpPercent))
                continue;

            return phase.SkillRules;
        }

        return _useBaseMonsterSkillsWhenNoPhaseMatches
            ? GetBaseMonsterSkillRules()
            : new List<SkillCastingData>();
    }

    public float GetHpPercent()
    {
        if (_statsManager == null || _context == null || _context.Stats.MaxHP <= 0f)
            return 1f;

        return Mathf.Clamp01(_statsManager.CurrentHP / _context.Stats.MaxHP);
    }

    public void SetRetreating(bool isRetreating)
    {
        _isRetreating = isRetreating;

        if (!isRetreating)
            _nextRetreatTime = Time.time + _retreatCooldown;
    }

    public void HealFromPuddle(float deltaTime)
    {
        if (_statsManager == null)
            return;

        _statsManager.RecoverHP(_puddleHealPerSecond * deltaTime);
    }

    private void UpdateSummoning()
    {
        if (_minionPrefab == null || _maxActiveMinions <= 0 || _summonAmountPerWave <= 0)
            return;

        if (_activeMinions.Count >= _maxActiveMinions)
            return;

        _summonTimer -= Time.deltaTime;

        if (_summonTimer > 0f)
            return;

        _summonTimer = _summonDelay;
        SummonMinions();
    }

    private void SummonMinions()
    {
        int availableSlots = _maxActiveMinions - _activeMinions.Count;
        int amountToSummon = Mathf.Min(_summonAmountPerWave, availableSlots);

        for (int i = 0; i < amountToSummon; i++)
        {
            if (!TryGetSummonPosition(i, out Vector3 position))
                position = transform.position;

            GameObject minion = Instantiate(_minionPrefab, position, _minionPrefab.transform.rotation);
            _activeMinions.Add(minion);
            ConfigureMinion(minion);
        }
    }

    private void ConfigureMinion(GameObject minion)
    {
        if (minion == null)
            return;

        if (!minion.TryGetComponent(out BossMinionFollower follower))
            follower = minion.AddComponent<BossMinionFollower>();

        follower.Configure(transform, _minionFollowDistance, _minionRepathInterval);
        StartCoroutine(EnterFollowStateNextFrame(follower));
    }

    private IEnumerator EnterFollowStateNextFrame(BossMinionFollower follower)
    {
        yield return null;

        if (follower != null && follower.HasLeader)
            follower.EnterFollowState();
    }

    private bool TryGetSummonPosition(int summonIndex, out Vector3 position)
    {
        Vector3Int bossCell = GridManager.Instance.WorldToCell(transform.position);

        for (int radius = 1; radius <= 3; radius++)
        {
            for (int directionIndex = 0; directionIndex < DirectionHelper._directions.Length; directionIndex++)
            {
                int rotatedIndex = (directionIndex + summonIndex) % DirectionHelper._directions.Length;
                Vector3Int cell = bossCell + DirectionHelper._directions[rotatedIndex] * radius;

                if (NodeManager.Instance == null || !NodeManager.Instance.IsWalkable(cell))
                    continue;

                position = GridHelper.GetCellCenterWorld(gameObject, cell);
                return true;
            }
        }

        position = transform.position;
        return false;
    }

    private void TryStartPuddleRetreat()
    {
        if (!_canRetreatToPuddle || _isRetreating || Time.time < _nextRetreatTime)
            return;

        if (GetHpPercent() > _retreatBelowHpPercent)
            return;

        if (!SkillPowerPuddle.TryFindNearest(transform.position, _puddleSearchRange, out SkillPowerPuddle puddle))
            return;

        _ai.ChangeState(new BossPuddleRetreatState(this, puddle));
    }

    private List<SkillCastingData> GetBaseMonsterSkillRules()
    {
        List<SkillCastingData> rules = new();

        foreach (Skill skill in _context.Stats.Skills)
        {
            rules.Add(new SkillCastingData
            {
                skill = skill,
                chanceOfCasting = _context.Stats.GetChanceOfCasting(skill),
                chanceOfChargedCasting = _context.Stats.GetChanceOfChargedCasting(skill),
                cooldown = _context.Stats.GetSkillCooldown(skill)
            });
        }

        return rules;
    }

    private void RemoveDeadMinions()
    {
        _activeMinions.RemoveAll(minion => minion == null);
    }
}

[System.Serializable]
public class BossHpSkillPhase
{
    [SerializeField, Range(0f, 1f)] private float _minHpPercent = 0f;
    [SerializeField, Range(0f, 1f)] private float _maxHpPercent = 1f;
    [SerializeField] private List<SkillCastingData> _skillRules = new();

    public List<SkillCastingData> SkillRules => _skillRules;

    public bool Contains(float hpPercent)
    {
        float min = Mathf.Min(_minHpPercent, _maxHpPercent);
        float max = Mathf.Max(_minHpPercent, _maxHpPercent);

        return hpPercent >= min && hpPercent <= max;
    }
}

public class BossPuddleRetreatState : IEnemyState
{
    private readonly BossMonsterController _boss;
    private readonly SkillPowerPuddle _puddle;

    private GameObject _self;
    private EnemyAI _ai;
    private EnemyAnimation _animation;
    private List<Node> _path;
    private int _index;
    private Vector3 _nextNodePosition;
    private bool _isMoving;

    public BossPuddleRetreatState(BossMonsterController boss, SkillPowerPuddle puddle)
    {
        _boss = boss;
        _puddle = puddle;
    }

    public void Enter(GameObject enemy)
    {
        _self = enemy;
        _ai = enemy.GetComponent<EnemyAI>();
        enemy.TryGetComponent(out _animation);
        _boss.SetRetreating(true);
        _ai.ClearTarget();

        RecalculatePath();
    }

    public void Execute()
    {
        if (_boss == null || _puddle == null)
        {
            FinishRetreat();
            return;
        }

        if (_boss.GetHpPercent() >= _boss.StopRetreatAtHpPercent)
        {
            FinishRetreat();
            return;
        }

        Vector3Int selfCell = GridManager.Instance.WorldToCell(_self.transform.position);

        if (selfCell == _puddle.Cell)
        {
            _animation?.SetMovementState(false);
            _boss.HealFromPuddle(Time.deltaTime);
            return;
        }

        Move();
    }

    public void Exit()
    {
        _isMoving = false;
        _animation?.SetMovementState(false);
        _boss?.SetRetreating(false);
    }

    private void RecalculatePath()
    {
        if (_puddle == null || NodeManager.Instance == null)
            return;

        Vector3Int selfCell = GridManager.Instance.WorldToCell(_self.transform.position);
        _path = NodeManager.Instance.FindPath(selfCell, _puddle.Cell);

        if (_path == null || _path.Count == 0)
        {
            _isMoving = false;
            return;
        }

        _index = 0;
        _isMoving = true;
        SetNextTargetCell();
    }

    private void Move()
    {
        if (!_isMoving || _path == null || _index >= _path.Count)
        {
            RecalculatePath();
            return;
        }

        Vector3 moveDirection3D = (_nextNodePosition - _self.transform.position).normalized;
        _animation?.FaceDirection(new Vector2(moveDirection3D.x, moveDirection3D.z));
        _animation?.SetMovementState(true);

        _self.transform.position = Vector3.MoveTowards(
            _self.transform.position,
            _nextNodePosition,
            _boss.GetComponent<EnemyContext>().Stats.MoveSpeed * Time.deltaTime);

        if (Vector3.Distance(_self.transform.position, _nextNodePosition) >= 0.1f)
            return;

        _self.transform.position = _nextNodePosition;
        _index++;

        if (_index >= _path.Count)
        {
            _isMoving = false;
            return;
        }

        SetNextTargetCell();
    }

    private void SetNextTargetCell()
    {
        if (_index >= _path.Count)
            return;

        Node nextNode = _path[_index];
        _nextNodePosition = GridHelper.GetCellCenterWorld(_self, nextNode._gridPosition);
    }

    private void FinishRetreat()
    {
        _boss?.SetRetreating(false);
        _ai.ReturnToDefaultState();
    }
}
