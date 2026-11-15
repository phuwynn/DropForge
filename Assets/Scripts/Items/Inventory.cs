using System.Collections.Generic;

[System.Serializable]
public class ItemSlot
{
    public string itemName;
    public int amount;
}

[System.Serializable]
public class Inventory
{
    public List<ItemSlot> slots = new List<ItemSlot>();

    // sequential search, return -1 if not found
    public int FindItem(string itemName)
    {
        int index = -1;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].itemName == itemName)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public void Add(string itemName, int amount)
    {
        // [1] search if we already have this item
        int index = FindItem(itemName);

        // [2] found = add the amount, not found = add a new slot
        if (index != -1)
        {
            slots[index].amount = slots[index].amount + amount;
        }
        else
        {
            ItemSlot newSlot = new ItemSlot();
            newSlot.itemName = itemName;
            newSlot.amount = amount;
            slots.Add(newSlot);
        }
    }

    public bool Has(string itemName, int amount)
    {
        int index = FindItem(itemName);
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

    public bool Remove(string itemName, int amount)
    {
        // [1] check that we have enough
        if (Has(itemName, amount) == false)
        {
            return false;
        }

        // [2] reduce the amount
        int index = FindItem(itemName);
        slots[index].amount = slots[index].amount - amount;

        // [3] remove the slot when it is empty
        if (slots[index].amount == 0)
        {
            slots.RemoveAt(index);
        }
        return true;
    }
}