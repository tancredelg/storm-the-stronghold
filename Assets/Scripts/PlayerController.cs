using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.Linq;

[System.Serializable]
public class PlayerController : MonoBehaviour, ISavableData
{
    public Skills Skills { get; private set; }
    public int Kills;

    [SerializeField] private List<GameObject> _weaponPrefabs;
    [SerializeField][Range(10, 1000)] private int _maxHealth = 100;
    [SerializeField][Range(2, 10)]  private float _moveSpeed = 3;
    private Weapon _weapon1, _weapon2;
    private int _health;
    private float _outOfCombatTimer;
    private bool _isDashing, _canDash, _isUpdating, _isHealing;

    [SerializeField][Range(100, 1000)] private int _dashStrength = 500;
    [SerializeField][Range(10, 200)] private int _dashDamage = 20;
    private Camera _cam;
    private Rigidbody2D _rigidbody;
    private DungeonGenerator _dungeonGen;
    private Image _leftWeaponHUDVisual, _rightWeaponHUDVisual;
    private TextMeshProUGUI _healthText;
    private GameObject _gameObjectOnCursor;
    private AudioSource _dashSound, _damageSound;

    private readonly WaitForSeconds _updateTilesCooldown = new WaitForSeconds(0.5f);
    private readonly WaitForSeconds _dashCooldown = new WaitForSeconds(1f);
    private readonly WaitForSeconds _dashingTime = new WaitForSeconds(0.15f);
    private readonly WaitForSeconds _healCooldown = new WaitForSeconds(0.1f);
    private readonly Color _emptyWeaponSlotColor = new Color(1, 1, 1, 0);

    private void Awake()
    {
        _cam = Camera.main;
        _rigidbody = GetComponent<Rigidbody2D>();
        _dungeonGen = FindObjectOfType<DungeonGenerator>();

        var sounds = GetComponents<AudioSource>();
        _dashSound = sounds[0];
        _damageSound = sounds[1];

        // Can't use GetComponentInChildren because Image component of parent will be returned
        _leftWeaponHUDVisual = GameObject.Find("LeftWeaponHUDVisual").transform.GetChild(0).GetComponent<Image>();
        _rightWeaponHUDVisual = GameObject.Find("RightWeaponHUDVisual").transform.GetChild(0).GetComponent<Image>();
        print(_rightWeaponHUDVisual.name);
        
        Skills = new Skills();
        Skills.OnSkillUnlocked += Player_OnSkillUnlocked;

        _health = _maxHealth;
        _healthText = GameObject.Find("HealthText").GetComponent<TextMeshProUGUI>();
        _healthText.text = _health.ToString();
        _canDash = true;
    }

    private void Update()
    {
        if (!_isUpdating)
        {
            StartCoroutine(UpdateTilesCR());
        }

        AttachCamera();
        FaceCursor();

        if (_outOfCombatTimer > 0)
        {
            _outOfCombatTimer -= Time.deltaTime;
        }
        else if (Skills.Has(SkillType.Heal) && _health < _maxHealth)
        {
            StartCoroutine(HandleHealCR());
        }
    }

    private void FixedUpdate()
    {
        // WASD movement, relative to position of cursor
        //Vector2 perpDirToCursor = new Vector2(dirToCursor.y, -dirToCursor.x);
        //rb.velocity = (Input.GetAxis("Vertical") * dirToCursor * Time.deltaTime * moveSpeed) + (Input.GetAxis("Horizontal") * perpDirToCursor * Time.deltaTime * moveSpeed);

        _rigidbody.velocity += 50 * _moveSpeed * Time.deltaTime * new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (_isDashing)
        {
            _rigidbody.AddForce(GetDirToCursor().normalized * _dashStrength, ForceMode2D.Impulse);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && _isDashing)
        {
            StartCoroutine(collision.gameObject.GetComponent<Enemy>().StunCR());
            collision.gameObject.GetComponent<Enemy>().TakeDamage(_dashDamage);
        }
    }

    public bool TryDash()
    {
        if (!Skills.Has(SkillType.Dash) || !_canDash) return false;
        _dashSound.Play();
        StartCoroutine(HandleDashCR());
        return true;
    }

