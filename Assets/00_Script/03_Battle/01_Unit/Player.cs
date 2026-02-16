using System;
using UnityEngine;

public class Player : Unit
{
    [SerializeField] private HPUI hpUI;
    private void Awake()
    {
        BindHPUI();
    }

    private void BindHPUI()
    {
        if (hpUI != null)
        {
            hpUI.Bind(this);
        }
    }
}
