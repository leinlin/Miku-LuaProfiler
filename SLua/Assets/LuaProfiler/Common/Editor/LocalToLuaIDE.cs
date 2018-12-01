/*
* ==============================================================================
* Filename: LocalToLuaIDE
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;


public class LocalToLuaIDE : Editor
{

#if UNITY_EDITOR_WIN
    /// <summary>
    /// 获取窗体的句柄函数
    /// </summary>
    /// <param name="lpClassName">窗口类名</param>
    /// <param name="lpWindowName">窗口标题名</param>
    /// <returns>返回句柄</returns>
    [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    /// <summary>
    /// 最大化窗口，最小化窗口，正常大小窗口；
    /// </summary>
    [DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto)]
    public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

    [DllImport("user32.dll", EntryPoint = "IsWindowVisible", CharSet = CharSet.Auto)]
    public static extern bool IsWindowVisible(IntPtr hwnd);
#endif

    private const string EXTERNAL_EDITOR_PATH_KEY = "miku_mTv8";
    private const string LUA_PROJECT_ROOT_FOLDER_PATH_KEY = "miku_obUd";


    public static void OnOpenAsset(string file, int line)
    {
        string filePath = file;

        string projectRootPath = EditorUserSettings.GetConfigValue(LUA_PROJECT_ROOT_FOLDER_PATH_KEY);
        if (string.IsNullOrEmpty(projectRootPath) || !Directory.Exists(projectRootPath))
        {
            SetExternalProjectRootPath();
            projectRootPath = EditorUserSettings.GetConfigValue(LUA_PROJECT_ROOT_FOLDER_PATH_KEY);
        }
        filePath = Path.Combine(projectRootPath.Trim(), filePath.Trim());//+ ".lua";

        if (File.Exists(filePath + ".lua"))
        {
            filePath = filePath + ".lua";
        }
        else if (File.Exists(filePath + ".txt"))
        {
            filePath = filePath + ".txt";
        }
        else if (File.Exists(filePath + ".bytes"))
        {
            filePath = filePath + ".bytes";
        }
        //到处找Resources目录,找不到就在 unity 工程目录全部找一遍
        else if(!GetResourcesPath(file, out filePath) 
            && !GetLuaPathInCurrentFile(file, out filePath)
            )
        {
            Debug.LogError("this is chunk file");
            return;
        }

        OpenFileAtLineExternal(filePath, line);
    }

    //这尼玛 已经不讲道理了啊
    public static bool GetLuaPathInCurrentFile(string fileName, out string path)
    {
        bool result = false;
        path = "";

        string curPath = Directory.GetCurrentDirectory();
        string[] pathArray = Directory.GetFiles(
            curPath, "*.lua",
            SearchOption.AllDirectories);

        foreach (var item in pathArray)
        {
            if (Path.GetFileNameWithoutExtension(item) == fileName)
            {
                path = Path.Combine(item, fileName + ".lua");
                return true;
            }
        }

        pathArray = Directory.GetFiles(
            curPath, "*.txt",
            SearchOption.AllDirectories);

        foreach (var item in pathArray)
        {
            if (Path.GetFileNameWithoutExtension(item) == fileName)
            {
                path = Path.Combine(item, fileName + ".txt");
                return true;
            }
        }

        pathArray = Directory.GetFiles(
            curPath, "*.bytes",
            SearchOption.AllDirectories);

        foreach (var item in pathArray)
        {
            if (Path.GetFileNameWithoutExtension(item) == fileName)
            {
                path = Path.Combine(item, fileName + ".bytes");
                return true;
            }
        }

        return result;
    }

    public static bool GetResourcesPath(string fileName, out string path)
    {
        bool result = false;
        path = "";

        string curPath = Directory.GetCurrentDirectory();
        string[] pathArray = Directory.GetDirectories(
            curPath, "Resources",
            SearchOption.AllDirectories);

        foreach (var item in pathArray)
        {
            path = Path.Combine(item, fileName + ".txt");
            if (File.Exists(path))
            {
                return true;
            }
            path = Path.Combine(item, fileName + ".bytes");
            if (File.Exists(path))
            {
                return true;
            }
        }

        return result;
    }

    static void OpenFileAtLineExternal(string filePath, int line)
    {
        string editorPath = EditorUserSettings.GetConfigValue(EXTERNAL_EDITOR_PATH_KEY);

        // 没有path就弹出面板设置
        if (string.IsNullOrEmpty(editorPath) || !File.Exists(editorPath))
        {
            SetExternalEditorPath();
        }
        System.Diagnostics.Process proc = new System.Diagnostics.Process();
        proc.StartInfo.FileName = editorPath;
        string procArgument = "";

        if (editorPath.IndexOf("Code") != -1)
        {
            procArgument = string.Format("-g {0}:{1}", filePath, line);
        }
        else if (editorPath.IndexOf("idea") != -1 || editorPath.IndexOf("clion") != -1 || editorPath.IndexOf("rider") != -1)
        {
            procArgument = string.Format("--line {0} {1}", line, filePath);
        }
        else
        {
            procArgument = string.Format("{0}:{1}", filePath, line);
        }
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.CreateNoWindow = true;
        proc.StartInfo.Arguments = procArgument;
        proc.Start();

        if (editorPath.IndexOf("idea") != -1 || editorPath.IndexOf("clion") != -1 || editorPath.IndexOf("rider") != -1)
        {
#if UNITY_EDITOR_WIN
            IntPtr hwd = FindWindow("SunAwtFrame", null);
            if (hwd != IntPtr.Zero && IsWindowVisible(hwd))
            {
                ShowWindow(hwd, 3);
            }
#endif
        }
    }

    #region set path
    public static void ClearPath()
    {
        EditorUserSettings.SetConfigValue(EXTERNAL_EDITOR_PATH_KEY, "");
        EditorUserSettings.SetConfigValue(LUA_PROJECT_ROOT_FOLDER_PATH_KEY, "");
        Debug.Log("Clear Suceess");
    }

    public static void SetExternalEditorPath()
    {
        string path = EditorUserSettings.GetConfigValue(EXTERNAL_EDITOR_PATH_KEY);
        path = EditorUtility.OpenFilePanel("Select Lua IDE", path, "");

        if (path != "")
        {
            EditorUserSettings.SetConfigValue(EXTERNAL_EDITOR_PATH_KEY, path);
            Debug.Log("Set Lua IDE Path: " + path);
        }
    }

    public static void SetExternalProjectRootPath()
    {
        string path = EditorUserSettings.GetConfigValue(LUA_PROJECT_ROOT_FOLDER_PATH_KEY);
        path = EditorUtility.OpenFolderPanel("Select Lua Project Root Path", path, "lua");

        if (path != "")
        {
            EditorUserSettings.SetConfigValue(LUA_PROJECT_ROOT_FOLDER_PATH_KEY, path);
            Debug.Log("Set Lua Project Root Path: " + path);
        }
    }
    #endregion

}

