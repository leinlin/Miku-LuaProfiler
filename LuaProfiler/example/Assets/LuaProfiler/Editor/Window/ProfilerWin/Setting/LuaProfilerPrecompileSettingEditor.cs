/*
               #########                       
              ############                     
              #############                    
             ##  ###########                   
            ###  ###### #####                  
            ### #######   ####                 
           ###  ########## ####                
          ####  ########### ####               
         ####   ###########  #####             
        #####   ### ########   #####           
       #####   ###   ########   ######         
      ######   ###  ###########   ######       
     ######   #### ##############  ######      
    #######  #####################  ######     
    #######  ######################  ######    
   #######  ###### #################  ######   
   #######  ###### ###### #########   ######   
   #######    ##  ######   ######     ######   
   #######        ######    #####     #####    
    ######        #####     #####     ####     
     #####        ####      #####     ###      
      #####       ###        ###      #        
        ###       ###        ###               
         ##       ###        ###               
__________#_______####_______####______________
                我们的未来没有BUG                
* ==============================================================================
* Filename: LuaProfilerPrecompileSetting.cs
* Created:  2020/4/10 11:01:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

#if UNITY_EDITOR_WIN || USE_LUA_PROFILER
namespace MikuLuaProfiler
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(LuaProfilerPrecompileSetting))]
    public class LuaProfilerPrecompileSettingEditor : Editor
    {
        LuaProfilerPrecompileSetting settings;

        public override void OnInspectorGUI()
        {
            settings = target as LuaProfilerPrecompileSetting;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical();
            var dirList = settings.luaDirList;
            EditorGUILayout.LabelField("Lua Folder");
            for (int i = dirList.Count - 1, imin = 0; i >= imin; i--)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField(dirList[i].scriptFolder);
                if (GUILayout.Button("remove", GUILayout.ExpandWidth(false)))
                {
                    dirList.RemoveAt(i);
                    EditorUtility.SetDirty(settings);
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Lua Folder", GUILayout.Height(50)))
            {
                GUI.FocusControl(null);
                string newPath = EditorUtility.OpenFolderPanel("Add Lua Folder", null, null);
                if (!string.IsNullOrEmpty(newPath))
                {
                    var item = new PreCompileFolderInfo();
                    item.scriptFolder = LuaProfilerPrecompileSetting.MakePathRelative(newPath);
                    dirList.Add(item);
                    EditorUtility.SetDirty(settings);
                    Repaint();
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Lua suffix:", GUILayout.Width(80));
            string oldStr = settings.luaSuffix;
            settings.luaSuffix = EditorGUILayout.TextField(settings.luaSuffix);
            if (oldStr != settings.luaSuffix)
            {
                EditorUtility.SetDirty(settings);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Out Path");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField(settings.outFolder);
            if (GUILayout.Button("browse", GUILayout.ExpandWidth(false)))
            {
                GUI.FocusControl(null);
                string newPath = EditorUtility.OpenFolderPanel("Out Folder", null, null);
                if (!string.IsNullOrEmpty(newPath))
                {
                    settings.outFolder = LuaProfilerPrecompileSetting.MakePathRelative(newPath);
                    EditorUtility.SetDirty(settings);
                    Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Pre Compile", GUILayout.Height(50)))
            {
                if (LuaProfilerPrecompileSetting.CompileLuaScript(true))
                {
                    Debug.Log("success");
                    foreach (var item in settings.luaDirList)
                    {
                        System.Diagnostics.Process.Start("explorer.exe", Environment.CurrentDirectory + "\\" + settings.outFolder);
                    }
                }
                else
                {
                    Debug.LogError("fail");
                }
                EditorUtility.SetDirty(settings);
            }

            if (GUILayout.Button("Clear", GUILayout.Height(50)))
            {
                var outFiles = Directory.GetFiles(settings.outFolder, settings.luaSuffix, SearchOption.AllDirectories);
                for (int i = 0, imax = outFiles.Length; i < imax; i++)
                {
                    File.Delete(outFiles[i]);
                }
                var outFolder = Directory.GetDirectories(settings.outFolder);
                for (int i = 0, imax = outFolder.Length; i < imax; i++)
                {
                    Directory.Delete(outFolder[i], true);
                }
                EditorUtility.SetDirty(settings);

            }
            EditorGUILayout.EndVertical();

            EditorGUI.EndChangeCheck();
        }
    }
}
#endif
