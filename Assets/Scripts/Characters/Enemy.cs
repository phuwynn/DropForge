using UnityEngine;

public class Enemy : Destructible
{
    public int atk = 1;
    public float moveSpeed = 2f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1f;

    Player player;
    Rigidbody2D rb;
    Animator animator;
    Collider2D bodyCollider;
    Collider2D playerCollider;
    float facingScale;
    static readonly int Moving = Animator.StringToHash("Moving");
    static readonly int AttackAnimation = Animator.StringToHash("Attack");
    static readonly int HurtAnimation = Animator.StringToHash("Hurt");
    static readonly int Dead = Animator.StringToHash("Dead");
    protected override float DestructionDelay => animator != null && animator.runtimeAnimatorController != null ? 0.65f : 0f;
    float attackTimer = 0;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bodyCollider = GetComponent<Collider2D>();
        facingScale = Mathf.Abs(transform.localScale.x);
    }

    void Start()
    {
        player = FindFirstObjectByType<Player>();
        if (player != null) playerCollider = player.GetComponent<Collider2D>();
    }

    void Update()
    {
        // game is paused (crafting menu is open) = do nothing
        if (Time.timeScale == 0 || IsDead)
        {
            return;
        }

        if (player == null || player.hp <= 0)
        {
            return;
        }

        // [1] count down the attack timer
        if (attackTimer > 0)
        {
            attackTimer = attackTimer - Time.deltaTime;
        }

        // [2] close enough and the timer is finished = attack the player
        bool inRange = IsInAttackRange();
        if (animator != null) animator.SetBool(Moving, !inRange);
        float horizontal = player.transform.position.x - transform.position.x;
        if (Mathf.Abs(horizontal) > 0.05f)
        {
            // The source monster artwork faces right.
            Vector3 scale = transform.localScale;
            scale.x = horizontal > 0 ? facingScale : -facingScale;
            transform.localScale = scale;
        }
        if (inRange && attackTimer <= 0)
        {
            if (animator != null)
            {
                animator.ResetTrigger(HurtAnimation);
                animator.SetTrigger(AttackAnimation);
            }
            player.TakeDamage(atk);
            attackTimer = attackCooldown;
        }
    }

    void FixedUpdate()
    {
        if (IsDead || player == null || player.hp <= 0)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        // [1] far from the player = walk toward the player
        if (!IsInAttackRange())
        {
            Vector2 direction = (player.transform.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            // [2] close enough = stop and attack
            rb.linearVelocity = Vector2.zero;
        }
    }

    bool IsInAttackRange()
    {
        if (Vector2.Distance(transform.position, player.transform.position) <= attackRange) return true;
        // Large monsters can touch the player while their origin is still outside attackRange.
        return bodyCollider != null && playerCollider != null &&
            bodyCollider.Distance(playerCollider).distance <= 0.05f;
    }

    protected override void OnDamaged()
    {
        if (animator == null) return;
        animator.ResetTrigger(AttackAnimation);
        animator.SetTrigger(HurtAnimation);
    }

    // the child class (Boss) can override this to play its own sound
    protected virtual void PlayDeathSound()
    {
        AudioManager am = FindFirstObjectByType<AudioManager>();
        if (am != null)
        {
            am.PlayEnemyDie();
        }
    }

    // override = rewrite Die() from Destructible
    public override void Die()
    {
        // [1] play the death sound
        PlayDeathSound();

        // [2] tell WaveManager that one enemy is dead
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.OnEnemyDied();
        }

        // [3] do the normal Die() from the parent class (drop items and destroy this object)
        base.Die();
    }
}
