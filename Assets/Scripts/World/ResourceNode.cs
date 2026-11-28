using UnityEngine;

public class ResourceNode : Destructible
{
    public string resourceType = "Iron Ore";

    // override Die(): do OnBreak() first, then the normal Die() from the parent
    public override void Die()
    {
        OnBreak();
        base.Die();
    }

    void OnBreak()
    {
        Debug.Log(resourceType + " node is broken");

        // play the break sound (the sound depends on the resource type)
        AudioManager am = FindFirstObjectByType<AudioManager>();
        if (am != null)
        {
            am.PlayNodeBreak(resourceType);
        }
    }
}