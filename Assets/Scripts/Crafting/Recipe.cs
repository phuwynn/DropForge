[System.Serializable]
public class Recipe
{
    // used by the logic
    public Item result;
    public Item[] ingredients;
    public int[] ingredientAmounts;

    // kept so the crafting menu code does not need to change
    public string resultName;
    public string[] ingredientNames;
    public bool isEquipment;
    public int atkBonus;
    public int defBonus;

    public Recipe(string resultName, string[] names, int[] amounts)
    {
        // [1] find the Item objects from the database
        this.resultName = resultName;
        result = ItemDatabase.Get(resultName);

        ingredientNames = names;
        ingredientAmounts = amounts;
        ingredients = new Item[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            ingredients[i] = ItemDatabase.Get(names[i]);
        }

        // [2] if the result is equipment, take the bonus from the Equipment class
        EquipmentItem equipment = result as EquipmentItem;
        if (equipment != null)
        {
            isEquipment = true;
            atkBonus = equipment.atkBonus;
            defBonus = equipment.defBonus;
        }
    }

    // check every ingredient: one missing = cannot craft
    public bool HasEnough(Inventory inv)
    {
        for (int i = 0; i < ingredients.Length; i++)
        {
            if (inv.Has(ingredients[i], ingredientAmounts[i]) == false)
            {
                return false;
            }
        }
        return true;
    }
}