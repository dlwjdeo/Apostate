using UnityEngine;
using UnityEngine.UI;
using System;

public class Node : MonoBehaviour
{
    public NodeData data;
    public Button button;

    public Action<Node> OnClick;

    private void Awake()
    {
        Initialize(data);
    }
    public void Initialize(NodeData nodeData)
    {
        data = nodeData;

        button.onClick.AddListener(() =>
        {
            OnClick?.Invoke(this);
        });
    }

    public void SetLocked(bool locked)
    {
        button.interactable = !locked;
    }
}
