using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private float typingSpeed = 0.03f;

    private List<DialogLine> lines;
    private int currentIndex = 0;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
    }

    private void OnEnable()
    {
        if(InputManager.Instance != null) InputManager.Instance.Dialog += nextLine;
    }

    private void OnDisable()
    {
        if(InputManager.Instance != null) InputManager.Instance.Dialog -= nextLine;
    }

    // 대화 시작
    public void StartDialog(DialogSequence sequence)
    {
        if (sequence == null || sequence.Lines.Count == 0) return;

        lines = sequence.Lines;
        currentIndex = 0;
        showCurrentLine();
        StartCoroutine(fadeIn());
    }

    // 다음 대사로 진행
    private void nextLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            dialogText.text = lines[currentIndex].Text;
            typingCoroutine = null;
            return;
        }

        currentIndex++;

        if (currentIndex < lines.Count - 1)
        {
            showCurrentLine();
        }
        else
        {
            StartCoroutine(fadeOut());
        }
    }

    private void showCurrentLine()
    {
        var line = lines[currentIndex];
        var speaker = line.Speaker;

        if (speaker != null)
        {
            nameText.text = speaker.SpeakerName;

            if (speaker.Portrait != null)
            {
                portraitImage.enabled = true;
                portraitImage.sprite = speaker.Portrait;
            }
            else portraitImage.enabled = false;
        }
        else
        {
            nameText.text = "";
            portraitImage.enabled = false;
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(typeText(line.Text));
    }

    private IEnumerator typeText(string text)
    {
        dialogText.text = "";
        foreach (char c in text)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        typingCoroutine = null;
    }

    private IEnumerator fadeIn()
    {
        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime * 3f;
            canvasGroup.alpha = Mathf.Lerp(0, 1, time);
            yield return null;
        }
        canvasGroup.interactable = true;
    }

    private IEnumerator fadeOut()
    {
        canvasGroup.interactable = false;
        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime * 3f;
            canvasGroup.alpha = Mathf.Lerp(1, 0, time);
            yield return null;
        }
    }
}
