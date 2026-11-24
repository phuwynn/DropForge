using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    public int hp = 15;
    public int def = 0;
    public DropTable dropTable = new DropTable();
    public ItemPickup pickupPrefab;
    public float barWidth = 44;

    int maxHp;
    Camera cam;
    Collider2D col;
    protected bool IsDead { get; private set; }
    protected virtual float DestructionDelay => 0f;

    protected virtual void Awake()
    {
        // remember the starting hp so we can draw the bar
        maxHp = hp;
        cam = Camera.main;
        col = GetComponent<Collider2D>();
    }

    public void TakeDamage(int attackerAtk)
    {
        if (IsDead) return;
        // [1] damage = attacker atk - our def, at least 1
        int damage = attackerAtk - def;
        if (damage < 1)
        {
            damage = 1;
        }

        // [2] reduce hp
        hp = hp - damage;
        Debug.Log(gameObject.name + " hp = " + hp);

        // [3] hp is 0 or lower = die
        if (hp <= 0)
        {
            Die();
        }
        else
        {
            OnDamaged();
        }
    }

    protected virtual void OnDamaged() { }

    // virtual = the child class can override (rewrite) this method
    public virtual void Die()
    {
        if (IsDead) return;
        IsDead = true;
        hp = 0;
        // [1] roll the drop table to see what we drop
        List<DropEntry> drops = dropTable.Roll();

        // [2] create one pickup on the ground for each drop
        for (int i = 0; i < drops.Count; i++)
        {
            if (pickupPrefab != null)
            {
                Vector3 offset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
                ItemPickup pickup = Instantiate(pickupPrefab, transform.position + offset, Quaternion.identity);
                pickup.itemName = drops[i].itemName;
                pickup.amount = drops[i].amount;
            }
        }

        // [3] remove this object
        if (DestructionDelay > 0)
            StartCoroutine(RemoveAfterAnimation());
        else
            Destroy(gameObject);
    }

    IEnumerator RemoveAfterAnimation()
    {
        // The final kill pauses gameplay, but the corpse should still disappear.
        yield return new WaitForSecondsRealtime(DestructionDelay);
        Destroy(gameObject);
    }

    // draw the hp bar above the head (temporary UI)
    void OnGUI()
    {
        if (IsDead) return;
        // only draw once per frame, and hide the bar while the game is paused
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }
        if (Time.timeScale == 0)
        {
            return;
        }
        if (cam == null || col == null)
        {
            return;
        }

        // [1] find the top of the collider and change it to a screen position
        Vector3 top = new Vector3(col.bounds.center.x, col.bounds.max.y, 0);
        Vector3 screen = cam.WorldToScreenPoint(top);

        // [2] GUI y is upside down compared with the screen y
        float barW = barWidth;
        float barH = 6;
        float x = screen.x - barW / 2;
        float y = Screen.height - screen.y - barH - 4;

        // [3] hp ratio from 0 to 1
        float ratio = (float)hp / maxHp;
        if (ratio < 0)
        {
            ratio = 0;
        }

        // [4] draw the dark background, then the red bar
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(new Rect(x - 1, y - 1, barW + 2, barH + 2), Texture2D.whiteTexture);
        GUI.color = Color.red;
        GUI.DrawTexture(new Rect(x, y, barW * ratio, barH), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
