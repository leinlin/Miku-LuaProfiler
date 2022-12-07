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
* Filename: TPSplitterGUILayout
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

#if UNITY_5_6_OR_NEWER && UNITY_EDITOR_WIN
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MikuLuaProfiler
{
    internal class SplitterGUILayout
    {
        private static MethodInfo _beginVerticalSplitMethod;

        private static MethodInfo _beginHorizontalSplitMethod;

        private static MethodInfo _endVerticalSplitMethod;

        private static MethodInfo _endHorizontalSplitMethod;
        public static Type SplitterGUILayoutType
        {
            get;
            private set;
        }

        static SplitterGUILayout()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(ActiveEditorTracker));
            SplitterGUILayout.SplitterGUILayoutType = assembly.GetType("UnityEditor.SplitterGUILayout");
        }

        public static void BeginVerticalSplit(SplitterState state, params GUILayoutOption[] options)
        {
            if (SplitterGUILayout._beginVerticalSplitMethod == null)
            {
                SplitterGUILayout._beginVerticalSplitMethod = SplitterGUILayout.SplitterGUILayoutType.GetMethod("BeginVerticalSplit", new Type[]
                {
                    SplitterState.SplitterStateType,
                    typeof(GUILayoutOption[])
                });
            }
            SplitterGUILayout._beginVerticalSplitMethod.Invoke(null, new object[]
            {
                state.InternalObject,
                options
            });
        }

        public static void BeginHorizontalSplit(SplitterState state, params GUILayoutOption[] options)
        {
            if (SplitterGUILayout._beginHorizontalSplitMethod == null)
            {
                SplitterGUILayout._beginHorizontalSplitMethod = SplitterGUILayout.SplitterGUILayoutType.GetMethod("BeginHorizontalSplit", new Type[]
                {
                    SplitterState.SplitterStateType,
                    typeof(GUILayoutOption[])
                });
            }
            SplitterGUILayout._beginHorizontalSplitMethod.Invoke(null, new object[]
            {
                state.InternalObject,
                options
            });
        }

        public static void EndVerticalSplit()
        {
            if (SplitterGUILayout._endVerticalSplitMethod == null)
            {
                SplitterGUILayout._endVerticalSplitMethod = SplitterGUILayout.SplitterGUILayoutType.GetMethod("EndVerticalSplit");
            }
            SplitterGUILayout._endVerticalSplitMethod.Invoke(null, null);
        }

        public static void EndHorizontalSplit()
        {
            if (SplitterGUILayout._endHorizontalSplitMethod == null)
            {
                SplitterGUILayout._endHorizontalSplitMethod = SplitterGUILayout.SplitterGUILayoutType.GetMethod("EndHorizontalSplit");
            }
            SplitterGUILayout._endHorizontalSplitMethod.Invoke(null, null);
        }
    }
}
#endif