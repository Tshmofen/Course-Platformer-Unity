﻿using UnityEngine;

namespace ThirdParty.QPathFinder.Script
{
    public static class Logger
    {
        public enum Level
        {
            Info = 1,    // Prints everything
            Warnings = 2,   // Prints warnings and errors
            Errors = 3,      // prints only errors
            None = 4
        }

        public static void SetLoggingLevel(Level level)
        {
            _logLevel = level;
        }

        public static bool CanLogWarning => _logLevel <= Level.Warnings || IsRunningInEditorMode;

        public static void LogWarning(string message, bool includeTimeStamp = false)
        {
            Log(Level.Warnings, message, includeTimeStamp);
        }

        public static bool CanLogError => _logLevel <= Level.Errors || IsRunningInEditorMode;

        public static void LogError(string message, bool includeTimeStamp = false)
        {
            Log(Level.Errors, message, includeTimeStamp);
        }

        public static bool CanLogInfo => _logLevel <= Level.Info || IsRunningInEditorMode;

        public static void LogInfo(string message, bool includeTimeStamp = false)
        {
            Log(Level.Info, message, includeTimeStamp);
        }

        private static void Log(Level level, string message, bool includeTimeStamp = false)
        {
            bool isEditorMode = IsRunningInEditorMode;
            if (includeTimeStamp)
                message = "[Time:" + Time.realtimeSinceStartup + "]" + message;

            if (level == Level.Info)
            {
                if (_logLevel <= level || isEditorMode)
                    Debug.Log("[QPathFinder:Info] " + message);
            }
            else if (level == Level.Warnings)
            {
                if (_logLevel <= level || isEditorMode)
                    Debug.LogWarning("[QPathFinder:Warn] " + message);
            }
            else if (level == Level.Errors)
            {
                if (_logLevel <= level || isEditorMode)
                    Debug.LogError("[QPathFinder:Err] " + message);
            }
        }

        public static void SetDebugDrawLineDuration(float duration)
        {
            DrawLineDuration = duration;
        }

        public static float DrawLineDuration { get; private set; }

        private static bool IsRunningInEditorMode => !Application.isPlaying;
        private static Level _logLevel = Level.Warnings;
    }
}