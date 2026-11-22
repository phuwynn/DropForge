using UnityEngine;

// test keys (F1, F2, F3, K) work only inside the Unity editor, not in a build
public static class DevKeys
{
    public static bool Enabled
    {
        get { return Application.isEditor; }
    }
}