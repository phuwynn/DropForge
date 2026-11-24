using UnityEngine;

public class Boss : Enemy
{
    // override Die(): Boss is a child of Enemy, so base.Die() runs Enemy.Die() first
    public override void Die()
    {
        if (IsDead) return;
        Debug.Log("Boss is defeated");
        base.Die();
    }
}
