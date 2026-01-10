using System;
using UnityEngine;
using UnityEngine.UI;

public class Node : MonoBehaviour
{
    [Header("Runtime Data")]
    [SerializeField] private int id = -1;
    [SerializeField] private int depth = -1;
    [SerializeField] private int indexInDepth = -1;
    [SerializeField] private NodeType type = NodeType.Battle;

    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;

    [Header("Type Sprites")]
    [SerializeField] private Sprite startSprite;
    [SerializeField] private Sprite battleSprite;
    [SerializeField] private Sprite eliteSprite;
    [SerializeField] private Sprite shopSprite;
    [SerializeField] private Sprite restSprite;
    [SerializeField] private Sprite bossSprite;

    [Header("Visual")]
    [Range(0f, 1f)][SerializeField] private float lockedAlpha = 0.25f;
    [Range(0f, 1f)][SerializeField] private float normalAlpha = 1.0f;

    public int Id => id;
    public int Depth => depth;
    public int IndexInDepth => indexInDepth;
    public NodeType Type => type;

    public event Action<Node> OnClick;

    private void Reset()
    {
        if (button == null) button = GetComponent<Button>();
        if (iconImage == null) iconImage = GetComponentInChildren<Image>(true);
    }

    public void SetData(int newId, int newDepth, int newIndexInDepth, NodeType newType)
    {
        id = newId;
        depth = newDepth;
        indexInDepth = newIndexInDepth;
        type = newType;

        ApplyTypeSprite();
    }

    public void Initialize()
    {
        if (button == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnClick?.Invoke(this));
    }

    public void SetLocked(bool locked)
    {
        if (button != null)
            button.interactable = !locked;

        if (iconImage != null)
        {
            var c = iconImage.color;
            c.a = locked ? lockedAlpha : normalAlpha;
            iconImage.color = c;
        }
    }

    private void ApplyTypeSprite()
    {
        if (iconImage == null) return;

        iconImage.sprite = type switch
        {
            NodeType.Start => startSprite,
            NodeType.Battle => battleSprite,
            NodeType.Elite => eliteSprite,
            NodeType.Shop => shopSprite,
            NodeType.Rest => restSprite,
            NodeType.Boss => bossSprite,
            _ => battleSprite
        };
    }
}
