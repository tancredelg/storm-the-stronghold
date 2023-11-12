using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ExperienceBar : MonoBehaviour
{
    private LevelSystem _levelSys;
    private Image _bar;
    private TextMeshProUGUI _levelText;

    private void Start()
    {
        _bar = transform.GetChild(1).GetComponent<Image>();
        _levelText = GetComponentInChildren<TextMeshProUGUI>();
        print("_bar.fillAmount = " + _levelSys.ExpNormalized);
        _bar.fillAmount = _levelSys.ExpNormalized;
        SetLevelText(_levelSys.Level);
    }

    public void SetLevelSystem(LevelSystem levelSystem) => _levelSys = levelSystem;
    
    public void UpdateExperienceBar()
    {
        //_bar.fillAmount = _levelSys.ExpNormalized;
        //SetLevelText(_levelSys.Level);
        StartCoroutine(UpdateExperienceBarCR());
    }

    public IEnumerator UpdateExperienceBarCR()
    {
        float start;

        if (_levelSys.Level - _levelSys.LevelBeforeNewExp > 1)
        {
            // skipped at least 1 whole level, so increase through multiple levels
            for (int i = _levelSys.LevelBeforeNewExp + 1; i <= _levelSys.Level; i++)
            {
                start = _bar.fillAmount;
                for (_bar.fillAmount = start; _bar.fillAmount < 1; _bar.fillAmount += Time.deltaTime * 5) yield return null;
                _bar.fillAmount = 1;

                for (_bar.fillAmount = 1; _bar.fillAmount > 0; _bar.fillAmount -= Time.deltaTime * 5) yield return null;
                _bar.fillAmount = 0;
                SetLevelText(i);
            }
        }
        if (_levelSys.ExpNormalized == 0)
        {
            // go to the end of the bar and then back to 0 on the next level
            start = _bar.fillAmount;

            for (_bar.fillAmount = start; _bar.fillAmount < 1; _bar.fillAmount += Time.deltaTime * 2) yield return null;
            _bar.fillAmount = 1;

            for (_bar.fillAmount = 1; _bar.fillAmount > 0; _bar.fillAmount -= Time.deltaTime * 5) yield return null;
            _bar.fillAmount = 0;

            SetLevelText(_levelSys.Level);
        }
        else if (_bar.fillAmount > _levelSys.ExpNormalized)
        {
            // go to the end of the bar and then increase on the next level
            start = _bar.fillAmount;

            for (_bar.fillAmount = start; _bar.fillAmount < 1; _bar.fillAmount += Time.deltaTime * 5) yield return null;
            _bar.fillAmount = 1;

            for (_bar.fillAmount = 1; _bar.fillAmount > 0; _bar.fillAmount -= Time.deltaTime * 5) yield return null;
            _bar.fillAmount = 0;

            SetLevelText(_levelSys.Level);

            start = 0;

            for (_bar.fillAmount = start; _bar.fillAmount < _levelSys.ExpNormalized; _bar.fillAmount += Time.deltaTime * 2) yield return null;
            _bar.fillAmount = _levelSys.ExpNormalized;
        }
        else
        {
            // increase normally
            start = _bar.fillAmount;

            for (_bar.fillAmount = start; _bar.fillAmount < _levelSys.ExpNormalized; _bar.fillAmount += Time.deltaTime * 2) yield return null;
            _bar.fillAmount = _levelSys.ExpNormalized;
        }
    }

    private void SetLevelText(int levelNum) => _levelText.text = levelNum.ToString();
}
