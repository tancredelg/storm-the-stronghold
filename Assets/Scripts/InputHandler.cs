using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private PlayerController _playerController;
    private GameSession _gameSession;

    private void Start()
    {
        _playerController = FindObjectOfType<PlayerController>();
        _gameSession = GetComponent<GameSession>();
    }

    private void Update()
    {
        if (!Input.anyKey) return;

        if (Time.timeScale > 0)
        {
            // Left click: Attack
            if (Input.GetMouseButtonDown(0))
            {
                _playerController.TryAttack();
            }

            // G: Drop weapon
            if (Input.GetKeyDown(KeyCode.G))
            {
                _playerController.TryDropWeapon();
            }

            // E: Interact
            if (Input.GetKeyDown(KeyCode.E))
            {
                _playerController.TryInteract();
            }

            // Q: Swap primary and secondary weapons
            if (Input.GetKeyDown(KeyCode.Q))
            {
                _playerController.SwapWeapons();
            }

            // Space: Dash
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _playerController.TryDash();
            }
        }

        // Y: Open skill tree
        if (Input.GetKeyDown(KeyCode.Y))
        {
            _gameSession.OpenSkillTree();
        }

        // Esc: Toggle pause screen or close skill tree
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_gameSession.SkillTreeIsOpen)
            {
                _gameSession.CloseSkillTree();
            }
            else if (_gameSession.IsPaused)
            {
                _gameSession.Resume();
            }
            else
            {
                _gameSession.Pause();
            }
        }
    }
}
