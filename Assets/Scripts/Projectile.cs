using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public bool IsLoaded { get; protected set; }

    [SerializeField][Range(1, 100)] protected int _projectileSpeed = 20;
    [SerializeField][Range(1, 100)] protected int _projectileDamage = 40;
    protected Rigidbody2D _rigibody;

    protected virtual void Awake()
    {
        _rigibody = GetComponent<Rigidbody2D>();
        IsLoaded = true;
    }

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

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsLoaded) return;
        if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject, 0.01f);
        }
    }

    protected IEnumerator DeleteCR()
    {
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }
}