    public bool TryInteract()
    {
        if (_gameObjectOnCursor == null) return false;
        if (_gameObjectOnCursor.CompareTag("Weapon") && Vector2.Distance(transform.position, _gameObjectOnCursor.transform.position) < 2.5f)
        {
            TryEquipWeapon(_gameObjectOnCursor.GetComponentInChildren<Weapon>());
            return true;
        }
        if (_gameObjectOnCursor.CompareTag("Chest") && Vector2.Distance(transform.position, _gameObjectOnCursor.transform.position) < 1.5f)
        {
            _gameObjectOnCursor.GetComponent<Chest>().Open();
            return true;
        }
        return false;
    }

    public bool TryDropWeapon()
    {
        if (_weapon1 == null || !_weapon1.Drop()) return false;
        _weapon1 = null;
        _leftWeaponHUDVisual.sprite = null;
        _leftWeaponHUDVisual.color = _emptyWeaponSlotColor;
        SwapWeapons();
        return true;
    }

    public bool TryAttack()
    {
        if (_weapon1 == null) return false;
        StartCoroutine(_weapon1.AttackCR());
        return true;
    }

    public void SetGameObjectOnCursor(GameObject g)
    {
        _gameObjectOnCursor = g;
    }

    public Vector2 GetDirToCursor()
    {
        return Input.mousePosition - _cam.WorldToScreenPoint(transform.position);
    }

    public void TakeDamage(int damage)
    {
        _damageSound.Play();
        _health = _health > damage ? _health - damage : 0;
        if (_health == 0)
        {
            Die();
        }
        _healthText.text = _health.ToString() + " / " + _maxHealth;
        _outOfCombatTimer = 3;
    }

    private void Die()
    {
        FindObjectOfType<GameSession>().ProcessGameOver();
    }

    private void FaceCursor()
    {
        transform.rotation = Quaternion.AngleAxis((Mathf.Atan2(GetDirToCursor().y, GetDirToCursor().x) * Mathf.Rad2Deg) - 90, Vector3.forward);
    }

