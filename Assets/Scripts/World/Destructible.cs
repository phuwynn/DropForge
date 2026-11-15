using UnityEngine;

public class Destructible : MonoBehaviour
{
    public int hp = 15;
    public int def = 0;

    public void TakeDamage(int attackerAtk)
    {
        // [1] damage = attacker atk - our def, at least 1
        int damage = attackerAtk - def;
        if (damage < 1)
        {
            damage = 1;
        }

        // [2] reduce hp
        hp = hp - damage;
        Debug.Log(gameObject.name + " hp = " + hp);

        // [3] hp is 0 or lower = die
        if (hp <= 0)
        {
            Die();
        }
    }

    // virtual = the child class can override (rewrite) this method
    public virtual void Die()
    {
        Destroy(gameObject);
    }
}