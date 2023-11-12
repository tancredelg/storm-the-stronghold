using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Skills
{
    public class OnSkillUnlockedEventArgs : EventArgs { public SkillType _skillType; }
    public event EventHandler<OnSkillUnlockedEventArgs> OnSkillUnlocked;

    private List<SkillType> _unlockedSkillTypes;

    public Skills()
    {
        _unlockedSkillTypes = new List<SkillType>();
    }

    public Skills(List<SkillType> unlockedSkills)
    {
        _unlockedSkillTypes = new List<SkillType>(unlockedSkills);
    }

    public bool Has(SkillType skillType)
    {
        return _unlockedSkillTypes.Contains(skillType);
    }

    public void UnlockSkill(SkillType skillType)
    {
        if (Has(skillType)) return;
        _unlockedSkillTypes.Add(skillType);
        OnSkillUnlocked?.Invoke(this, new OnSkillUnlockedEventArgs { _skillType = skillType });
    }

    public List<SkillType> GetUnlockedSkillTypes()
    {
        return _unlockedSkillTypes;
    }
}
