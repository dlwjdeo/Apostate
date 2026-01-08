using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeMapManager : MonoBehaviour
{
    [SerializeField] private PlayerIcon playerIcon;
    [SerializeField] private Transform nodeRoot;

    private Dictionary<int, Node> nodes = new Dictionary<int, Node>();
    private Node currentNode;

    private void Awake()
    {
        LoadNodes();
    }

    private void Start()
    {
        SetStartNode();
        UpdateAvailableNodes();
    }

    private void LoadNodes()
    {
        foreach (Transform child in nodeRoot)
        {
            Node node = child.GetComponent<Node>();
            nodes.Add(node.data.id, node);

            node.OnClick = OnNodeClicked;
        }
    }

    private void SetStartNode()
    {
        currentNode = nodes.Values.First(n => n.data.type == NodeType.Start);
        playerIcon.MoveTo(currentNode.transform.position);
    }

    private void OnNodeClicked(Node clicked)
    {
        if (!IsConnected(currentNode, clicked)) return;

        currentNode = clicked;
        playerIcon.MoveTo(clicked.transform.position);
        UpdateAvailableNodes();
    }

    private bool IsConnected(Node from, Node to)
    {
        return from.data.connectedNodeIds.Contains(to.data.id);
    }

    private void UpdateAvailableNodes()
    {
        foreach (var node in nodes.Values)
        {
            bool connected = IsConnected(currentNode, node);
            bool isCurrent = node == currentNode;

            node.SetLocked(!(connected || isCurrent));
        }
    }
}
