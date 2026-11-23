using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// one picture for one item name (we fill this list in the Inspector)
[System.Serializable]
public class ItemIconEntry
{
    public string itemName;
    public Sprite icon;
}

public class InventoryUI : MonoBehaviour
{
    public List<ItemIconEntry> icons = new List<ItemIconEntry>();
    public bool isOpen = false;

    Player player;

    GUIStyle titleStyle;
    GUIStyle sectionStyle;
    GUIStyle nameStyle;
    GUIStyle countStyle;
    GUIStyle countShadowStyle;
    GUIStyle fallbackStyle;

    void Start()
    {
        player = FindFirstObjectByType<Player>();
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null || player == null)
        {
            return;
        }

        // paused by another screen (crafting menu, door box, win or lose) = do nothing
        if (Time.timeScale == 0 && isOpen == false)
        {
            return;
        }

        // press I to open or close the bag
        if (kb.iKey.wasPressedThisFrame)
        {
            if (isOpen)
            {
                CloseBag();
            }
            else
            {
                OpenBag();
            }
        }
    }

    void OpenBag()
    {
        isOpen = true;
        Time.timeScale = 0; // pause the game
    }

    void CloseBag()
    {
        isOpen = false;
        Time.timeScale = 1;
    }

    // sequential search: find the picture of an item by name, null = no picture yet
    public Sprite GetIcon(string itemName)
    {
        for (int i = 0; i < icons.Count; i++)
        {
            if (icons[i].itemName == itemName)
            {
                return icons[i].icon;
            }
        }
        return null;
    }

    // find the equipment that the player is wearing in this slot, null = empty
    EquipmentItem GetEquippedAt(EquipSlot slot)
    {
        for (int i = 0; i < player.equippedItems.Count; i++)
        {
            EquipmentItem equipment = ItemDatabase.Get(player.equippedItems[i]) as EquipmentItem;
            if (equipment != null && equipment.slot == slot)
            {
                return equipment;
            }
        }
        return null;
    }

    // ---------- drawing (temporary UI, we will make a Canvas later) ----------

    void LockColor(GUIStyle style, Color color)
    {
        style.normal.textColor = color;
        style.hover.textColor = color;
        style.active.textColor = color;
        style.focused.textColor = color;
        style.onNormal.textColor = color;
        style.onHover.textColor = color;
        style.onActive.textColor = color;
        style.onFocused.textColor = color;
    }

    void MakeStyles()
    {
        if (titleStyle != null)
        {
            return;
        }

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 28;
        titleStyle.fontStyle = FontStyle.Bold;
        LockColor(titleStyle, Color.white);

        sectionStyle = new GUIStyle(GUI.skin.label);
        sectionStyle.fontSize = 14;
        LockColor(sectionStyle, new Color(0.7f, 0.7f, 0.7f));

        nameStyle = new GUIStyle(GUI.skin.label);
        nameStyle.fontSize = 12;
        nameStyle.alignment = TextAnchor.UpperCenter;
        nameStyle.wordWrap = false;
        LockColor(nameStyle, new Color(0.8f, 0.8f, 0.8f));

        countStyle = new GUIStyle(GUI.skin.label);
        countStyle.fontSize = 18;
        countStyle.fontStyle = FontStyle.Bold;
        countStyle.alignment = TextAnchor.LowerRight;
        LockColor(countStyle, Color.white);

        countShadowStyle = new GUIStyle(countStyle);
        LockColor(countShadowStyle, Color.black);

        fallbackStyle = new GUIStyle(GUI.skin.label);
        fallbackStyle.fontSize = 13;
        fallbackStyle.fontStyle = FontStyle.Bold;
        fallbackStyle.alignment = TextAnchor.MiddleCenter;
        fallbackStyle.wordWrap = true;
        LockColor(fallbackStyle, Color.white);
    }

    void DrawRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    // draw a sprite and keep its ratio (not stretched)
    public void DrawSprite(Rect rect, Sprite sprite)
    {
        Texture2D tex = sprite.texture;
        Rect r = sprite.textureRect;
        Rect uv = new Rect(r.x / tex.width, r.y / tex.height, r.width / tex.width, r.height / tex.height);

        float w = rect.width;
        float h = rect.height;
        float ratio = r.width / r.height;
        if (ratio > 1)
        {
            h = w / ratio;
        }
        else
        {
            w = h * ratio;
        }

        Rect fit = new Rect(rect.x + (rect.width - w) / 2, rect.y + (rect.height - h) / 2, w, h);
        GUI.DrawTextureWithTexCoords(fit, tex, uv);
    }

    // color of the temporary box (used when an item has no picture yet)
    public Color GetTypeColor(Item item)
    {
        string type = item.GetTypeName();
        if (type == "Material")
        {
            return new Color(0.45f, 0.35f, 0.25f);
        }
        if (type == "Component")
        {
            return new Color(0.3f, 0.45f, 0.7f);
        }
        return new Color(0.75f, 0.6f, 0.2f); // Equipment
    }

    // one square slot: border, background, picture (or color box), amount
    void DrawSlot(float x, float y, float size, Item item, int amount)
    {
        DrawRect(new Rect(x - 2, y - 2, size + 4, size + 4), new Color(0.35f, 0.4f, 0.55f));
        DrawRect(new Rect(x, y, size, size), new Color(0.14f, 0.16f, 0.23f));

        // empty slot = nothing more to draw
        if (item == null)
        {
            return;
        }

        // [1] picture, or a color box with the name if there is no picture
        Sprite sprite = GetIcon(item.itemName);
        if (sprite != null)
        {
            DrawSprite(new Rect(x + 8, y + 8, size - 16, size - 16), sprite);
        }
        else
        {
            DrawRect(new Rect(x + 6, y + 6, size - 12, size - 12), GetTypeColor(item));
            GUI.Label(new Rect(x + 6, y + 6, size - 12, size - 12), item.itemName, fallbackStyle);
        }

        // [2] amount at the bottom-right corner (black shadow first, then white text)
        if (amount > 0)
        {
            string text = "x" + amount;
            GUI.Label(new Rect(x + 1, y + 1, size - 4, size - 4), text, countShadowStyle);
            GUI.Label(new Rect(x, y, size - 5, size - 5), text, countStyle);
        }
    }

    void OnGUI()
    {
        if (isOpen == false || player == null)
        {
            return;
        }
        GUI.depth = -1;
        MakeStyles();

        // [1] layout numbers
        float panelW = 640;
        float slot = 84;
        float gap = 14;
        int columns = 6;

        int totalSlots = 12;
        if (player.inventory.slots.Count > totalSlots)
        {
            totalSlots = ((player.inventory.slots.Count + columns - 1) / columns) * columns;
        }
        int rows = totalSlots / columns;
        float rowStep = slot + 28;
        float panelH = 256 + rows * rowStep + 16;

        float px = (Screen.width - panelW) / 2;
        float py = (Screen.height - panelH) / 2;
        if (py < 5)
        {
            py = 5;
        }
        float gridX = px + (panelW - (columns * slot + (columns - 1) * gap)) / 2;

        // [2] dark screen and the panel
        DrawRect(new Rect(0, 0, Screen.width, Screen.height), new Color(0, 0, 0, 0.6f));
        DrawRect(new Rect(px - 3, py - 3, panelW + 6, panelH + 6), new Color(0.35f, 0.4f, 0.55f));
        DrawRect(new Rect(px, py, panelW, panelH), new Color(0.1f, 0.12f, 0.18f));

        // [3] title
        GUI.Label(new Rect(px + 20, py + 12, 400, 40), "INVENTORY", titleStyle);
        GUI.Label(new Rect(px + 20, py + 52, 400, 22), "press I to close", sectionStyle);

        // [4] equipped slots (weapon, head, chest, legs)
        GUI.Label(new Rect(px + 20, py + 84, 300, 22), "EQUIPPED", sectionStyle);
        EquipSlot[] slotTypes = { EquipSlot.Weapon, EquipSlot.Head, EquipSlot.Chest, EquipSlot.Legs };
        string[] slotNames = { "WEAPON", "HEAD", "CHEST", "LEGS" };
        for (int i = 0; i < slotTypes.Length; i++)
        {
            float sx = gridX + i * (slot + gap);
            float sy = py + 110;
            DrawSlot(sx, sy, slot, GetEquippedAt(slotTypes[i]), 0);
            GUI.Label(new Rect(sx - 6, sy + slot + 2, slot + 12, 20), slotNames[i], nameStyle);
        }

        // [5] items in the bag (grid of 6 columns)
        GUI.Label(new Rect(px + 20, py + 230, 300, 22), "ITEMS", sectionStyle);
        for (int i = 0; i < totalSlots; i++)
        {
            int col = i % columns;
            int row = i / columns;
            float sx = gridX + col * (slot + gap);
            float sy = py + 256 + row * rowStep;

            Item item = null;
            int amount = 0;
            if (i < player.inventory.slots.Count)
            {
                item = player.inventory.slots[i].item;
                amount = player.inventory.slots[i].amount;
            }

            DrawSlot(sx, sy, slot, item, amount);
            if (item != null)
            {
                GUI.Label(new Rect(sx - 6, sy + slot + 2, slot + 12, 20), item.itemName, nameStyle);
            }
        }
    }
}