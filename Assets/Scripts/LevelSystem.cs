using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelSystem
{
    public int Level { get; private set; }
    public int LevelBeforeNewExp { get; private set; }
    public int TotalExp { get; private set; }
    public float ExpNormalized => (float)_currentExp / _currentLevelTotalExp;
    public float ExpToNextLevelNormalized => 1 - ExpNormalized;
    
    private int _currentLevelTotalExp, _currentExp;

    private const int EXP_FOR_FIRST_LEVEL = 200;

    public LevelSystem()
    {
        Level = 0;
        _currentLevelTotalExp = EXP_FOR_FIRST_LEVEL;
        _currentExp = 0;
        TotalExp = 0;
    }

    public LevelSystem(int[] saveData)
    {
        Level = saveData[0];
        TotalExp = saveData[1];
        _currentLevelTotalExp = saveData[2];
        _currentExp = saveData[3];
    }

    public void AddExp(int amount)
    {
        LevelBeforeNewExp = Level;
        _currentExp += amount;
        TotalExp += amount;
        while (_currentExp >= _currentLevelTotalExp)
        {
            LevelUp();
        }
    }

    public int[] GetSaveData()
    {
        return new int[] { Level, TotalExp, _currentLevelTotalExp, _currentExp };
    }

    private void LevelUp()
    {
        Level++;
        _currentExp -= _currentLevelTotalExp;
        _currentLevelTotalExp += EXP_FOR_FIRST_LEVEL / 10;
    }
}
