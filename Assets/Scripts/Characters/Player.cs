using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public int hp = 100;
    public int maxHp = 100;
    public int atk = 5;
    public int def = 0;
    public float attackRange = 1.5f;
    public float attackCooldown = 0.5f;
    public Inventory inventory = new Inventory();
    public List<string> equippedItems = new List<string>();

    Rigidbody2D rb;
    Vector2 moveInput;
    float attackTimer = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // game is paused (crafting menu is open) = do nothing
        if (Time.timeScale == 0)
        {
            return;
        }

        Keyboard kb = Keyboard.current;
        if (kb == null)
        {
            return;
        }

        // [1] read the keys (W A S D or arrow keys)
        float x = 0;
        float y = 0;

        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed)
        {
            x = 1;
        }
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)
        {
            x = -1;
        }
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed)
        {
            y = 1;
        }
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed)
        {
            y = -1;
        }

        // [2] make diagonal speed the same as straight speed
        moveInput = new Vector2(x, y).normalized;

        // [3] count down the attack timer
        if (attackTimer > 0)
        {
            attackTimer = attackTimer - Time.deltaTime;
        }

        // [4] click the left mouse button to attack (only when the timer is finished)
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && attackTimer <= 0)
        {
            Attack();
            attackTimer = attackCooldown;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    public void Attack()
    {
        // [1] find all colliders inside the attack range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);

        // [2] hit every Destructible that we found
        for (int i = 0; i < hits.Length; i++)
        {
            Destructible target = hits[i].GetComponent<Destructible>();
            if (target != null)
            {
                target.TakeDamage(atk);
            }
        }
    }

    public void TakeDamage(int enemyAtk)
    {
        // [1] damage = enemy atk - our def, at least 1
        int damage = enemyAtk - def;
        if (damage < 1)
        {
            damage = 1;
        }

        // [2] reduce hp, not lower than 0
        hp = hp - damage;
        if (hp < 0)
        {
            hp = 0;
        }
        Debug.Log("Player hp = " + hp);

        // hp is 0 = tell GameManager that we lost
        if (hp == 0)
        {
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.Lose();
            }
        }
    }

    public void Pickup(string itemName, int amount)
    {
        inventory.Add(itemName, amount);
    }

    public void Equip(string itemName, int atkBonus, int defBonus)
    {
        // [1] already wearing it = do nothing
        if (IsEquipped(itemName))
        {
            return;
        }

        // [2] remember it and add the bonus to our stats
        equippedItems.Add(itemName);
        atk = atk + atkBonus;
        def = def + defBonus;
        Debug.Log("Equip " + itemName + " : ATK " + atk + " DEF " + def);
    }

    // sequential search, true if we are wearing this item
    public bool IsEquipped(string itemName)
    {
        for (int i = 0; i < equippedItems.Count; i++)
        {
            if (equippedItems[i] == itemName)
            {
                return true;
            }
        }
        return false;
    }

    // draw the attack range in the Scene view (when Player is selected)
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    // temporary HUD (we will make a real UI later)
    void OnGUI()
    {
        GUI.skin.label.fontSize = 18;
        GUI.Label(new Rect(10, 10, 400, 30), "HP: " + hp + "/" + maxHp);
        GUI.Label(new Rect(10, 35, 400, 30), "ATK: " + atk + "   DEF: " + def);

        for (int i = 0; i < inventory.slots.Count; i++)
        {
            string text = inventory.slots[i].itemName + " x " + inventory.slots[i].amount;
            GUI.Label(new Rect(10, 70 + i * 25, 400, 30), text);
        }
    }
}