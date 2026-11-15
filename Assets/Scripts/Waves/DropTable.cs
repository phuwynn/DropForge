using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DropEntry
{
    public string itemName;
    public int amount = 1;
    public int chance = 100; // percent, 100 = always drop
}

[System.Serializable]
public class DropTable
{
    public List<DropEntry> dropList = new List<DropEntry>();

    // go through the list and return the items that we got
    public List<DropEntry> Roll()
    {
        List<DropEntry> result = new List<DropEntry>();

        for (int i = 0; i < dropList.Count; i++)
        {
            // random number 0-99, less than chance = we got this item
            int number = Random.Range(0, 100);
            if (number < dropList[i].chance)
            {
                result.Add(dropList[i]);
            }
        }
        return result;
    }
}