using UnityEngine;

[CreateAssetMenu(fileName = "NodeData", menuName = "Game/Node")]
public class NodeData : ScriptableObject
{
    public int id;
    public NodeType type;
    public int[] connectedNodeIds;
}