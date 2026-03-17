using System;

public static class ActionExtensions
{
    /// <summary>
    /// Clears all listeners from an Action.
    /// </summary>
    public static void ClearListeners(this Action action)
    {
        action = null;
    }

    /// <summary>
    /// Clears all listeners from an Action with a parameter.
    /// </summary>
    public static void ClearListeners<T>(this Action<T> action)
    {
        action = null;
    }

    /// <summary>
    /// Adds multiple listeners to an Action.
    /// </summary>
    public static void AddListeners(this Action action, params Action[] listeners)
    {
        foreach (var listener in listeners)
        {
            action += listener;
        }
    }

    /// <summary>
    /// Removes multiple listeners from an Action.
    /// </summary>
    public static void RemoveListeners(this Action action, params Action[] listeners)
    {
        foreach (var listener in listeners)
        {
            action -= listener;
        }
    }
}
