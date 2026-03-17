using System.Diagnostics;


public static class LoggingUtility
{
    [Conditional("UNITY_EDITOR")]
    public static void EditorOnlyLog(string message, bool log = true)
    {
        if (!log) return;

        UnityEngine.Debug.Log(message);
    }

    [Conditional("UNITY_EDITOR")]
    public static void EditorOnlyLogWarning(string message, bool log = true)
    {
        if (!log) return;

        UnityEngine.Debug.LogWarning(message);
    }

    [Conditional("UNITY_EDITOR")]
    public static void EditorOnlyLogError(string message, bool log = true)
    {
        if (!log) return;

        UnityEngine.Debug.LogError(message);
    }

    //Deprecated

    /// <summary>
    /// Log a stack trace that has removed most of the information that is not a script.
    /// </summary>
    public static void LogCleanedUpStackTrace(string MessageToLogWithTrace, string fullStackTrace)
    {
#if UNITY_EDITOR

        string[] stackLines = fullStackTrace.Split('\n'); // Split stack trace into lines
        string scriptStackTrace = "";

        foreach (string line in stackLines)
        {
            // Filter out lines that don't contain UnityEngine (usually internal Unity methods)
            if (line.Contains("at"))
            {
                if (!line.Contains("UnityEngine.") && !line.Contains("System.") && !line.Contains("Mono."))
                {
                    scriptStackTrace += line + "\n";
                }
            }
        }

        UnityEngine.Debug.Log($"{MessageToLogWithTrace}.\n{scriptStackTrace}");
#endif
    }
}
