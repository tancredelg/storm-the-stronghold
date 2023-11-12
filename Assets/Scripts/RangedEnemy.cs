using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    [SerializeField] private GameObject _projectile;
    private Transform _projectileGrouper;
    private const float CHASE_RANGE = 10f;

    private void Start()
    {
        Init();
        _projectileGrouper = GameObject.Find("ProjectileGrouper").transform;
    }

    private void Update()
    {
        if (_isStunned) return;
        if (Vector2.Distance(transform.position, _playerController.transform.position) <= CHASE_RANGE)
        {
            FacePlayer();
            if (InSameRoomAsPlayer())
            {
                ClearPath();
                StartCoroutine(AttackCR());
            }
            else
            {
                MoveToNextNode();
                if (!_isPathfinding)
                {
                    StartCoroutine(PathfindCR());
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == _playerController.gameObject)
        {
            // Rebound force
            _rigidbody.AddForce(10000 * Time.deltaTime * -(_playerController.transform.position - transform.position).normalized, ForceMode2D.Impulse);
        }
    }

    public override IEnumerator AttackCR()
    {
        if (_isAttacking) yield break;
        _isAttacking = true;
        _rigidbody.velocity = Vector2.zero;
        Instantiate(_projectile, transform.position, transform.rotation, _projectileGrouper);
        yield return _attackCooldown;
        _isAttacking = false;
    }
}
