using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoadSaveManager : MonoBehaviour
{
    [SerializeField] private GameObject _loadButtonPrefab;

    private void Start()
    {
        var saveFiles = SerializationManager.GetSaveGameFiles();
        foreach (Transform buttonTransform in transform)
        {
            Destroy(buttonTransform.gameObject);
        }

        for (int i = 0; i < saveFiles.Count; i++)
        {
            var buttonObject = Instantiate(_loadButtonPrefab, transform);
            int index = i;
            buttonObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                SceneLoader.Instance.LoadSaveGame(saveFiles[index]);
            });
            buttonObject.GetComponentInChildren<TextMeshProUGUI>().text = "\"" + saveFiles[i].Replace(SerializationManager.SavesPath, "").Replace(".json", "") + "\"";
        }
    }
}
