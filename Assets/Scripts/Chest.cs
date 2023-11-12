using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public bool IsLooted { get; private set; }
    public GameObject Loot { get; private set; }

    [SerializeField] private List<GameObject> _projectileLootPool, _meleeLootPool;
    private PlayerController _playerController;
    private AudioSource _openSound;

    private void Awake()
    {
        _playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        _openSound = GetComponent<AudioSource>();
    }

    private void OnMouseEnter()
    {
        if (IsLooted) return;
        _playerController.SetGameObjectOnCursor(gameObject);
    }

    private void OnMouseExit()
    {
        if (!IsLooted) return;
        _playerController.SetGameObjectOnCursor(null);
    }

    public void GenerateLoot(RoomType lootType)
    {
        switch (lootType)
        {
            case RoomType.Start:
                Loot = _meleeLootPool[Random.Range(0, _meleeLootPool.Count)];
                break;
            case RoomType.Melee:
                Loot = _meleeLootPool[Random.Range(0, _meleeLootPool.Count)];
                break;
            case RoomType.Projectile:
                Loot = _projectileLootPool[Random.Range(0, _projectileLootPool.Count)];
                break;
            default:
                break;
        }
    }

    public void Open()
    {
        if (IsLooted) return;
        _openSound.Play();
        Loot = Instantiate(Loot, transform.position, Quaternion.identity);
        Loot.GetComponentInChildren<Weapon>().Drop();
        IsLooted = true;
    }
}
