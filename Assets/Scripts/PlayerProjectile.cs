using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : Projectile
{
    private void Update()
    {
        if (IsLoaded)
        {
            transform.position = transform.parent.position;
        }

        if (transform.parent.CompareTag("Weapon") && transform.parent.GetComponent<ProjectileWeapon>().IsDropped())
        {
            Destroy(gameObject);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<Enemy>().TakeDamage(_projectileDamage);
        }
    }

    public void Shoot()
    {
        var dirToCursor = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        _rigibody.velocity = dirToCursor.normalized * _projectileSpeed;
        IsLoaded = false;
        StartCoroutine(DeleteCR());
    }
}
