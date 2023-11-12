using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillTree : MonoBehaviour, IDragHandler
{
    private Canvas _canvas;
    private RectTransform _rectTransform;
    private AudioSource _unlockSound;
    private PlayerLevelManager _playerLevelMgr;
    private PlayerController _playerController;
    private List<SkillNode> _skillNodes;

    private void Awake()
    {
        _canvas = transform.parent.GetComponent<Canvas>();
        _playerLevelMgr = FindObjectOfType<PlayerLevelManager>();
        _playerController = FindObjectOfType<PlayerController>();
        _rectTransform = GetComponent<RectTransform>();
        _skillNodes = new List<SkillNode>(GetComponentsInChildren<SkillNode>());
        _skillNodes.Where(n => n.PreviousNode != null && n.PreviousNode.Unlocked).ToList()
                   .ForEach(n => n.UpdateNode());
        _unlockSound = GetComponent<AudioSource>();
    }

    public bool TryUnlockNode(SkillNode skillNode)
    {
        if (_playerLevelMgr.GetSkillPoints() > 0)
        {
            _playerLevelMgr.DecreaseSkillPoints();
            _playerController.Skills.UnlockSkill(skillNode.SkillType);
            _skillNodes.Where(n => n.PreviousNode == skillNode).ToList().ForEach(n => n.UpdateNode());
            _unlockSound.Play();
            return true;
        }
        return false;
    }

    private void UnlockLoadedNode(SkillNode skillNode)
    {
        skillNode.Unlocked = true;
        skillNode.UpdateNode();
        _skillNodes.Where(n => n.PreviousNode == skillNode).ToList().ForEach(n => n.UpdateNode());
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_rectTransform.anchoredPosition.x + eventData.delta.x - _rectTransform.rect.width / 2 > -_canvas.pixelRect.width / 2
            || _rectTransform.anchoredPosition.x + eventData.delta.x + _rectTransform.rect.width / 2 + 420 < _canvas.pixelRect.width / 2
            || _rectTransform.anchoredPosition.y + eventData.delta.y - _rectTransform.rect.height / 2 > -_canvas.pixelRect.height / 2
            || _rectTransform.anchoredPosition.y + eventData.delta.y + _rectTransform.rect.height / 2 < _canvas.pixelRect.height / 2) return;

        _rectTransform.anchoredPosition += eventData.delta;
    }

    public void UpdateSkillTreeOnLoad()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<SkillNode>() != null) continue;
            Destroy(child.gameObject);
        }

        foreach (var node in _skillNodes)
        {
            if (node.PreviousNode == null || !_playerController.Skills.Has(node.SkillType)) continue;
            UnlockLoadedNode(node);
        }

        _skillNodes.Where(n => n.PreviousNode != null && n.PreviousNode.Unlocked).ToList()
                   .ForEach(n => n.UpdateNode());
    }

    public void Open()
    {
        if (_canvas.enabled) return;
        _canvas.enabled = true;
        Time.timeScale = 0;
    }

    public void Close()
    {
        if (!_canvas.enabled) return;
        _canvas.enabled = false;
        Time.timeScale = 1;
    }
}
