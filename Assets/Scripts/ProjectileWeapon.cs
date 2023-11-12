using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProjectileWeapon : Weapon
{
    [SerializeField][Range(1, 100)] protected int _maxAmmo = 30;
    [SerializeField][Range(0, 1)] protected float _shootCooldownTime = 0.1f;
    [SerializeField] protected GameObject _projectile;
    protected Transform _projectileGrouper;
    protected WaitForSeconds _shootCooldown;
    protected int _ammo;

    private void Awake()
    {
        Init();
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
        _ammo--;
        Instantiate(_projectile, transform.position, transform.rotation, _projectileGrouper).GetComponent<PlayerProjectile>().Shoot();

        yield return _shootCooldown;
        _isAttacking = false;
    }

    public override IEnumerator ReloadCR()
    {
        if (_dropped) yield break;
        _isReloading = true;
        _ammo = _maxAmmo;
        yield return _reloadCooldown;
        _isReloading = false;
    }

    public override void Equip()
    {
        base.Equip();

        transform.parent.localEulerAngles = Vector3.zero;
        transform.localEulerAngles = new Vector3(0, 0, -45);
        transform.localPosition = new Vector3(0, 0.6f, 0);
    }

    public override bool Drop()
    {
        if (_isReloading || _isAttacking) return false;
        base.Drop();
        return true;
    }

    protected override void Init()
    {
        base.Init();
        _shootCooldown = new WaitForSeconds(_shootCooldownTime);
        _projectileGrouper = GameObject.Find("ProjectileGrouper").transform;
    }
}
