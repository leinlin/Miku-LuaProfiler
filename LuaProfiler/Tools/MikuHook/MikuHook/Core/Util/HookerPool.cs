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
* Filename: HookerPool
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
using System;
using System.Collections.Generic;

namespace MikuHook
{
    /// <summary>
    /// Hooker 池，防止重复 Hook
    /// </summary>
    public static class HookerPool
    {
        private static Dictionary<IntPtr, HookerBase> _hookers = new Dictionary<IntPtr, HookerBase>();

        public static void AddHooker(IntPtr targetAddr, HookerBase hooker)
        {
            HookerBase preHooker;
            if (_hookers.TryGetValue(targetAddr, out preHooker))
            {
                preHooker.Uninstall();
                _hookers[targetAddr] = hooker;
            }
            else
            {
                _hookers.Add(targetAddr, hooker);
            }
        }

        public static void RemoveHooker(IntPtr targetAddr)
        {
            _hookers.Remove(targetAddr);
        }
    }
}
