using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    public event Action Dialog;

    public bool DialogPressed { get; private set; }
    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        detectDialog();
        
    }

    private void detectDialog()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z))
        {
            Dialog?.Invoke();
            DialogPressed = true;
        }
        else
        {
            DialogPressed = false;
        }
    }
}
