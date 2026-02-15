using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 개별 카드를 UI 버튼으로 표현합니다.
/// </summary>
public class CardButtonUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Button button;
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI cardCostText;
    [SerializeField] private Image cardTypeImage;

    private CardData cardData;
    private HandCardUIManager uiManager;

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnClicked);
            Debug.Log($"[CardButtonUI] 버튼 클릭 이벤트 등록됨");
        }
        else
        {
            Debug.LogWarning("[CardButtonUI] Button 컴포넌트를 찾을 수 없습니다!");
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnClicked);
        }
    }

    /// <summary>
    /// 카드 버튼을 초기화합니다.
    /// </summary>
    public void Initialize(CardData card, HandCardUIManager manager)
    {
        cardData = card;
        uiManager = manager;

        UpdateUI();
    }

    /// <summary>
    /// UI를 카드 정보로 업데이트합니다.
    /// </summary>
    private void UpdateUI()
    {
        if (cardData == null)
        {
            Debug.LogWarning("[CardButtonUI] 카드 데이터가 없습니다!");
            return;
        }

        // 카드 이미지
        if (cardImage != null && cardData.cardImage != null)
        {
            cardImage.sprite = cardData.cardImage;
        }

        // 카드 이름
        if (cardNameText != null)
        {
            cardNameText.text = cardData.cardName;
        }

        // 카드 비용
        if (cardCostText != null)
        {
            cardCostText.text = cardData.cost.ToString();
        }

        // 카드 타입 이미지
        if (cardTypeImage != null && cardData.cardTypeImage != null)
        {
            cardTypeImage.sprite = cardData.cardTypeImage;
        }
    }

    /// <summary>
    /// 버튼이 클릭되었을 때 호출됩니다.
    /// </summary>
    private void OnClicked()
    {
        Debug.Log($"[CardButtonUI] {cardData.cardName} 버튼 클릭됨!");
        if (uiManager != null)
        {
            uiManager.OnCardButtonClicked(cardData);
        }
        else
        {
            Debug.LogWarning("[CardButtonUI] HandCardUIManager 참조가 없습니다!");
        }
    }
}
