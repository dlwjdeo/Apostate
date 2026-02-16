using System;
using UnityEngine;

public class Enemy : Unit
{
    [SerializeField] private GameObject hpUIPrefab;
    private void Awake()
    {
        BindHPUI();
    }

    private void BindHPUI()
    {
        if (hpUIPrefab != null)
        {
            GameObject hpUIObj = Instantiate(hpUIPrefab, UIManager.Instance.InGameUIRoot.transform);
            HPUI hpUI = hpUIObj.GetComponentInChildren<HPUI>();
            if (hpUI != null)
            {
                hpUI.Bind(this);
            }
        }
    }

    public virtual void ExcuteTurn(Unit player)
    {
        player.GetDamage(5);
    }
}
