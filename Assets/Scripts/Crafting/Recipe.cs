[System.Serializable]
public class Recipe
{
    public string resultName;
    public bool isEquipment;
    public string[] ingredientNames;
    public int[] ingredientAmounts;
    public int atkBonus;
    public int defBonus;

    public Recipe(string resultName, bool isEquipment, string[] names, int[] amounts, int atkBonus, int defBonus)
    {
        this.resultName = resultName;
        this.isEquipment = isEquipment;
        this.ingredientNames = names;
        this.ingredientAmounts = amounts;
        this.atkBonus = atkBonus;
        this.defBonus = defBonus;
    }

    // check every ingredient: one missing = cannot craft
    public bool HasEnough(Inventory inv)
    {
        for (int i = 0; i < ingredientNames.Length; i++)
        {
            if (inv.Has(ingredientNames[i], ingredientAmounts[i]) == false)
            {
                return false;
            }
        }
        return true;
    }
}