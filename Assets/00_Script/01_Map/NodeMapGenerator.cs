using System;
using System.Collections.Generic;
using UnityEngine;

public class NodeMapGenerator : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private NodeMapConfig config;

    [Header("Spawn")]
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Transform nodeRoot;

    [Header("Receiver")]
    [SerializeField] private NodeMapManager mapManager;

    [Header("Generate On Start")]
    [SerializeField] private bool generateOnStart = true;

    [Header("Optional View")]
    [SerializeField] private NodeMapLineDrawer lineDrawer;

    private void Start()
    {
        if (generateOnStart)
            GenerateAndApply();
    }

    [ContextMenu("Generate And Apply")]
    public void GenerateAndApply()
    {
        if (config == null || nodePrefab == null || nodeRoot == null || mapManager == null) return;

        int seed = config.useFixedSeed ? config.fixedSeed : Environment.TickCount;
        var rng = new System.Random();

        ClearChildren(nodeRoot);

        var depthNodes = SpawnNodes(rng, config, nodePrefab, nodeRoot, out int startNodeId);

        // 2) Build graph (depth -> next depth)
        var graph = BuildGraph(rng, config, depthNodes);

        // 3) Flatten nodesById
        var nodesById = new Dictionary<int, Node>();
        foreach (var layer in depthNodes)
        {
            foreach (var node in layer)
                nodesById[node.Id] = node;
        }

        // 4) Apply to manager
        mapManager.ApplyGeneratedMap(nodesById, graph, startNodeId);

        if (lineDrawer != null)
            lineDrawer.DrawLines(nodesById, graph);

        Debug.Log($"[NodeMapGenerator] Generated map. seed={seed}, nodes={nodesById.Count}");
    }

    private static void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);
    }

    private static List<List<Node>> SpawnNodes(System.Random rng, NodeMapConfig cfg, Node prefab, Transform root, out int startNodeId)
    {
        var result = new List<List<Node>>();
        int idCounter = 0;
        startNodeId = -1;

        int depthCount =
            (cfg.depthCounts == null || cfg.depthCounts.Length == 0)
            ? 1
            : cfg.depthCounts.Length;

        for (int d = 0; d < depthCount; d++)
        {
            int count =
                (cfg.depthCounts != null && d < cfg.depthCounts.Length)
                ? Mathf.Max(1, cfg.depthCounts[d])
                : 1;

            // =========================
            // 왼쪽 -> 오른쪽 배치 핵심
            // =========================
            float x = d * cfg.xSpacing;                 // depth = X축
            float height = (count - 1) * cfg.ySpacing;  // 같은 depth의 총 높이
            float startY = height * 0.5f;               // 위에서 아래로 정렬

            var layer = new List<Node>(count);

            for (int i = 0; i < count; i++)
            {
                var node = UnityEngine.Object.Instantiate(prefab, root);

                // UI(Canvas) 기준이면 anchoredPosition이 더 정확
                if (node.transform is RectTransform rect)
                {
                    rect.anchoredPosition =
                        new Vector2(x, startY - i * cfg.ySpacing);
                }
                else
                {
                    node.transform.localPosition =
                        new Vector3(x, startY - i * cfg.ySpacing, 0f);
                }

                NodeType type = PickNodeType(rng, cfg, d, i, depthCount);

                node.SetData(idCounter, d, i, type);
                node.Initialize();

                if (type == NodeType.Start)
                    startNodeId = idCounter;

                layer.Add(node);
                idCounter++;
            }

            result.Add(layer);
        }

        // Start 강제: depth 0의 첫 노드
        if (cfg.forceStartNode && result.Count > 0 && result[0].Count > 0)
        {
            var first = result[0][0];
            first.SetData(first.Id, 0, 0, NodeType.Start);
            startNodeId = first.Id;
        }

        // Boss 강제: 마지막 depth의 가운데 노드
        if (cfg.forceLastDepthBoss && result.Count > 0)
        {
            var lastLayer = result[^1];
            int mid = lastLayer.Count / 2;
            var boss = lastLayer[mid];
            boss.SetData(boss.Id, boss.Depth, boss.IndexInDepth, NodeType.Boss);
        }

        // Start 안전장치
        if (startNodeId < 0 && result.Count > 0 && result[0].Count > 0)
        {
            var fallback = result[0][0];
            fallback.SetData(fallback.Id, fallback.Depth, fallback.IndexInDepth, NodeType.Start);
            startNodeId = fallback.Id;
        }

        return result;
    }


    private static NodeType PickNodeType(System.Random rng, NodeMapConfig cfg, int depth, int index, int depthCount)
    {
        // Start/Boss는 외부에서 강제 처리하므로 여기선 일반 타입만
        int total = Mathf.Max(0, cfg.battleWeight) + Mathf.Max(0, cfg.shopWeight) + Mathf.Max(0, cfg.restWeight) + Mathf.Max(0, cfg.eliteWeight);
        if (total <= 0) return NodeType.Battle;

        int roll = rng.Next(0, total);
        int acc = 0;

        acc += Mathf.Max(0, cfg.battleWeight);
        if (roll < acc) return NodeType.Battle;

        acc += Mathf.Max(0, cfg.shopWeight);
        if (roll < acc) return NodeType.Shop;

        acc += Mathf.Max(0, cfg.restWeight);
        if (roll < acc) return NodeType.Rest;

        return NodeType.Elite;
    }

    private static Dictionary<int, HashSet<int>> BuildGraph(System.Random rng, NodeMapConfig cfg, List<List<Node>> depthNodes)
    {
        var graph = new Dictionary<int, HashSet<int>>();

        foreach (var layer in depthNodes)
            foreach (var node in layer)
                graph[node.Id] = new HashSet<int>();

        for (int d = 0; d < depthNodes.Count - 1; d++)
        {
            var fromLayer = depthNodes[d];
            var toLayer = depthNodes[d + 1];

            int fromCount = fromLayer.Count;
            int toCount = toLayer.Count;

            // ====== [예외 규칙] 첫 번째 depth는 "전부 연결" ======
            // Start(보통 depth0의 첫 노드) -> depth1의 모든 노드에 연결
            // (max 2 규칙은 이 구간에서만 예외)
            if (d == 0)
            {
                // Start를 depth0의 "Start 타입"에서 찾고, 없으면 첫 노드 사용
                Node start = null;
                for (int i = 0; i < fromLayer.Count; i++)
                {
                    if (fromLayer[i].Type == NodeType.Start)
                    {
                        start = fromLayer[i];
                        break;
                    }
                }
                start ??= fromLayer[0];

                for (int j = 0; j < toCount; j++)
                    graph[start.Id].Add(toLayer[j].Id);

                // depth0에 노드가 여러 개 있는 설정을 쓴다면,
                // 나머지 from 노드는 최소 1개 연결만 보장 (선 꼬임 없이)
                for (int i = 0; i < fromCount; i++)
                {
                    var from = fromLayer[i];
                    if (from == start) continue;

                    int mapped = (fromCount == 1) ? (toCount / 2)
                        : Mathf.RoundToInt(i * (toCount - 1) / (float)(fromCount - 1));
                    mapped = Mathf.Clamp(mapped, 0, toCount - 1);
                    graph[from.Id].Add(toLayer[mapped].Id);
                }

                // 다음 depth로 넘어감
                continue;
            }

            // ====== 일반 규칙(꼬임 방지 + 2개 제한 + 노드 줄면 분기 줄임) ======
            var fromOut = new int[fromCount];
            var toIn = new int[toCount];

            int[] baseIndex = new int[fromCount];
            int prev = 0;

            for (int i = 0; i < fromCount; i++)
            {
                int mapped;
                if (fromCount == 1) mapped = toCount / 2;
                else mapped = Mathf.RoundToInt(i * (toCount - 1) / (float)(fromCount - 1));

                mapped = Mathf.Clamp(mapped, 0, toCount - 1);
                if (mapped < prev) mapped = prev;   // 단조 증가 => 교차 방지
                prev = mapped;

                baseIndex[i] = mapped;
            }

            bool AddEdge(int fi, int tj)
            {
                if (fi < 0 || fi >= fromCount) return false;
                if (tj < 0 || tj >= toCount) return false;

                if (fromOut[fi] >= 2) return false; // 최대 2개 제한

                int fromId = fromLayer[fi].Id;
                int toId = toLayer[tj].Id;

                if (graph[fromId].Add(toId))
                {
                    fromOut[fi]++;
                    toIn[tj]++;
                    return true;
                }
                return false;
            }

            // 1) 기본 1개 연결
            for (int i = 0; i < fromCount; i++)
                AddEdge(i, baseIndex[i]);

            // 2) 고립 방지 (toIn==0 없게)
            for (int tj = 0; tj < toCount; tj++)
            {
                if (toIn[tj] > 0) continue;

                int fiClosest = (toCount == 1) ? 0
                    : Mathf.RoundToInt(tj * (fromCount - 1) / (float)(toCount - 1));
                fiClosest = Mathf.Clamp(fiClosest, 0, fromCount - 1);

                bool linked = false;

                for (int delta = 0; delta <= Mathf.Max(fromCount, 6); delta++)
                {
                    int fiA = fiClosest - delta;
                    int fiB = fiClosest + delta;

                    if (TryLink(fiA, tj)) { linked = true; break; }
                    if (TryLink(fiB, tj)) { linked = true; break; }
                }

                bool TryLink(int fi, int targetTo)
                {
                    if (fi < 0 || fi >= fromCount) return false;
                    int b = baseIndex[fi];

                    // 교차/꼬임 방지: baseIndex 주변(±1)만 허용
                    if (Mathf.Abs(targetTo - b) > 1) return false;
                    return AddEdge(fi, targetTo);
                }

                if (!linked)
                    AddEdge(fiClosest, tj);
            }

            // 3) 분기(두 번째 연결)는 "노드 수가 줄지 않을 때"만 적극 허용
            bool allowBranch = toCount >= fromCount;

            if (allowBranch)
            {
                for (int i = 0; i < fromCount; i++)
                {
                    if (fromOut[i] >= 2) continue;

                    // 확률 분기(필요하면 cfg로 뺄 수 있음)
                    if (rng.NextDouble() > 0.45) continue;

                    int b = baseIndex[i];

                    int up = b - 1;
                    int dn = b + 1;

                    int candidate = -1;
                    if (up >= 0 && dn < toCount)
                        candidate = (toIn[up] <= toIn[dn]) ? up : dn;
                    else if (up >= 0) candidate = up;
                    else if (dn < toCount) candidate = dn;

                    if (candidate >= 0)
                        AddEdge(i, candidate); // 인접만 => (두 도착 노드 id 차이 1)
                }
            }
            // else: 노드가 줄어드는 구간은 1개 연결 위주 -> 라인도 자연스럽게 줄어듦
        }

        return graph;
    }
}
