using UnityEngine;

[CreateAssetMenu(menuName = "Game/NodeMap/Map Config", fileName = "NodeMapConfig")]
public class NodeMapConfig : ScriptableObject
{
    [Header("Depth / Count (each depth node count)")]
    public int[] depthCounts = { 1, 3, 4, 4, 3, 2, 1 };

    [Header("Layout")]
    public float xSpacing = 220f;
    public float ySpacing = 160f;

    [Header("Connections (From -> Next Depth)")]
    [Min(1)] public int minOut = 1;
    [Min(1)] public int maxOut = 2;
    [Range(0f, 1f)] public float preferNearIndex = 0.8f; // 1이면 근처 인덱스 선호 강함

    [Header("Seed")]
    public bool useFixedSeed = false;
    public int fixedSeed = 12345;

    [Header("Type Weights (Except Start/Boss)")]
    [Range(0, 100)] public int battleWeight = 70;
    [Range(0, 100)] public int shopWeight = 10;
    [Range(0, 100)] public int restWeight = 15;
    [Range(0, 100)] public int eliteWeight = 5;

    [Header("Type Rules")]
    [Tooltip("마지막 depth는 Boss로 강제")]
    public bool forceLastDepthBoss = true;

    [Tooltip("Start는 depth 0의 첫 노드만 Start로 강제")]
    public bool forceStartNode = true;
}
