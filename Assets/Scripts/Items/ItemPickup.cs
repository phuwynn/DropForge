using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public string itemName;
    public int amount = 1;

    void Start()
    {
        // change the color by item name so we can tell them apart
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            return;
        }

        if (itemName == "Bone")
        {
            sr.color = Color.white;
        }
        else if (itemName == "Hide")
        {
            sr.color = new Color(0.6f, 0.4f, 0.2f);
        }
        else if (itemName == "Slime Gel")
        {
            sr.color = Color.green;
        }
        else if (itemName == "Cotton")
        {
            sr.color = new Color(1f, 0.8f, 0.9f);
        }
        else if (itemName == "Iron Ore")
        {
            sr.color = Color.gray;
        }
    }

    // called when something enters this trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        // [1] check if it is the player
        Player player = other.GetComponent<Player>();

        // [2] yes = give the item to the player and remove this pickup
        if (player != null)
        {
            player.Pickup(itemName, amount);
            Destroy(gameObject);
        }
    }
}