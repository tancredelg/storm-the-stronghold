using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crossbow : ProjectileWeapon
{
    [SerializeField] private Sprite _weaponLoaded, _weaponLoose;
    private GameObject _loadedProj;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        Init();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override IEnumerator AttackCR()
    {
        if (_ammo == 0)
        {
            StartCoroutine(ReloadCR());
        }

        if (_isAttacking || _isReloading)
        {
            yield break;
        }

        _isAttacking = true;
        PlayAttackSound();
        if (_loadedProj != null && _loadedProj.GetComponent<PlayerProjectile>().IsLoaded)
        {
            _loadedProj.GetComponent<PlayerProjectile>().Shoot();
            _loadedProj.transform.SetParent(_projectileGrouper);
            _loadedProj = null;
        }
        _spriteRenderer.sprite = _weaponLoose;
        _ammo--;

        yield return _shootCooldown;
        _isAttacking = false;
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(ReloadCR());
        }
    }

    public override IEnumerator ReloadCR()
    {
        if (_dropped) yield break;
        _isReloading = true;
        _ammo = _maxAmmo;

        yield return _reloadCooldown;
        if (_loadedProj == null)
        {
            _loadedProj = Instantiate(_projectile, transform.position, transform.rotation, transform);
        }

        _spriteRenderer.sprite = _weaponLoaded;
        _isReloading = false;
    }

    public override bool Drop()
    {
        if (_isReloading || _isAttacking) return false;
        transform.parent.parent = null;

        _spriteRenderer.sprite = _weaponLoose;
        if (_loadedProj != null && _loadedProj.transform.parent == transform)
        {
            Destroy(_loadedProj);
        }

        _glowLight.enabled = false;
        _rigidbody.isKinematic = false;
        _rigidbody.constraints = RigidbodyConstraints2D.None;
        _rigidbody.AddForce(500 * _playerController.GetDirToCursor().normalized);
        _rigidbody.AddTorque(Random.Range(0, 2) == 0 ? Random.Range(-1200, -600) : Random.Range(1200, 600));
        _dropped = true;
        return true;
    }
}
