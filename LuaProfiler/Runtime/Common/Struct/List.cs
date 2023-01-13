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
* Filename: List.cs
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

#if UNITY_EDITOR || USE_LUA_PROFILER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuLuaProfiler
{
    public class MList<T>
    {
        private const int DefaultCapacity = 4;
        private T[] _items;
        private int _size;
        private int _version;

        public MList(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity");
            }
            this._items = new T[capacity];
        }

        public int Capacity
        {
            get
            {
                return this._items.Length;
            }
            set
            {
                if (value < this._size)
                {
                    throw new ArgumentOutOfRangeException();
                }
                Array.Resize<T>(ref this._items, value);
            }
        }

        public int Count
        {
            get
            {
                return this._size;
            }
        }

        public T this[int index]
        {
            get
            {
                return this._items[index];
            }
            set
            {
                this._items[index] = value;
            }
        }

        public void Clear()
        {
            this._size = 0;
            this._version++;
        }

        public void Add(T item)
        {
            if (this._size == this._items.Length)
            {
                this.GrowIfNeeded(1);
            }
            this._items[this._size++] = item;
            this._version++;
        }

        private void GrowIfNeeded(int newCount)
        {
            int num = this._size + newCount;
            if (num > this._items.Length)
            {
                this.Capacity = Math.Max(Math.Max(this.Capacity * 2, 4), num);
            }
        }


        public void RemoveAt(int index)
        {
            if (index < 0 || index >= this._size)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            this.Shift(index, -1);
            Array.Clear(this._items, this._size, 1);
            this._version++;
        }
        private void Shift(int start, int delta)
        {
            if (delta < 0)
            {
                start -= delta;
            }
            if (start < this._size)
            {
                Array.Copy(this._items, start, this._items, start + delta, this._size - start);
            }
            this._size += delta;
            if (delta < 0)
            {
                Array.Clear(this._items, this._size, -delta);
            }
        }
    }
}
#endif