using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public string itemName;
    public int amount = 1;

    // pictures for each item (leave empty = use a colored circle)
    public Sprite boneSprite;
    public Sprite hideSprite;
    public Sprite slimeGelSprite;
    public Sprite cottonSprite;
    public Sprite ironOreSprite;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            return;
        }

        // [1] choose the picture by item name
        Sprite picture = null;
        Color color = Color.white;

        if (itemName == "Bone")
        {
            picture = boneSprite;
            color = Color.white;
        }
        else if (itemName == "Hide")
        {
            picture = hideSprite;
            color = new Color(0.6f, 0.4f, 0.2f);
        }
        else if (itemName == "Slime Gel")
        {
            picture = slimeGelSprite;
            color = Color.green;
        }
        else if (itemName == "Cotton")
        {
            picture = cottonSprite;
            color = new Color(1f, 0.8f, 0.9f);
        }
        else if (itemName == "Iron Ore")
        {
            picture = ironOreSprite;
            color = Color.gray;
        }

        // [2] have a picture = use it, no picture = keep the circle with a color
        if (picture != null)
        {
            sr.sprite = picture;
            sr.color = Color.white;
        }
        else
        {
            sr.color = color;
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