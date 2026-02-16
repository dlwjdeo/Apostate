using UnityEngine;

public class CostSlot : MonoBehaviour
{
    [SerializeField] private GameObject filledSlot;

    public void SetFilled(bool isFilled)
    {
        if (filledSlot != null)
        {
            filledSlot.SetActive(isFilled);
        }
    }
}
