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
    Animator animator;
    SpriteRenderer spriteRenderer;
    static readonly int Moving = Animator.StringToHash("Moving");
    static readonly int AttackAnimation = Animator.StringToHash("Attack");
    static readonly int HurtAnimation = Animator.StringToHash("Hurt");
    static readonly int Dead = Animator.StringToHash("Dead");
    Vector2 moveInput;
    float attackTimer = 0;
    readonly List<RaycastHit2D> movementHits = new List<RaycastHit2D>();
    const float CollisionSkin = 0.02f;

    GUIStyle barTextStyle;
    GUIStyle hudTextStyle;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Physics contacts cannot shove a kinematic player; movement casts stop us at obstacles.
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // game is paused (menu, bag, door box, win or lose) = do nothing
        if (Time.timeScale == 0 || hp <= 0)
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
        if (animator != null)
        {
            animator.SetBool(Moving, moveInput.sqrMagnitude > 0);
        }
        if (spriteRenderer != null && x != 0)
        {
            spriteRenderer.flipX = x < 0;
        }

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
        rb.linearVelocity = Vector2.zero;
        if (hp > 0 && Time.timeScale > 0)
            Move(moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    void Move(Vector2 displacement)
    {
        Vector2 position = rb.position;
        var filter = new ContactFilter2D();
        filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        filter.useTriggers = false;

        // Sweep before moving, then slide along the surface rather than passing through it.
        for (int iteration = 0; iteration < 3 && displacement.sqrMagnitude > 0.000001f; iteration++)
        {
            float distance = displacement.magnitude;
            Vector2 direction = displacement / distance;
            int count = rb.Cast(position, rb.rotation, direction, filter, movementHits, distance + CollisionSkin);
            float travel = distance;
            Vector2 normal = Vector2.zero;
            for (int i = 0; i < count; i++)
            {
                var hit = movementHits[i];
                if (Vector2.Dot(direction, hit.normal) >= -0.0001f) continue;
                float available = Mathf.Max(0, hit.distance - CollisionSkin);
                if (available < travel)
                {
                    travel = available;
                    normal = hit.normal;
                }
            }

            Vector2 step = direction * travel;
            position += step;
            displacement -= step;
            if (normal == Vector2.zero) break;
            displacement -= normal * Mathf.Min(0, Vector2.Dot(displacement, normal));
        }

        rb.MovePosition(position);
    }

    public void Attack()
    {
        if (hp <= 0) return;
        if (animator != null)
        {
            animator.ResetTrigger(HurtAnimation);
            animator.SetTrigger(AttackAnimation);
        }
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
        if (hp <= 0) return;
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
            moveInput = Vector2.zero;
            if (rb != null) rb.linearVelocity = Vector2.zero;
            if (animator != null)
            {
                // The lose screen pauses gameplay; let the death animation finish.
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                animator.ResetTrigger(AttackAnimation);
                animator.ResetTrigger(HurtAnimation);
                animator.SetBool(Dead, true);
            }
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.Lose();
            }
        }
        else if (animator != null)
        {
            animator.ResetTrigger(AttackAnimation);
            animator.SetTrigger(HurtAnimation);
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
        // game is paused (a menu is open) = hide the HUD so it does not cover the menu
        if (Time.timeScale == 0)
        {
            return;
        }

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
        DrawBar(10, 10, "HP", hp, maxHp, new Color(0.2f, 0.75f, 0.25f));
        DrawBar(10, 38, "DEF", def, maxDef, new Color(0.2f, 0.5f, 0.9f));
        DrawBar(10, 66, "ATK", atk, maxAtk, new Color(0.95f, 0.6f, 0.1f));

        
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
