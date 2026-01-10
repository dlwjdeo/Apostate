using UnityEngine;

public class NodeLink : MonoBehaviour
{
    [Header("From -> To")]
    [SerializeField] private Node from;
    [SerializeField] private Node to;

    public Node From => from;
    public Node To => to;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (from == to)
            to = null;
    }
#endif
}
