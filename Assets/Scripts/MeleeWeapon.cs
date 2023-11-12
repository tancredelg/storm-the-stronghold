using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    [SerializeField][Range(1, 100)] private int _damage = 20;
    [SerializeField][Range(1, 20)] private int _swingSpeed = 12;
    [SerializeField][Range(20, 240)] private int _swingRotation = 140;
    private CircleCollider2D _circleCol;

    private void Awake()
    {
        Init();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !_dropped && _isAttacking)
        {
            collision.gameObject.GetComponent<Enemy>().TakeDamage(_damage);
        }
    }

    public override IEnumerator AttackCR()
    {
        if (_isAttacking || _isReloading) yield break;

        _isAttacking = true;
        PlayAttackSound();
        int z = transform.parent.localEulerAngles.z < 180 ? Mathf.RoundToInt(transform.parent.localEulerAngles.z) : Mathf.RoundToInt(transform.parent.localEulerAngles.z) - 360;
        int framesPerSwing = Mathf.RoundToInt(1 / (Time.deltaTime * _swingSpeed));
        float deltaAnglePerFrame = (float)-2 * z / framesPerSwing;

        for (int i = 0; i < framesPerSwing; i++)
        {
            transform.parent.Rotate(0, 0, deltaAnglePerFrame);
            yield return null;
        }
        _isAttacking = false;
        StartCoroutine(ReloadCR());
    }

    public override void Equip()
    {
        base.Equip();

        _circleCol.enabled = false;
        transform.parent.localEulerAngles = new Vector3(0, 0, -0.5f * _swingRotation);
        transform.localEulerAngles = new Vector3(0, 0, -45);
        transform.localPosition = new Vector3(0, 0.7f, 0);
    }

    public override bool Drop()
    {
        if (_isAttacking) return false;
        _circleCol.enabled = true;
        base.Drop();
        return true;
    }

    protected override void Init()
    {
        base.Init();
        _circleCol = GetComponent<CircleCollider2D>();
    }
}