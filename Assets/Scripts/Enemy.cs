using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField][Range(10, 500)] protected int _maxHealth = 100;
    [SerializeField][Range(0.3f, 3f)] protected float _attackCooldownTime = 1f;
    [SerializeField][Range(5, 100)] protected int _expForKill = 50;
    protected Rigidbody2D _rigidbody;
    protected PlayerController _playerController;
    protected PlayerLevelManager _playerLevelMgr;
    protected DungeonGenerator _dungeonGenerator;
    protected WaitForSeconds _attackCooldown;
    protected int _health;
    protected bool _isAttacking, _isPathfinding, _isStunned;

    [SerializeField][Range(1, 5)] private float _moveSpeed = 2.5f;
    [SerializeField] private AudioSource _damageSound, _dieSound;
    private Pathfinding _pathfinding;
    private List<PathNode> _nodePath;
    private readonly WaitForSeconds _pathfindCooldown = new WaitForSeconds(0.5f);
    private readonly WaitForSeconds _stunnedCooldown = new WaitForSeconds(0.3f);

    public virtual IEnumerator AttackCR()
    {
        print("attack not implemented in child");
        yield break;
    }

    public IEnumerator StunCR()
    {
        _isStunned = true;
        yield return _stunnedCooldown;
        _isStunned = false;
    }

    public void TakeDamage(int damage)
    {
        if (_health > damage)
        {
            _health -= damage;
            _damageSound.Play();
        }
        else
        {
            _health = 0;
            _damageSound.Play();
            _dieSound.Play();
            Die();
        }
    }

    protected void Init()
    {
        _playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        _playerLevelMgr = _playerController.GetComponent<PlayerLevelManager>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _dungeonGenerator = FindObjectOfType<DungeonGenerator>();
         _attackCooldown = new WaitForSeconds(_attackCooldownTime);
        _health = _maxHealth;

        _pathfinding = new Pathfinding(_dungeonGenerator.Size, _dungeonGenerator.Size, true);
        _nodePath = new List<PathNode>();

        // Change floor tiles to walkable for pathfinding
        for (int x = 0; x < _dungeonGenerator.Size; x++)
        {
            for (int y = 0; y < _dungeonGenerator.Size; y++)
            {
                if (_dungeonGenerator.GridArr[x, y] == TileType.Floor)
                {
                    _pathfinding.GetGrid().GridArr[x, y].IsWalkable = true;
                }
            }
        }
    }

    protected IEnumerator PathfindCR()
    {
        // Guard clause cancels new FindPath call if the existing path is still suitable
        if (_nodePath.Count > 0 && Vector2.Distance(_nodePath[0].WorldPos, _playerController.transform.position) < Vector2.Distance(transform.position, _playerController.transform.position)) yield break;
        _isPathfinding = true;
        _nodePath = _pathfinding.FindPath(Mathf.RoundToInt(transform.position.x),
                                          Mathf.RoundToInt(transform.position.y),
                                          Mathf.RoundToInt(_playerController.transform.position.x),
                                          Mathf.RoundToInt(_playerController.transform.position.y));
        yield return _pathfindCooldown;
        _isPathfinding = false;
    }

    protected void FacePlayer()
    {
        Vector2 dir = _playerController.transform.position - transform.position;
        float angle = (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg) - 90;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    protected void ClearPath()
    {
        if (_nodePath == null) return;
        _nodePath.Clear();
    }

    protected void MoveToNextNode()
    {
        if (_nodePath == null || _nodePath.Count == 0) return;
        Vector2 nextNodePos = _nodePath[0].WorldPos;
        MoveTowards(nextNodePos);
        if (Vector2.Distance(nextNodePos, transform.position) < 0.3f)
        {
            _nodePath.RemoveAt(0);
        }
    }

    protected void MoveTowards(Vector2 targetPos)
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPos, _moveSpeed * Time.deltaTime);
    }

    protected bool InSameRoomAsPlayer()
    {
        Room room = _dungeonGenerator.Rooms.FirstOrDefault(r => r.Contains(_playerController.transform.position));
        if (room == null) return false;
        return room.Contains(transform.position);
    }

    private void Die()
    {
        transform.parent.GetComponent<EnemyUnitsHandler>().ActiveUnits.Remove(gameObject);
        _playerLevelMgr.OnEnemyKilled(_expForKill);
        _playerController.Kills++;
        Destroy(gameObject, 0.1f);
    }

    private void ShowPathDebug()
    {
        if (_nodePath == null || _nodePath.Count == 0) return;
        for (int i = 0; i < _nodePath.Count; i++)
        {
            var pos = _pathfinding.GetGrid().GetWorldPos(_nodePath[i].X, _nodePath[i].Y);

            Debug.DrawLine(new Vector3(pos.x - 0.2f, pos.y - 0.2f, pos.z), new Vector3(pos.x + 0.2f, pos.y + 0.2f, pos.z), Color.white, 1);
            Debug.DrawLine(new Vector3(pos.x + 0.2f, pos.y - 0.2f, pos.z), new Vector3(pos.x - 0.2f, pos.y + 0.2f, pos.z), Color.white, 1);
        }
    }
}