using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
[RequireComponent(typeof(EnemyContext))]
public class BossMinionFollower : MonoBehaviour
{
    [SerializeField] private Transform _leader;
    [SerializeField, Min(1)] private int _preferredDistance = 2;
    [SerializeField, Min(0.1f)] private float _repathInterval = 0.5f;

    private EnemyAI _ai;

    public bool HasLeader => _leader != null;
    public Transform Leader => _leader;
    public int PreferredDistance => _preferredDistance;
    public float RepathInterval => _repathInterval;

    private void Awake()
    {
        _ai = GetComponent<EnemyAI>();
    }

    public void SetLeader(Transform leader)
    {
        _leader = leader;
    }

    public void Configure(Transform leader, int preferredDistance, float repathInterval)
    {
        _leader = leader;
        _preferredDistance = Mathf.Max(1, preferredDistance);
        _repathInterval = Mathf.Max(0.1f, repathInterval);
    }

    public void EnterFollowState()
    {
        if (_ai == null)
            _ai = GetComponent<EnemyAI>();

        if (_leader != null)
            _ai.ChangeState(new FollowLeaderState(_leader, _preferredDistance, _repathInterval));
    }
}

public class FollowLeaderState : IEnemyState
{
    private readonly Transform _leader;
    private readonly int _preferredDistance;
    private readonly float _repathInterval;

    private GameObject _self;
    private EnemyContext _context;
    private EnemyAI _ai;
    private EnemyAnimation _animation;
    private GameObject _player;
    private List<Node> _path;
    private int _index;
    private Vector3 _nextNodePosition;
    private float _repathTimer;
    private bool _isMoving;

    public FollowLeaderState(Transform leader, int preferredDistance, float repathInterval)
    {
        _leader = leader;
        _preferredDistance = Mathf.Max(1, preferredDistance);
        _repathInterval = Mathf.Max(0.1f, repathInterval);
    }

    public void Enter(GameObject enemy)
    {
        _self = enemy;
        _context = enemy.GetComponent<EnemyContext>();
        _ai = enemy.GetComponent<EnemyAI>();
        enemy.TryGetComponent(out _animation);
        _player = GameObject.FindWithTag("Player");
        _ai.ClearTarget();
        _repathTimer = 0f;
        _isMoving = false;

        GridHelper.SnapToNearestCellCenter(_self);
        RecalculatePath();
    }

    public void Execute()
    {
        if (_leader == null)
        {
            _ai.ChangeState(new PassiveState());
            return;
        }

        if (TryAggroPlayer())
            return;

        _repathTimer -= Time.deltaTime;

        if (_repathTimer <= 0f)
        {
            RecalculatePath();
            _repathTimer = _repathInterval;
        }

        Move();
    }

    public void Exit()
    {
        _isMoving = false;
        _animation?.SetMovementState(false);
    }

    private bool TryAggroPlayer()
    {
        if (_player == null || _context.Stats.Nature != MonsterData.MonsterNature.Aggressive)
            return false;

        Vector3Int selfCell = GridManager.Instance.WorldToCell(_self.transform.position);
        Vector3Int playerCell = GridManager.Instance.WorldToCell(_player.transform.position);

        if (!DistanceHelper.IsInAttackRange(selfCell, playerCell, _context.Stats.SightRangeToTurnAgressive))
            return false;

        _ai.SetTarget(_player);
        _ai.ChangeState(new AggressiveState());
        return true;
    }

    private void RecalculatePath()
    {
        if (_leader == null || NodeManager.Instance == null)
            return;

        Vector3Int selfCell = GridManager.Instance.WorldToCell(_self.transform.position);
        Vector3Int leaderCell = GridManager.Instance.WorldToCell(_leader.position);

        if (DistanceHelper.IsInAttackRange(selfCell, leaderCell, _preferredDistance))
        {
            _isMoving = false;
            _animation?.SetMovementState(false);
            return;
        }

        Vector3Int destination = FindFollowPosition(selfCell, leaderCell);
        _path = NodeManager.Instance.FindPath(selfCell, destination);

        if (_path == null || _path.Count == 0)
        {
            _isMoving = false;
            _animation?.SetMovementState(false);
            return;
        }

        _index = 0;
        _isMoving = true;
        SetNextTargetCell();
    }

    private Vector3Int FindFollowPosition(Vector3Int selfCell, Vector3Int leaderCell)
    {
        Vector3Int closestPosition = leaderCell;
        float closestDistance = float.MaxValue;

        for (int x = -_preferredDistance; x <= _preferredDistance; x++)
        {
            for (int y = -_preferredDistance; y <= _preferredDistance; y++)
            {
                Vector3Int potentialPosition = leaderCell + new Vector3Int(x, y, 0);

                if (potentialPosition == leaderCell)
                    continue;

                if (!DistanceHelper.IsInAttackRange(potentialPosition, leaderCell, _preferredDistance))
                    continue;

                if (!NodeManager.Instance.IsWalkable(potentialPosition))
                    continue;

                float distance = Vector3Int.Distance(selfCell, potentialPosition);

                if (distance >= closestDistance)
                    continue;

                closestDistance = distance;
                closestPosition = potentialPosition;
            }
        }

        return closestPosition;
    }

    private void Move()
    {
        if (!_isMoving || _path == null || _index >= _path.Count)
            return;

        Vector3 moveDirection3D = (_nextNodePosition - _self.transform.position).normalized;
        _animation?.FaceDirection(new Vector2(moveDirection3D.x, moveDirection3D.z));
        _animation?.SetMovementState(true);

        _self.transform.position = Vector3.MoveTowards(
            _self.transform.position,
            _nextNodePosition,
            _context.Stats.MoveSpeed * Time.deltaTime);

        if (Vector3.Distance(_self.transform.position, _nextNodePosition) >= 0.1f)
            return;

        _self.transform.position = _nextNodePosition;
        _index++;

        if (_index >= _path.Count)
        {
            _isMoving = false;
            _animation?.SetMovementState(false);
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
}
