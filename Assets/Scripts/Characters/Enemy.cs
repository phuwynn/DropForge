using UnityEngine;

public class Enemy : Destructible
{
    public int atk = 1;
    public float moveSpeed = 2f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1f;

    Player player;
    Rigidbody2D rb;
    float attackTimer = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = FindFirstObjectByType<Player>();
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }

        // [1] count down the attack timer
        if (attackTimer > 0)
        {
            attackTimer = attackTimer - Time.deltaTime;
        }

        // [2] close enough and the timer is finished = attack the player
        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance <= attackRange && attackTimer <= 0)
        {
            player.TakeDamage(atk);
            attackTimer = attackCooldown;
        }
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            return;
        }

        // [1] far from the player = walk toward the player
        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance > attackRange)
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

        // override = rewrite Die() from Destructible
    public override void Die()
    {
        // [1] tell WaveManager that one enemy is dead
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.OnEnemyDied();
        }

        // [2] do the normal Die() from the parent class (destroy this object)
        base.Die();
    }
}