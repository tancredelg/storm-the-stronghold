using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerLevelManager : MonoBehaviour, ISavableData
{
    private LevelSystem _levelSys;
    private ExperienceBar _expBar;
    private TextMeshProUGUI _skillPointsOnHUDText, _skillPointsOnTreeText;
    private int _skillPoints, _nextMilestone;

    private const int LEVELS_TO_NEW_MILESTONE = 1;

    private void Start()
    {
        _skillPointsOnHUDText = GameObject.Find("SkillPointsHUDText").GetComponent<TextMeshProUGUI>();
        _skillPointsOnTreeText = GameObject.Find("SkillPointsTreeText").GetComponent<TextMeshProUGUI>();
        _expBar = GameObject.Find("ExperienceBar").GetComponent<ExperienceBar>();
        _levelSys = new LevelSystem();
        _expBar.SetLevelSystem(_levelSys);
        _nextMilestone = LEVELS_TO_NEW_MILESTONE;
    }

    public int GetSkillPoints() => _skillPoints;

    public void OnEnemyKilled(int exp)
    {
        _levelSys.AddExp(exp);
        if (_levelSys.Level - _nextMilestone >= 0)
        {
            // Milestone reached, player gets a skill point
            _skillPoints++;
            _nextMilestone += LEVELS_TO_NEW_MILESTONE;
            _skillPointsOnHUDText.text = "Skill Points: " + _skillPoints.ToString();
            _skillPointsOnTreeText.text = _skillPoints.ToString();
        }
        _expBar.UpdateExperienceBar();
    }

    public void DecreaseSkillPoints()
    {
        _skillPoints--;
        _skillPointsOnHUDText.text = "Skill Points: " + _skillPoints.ToString();
        _skillPointsOnTreeText.text = _skillPoints.ToString();
    }

    public void Load()
    {
        _levelSys = new LevelSystem(GameData.Instance.PlayerLevel.LevelSystemData);
        _skillPoints = GameData.Instance.PlayerLevel.SkillPoints;
        _nextMilestone = GameData.Instance.PlayerLevel.NextMilestone;
        _expBar.SetLevelSystem(_levelSys);

        FindObjectOfType<SkillTree>().UpdateSkillTreeOnLoad();
        _expBar.UpdateExperienceBar();
        _skillPointsOnHUDText.text = "Skill Points: " + _skillPoints.ToString();
        _skillPointsOnTreeText.text = _skillPoints.ToString();
    }

    public void Save()
    {
        GameData.Instance.PlayerLevel.LevelSystemData = _levelSys.GetSaveData();
        GameData.Instance.PlayerLevel.SkillPoints = _skillPoints;
        GameData.Instance.PlayerLevel.NextMilestone = _nextMilestone;
    }
}