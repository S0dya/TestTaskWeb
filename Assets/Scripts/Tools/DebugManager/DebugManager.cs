using UnityEngine;
using System;
using UnityEditor;

#region enums

public enum DebugStatus
{
    Log,
    Point,
    Warining,
    Error,
}

[Flags]
public enum DebugCategory
{
    None = 0,
    Points = 1 << 0,
    Errors = 1 << 1,
    UI = 1 << 2,
    Net = 1 << 3,
}

#endregion

public static class DebugManager
{
    private static DebugCategory _activeCategories;

    private const string _debugPrefsKey = "DebugManagerCategories";

    static DebugManager()
    {
#if UNITY_EDITOR
        LoadDebugCategories();
#endif
    }

    public static void EnableDebug(DebugCategory category)
    {
        _activeCategories |= category;
    }

    public static void DisableDebug(DebugCategory category)
    {
        _activeCategories &= ~category;
    }

    public static bool IsDebugEnabled(DebugCategory category) => _activeCategories.HasFlag(category);

    public static void Log(DebugCategory category, string message, DebugStatus debugStatus = DebugStatus.Log)
    {
        if (category == DebugCategory.None) return;

        if (IsDebugEnabled(category))
        {
            var log = $"[{category}] {message}";

            switch (debugStatus)
            {
                case DebugStatus.Point: log = "<--" + log + "-->"; break;
                case DebugStatus.Warining: log = "<color=yellow>WARNING:</color> " + log; break;
                case DebugStatus.Error: log = "<color=red>ERROR:</color> " + log; break;
            }

            Debug.Log(log);
        }
    }

    #region save, load, editor
#if UNITY_EDITOR
    public static void SaveDebugCategories()
    {
        EditorPrefs.SetInt(_debugPrefsKey, (int)_activeCategories);
    }
    private static void LoadDebugCategories()
    {
        _activeCategories = EditorPrefs.HasKey(_debugPrefsKey) ?
            (DebugCategory)EditorPrefs.GetInt(_debugPrefsKey) :
             DebugCategory.None;
    }
#endif
    #endregion
}
