using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : Projectile
{
    private PlayerController _playerController;

    protected override void Awake()
    {
        base.Awake();
        _playerController = FindObjectOfType<PlayerController>();
        var dirToPlayer = _playerController.transform.position - transform.position;
        _rigibody.velocity = dirToPlayer.normalized * _projectileSpeed;
        IsLoaded = false;
        StartCoroutine(DeleteCR());
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (collision.gameObject == _playerController.gameObject)
        {
            _playerController.TakeDamage(_projectileDamage);
            Destroy(gameObject);
        }
    }
}