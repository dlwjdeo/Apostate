using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dialog/Dialog Sequence", fileName = "NewDialog")]
public class DialogSequence : ScriptableObject
{
    public List<DialogLine> Lines;
}