using UnityEngine;
using System.IO;
using System;

public static class DebugLogger
{
    private static string logFilePath;
    private static bool isInitialized = false;
    
    public static void Initialize()
    {
        if (isInitialized) return;
        
        logFilePath = Path.Combine(Application.dataPath, "..", "enemy_movement_debug.txt");
        
        // Clear the log file at start
        File.WriteAllText(logFilePath, $"Enemy Movement Debug Log - Started at {DateTime.Now}\n");
        File.AppendAllText(logFilePath, "===========================================\n\n");
        
        isInitialized = true;
        Log("DebugLogger initialized. Log file: " + logFilePath);
    }
    
    public static void Log(string message)
    {
        if (!isInitialized) Initialize();
        
        string timestampedMessage = $"[{Time.time:F3}] {message}\n";
        
        // Also log to Unity console
        Debug.Log(message);
        
        // Write to file
        try
        {
            File.AppendAllText(logFilePath, timestampedMessage);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write to log file: {e.Message}");
        }
    }
    
    public static void LogError(string message)
    {
        Log($"ERROR: {message}");
    }
}