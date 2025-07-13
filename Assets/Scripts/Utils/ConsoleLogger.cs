using UnityEngine;
using System.IO;
using System;

public class ConsoleLogger : MonoBehaviour
{
    [Header("Console Logging Settings")]
    public bool enableConsoleLogging = true;
    public bool logToFile = true;
    public bool includeStackTrace = false;
    
    private StreamWriter logWriter;
    private string logFilePath;
    
    private void Awake()
    {
        if (enableConsoleLogging)
        {
            InitializeConsoleLogger();
            Application.logMessageReceived += OnLogMessageReceived;
        }
    }
    
    private void InitializeConsoleLogger()
    {
        if (!logToFile) return;
        
        try
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"console_log_{timestamp}.txt";
            logFilePath = Path.Combine(Application.dataPath, "..", fileName);
            
            logWriter = new StreamWriter(logFilePath, false);
            logWriter.WriteLine($"=== UNITY CONSOLE LOG ===");
            logWriter.WriteLine($"Session started: {DateTime.Now}");
            logWriter.WriteLine($"Unity Version: {Application.unityVersion}");
            logWriter.WriteLine($"Platform: {Application.platform}");
            logWriter.WriteLine();
            logWriter.Flush();
            
            Debug.Log($"Console logger initialized: {logFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize console logger: {e.Message}");
            logToFile = false;
        }
    }
    
    private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
    {
        if (!logToFile || logWriter == null) return;
        
        // Filter out repetitive messages
        if (ShouldFilterMessage(logString)) return;
        
        try
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string unityTime = Time.time.ToString("F3");
            string logLevel = GetLogTypeString(type);
            
            logWriter.WriteLine($"[{timestamp}] [{unityTime}s] [{logLevel}] {logString}");
            
            if (includeStackTrace && !string.IsNullOrEmpty(stackTrace) && type == LogType.Error)
            {
                logWriter.WriteLine($"Stack Trace:");
                logWriter.WriteLine(stackTrace);
                logWriter.WriteLine();
            }
            
            logWriter.Flush();
        }
        catch (Exception e)
        {
            // Avoid recursive logging by not using Debug.LogError here
            Console.WriteLine($"Error writing to log file: {e.Message}");
        }
    }
    
    private bool ShouldFilterMessage(string message)
    {
        // Filter out repetitive action failure and money messages
        if (message.Contains("Action ") && message.Contains("failed to execute")) return true;
        if (message.Contains("Action ") && message.Contains("executed successfully")) return true;
        if (message.Contains("Not enough money to build that!")) return true;
        
        return false;
    }
    
    private string GetLogTypeString(LogType type)
    {
        switch (type)
        {
            case LogType.Error: return "ERROR";
            case LogType.Assert: return "ASSERT";
            case LogType.Warning: return "WARN";
            case LogType.Log: return "INFO";
            case LogType.Exception: return "EXCEPTION";
            default: return "UNKNOWN";
        }
    }
    
    private void CloseLogger()
    {
        if (logWriter != null)
        {
            try
            {
                logWriter.WriteLine();
                logWriter.WriteLine($"=== LOG SESSION ENDED: {DateTime.Now} ===");
                logWriter.Close();
                logWriter.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error closing log file: {e.Message}");
            }
            finally
            {
                logWriter = null;
            }
        }
    }
    
    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLogMessageReceived;
        CloseLogger();
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) CloseLogger();
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) CloseLogger();
    }
    
    private void OnApplicationQuit()
    {
        Application.logMessageReceived -= OnLogMessageReceived;
        CloseLogger();
    }
}