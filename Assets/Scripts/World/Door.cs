using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isOpen = false;
    public int requiredEquipment = 4;
    public Sprite closedSprite;
    public Sprite openSprite;

    WaveManager waveManager;
    SpriteRenderer sr;
    bool showWarning = false;

    GUIStyle titleStyle;
    GUIStyle textStyle;
    GUIStyle buttonStyle;

    void Start()
    {
        waveManager = FindFirstObjectByType<WaveManager>();
        SetOpen(false);
    }

    // open = glowing cave door, closed = normal cave door
    public void SetOpen(bool open)
    {
        isOpen = open;

        if (sr == null)
        {
            sr = GetComponent<SpriteRenderer>();
        }
        if (sr == null)
        {
            return;
        }

        // [1] have both pictures = swap the picture
        if (closedSprite != null && openSprite != null)
        {
            if (open)
            {
                sr.sprite = openSprite;
            }
            else
            {
                sr.sprite = closedSprite;
            }
            sr.color = Color.white;
        }
        else
        {
            // [2] no pictures yet = gray color when closed, green when open
            if (open)
            {
                sr.color = new Color(0.4f, 1f, 0.4f);
            }
            else
            {
                sr.color = new Color(0.4f, 0.4f, 0.4f);
            }
        }
    }

    // called when something enters the trigger area of the door
    void OnTriggerEnter2D(Collider2D other)
    {
        // [1] closed door = do nothing
        if (isOpen == false)
        {
            return;
        }

        // [2] only the player can use the door
        Player player = other.GetComponent<Player>();
        if (player == null)
        {
            return;
        }

        // [3] not all equipment crafted = ask first, otherwise go straight to wave 2
        if (player.equippedItems.Count < requiredEquipment)
        {
            showWarning = true;
            Time.timeScale = 0; // pause the game while the box is open
        }
        else
        {
            EnterNextWave();
        }
    }

    void EnterNextWave()
    {
        showWarning = false;
        Time.timeScale = 1;

        if (waveManager != null)
        {
            waveManager.StartNextWave();
        }

        // the door is used, hide it
        gameObject.SetActive(false);
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

    // warning box (temporary UI for the draft)
    void OnGUI()
    {
        if (showWarning == false)
        {
            return;
        }
        GUI.depth = -5;

        // [1] make the styles once
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 36;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            LockColor(titleStyle, new Color(0.95f, 0.75f, 0.2f));

            textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontSize = 18;
            textStyle.alignment = TextAnchor.MiddleCenter;
            textStyle.wordWrap = true;
            LockColor(textStyle, Color.white);

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 20;
        }

        // [2] dark screen and the box in the middle
        DrawRect(new Rect(0, 0, Screen.width, Screen.height), new Color(0, 0, 0, 0.6f));

        float boxW = 520;
        float boxH = 280;
        float x = (Screen.width - boxW) / 2;
        float y = (Screen.height - boxH) / 2;

        DrawRect(new Rect(x - 4, y - 4, boxW + 8, boxH + 8), new Color(0.95f, 0.75f, 0.2f));
        DrawRect(new Rect(x, y, boxW, boxH), new Color(0.1f, 0.12f, 0.18f, 1f));

        // [3] text
        GUI.Label(new Rect(x, y + 15, boxW, 60), "NOT READY!", titleStyle);
        GUI.Label(new Rect(x + 20, y + 80, boxW - 40, 90), "You have not crafted all " + requiredEquipment + " pieces of equipment. The boss in wave 2 is very strong. Go anyway?", textStyle);

        // [4] two buttons
        if (GUI.Button(new Rect(x + 40, y + 195, 200, 55), "Go back", buttonStyle))
        {
            showWarning = false;
            Time.timeScale = 1;
        }
        if (GUI.Button(new Rect(x + boxW - 240, y + 195, 200, 55), "Go anyway", buttonStyle))
        {
            EnterNextWave();
        }
    }
}