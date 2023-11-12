using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SkillType SkillType;
    public SkillNode PreviousNode;
    public bool Unlocked;

    [SerializeField] private GameObject _connectionPrefab;
    private SkillTree _skillTree;
    private Image _skillNodeImage, _skillImage;
    private TextMeshProUGUI _skillNameText, _skillDescText;
    private AudioSource _hoverSound, _clickSound;
    private readonly Dictionary<SkillType, string> _skillDescritions = new Dictionary<SkillType, string>()
    {
        [SkillType.Dash] = "Make your character charge quickly in the direction you are aiming.\n\nUse by pressing 'space'.\n\n2s cooldown.",
        [SkillType.Heal] = "Let your character slowly regain health while not in combat.\n\nPassive effect, no input required.\n\n3s after exit from combat.",
        [SkillType.Health1] = "Increase your character health by 50hp.\n\nSingle use statistical upgrade.",
        [SkillType.Health2] = "Increase your character health by a further 50hp.\n\nSingle use statistical upgrade.",
        [SkillType.Health3] = "Increase your character health by a further 50hp.\n\nSingle use statistical upgrade.",
        [SkillType.Speed1] = "Increase your character's movement speed by 20%.\n\nSingle use statistical upgrade.",
        [SkillType.Speed2] = "Increase your character's movement speed by a further 20%.\n\nSingle use statistical upgrade.",
        [SkillType.Earthquake] = "A crowd control skill which stuns any enemy in a short radius of your character.\n\nUse by pressing '1'\n\n10s cooldown",
    };

    private void Awake()
    {
        _skillTree = transform.parent.GetComponent<SkillTree>();
        _skillNodeImage = GetComponent<Image>();
        _skillImage = transform.GetChild(0).GetComponent<Image>();
        _skillNameText = GameObject.Find("SkillNameText").GetComponent<TextMeshProUGUI>();
        _skillDescText = GameObject.Find("SkillDescText").GetComponent<TextMeshProUGUI>();
        _skillNameText.text = "";
        _skillDescText.text = "";

        var sounds = GetComponents<AudioSource>();
        _hoverSound = sounds[0];
        _clickSound = sounds[1];

        if (PreviousNode != null)
        {
            _skillNodeImage.color = new Color(0.56f, 0.56f, 0.56f);
            _skillImage.color = new Color(1, 1, 1, 0.4f);
        }
        else
        {
            Unlocked = true;
        }
    }

    public void OnClick()
    {
        _clickSound.Play();
        if (Unlocked || !PreviousNode.Unlocked) return;
        if (_skillTree.TryUnlockNode(this))
        {
            Unlocked = true;
            UpdateNode();
        }
    }
    
    public void UpdateNode()
    {
        _skillNodeImage = GetComponent<Image>();
        _skillImage = transform.GetChild(0).GetComponent<Image>();
        if (Unlocked)
        {
            _skillNameText.text += " [unlocked]";
            _skillNodeImage.color = Color.white;
            _skillImage.color = Color.white;
        }
        else if (PreviousNode != null && PreviousNode.Unlocked)
        {
            _skillNodeImage.color = new Color(0.76f, 0.76f, 0.76f);
            _skillImage.color = new Color(1, 1, 1, 0.8f);
        }
        CreateConnection();
    }
    
    public Vector3 GetPosition() => GetComponent<RectTransform>().anchoredPosition;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (PreviousNode == null) return;
        _hoverSound.Play();
        _skillNameText.text = SkillType.ToString().ToUpper() + (Unlocked ? " [unlocked]" : "");
        _skillDescText.text = _skillDescritions[SkillType];
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _skillNameText.text = "";
        _skillDescText.text = "Hover over a skill to find out more information about it.";
    }

    private void CreateConnection()
    {
        var newConnection = Instantiate(_connectionPrefab, transform.parent);
        newConnection.transform.SetAsFirstSibling();
        var connectionImage = newConnection.GetComponent<Image>();
        connectionImage.rectTransform.anchoredPosition = new Vector3((GetPosition().x + PreviousNode.GetPosition().x) * 0.5f, (GetPosition().y + PreviousNode.GetPosition().y) * 0.5f, 0);
        connectionImage.rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2((GetPosition() - PreviousNode.GetPosition()).y, (GetPosition() - PreviousNode.GetPosition()).x) * Mathf.Rad2Deg);

        if (Unlocked)
        {
            connectionImage.color = new Color(1, 1, 1, 0.6f);
            connectionImage.rectTransform.sizeDelta = new Vector2(Vector2.Distance(GetPosition(), PreviousNode.GetPosition()), 12);
        }
        else
        {
            connectionImage.color = new Color(1, 1, 1, 0.04f);
            connectionImage.rectTransform.sizeDelta = new Vector2(Vector2.Distance(GetPosition(), PreviousNode.GetPosition()), 8);
        }
        //print($"[{name} to {PreviousNode.name}] Length of the connection 'line': {Vector2.Distance(GetPosition(), PreviousNode.GetPosition())}");
    }
}
