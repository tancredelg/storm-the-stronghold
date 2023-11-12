using System.Collections;
using UnityEngine;


public class Weapon : MonoBehaviour
{
    public WeaponName WeaponName;
    public Sprite Sprite { get; private set; }

    [SerializeField][Range(0, 2)] protected float _reloadCooldownTime = 0.5f;
    protected PlayerController _playerController;
    protected Rigidbody2D _rigidbody;
    protected UnityEngine.Rendering.Universal.Light2D _glowLight;
    protected Transform _infoText;
    protected WaitForSeconds _reloadCooldown;
    protected bool _isReloading, _isAttacking, _dropped;
    private AudioSource _attackSound;

    private void OnMouseEnter()
    {
        if (!_dropped) return;
        _playerController.SetGameObjectOnCursor(gameObject);
        _glowLight.enabled = true;
        ShowInfo(true);
    }

    private void OnMouseExit()
    {
        if (!_dropped) return;
        _playerController.SetGameObjectOnCursor(null);
        _glowLight.enabled = false;
        ShowInfo(false);
    }

    private void OnEnable()
    {
        StartCoroutine(ReloadCR());
    }

    public virtual IEnumerator AttackCR()
    {
        print("Attack not implemented");
        yield break;
    }

    public virtual IEnumerator ReloadCR()
    {
        if (_dropped) yield break;
        _isReloading = true;
        yield return _reloadCooldown;
        _isReloading = false;
    }

    public virtual void Equip()
    {
        if (!_dropped) return;
        ShowInfo(false);
        _rigidbody.isKinematic = true;
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        transform.parent.parent = _playerController.transform;
        transform.parent.localPosition = Vector3.zero;
        _dropped = false;
    }

    public virtual bool Drop()
    {
        transform.parent.parent = GameObject.Find("DroppedWeapons").transform;
        _glowLight.enabled = false;
        _rigidbody.isKinematic = false;
        _rigidbody.constraints = RigidbodyConstraints2D.None;
        _rigidbody.AddForce(500 * _playerController.GetDirToCursor().normalized);
        _rigidbody.AddTorque(Random.Range(0, 2) == 0 ? Random.Range(-1200, -600) : Random.Range(1200, 600));
        _dropped = true;
        return true;
    }

    public virtual void Disable()
    {
        _isAttacking = false;
        _isReloading = false;
    }

    public void ShowInfo(bool active)
    {
        _infoText.position = transform.position + Vector3.up * 0.7f;
        _infoText.eulerAngles = Vector3.zero;
        _infoText.gameObject.SetActive(active);
    }

    public bool IsAttacking() => _isAttacking;

    public bool IsDropped() => _dropped;

    protected void PlayAttackSound()
    {
        _attackSound.Play();
    }

    protected virtual void Init()
    {
        Sprite = GetComponent<SpriteRenderer>().sprite;
        _playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _glowLight = transform.GetChild(0).GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        _infoText = transform.parent.GetChild(0);
        _infoText.gameObject.SetActive(false);
        _attackSound = GetComponent<AudioSource>();
        _reloadCooldown = new WaitForSeconds(_reloadCooldownTime);
        _dropped = true;
    }
}