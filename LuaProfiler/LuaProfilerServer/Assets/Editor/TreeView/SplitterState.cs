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
* Filename: SplitterState
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

using System;
using System.Reflection;
using UnityEditor;

namespace MikuLuaProfiler
{
    public class SplitterState
    {
        public int[] RelativeSizes = null;

        public object InternalObject
        {
            get;
            private set;
        }

        public static Type SplitterStateType
        {
            get;
            private set;
        }

        static SplitterState()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(ActiveEditorTracker));
            SplitterState.SplitterStateType = assembly.GetType("UnityEditor.SplitterState");
        }

        public SplitterState(int[] relativeSizes, int[] minSizes, int[] maxSizes)
        {
            this.RelativeSizes = relativeSizes;
            this.InternalObject = Activator.CreateInstance(SplitterState.SplitterStateType, new object[]
                {
                relativeSizes,
                minSizes,
                maxSizes
                });
        }
    }
}
