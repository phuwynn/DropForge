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
    public int maxAtk = 20;
    public int maxDef = 24;
    public float attackRange = 1.5f;
    public float attackCooldown = 0.5f;
    public Inventory inventory = new Inventory();
    public List<string> equippedItems = new List<string>();
    public string lastHitText = "";

    Rigidbody2D rb;
    Vector2 moveInput;
    float attackTimer = 0;

    GUIStyle barTextStyle;
    GUIStyle hudTextStyle;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // game is paused (menu, bag, door box, win or lose) = do nothing
        if (Time.timeScale == 0)
        {
            return;
        }

        Keyboard kb = Keyboard.current;
        if (kb == null)
        {
            return;
        }

        // test key (editor only): F2 = take one boss hit (ATK 26)
        if (DevKeys.Enabled && kb.f2Key.wasPressedThisFrame)
        {
            TakeDamage(26);
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

        // [3] remember the last hit (only shown in the editor)
        lastHitText = "Last hit: enemy ATK " + enemyAtk + " - DEF " + def + " = " + damage;
        Debug.Log(lastHitText + " | HP = " + hp);

        // [4] hp is 0 = tell GameManager that we lost
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

    // wear an EquipmentItem (take the bonus from the Equipment class)
    public void Equip(EquipmentItem equipment)
    {
        Equip(equipment.itemName, equipment.atkBonus, equipment.defBonus);
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

    // HUD: HP, DEF, ATK bars at the top-left of the screen
    void OnGUI()
    {
        // draw behind the menus
        GUI.depth = 1;

        // [1] make the styles once
        if (barTextStyle == null)
        {
            barTextStyle = new GUIStyle(GUI.skin.label);
            barTextStyle.fontSize = 14;
            barTextStyle.fontStyle = FontStyle.Bold;
            barTextStyle.alignment = TextAnchor.MiddleCenter;
            barTextStyle.normal.textColor = Color.white;

            hudTextStyle = new GUIStyle(GUI.skin.label);
            hudTextStyle.fontSize = 16;
            hudTextStyle.normal.textColor = Color.white;
        }

        // [2] the three bars
        DrawBar(10, 10, "HP", hp, maxHp, new Color(0.85f, 0.15f, 0.15f));
        DrawBar(10, 38, "DEF", def, maxDef, new Color(0.2f, 0.5f, 0.9f));
        DrawBar(10, 66, "ATK", atk, maxAtk, new Color(0.95f, 0.6f, 0.1f));

        // [3] last hit text: only in the editor (it is a test tool, not for players)
        if (DevKeys.Enabled && lastHitText != "")
        {
            GUI.Label(new Rect(10, 98, 600, 28), lastHitText, hudTextStyle);
        }
    }

    void DrawBar(float x, float y, string label, int value, int maxValue, Color color)
    {
        float w = 220;
        float h = 22;

        // ratio from 0 to 1
        float ratio = 0;
        if (maxValue > 0)
        {
            ratio = (float)value / maxValue;
        }
        if (ratio > 1)
        {
            ratio = 1;
        }
        if (ratio < 0)
        {
            ratio = 0;
        }

        // dark background, color bar, then the text on top
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(new Rect(x - 2, y - 2, w + 4, h + 4), Texture2D.whiteTexture);
        GUI.color = color;
        GUI.DrawTexture(new Rect(x, y, w * ratio, h), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(x, y, w, h), label + " " + value + " / " + maxValue, barTextStyle);
    }
}