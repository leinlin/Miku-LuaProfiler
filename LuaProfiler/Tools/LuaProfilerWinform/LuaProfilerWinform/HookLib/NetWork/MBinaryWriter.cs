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
* Filename: NetWorkClient
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

using System;
using System.IO;

namespace MikuLuaProfiler_Winform
{
    public class MBinaryWriter : BinaryWriter
    {
        public MBinaryWriter(Stream output) : base(output)
        {
            this._buffer = new byte[8];
        }

        public unsafe override void Write(float value)
        {
            fixed (byte* ptr = &this._buffer[0])
            {
                *(float*)ptr = value;
                this.OutStream.Write(this._buffer, 0, 4);
            }
        }

        public unsafe override void Write(short value)
        {
            fixed (byte* ptr = &this._buffer[0])
            {
                *(short*)ptr = value;
                this.OutStream.Write(this._buffer, 0, 2);
            }
        }

        public unsafe override void Write(ushort value)
        {
            fixed (byte* ptr = &this._buffer[0])
            {
                *(short*)ptr = (short)value;
                this.OutStream.Write(this._buffer, 0, 2);
            }
        }

        public unsafe override void Write(int value)
        {
            fixed (byte* ptr = &this._buffer[0])
            {
                *(int*)ptr = value;
                this.OutStream.Write(this._buffer, 0, 4);
            }
        }

        public unsafe override void Write(uint value)
        {
            fixed (byte* ptr = &this._buffer[0])
            {
                *(uint*)ptr = value;
                this.OutStream.Write(this._buffer, 0, 4);
            }
        }

        private byte[] _buffer;
    }
}
