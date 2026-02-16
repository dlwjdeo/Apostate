using UnityEngine;

public class BaseEnemy : Enemy
{
    private int chargeCount = 0;
    public override void ExcuteTurn(Unit player)
    {
        int random = Random.Range(0, 100);
        if(random < 50)
        {
            int damage = 5 + chargeCount * 2;
            DefaltAttack(player, damage);
        }
        else if(random < 80)
        {
            GetBarrier();
        }
        else
        {
            Charge();
        }
    }

    public void DefaltAttack(Unit player, int damage)
    {
        player.GetDamage(damage);
    }
    public void GetBarrier()
    {
        this.AddBarrier(5);
    }
    public void Charge()
    {
        chargeCount++;
    }
}
