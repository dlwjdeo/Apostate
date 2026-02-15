using UnityEngine;
using UnityEngine.UI;

public class HPUI : MonoBehaviour
{
    [SerializeField] private Image hpImage;

    private Unit boundUnit;

    public void Bind(Unit unit)
    {
        Unbind();
        boundUnit = unit;
        if (boundUnit != null)
        {
            boundUnit.OnDamageReceived += OnUnitDamaged;
            boundUnit.OnHealed += OnUnitHealed;
            boundUnit.OnDead += OnUnitDead;
            UpdateFill(boundUnit.CurrentHp, boundUnit.MaxHp);
        }
    }

    public void Unbind()
    {
        if (boundUnit != null)
        {
            boundUnit.OnDamageReceived -= OnUnitDamaged;
            boundUnit.OnHealed -= OnUnitHealed;
            boundUnit.OnDead -= OnUnitDead;
            boundUnit = null;
        }
    }

    private void OnDestroy() => Unbind();

    private void OnUnitDamaged(int dmg) => UpdateFill(boundUnit.CurrentHp, boundUnit.MaxHp);
    private void OnUnitHealed(int amt) => UpdateFill(boundUnit.CurrentHp, boundUnit.MaxHp);
    private void OnUnitDead() => UpdateFill(0, boundUnit != null ? boundUnit.MaxHp : 1);

    private void UpdateFill(int current, int max)
    {
        hpImage.fillAmount = max > 0 ? (float)current / max : 0f;
    }
}
