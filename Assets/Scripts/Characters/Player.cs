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
        // game is paused (crafting menu is open) = do nothing
        if (Time.timeScale == 0 || hp <= 0)
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
        Debug.Log("Player hp = " + hp);

        // hp is 0 = tell GameManager that we lost
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
