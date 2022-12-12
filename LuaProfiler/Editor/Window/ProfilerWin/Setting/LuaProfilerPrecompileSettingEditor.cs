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

#if UNITY_5_6_OR_NEWER && UNITY_EDITOR_WIN
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

            #region lua folders
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
                    AssetDatabase.SaveAssets();
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
                    AssetDatabase.SaveAssets();
                    Repaint();
                }
            }
            #endregion

            #region lua filter folders
            var filterDirList = settings.luaFilterDirList;
            EditorGUILayout.LabelField("Lua filter Folder");
            for (int i = filterDirList.Count - 1, imin = 0; i >= imin; i--)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField(filterDirList[i]);
                if (GUILayout.Button("remove", GUILayout.ExpandWidth(false)))
                {
                    filterDirList.RemoveAt(i);
                    EditorUtility.SetDirty(settings);
                    AssetDatabase.SaveAssets();
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Lua Filter Folder", GUILayout.Height(50)))
            {
                GUI.FocusControl(null);
                string defaultPath = null;
                if (dirList.Count > 0)
                {
                    defaultPath = dirList[0].scriptFolder;
                }
                string newPath = EditorUtility.OpenFolderPanel("Add Lua Folder", defaultPath, null);
                if (!string.IsNullOrEmpty(newPath))
                {
                    filterDirList.Add(LuaProfilerPrecompileSetting.MakePathRelative(newPath));
                    EditorUtility.SetDirty(settings);
                    AssetDatabase.SaveAssets();
                    Repaint();
                }
            }
            #endregion

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
                    AssetDatabase.SaveAssets();
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
                AssetDatabase.SaveAssets();
            }

            if (GUILayout.Button("Clear", GUILayout.Height(50)))
            {
                foreach (var item in settings.luaDirList)
                {
                    item.ClearDict();
                }

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
                AssetDatabase.SaveAssets();

            }
            EditorGUILayout.EndVertical();

            EditorGUI.EndChangeCheck();
        }
    }
}
#endif
