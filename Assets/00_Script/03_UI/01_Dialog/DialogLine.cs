using UnityEngine;

[System.Serializable]
public class DialogLine
{
    public Speaker Speaker;
    [TextArea(2, 5)] public string Text;
}