using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LoggingUtility
{

    /// <summary>
    /// Log a stack trace that has removed most of the information that is not a script.
    /// </summary>
    public void LogCleanedUpStackTrace(string MessageToLogWithTrace, string fullStackTrace)
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
