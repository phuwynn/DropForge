using System.Collections.Generic;

[System.Serializable]
public class ItemSlot
{
    public Item item;
    public int amount;

    // keep the old name so other scripts can still read slot.itemName
    public string itemName
    {
        get { return item.itemName; }
    }
}

[System.Serializable]
public class Inventory
{
    public List<ItemSlot> slots = new List<ItemSlot>();

    // sequential search by item, return -1 if not found
    public int FindItem(Item item)
    {
        if (item == null)
        {
            return -1;
        }

        int index = -1;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item.itemName == item.itemName)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    // same search, but with the item name
    public int FindItem(string itemName)
    {
        return FindItem(ItemDatabase.Get(itemName));
    }

    public void Add(Item item, int amount)
    {
        if (item == null)
        {
            return;
        }

        // [1] search if we already have this item
        int index = FindItem(item);

        // [2] found = add the amount, not found = add a new slot
        if (index != -1)
        {
            slots[index].amount = slots[index].amount + amount;
        }
        else
        {
            ItemSlot newSlot = new ItemSlot();
            newSlot.item = item;
            newSlot.amount = amount;
            slots.Add(newSlot);
        }
    }

    public void Add(string itemName, int amount)
    {
        Add(ItemDatabase.Get(itemName), amount);
    }

    public bool Has(Item item, int amount)
    {
        int index = FindItem(item);
        if (index == -1)
        {
            return false;
        }
        if (slots[index].amount >= amount)
        {
            return true;
        }
        return false;
    }

    public bool Has(string itemName, int amount)
    {
        return Has(ItemDatabase.Get(itemName), amount);
    }

    public bool Remove(Item item, int amount)
    {
        // [1] check that we have enough
        if (Has(item, amount) == false)
        {
            return false;
        }

        // [2] reduce the amount
        int index = FindItem(item);
        slots[index].amount = slots[index].amount - amount;

        // [3] remove the slot when it is empty
        if (slots[index].amount == 0)
        {
            slots.RemoveAt(index);
        }
        return true;
    }

    public bool Remove(string itemName, int amount)
    {
        return Remove(ItemDatabase.Get(itemName), amount);
    }
}