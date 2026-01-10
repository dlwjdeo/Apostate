using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeMapLineDrawer : MonoBehaviour
{
    [Header("Root (UI)")]
    [SerializeField] private RectTransform lineRoot;

    [Header("Line Look")]
    [SerializeField] private Sprite lineSprite;
    [SerializeField] private float lineThickness = 6f;
    [SerializeField] private Color lineColor = Color.white;

    [Header("Sorting")]
    [Tooltip("노드보다 뒤로 가게 하려면 낮게")]
    [SerializeField] private int siblingIndex = 0;

    private readonly List<GameObject> _spawned = new();

    public void ClearLines()
    {
        for (int i = 0; i < _spawned.Count; i++)
        {
            if (_spawned[i] != null)
                Destroy(_spawned[i]);
        }
        _spawned.Clear();
    }

    public void DrawLines(Dictionary<int, Node> nodesById, Dictionary<int, HashSet<int>> graph)
    {
        if (lineRoot == null) return;

        ClearLines();

        foreach (var kv in graph)
        {
            int fromId = kv.Key;
            if (!nodesById.TryGetValue(fromId, out var fromNode) || fromNode == null)
                continue;

            foreach (int toId in kv.Value)
            {
                if (!nodesById.TryGetValue(toId, out var toNode) || toNode == null)
                    continue;

                CreateLine(fromNode.transform, toNode.transform);
            }
        }
    }

    private void CreateLine(Transform a, Transform b)
    {
        // UI 노드면 RectTransform 사용
        var aRect = a as RectTransform;
        var bRect = b as RectTransform;

        // lineRoot 좌표계에서의 점을 얻기 위해 InverseTransformPoint 사용
        Vector2 p1;
        Vector2 p2;

        if (aRect != null && bRect != null)
        {
            // RectTransform world position을 lineRoot local로 변환
            p1 = (Vector2)lineRoot.InverseTransformPoint(aRect.position);
            p2 = (Vector2)lineRoot.InverseTransformPoint(bRect.position);
            Debug.Log("rect transform line");
        }
        else
        {
            // 월드 오브젝트면 그냥 position으로 처리(그래도 lineRoot 기준 변환)
            p1 = (Vector2)lineRoot.InverseTransformPoint(a.position);
            p2 = (Vector2)lineRoot.InverseTransformPoint(b.position);
        }

        Vector2 dir = p2 - p1;
        float length = dir.magnitude;
        if (length < 0.001f) return;

        // 라인 오브젝트 생성
        var go = new GameObject("Line", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(lineRoot, false);

        if (siblingIndex >= 0 && siblingIndex <= lineRoot.childCount - 1)
            go.transform.SetSiblingIndex(siblingIndex);

        var rt = go.GetComponent<RectTransform>();
        var img = go.GetComponent<Image>();

        img.color = lineColor;
        if (lineSprite != null) img.sprite = lineSprite;

        Vector2 mid = (p1 + p2) * 0.5f;

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = mid;
        rt.sizeDelta = new Vector2(length, lineThickness);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rt.localRotation = Quaternion.Euler(0f, 0f, angle);

        _spawned.Add(go);
    }
}
