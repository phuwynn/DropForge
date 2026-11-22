using UnityEngine;

public enum EquipSlot
{
    Weapon,
    Chest,
    Legs,
    Head
}

// parent class of everything that can be in the inventory
public class Item
{
    public string itemName;
    public Sprite icon; // picture (we will set it later when we have the assets)

    public Item(string itemName)
    {
        this.itemName = itemName;
    }

    // virtual = every child class writes its own answer
    public virtual string GetTypeName()
    {
        return "Item";
    }
}

// raw material: dropped by enemies or resource nodes
public class MaterialItem : Item
{
    public MaterialItem(string itemName) : base(itemName)
    {
    }

    public override string GetTypeName()
    {
        return "Material";
    }
}

// component (part): can only be made by crafting
public class ComponentItem : Item
{
    public ComponentItem(string itemName) : base(itemName)
    {
    }

    public override string GetTypeName()
    {
        return "Component";
    }
}

// equipment: the player can wear it
public class EquipmentItem : Item
{
    public EquipSlot slot;
    public int atkBonus;
    public int defBonus;

    public EquipmentItem(string itemName, EquipSlot slot, int atkBonus, int defBonus) : base(itemName)
    {
        this.slot = slot;
        this.atkBonus = atkBonus;
        this.defBonus = defBonus;
    }

    public override string GetTypeName()
    {
        return "Equipment";
    }
}