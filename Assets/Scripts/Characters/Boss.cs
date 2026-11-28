using UnityEngine;

public class Boss : Enemy
{
    // override = the boss plays its own death sound instead of the normal enemy sound
    protected override void PlayDeathSound()
    {
        AudioManager am = FindFirstObjectByType<AudioManager>();
        if (am != null)
        {
            am.PlayBossDie();
        }
    }

    public override void Die()
    {
        Debug.Log("Boss is defeated");
        base.Die();
    }
}