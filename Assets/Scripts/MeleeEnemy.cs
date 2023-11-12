using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : Enemy
{
    [SerializeField][Range(1, 100)] private int _attackDamage = 20;
    private bool _isDashing;

    private readonly WaitForSeconds _dashingTime = new WaitForSeconds(0.5f);
    private const int DASH_STRENGTH = 7000;
    private const float CHASE_RANGE = 10f;
    private const float ATTACK_RANGE = 1.35f;
        
    private void Start()
    {
        Init();
        _attackCooldown = new WaitForSeconds(_attackCooldownTime);
    }

    private void Update()
    {
        if (_isStunned) return;
        if (Vector2.Distance(transform.position, _playerController.transform.position) <= CHASE_RANGE)
        {
            FacePlayer();
            if (Vector2.Distance(transform.position, _playerController.transform.position) <= ATTACK_RANGE)
            {
                ClearPath();
                StartCoroutine(AttackCR());
            }
            else if (InSameRoomAsPlayer())
            {
                MoveTowards(_playerController.transform.position);
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
            if (_isDashing)
            {
                _playerController.TakeDamage(_attackDamage);
                _rigidbody.velocity = Vector2.zero;
                _rigidbody.AddForce(DASH_STRENGTH * Time.deltaTime * -(_playerController.transform.position - transform.position).normalized, ForceMode2D.Impulse);
                _isDashing = false;
            }
            else
            {
                _rigidbody.AddForce(DASH_STRENGTH * 3 * Time.deltaTime * -(_playerController.transform.position - transform.position).normalized, ForceMode2D.Impulse);
            }
        }
    }

    public override IEnumerator AttackCR()
    {
        if (_isAttacking) yield break;
        _isAttacking = true;
        _isDashing = true;
        _rigidbody.velocity = (DASH_STRENGTH * Time.deltaTime * (_playerController.transform.position - transform.position).normalized);
        yield return _dashingTime;
        _rigidbody.velocity = Vector2.zero;
        _isDashing = false;
        yield return _attackCooldown;
        _isAttacking = false;
    }
}
