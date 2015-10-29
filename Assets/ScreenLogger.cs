﻿using UnityEngine;
using System.Collections.Generic;

public class ScreenLogger : MonoBehaviour
{
    [Tooltip("Height of the log area as a percentage of the screen height")]
    [Range(0.3f, 1.0f)]
    public float Height = 1f;
    public int Padding = 20;

    [Range(10, 30)]
    public int FontSize = 13;
    public bool ShowInEditor = false;

    public bool StackTraceForLog = false;
    public bool StackTraceForWarning = false;
    public bool StackTraceForError = true;
    public bool StackTraceForException = true;
    public bool StackTraceForAssert = true;

    static Queue<LogMessage> queue = new Queue<LogMessage>();
    int textHeight { get {
            if (FontSize < 10) FontSize = 10;
            if (FontSize > 30) FontSize = 30;

            return fontHeights[FontSize - 10];
        }
    }

    int[] fontHeights =
    {
        11, // 10
        13, // 11
        14, // 12
        15, // 13
        16, // 14
        17, // 15
        18, // 16
        20, // 17
        21, // 18
        22, // 19
        23, // 20
        24, // 21
        25, // 22
        26, // 23
        28, // 24
        29, // 25
        30, // 26
        31, // 27
        32, // 28
        33, // 29
        34  // 30
    };

    void OnEnable()
    {
        if (!ShowInEditor && Application.isEditor) return;

        queue = new Queue<LogMessage>();

        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        if (!ShowInEditor && Application.isEditor) return;

        Application.logMessageReceived -= HandleLog;
    }

    void Update()
    {
        if (!ShowInEditor && Application.isEditor) return;

        while (queue.Count > (Screen.height - 2 * Padding) * Height / textHeight)
            queue.Dequeue();
    }

    void OnGUI()
    {
        if (!ShowInEditor && Application.isEditor) return;

        GUILayout.BeginArea(
            new Rect(
                Padding,
                Padding + (Screen.height - 2 * Padding) * (1 - Height),
                Screen.width - 2 * Padding,
                (Screen.height - 2 * Padding) * Height)
        );

        GUIStyle style = new GUIStyle();
        style.fontSize = FontSize;

        foreach (LogMessage m in queue)
        {
            switch (m.Type)
            {
                case LogType.Warning:
                    style.normal.textColor = Color.yellow;
                    break;

                case LogType.Log:
                    style.normal.textColor = Color.white;
                    break;

                case LogType.Assert:
                case LogType.Exception:
                case LogType.Error:
                    style.normal.textColor = Color.red;
                    break;

                default:
                    style.normal.textColor = Color.white;
                    break;
            }

            GUILayout.Label(m.Message, style);
        }

        GUILayout.EndArea();
    }

    void HandleLog(string message, string stackTrace, LogType type)
    {
        queue.Enqueue(new LogMessage(message, type));

        if (type == LogType.Assert && !StackTraceForAssert) return;
        if (type == LogType.Error && !StackTraceForError) return;
        if (type == LogType.Exception && !StackTraceForException) return;
        if (type == LogType.Log && !StackTraceForLog) return;
        if (type == LogType.Warning && !StackTraceForWarning) return;

        string[] trace = stackTrace.Split(new char[] { '\n' });

        foreach (string t in trace)
            if (t.Length != 0) queue.Enqueue(new LogMessage("  " + t, type));
    }
}

class LogMessage
{
    public string Message;
    public LogType Type;

    public LogMessage(string msg, LogType type)
    {
        Message = msg;
        Type = type;
    }
}