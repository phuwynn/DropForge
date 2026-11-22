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

        // game is over (paused but our menu is not open) = do nothing
        if (Time.timeScale == 0 && isOpen == false)
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
        if (DevKeys.Enabled && kb.f1Key.wasPressedThisFrame)
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

    GUIStyle grayStyle;

        void MakeStyles()
    {
        if (normalStyle != null)
        {
            return;
        }

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 26;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.wordWrap = false;
        LockColor(titleStyle, Color.white);

        normalStyle = new GUIStyle(GUI.skin.label);
        normalStyle.fontSize = 15;
        normalStyle.wordWrap = false;
        normalStyle.alignment = TextAnchor.MiddleLeft;
        LockColor(normalStyle, Color.white);

        grayStyle = new GUIStyle(normalStyle);
        LockColor(grayStyle, new Color(0.7f, 0.7f, 0.7f));

        greenStyle = new GUIStyle(normalStyle);
        LockColor(greenStyle, new Color(0.4f, 1f, 0.4f));

        redStyle = new GUIStyle(normalStyle);
        LockColor(redStyle, new Color(1f, 0.4f, 0.4f));

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 15;
    }

    // set the same text color for every state (so hover does not change the color)
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

    // draw a solid color rectangle
    void DrawRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    void OnGUI()
    {
        if (isOpen == false || player == null)
        {
            return;
        }
        MakeStyles();

        // [1] dark screen so the game behind the menu is hard to see
        DrawRect(new Rect(0, 0, Screen.width, Screen.height), new Color(0, 0, 0, 0.7f));

        // [2] the menu panel
        float panelW = 900;
        float panelH = 460;
        float x0 = (Screen.width - panelW) / 2;
        float y0 = (Screen.height - panelH) / 2;
        if (y0 < 10)
        {
            y0 = 10;
        }
        DrawRect(new Rect(x0, y0, panelW, panelH), new Color(0.1f, 0.12f, 0.18f, 1f));

        // [3] title
        GUI.Label(new Rect(x0 + 20, y0 + 10, 600, 36), "CRAFTING", titleStyle);
        GUI.Label(new Rect(x0 + 20, y0 + 46, 860, 24), "Can craft now: " + GetCraftable().Count + "     (press C to close, or press the number key to craft)", grayStyle);

        float y = y0 + 80;
        float rowHeight = 42;

        // [4] parts
        DrawSectionHeader("PARTS", x0, y, panelW);
        y = y + 28;
        for (int i = 0; i < recipes.Count; i++)
        {
            if (recipes[i].isEquipment == false)
            {
                DrawRecipe(recipes[i], i, x0, y);
                y = y + rowHeight;
            }
        }

        // [5] equipment
        DrawSectionHeader("EQUIPMENT", x0, y, panelW);
        y = y + 28;
        for (int i = 0; i < recipes.Count; i++)
        {
            if (recipes[i].isEquipment)
            {
                DrawRecipe(recipes[i], i, x0, y);
                y = y + rowHeight;
            }
        }

        // line under the last row
        DrawRect(new Rect(x0 + 10, y, panelW - 20, 1), new Color(1, 1, 1, 0.3f));
    }

    void DrawSectionHeader(string text, float x0, float y, float panelW)
    {
        DrawRect(new Rect(x0 + 10, y, panelW - 20, 26), new Color(1, 1, 1, 0.12f));
        GUI.Label(new Rect(x0 + 20, y, 300, 26), text, grayStyle);
    }

    void DrawRecipe(Recipe r, int number, float x0, float y)
    {
        // line on top of the row
        DrawRect(new Rect(x0 + 10, y, 880, 1), new Color(1, 1, 1, 0.3f));

        // name
        GUI.Label(new Rect(x0 + 20, y, 160, 42), (number + 1) + ". " + r.resultName, normalStyle);

        // bonus
        string bonusText = "";
        if (r.atkBonus > 0)
        {
            bonusText = "ATK +" + r.atkBonus;
        }
        if (r.defBonus > 0)
        {
            bonusText = "DEF +" + r.defBonus;
        }
        GUI.Label(new Rect(x0 + 180, y, 80, 42), bonusText, grayStyle);

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
            string text = r.ingredientNames[i] + " " + have + "/" + r.ingredientAmounts[i];
            GUI.Label(new Rect(x0 + 270 + i * 130, y, 130, 42), text, style);
        }

        // button (or "Crafted" for equipment that we already have)
        if (r.isEquipment && player.IsEquipped(r.resultName))
        {
            GUI.Label(new Rect(x0 + 800, y, 80, 42), "Crafted", greenStyle);
        }
        else
        {
            GUI.enabled = CanCraft(r);
            if (GUI.Button(new Rect(x0 + 800, y + 6, 80, 30), "Craft", buttonStyle))
            {
                craftRequest = r;
            }
            GUI.enabled = true;
        }
    }
}