using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogTrigger : MonoBehaviour
{
    [SerializeField] private DialogSequence dialogSequence;
    public DialogManager DialogManager;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            DialogManager.StartDialog(dialogSequence);
        }
    }
}
