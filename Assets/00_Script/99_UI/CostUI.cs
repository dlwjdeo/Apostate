using UnityEngine;

public class CostUI : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private CostSlot[] costSlots;

    private void Start()
    {
        if (player == null) return;
        if(costSlots == null)
        {
            costSlots = GetComponentsInChildren<CostSlot>();
        }

        player.OnCostChanged += UpdateCostUI;
        UpdateCostUI(player.CurrentCost);
    }

    private void OnDestroy()
    {
        if (player != null)
            player.OnCostChanged -= UpdateCostUI;
    }

    private void UpdateCostUI(int currentCost)
    {
        for (int i = 0; i < costSlots.Length; i++)
        {
            costSlots[i].SetFilled(i < currentCost);
        }
    }
}
