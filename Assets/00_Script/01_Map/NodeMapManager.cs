using System;
using System.Collections.Generic;
using UnityEngine;

public class NodeMapManager : MonoBehaviour
{
    [Header("View")]
    [SerializeField] private PlayerIcon playerIcon;

    [Header("Confirm UI")]
    [SerializeField] private NodeEnterConfirmUI confirmUI;

    [Header("Confirm Options")]
    [SerializeField] private bool confirmBattle = true;
    [SerializeField] private bool confirmElite = true;
    [SerializeField] private bool confirmShop = true;
    [SerializeField] private bool confirmRest = true;
    [SerializeField] private bool confirmBoss = true;

    // Generator가 주입
    private readonly Dictionary<int, Node> _nodesById = new();
    private readonly Dictionary<int, HashSet<int>> _graph = new();

    private Node _currentNode;
    private readonly HashSet<int> _visited = new();

    private Node _pendingNode; // 확인 대기 노드

    //이동이 확정되면 여기로 알려줌(전투/상점 진입 연결 지점)
    public event Action<Node> OnNodeEntered;

    public void ApplyGeneratedMap(
        Dictionary<int, Node> nodesById,
        Dictionary<int, HashSet<int>> graph,
        int startNodeId)
    {
        _nodesById.Clear();
        _graph.Clear();

        foreach (var kv in nodesById) _nodesById.Add(kv.Key, kv.Value);
        foreach (var kv in graph) _graph.Add(kv.Key, kv.Value);

        BindClicks();

        if (!_nodesById.TryGetValue(startNodeId, out _currentNode) || _currentNode == null)
        {
            Debug.LogError($"[NodeMapManager] Invalid startNodeId: {startNodeId}", this);
            return;
        }

        _pendingNode = null;
        _visited.Clear();
        _visited.Add(_currentNode.Id);

        if (playerIcon != null)
            playerIcon.MoveTo(_currentNode.transform.position);

        UpdateAvailableNodes();
    }

    private void BindClicks()
    {
        foreach (var node in _nodesById.Values)
        {
            node.Initialize();
            node.OnClick -= OnNodeClicked;
            node.OnClick += OnNodeClicked;
        }
    }

    private void OnNodeClicked(Node clicked)
    {
        if (clicked == null || _currentNode == null) return;
        if (clicked == _currentNode) return;

        int fromId = _currentNode.Id;
        int toId = clicked.Id;

        // 연결돼야 함
        if (!_graph.TryGetValue(fromId, out var nexts) || !nexts.Contains(toId))
            return;

        // 뒤로가기 금지(방문한 노드 금지)
        if (_visited.Contains(toId))
            return;

        // 확인이 필요한 타입이면 "대기"로 돌리고 UI 띄움
        if (NeedsConfirm(clicked.Type) && confirmUI != null)
        {
            _pendingNode = clicked;

            string msg = $"{GetTypeName(clicked.Type)}로 이동할까요?";
            confirmUI.Show(
                msg,
                onConfirm: () =>
                {
                    if (_pendingNode != clicked) return;
                    CommitMove(clicked);
                    _pendingNode = null;
                },
                onCancel: () =>
                {
                    _pendingNode = null;
                });

            return;
        }

        CommitMove(clicked);
    }

    private void CommitMove(Node clicked)
    {
        if (clicked == null) return;

        int toId = clicked.Id;

        _currentNode = clicked;
        _visited.Add(toId);

        if (playerIcon != null)
            playerIcon.MoveTo(clicked.transform.position);

        UpdateAvailableNodes();

        OnNodeEntered?.Invoke(clicked);
    }

    private void UpdateAvailableNodes()
    {
        if (_currentNode == null) return;

        int currentId = _currentNode.Id;
        _graph.TryGetValue(currentId, out var nexts);
        nexts ??= new HashSet<int>();

        foreach (var node in _nodesById.Values)
        {
            if (node == null) continue;

            if (node == _currentNode)
            {
                node.SetLocked(true); // 현재 노드 잠금
                continue;
            }

            bool canGo = nexts.Contains(node.Id) && !_visited.Contains(node.Id);
            node.SetLocked(!canGo);
        }
    }

    private bool NeedsConfirm(NodeType type)
    {
        return type switch
        {
            NodeType.Battle => confirmBattle,
            NodeType.Elite => confirmElite,
            NodeType.Shop => confirmShop,
            NodeType.Rest => confirmRest,
            NodeType.Boss => confirmBoss,
            _ => false, // Start 등은 보통 확인 안 함
        };
    }

    private string GetTypeName(NodeType type)
    {
        return type switch
        {
            NodeType.Battle => "전투",
            NodeType.Elite => "정예 전투",
            NodeType.Shop => "상점",
            NodeType.Rest => "휴식",
            NodeType.Boss => "보스",
            NodeType.Start => "시작",
            _ => type.ToString(),
        };
    }
}
