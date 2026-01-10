using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeEnterConfirmUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action _onConfirm;
    private Action _onCancel;

    private void Awake()
    {
        Hide();

        if (confirmButton != null)
            confirmButton.onClick.AddListener(() =>
            {
                var cb = _onConfirm;
                Hide();
                cb?.Invoke();
            });

        if (cancelButton != null)
            cancelButton.onClick.AddListener(() =>
            {
                var cb = _onCancel;
                Hide();
                cb?.Invoke();
            });
    }

    public void Show(string message, Action onConfirm, Action onCancel = null)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;

        if (messageText != null) messageText.text = message;
        if (root != null) root.SetActive(true);
        else gameObject.SetActive(true);
    }

    public void Hide()
    {
        _onConfirm = null;
        _onCancel = null;

        if (root != null) root.SetActive(false);
        else gameObject.SetActive(false);
    }
}