    private void AttachCamera()
    {
        _cam.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1);
    }

    private bool TryEquipWeapon(Weapon newWeapon)
    {
        if (_weapon1 != null && _weapon2 != null) return false; // <-- Two weapons are already equipped
        if (_weapon1 == null) // <-- weapon1 slot is free
        {
            if (_weapon2 != null && newWeapon.GetType() == _weapon2.GetType()) return false; // <-- equipped weapon2 is same weapon

            _weapon1 = newWeapon;
            _weapon1.Equip();
            _leftWeaponHUDVisual.sprite = _weapon1.Sprite;
            _leftWeaponHUDVisual.color = Color.white;
            return true;
        }
        else if (_weapon2 == null) // <-- weapon1 is occupied, but weapon2 is free
        {
            if (_weapon1 != null && newWeapon.GetType() == _weapon1.GetType()) return false; // <-- equipped weapon1 is same weapon

            _weapon2 = newWeapon;
            _weapon2.Equip();
            _rightWeaponHUDVisual.sprite = _weapon2.Sprite;
            _rightWeaponHUDVisual.color = Color.white;
            _weapon2.gameObject.SetActive(false);
            return true;
        }
        return false;
    }

    public void SwapWeapons()
    {
        if (_weapon1 != null)
        {
            if (_weapon2 != null)
            {
                // Has _weapon1 & _weapon2 > swap _weapon1 & _weapon2
                (_weapon1, _weapon2) = (_weapon2, _weapon1);
                _weapon2.Disable();
                _rightWeaponHUDVisual.sprite = _weapon2.Sprite;
                _leftWeaponHUDVisual.sprite = _weapon1.Sprite;
                _weapon2.gameObject.SetActive(false);
                _weapon1.gameObject.SetActive(true);
            }
            else
            {
                // Has only _weapon1 > move to _weapon2
                _weapon2 = _weapon1;
                _weapon1 = null;
                _rightWeaponHUDVisual.sprite = _weapon2.Sprite;
                _leftWeaponHUDVisual.sprite = null;
                _weapon2.gameObject.SetActive(false);
            }
        }
        else if (_weapon2 != null)
        {
            // Has only _weapon2 > move to _weapon1
            _weapon2.gameObject.SetActive(false);
            _weapon1 = _weapon2;
            _weapon2 = null;
            _rightWeaponHUDVisual.sprite = null;
            _leftWeaponHUDVisual.sprite = _weapon1.Sprite;
            _weapon1.gameObject.SetActive(true);
        }

        _rightWeaponHUDVisual.color = (_rightWeaponHUDVisual.sprite == null) ? _emptyWeaponSlotColor : Color.white;
        _leftWeaponHUDVisual.color = (_leftWeaponHUDVisual.sprite == null) ? _emptyWeaponSlotColor : Color.white;
    }

    private void Player_OnSkillUnlocked(object sender, Skills.OnSkillUnlockedEventArgs e)
    {
        switch (e._skillType)
        {
            case SkillType.Dash:
                break;
            case SkillType.Heal:
                break;
            case SkillType.Health1:
                _maxHealth += 50;
                break;
            case SkillType.Health2:
                _maxHealth += 50;
                break;
            case SkillType.Health3:
                _maxHealth += 50;
                break;
            case SkillType.Speed1:
                _moveSpeed *= 1.2f;
                break;
            case SkillType.Speed2:
                _moveSpeed *= 1.2f;
                break;
            case SkillType.Earthquake:
                break;
            default:
                break;
        }
        _healthText.text = _health.ToString() + " / " + _maxHealth;
    }

    private IEnumerator HandleDashCR()
    {
        _canDash = false;
        _isDashing = true;
        yield return _dashingTime;
        _isDashing = false;
        yield return _dashCooldown;
        _canDash = true;
    }

    private IEnumerator HandleHealCR()
    {
        if (_isHealing) yield break;
        _isHealing = true;
        _health++;
        _healthText.text = _health.ToString() + " / " + _maxHealth;
        yield return _healCooldown;
        _isHealing = false;
    }

    private IEnumerator UpdateTilesCR()
    {
        _isUpdating = true;
        _dungeonGen.LoadUnloadTiles(transform.position);
        yield return _updateTilesCooldown;
        _isUpdating = false;
    }

    public void Load()
    {
        _leftWeaponHUDVisual.sprite = null;
        _rightWeaponHUDVisual.sprite = null;
        _leftWeaponHUDVisual.color = _emptyWeaponSlotColor;
        _rightWeaponHUDVisual.color = _emptyWeaponSlotColor;

        if (GameData.Instance.Player.Weapon1Name != WeaponName.None) // <-- Player had a weapon1
        {
            foreach (var weaponPrefab in _weaponPrefabs)
            {
                if (weaponPrefab.GetComponentInChildren<Weapon>().WeaponName != GameData.Instance.Player.Weapon1Name) continue;
                _weapon1 = Instantiate(weaponPrefab).GetComponentInChildren<Weapon>();
                break;
            }
            _weapon1.Equip();
            _leftWeaponHUDVisual.sprite = _weapon1.Sprite;
            _leftWeaponHUDVisual.color = Color.white;
        }

        if (GameData.Instance.Player.Weapon2Name != WeaponName.None) // <-- Player had a weapon2
        {
            foreach (var weaponPrefab in _weaponPrefabs)
            {
                if (weaponPrefab.GetComponentInChildren<Weapon>().WeaponName != GameData.Instance.Player.Weapon2Name) continue;
                _weapon2 = Instantiate(weaponPrefab).GetComponentInChildren<Weapon>();
                break;
            }
            _weapon2.Equip();
            _rightWeaponHUDVisual.sprite = _weapon2.Sprite;
            _rightWeaponHUDVisual.color = Color.white;
            _weapon2.gameObject.SetActive(false);
        }

        transform.position = new Vector3(GameData.Instance.Player.Position[0], GameData.Instance.Player.Position[1], GameData.Instance.Player.Position[2]);
        Skills = new Skills(GameData.Instance.Player.UnlockedSkillTypes);
        _maxHealth = GameData.Instance.Player.MaxHealth;
        _health = GameData.Instance.Player.Health;
        _moveSpeed = GameData.Instance.Player.MoveSpeed;
        Kills = GameData.Instance.Player.Kills;
        _dungeonGen.LoadUnloadTiles(transform.position);
        _healthText.text = _health.ToString() + " / " + _maxHealth;
    }

    public void Save()
    {
        GameData.Instance.Player.Position = new float[] {
            transform.position.x,
            transform.position.y,
            transform.position.z };
        GameData.Instance.Player.UnlockedSkillTypes = Skills.GetUnlockedSkillTypes();
        GameData.Instance.Player.MaxHealth = _maxHealth;
        GameData.Instance.Player.Health = _health;
        GameData.Instance.Player.MoveSpeed = _moveSpeed;
        GameData.Instance.Player.Weapon1Name = _weapon1 != null ? _weapon1.WeaponName : WeaponName.None;
        GameData.Instance.Player.Weapon2Name = _weapon2 != null ? _weapon2.WeaponName : WeaponName.None;
        GameData.Instance.Player.Kills = Kills;
    }
}