using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    Won,
    Lost
}

public class GameManager : MonoBehaviour
{
    public GameState state = GameState.Playing;

    GUIStyle titleStyle;
    GUIStyle buttonStyle;
    bool restarting = false;

    void Start()
    {
        // make sure the game is running when the scene starts
        Time.timeScale = 1;
        state = GameState.Playing;
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
        {
            return;
        }

        // test key for the draft: F3 = show the win screen
        if (DevKeys.Enabled && kb.f3Key.wasPressedThisFrame)
        {
            Win();
        }
    }

    public void Win()
    {
        // already finished = do nothing
        if (state != GameState.Playing)
        {
            return;
        }

        state = GameState.Won;
        Time.timeScale = 0; // pause the game
        Debug.Log("You win");
        AudioManager am = FindFirstObjectByType<AudioManager>();
        if (am != null)
        {
            am.PlayWin();
        }
    }

    public void Lose()
    {
        // already finished = do nothing
        if (state != GameState.Playing)
        {
            return;
        }

        state = GameState.Lost;
        Time.timeScale = 0; // pause the game
        Debug.Log("You lose");
        AudioManager am = FindFirstObjectByType<AudioManager>();
        if (am != null)
        {
            am.PlayLose();
        }
    }

    public void Restart()
    {
        // already restarting = ignore extra clicks
        if (restarting)
        {
            return;
        }
        restarting = true;

        // [1] play the click sound
        AudioManager am = FindFirstObjectByType<AudioManager>();
        if (am != null)
        {
            am.PlayClick();
        }

        // [2] wait a moment, then load the scene (the scene load destroys the AudioManager and cuts the sound)
        StartCoroutine(RestartAfterDelay());
    }

    IEnumerator RestartAfterDelay()
    {
        // WaitForSecondsRealtime still works when Time.timeScale is 0
        yield return new WaitForSecondsRealtime(0.25f);

        // unpause, then load the same scene again (everything starts from wave 1)
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // draw a solid color rectangle
    void DrawRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
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

    // end screen box (temporary UI for the draft)
    void OnGUI()
    {
        if (state == GameState.Playing)
        {
            return;
        }

        // draw on top of every other GUI
        GUI.depth = -10;

        // [1] make the styles once
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 48;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 24;
        }

        // [2] dark screen behind the box
        DrawRect(new Rect(0, 0, Screen.width, Screen.height), new Color(0, 0, 0, 0.7f));

        // [3] the box in the middle of the screen (with a colored border)
        float boxW = 420;
        float boxH = 240;
        float x = (Screen.width - boxW) / 2;
        float y = (Screen.height - boxH) / 2;

        string title = "YOU DIED";
        Color borderColor = new Color(0.9f, 0.25f, 0.25f);
        if (state == GameState.Won)
        {
            title = "YOU WIN!";
            borderColor = new Color(0.4f, 0.9f, 0.4f);
        }

        DrawRect(new Rect(x - 4, y - 4, boxW + 8, boxH + 8), borderColor);
        DrawRect(new Rect(x, y, boxW, boxH), new Color(0.1f, 0.12f, 0.18f, 1f));

        // [4] title text (lock the color so it does not turn white when the mouse is over it)
        LockColor(titleStyle, borderColor);
        GUI.Label(new Rect(x, y + 25, boxW, 80), title, titleStyle);

        // [5] restart button
        if (GUI.Button(new Rect(x + 100, y + 135, 220, 60), "Restart", buttonStyle))
        {
            Restart();
        }
    }
}