using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CraftingSystem : MonoBehaviour
{
    public List<Recipe> recipes = new List<Recipe>();
    public bool isOpen = false;

    Player player;
    Recipe craftRequest = null;

    GUIStyle titleStyle;
    GUIStyle normalStyle;
    GUIStyle greenStyle;
    GUIStyle redStyle;
    GUIStyle buttonStyle;

    void Start()
    {
        player = FindFirstObjectByType<Player>();

        // [1] step 1: raw materials -> parts
        recipes.Add(new Recipe("Iron Plate", false, new string[] { "Iron Ore" }, new int[] { 2 }, 0, 0));
        recipes.Add(new Recipe("Cloth", false, new string[] { "Cotton" }, new int[] { 2 }, 0, 0));

        // [2] step 2: materials + parts -> sword handle
        recipes.Add(new Recipe("Sword Handle", false, new string[] { "Bone", "Hide", "Slime Gel", "Cloth" }, new int[] { 2, 1, 1, 1 }, 0, 0));

        // [3] step 3: parts + materials -> equipment
        recipes.Add(new Recipe("Sword", true, new string[] { "Iron Plate", "Sword Handle" }, new int[] { 2, 1 }, 15, 0));
        recipes.Add(new Recipe("Chestplate", true, new string[] { "Iron Plate", "Hide", "Cloth", "Slime Gel" }, new int[] { 2, 2, 1, 1 }, 0, 10));
        recipes.Add(new Recipe("Leggings", true, new string[] { "Iron Plate", "Hide", "Cloth", "Slime Gel" }, new int[] { 1, 2, 1, 1 }, 0, 8));
        recipes.Add(new Recipe("Helmet", true, new string[] { "Iron Plate", "Hide", "Cloth", "Slime Gel" }, new int[] { 1, 1, 1, 1 }, 0, 6));
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null || player == null)
        {
            return;
        }

        // [1] press C to open or close the menu
        if (kb.cKey.wasPressedThisFrame)
        {
            if (isOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }

        // [2] cheat key for the demo video (same amounts as the real drops)
        if (kb.f1Key.wasPressedThisFrame)
        {
            player.Pickup("Bone", 3);
            player.Pickup("Hide", 7);
            player.Pickup("Slime Gel", 5);
            player.Pickup("Cotton", 10);
            player.Pickup("Iron Ore", 14);
        }

        // [3] menu is open: keys 1-7 craft the recipe with that number
        if (isOpen)
        {
            Key[] numberKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7 };
            for (int i = 0; i < numberKeys.Length && i < recipes.Count; i++)
            {
                if (kb[numberKeys[i]].wasPressedThisFrame)
                {
                    craftRequest = recipes[i];
                }
            }
        }

        // [4] do the craft request (the button in OnGUI only sets craftRequest)
        if (craftRequest != null)
        {
            Craft(craftRequest);
            craftRequest = null;
        }
    }

    void OpenMenu()
    {
        isOpen = true;
        Time.timeScale = 0; // pause the game
    }

    void CloseMenu()
    {
        isOpen = false;
        Time.timeScale = 1;
    }

    // return the recipes that the player can craft right now
    public List<Recipe> GetCraftable()
    {
        List<Recipe> result = new List<Recipe>();
        for (int i = 0; i < recipes.Count; i++)
        {
            if (CanCraft(recipes[i]))
            {
                result.Add(recipes[i]);
            }
        }
        return result;
    }

    public bool CanCraft(Recipe recipe)
    {
        // equipment can be crafted only once
        if (recipe.isEquipment && player.IsEquipped(recipe.resultName))
        {
            return false;
        }
        return recipe.HasEnough(player.inventory);
    }

    public bool Craft(Recipe recipe)
    {
        if (CanCraft(recipe) == false)
        {
            return false;
        }

        // [1] take the ingredients out of the inventory
        for (int i = 0; i < recipe.ingredientNames.Length; i++)
        {
            player.inventory.Remove(recipe.ingredientNames[i], recipe.ingredientAmounts[i]);
        }

        // [2] equipment = wear it, part = put it in the inventory
        if (recipe.isEquipment)
        {
            player.Equip(recipe.resultName, recipe.atkBonus, recipe.defBonus);
        }
        else
        {
            player.Pickup(recipe.resultName, 1);
        }
        return true;
    }

    // ---------- menu (temporary UI for the draft) ----------

    void MakeStyles()
    {
        if (normalStyle != null)
        {
            return;
        }

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 24;
        titleStyle.fontStyle = FontStyle.Bold;

        normalStyle = new GUIStyle(GUI.skin.label);
        normalStyle.fontSize = 15;

        greenStyle = new GUIStyle(normalStyle);
        greenStyle.normal.textColor = Color.green;

        redStyle = new GUIStyle(normalStyle);
        redStyle.normal.textColor = Color.red;

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 15;
    }

    void OnGUI()
    {
        if (isOpen == false || player == null)
        {
            return;
        }
        MakeStyles();

        GUILayout.BeginArea(new Rect(Screen.width / 2 - 380, 40, 760, Screen.height - 80), GUI.skin.box);
        GUILayout.Label("CRAFTING   (press C to close)", titleStyle);
        GUILayout.Label("Can craft now: " + GetCraftable().Count, normalStyle);

        // [1] parts
        GUILayout.Label("---- Parts ----", normalStyle);
        for (int i = 0; i < recipes.Count; i++)
        {
            if (recipes[i].isEquipment == false)
            {
                DrawRecipe(recipes[i], i);
            }
        }

        // [2] equipment
        GUILayout.Label("---- Equipment ----", normalStyle);
        for (int i = 0; i < recipes.Count; i++)
        {
            if (recipes[i].isEquipment)
            {
                DrawRecipe(recipes[i], i);
            }
        }

        GUILayout.EndArea();
    }

    void DrawRecipe(Recipe r, int number)
    {
        GUILayout.BeginHorizontal();

        // name and bonus
        string bonusText = "";
        if (r.atkBonus > 0)
        {
            bonusText = " (ATK +" + r.atkBonus + ")";
        }
        if (r.defBonus > 0)
        {
            bonusText = " (DEF +" + r.defBonus + ")";
        }
        GUILayout.Label((number + 1) + ". " + r.resultName + bonusText, normalStyle, GUILayout.Width(190));

        // ingredients: green = enough, red = not enough
        for (int i = 0; i < r.ingredientNames.Length; i++)
        {
            int have = 0;
            int index = player.inventory.FindItem(r.ingredientNames[i]);
            if (index != -1)
            {
                have = player.inventory.slots[index].amount;
            }

            GUIStyle style = redStyle;
            if (have >= r.ingredientAmounts[i])
            {
                style = greenStyle;
            }
            GUILayout.Label(r.ingredientNames[i] + " " + have + "/" + r.ingredientAmounts[i], style, GUILayout.Width(105));
        }
        GUILayout.FlexibleSpace();

        // button
        if (r.isEquipment && player.IsEquipped(r.resultName))
        {
            GUILayout.Label("Crafted", greenStyle, GUILayout.Width(80));
        }
        else
        {
            GUI.enabled = CanCraft(r);
            if (GUILayout.Button("Craft", buttonStyle, GUILayout.Width(80)))
            {
                craftRequest = r;
            }
            GUI.enabled = true;
        }

        GUILayout.EndHorizontal();
    }
}