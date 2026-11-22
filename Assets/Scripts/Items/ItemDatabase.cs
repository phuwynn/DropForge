using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    static List<Item> items = new List<Item>();

    static void Build()
    {
        // [1] raw materials
        items.Add(new MaterialItem("Bone"));
        items.Add(new MaterialItem("Hide"));
        items.Add(new MaterialItem("Slime Gel"));
        items.Add(new MaterialItem("Cotton"));
        items.Add(new MaterialItem("Iron Ore"));

        // [2] components (only from crafting)
        items.Add(new ComponentItem("Iron Plate"));
        items.Add(new ComponentItem("Cloth"));
        items.Add(new ComponentItem("Sword Handle"));

        // [3] equipment (name, slot, atk bonus, def bonus)
        items.Add(new EquipmentItem("Sword", EquipSlot.Weapon, 15, 0));
        items.Add(new EquipmentItem("Chestplate", EquipSlot.Chest, 0, 10));
        items.Add(new EquipmentItem("Leggings", EquipSlot.Legs, 0, 8));
        items.Add(new EquipmentItem("Helmet", EquipSlot.Head, 0, 6));
    }

    // sequential search by name, return null if not found
    public static Item Get(string itemName)
    {
        if (items.Count == 0)
        {
            Build();
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemName == itemName)
            {
                return items[i];
            }
        }

        Debug.LogWarning("Item not found: " + itemName);
        return null;
    }
}