using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    public int hp = 15;
    public int def = 0;
    public DropTable dropTable = new DropTable();
    public ItemPickup pickupPrefab;

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
        // [1] roll the drop table to see what we drop
        List<DropEntry> drops = dropTable.Roll();

        // [2] create one pickup on the ground for each drop
        for (int i = 0; i < drops.Count; i++)
        {
            if (pickupPrefab != null)
            {
                Vector3 offset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
                ItemPickup pickup = Instantiate(pickupPrefab, transform.position + offset, Quaternion.identity);
                pickup.itemName = drops[i].itemName;
                pickup.amount = drops[i].amount;
            }
        }

        // [3] remove this object
        Destroy(gameObject);
    }
}