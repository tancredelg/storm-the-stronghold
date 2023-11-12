using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInteractable : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    private AudioSource _hoverSource, _clickSource;

    private void Awake()
    {
        _hoverSource = GetComponent<AudioSource>();
        _clickSource = SceneLoader.Instance.GetComponent<AudioSource>();
    }

    public void LoadScene(string sceneName)
    {
        SceneLoader.Instance.LoadScene(sceneName);
    }

    public void LoadNewGame()
    {
        SceneLoader.Instance.LoadNewGame();
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hoverSource.Play();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _clickSource.Play();
    }
}
