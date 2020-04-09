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
* Filename: HistoryCurve
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

#if UNITY_5_6_OR_NEWER && UNITY_EDITOR_WIN
using System.Collections.Generic;
using UnityEngine;

namespace MikuLuaProfiler
{
    public class HistoryCurve
    {
        #region field
        private int addLuaCount = 0;
        private int addMonoCount = 0;
        private int addFpsCount = 0;
        private int addPssCount = 0;
        private int addPowerCount = 0;
        public const int splitCount = 60;
        public const int RECORD_FRAME_COUNT = 100;

        private readonly List<int> m_luaMemoryHistory;
        private readonly List<int> m_monoMemoryHistory;
        private readonly List<float> m_fpsHistory;
        private readonly List<uint> m_pssHistory;
        private readonly List<float> m_powerHistory;

        private LuaDeepProfilerSetting setting = LuaDeepProfilerSetting.Instance;
        #endregion

        public HistoryCurve(int count)
        {
            m_luaMemoryHistory = new List<int>(count);
            m_monoMemoryHistory = new List<int>(count);
            m_fpsHistory = new List<float>(count);
            m_pssHistory = new List<uint>(count);
            m_powerHistory = new List<float>(count);
        }

        #region lua
        private float m_minLuaValue = int.MaxValue;
        public float minLuaValue
        {
            get
            {
                if (m_minLuaValue < 0)
                {
                    m_minLuaValue = FindMinLuaValue();
                }

                return m_minLuaValue;
            }
        }
        private float m_maxLuaValue = 0;
        private float FindMinLuaValue()
        {
            int result = int.MaxValue;
            int imax = m_luaMemoryHistory.Count - 1;
            int imin = Mathf.Max(imax - RECORD_FRAME_COUNT, 0);
            for (int i = imax; i >= imin; --i)
            {
                if (result > m_luaMemoryHistory[i])
                {
                    result = m_luaMemoryHistory[i];
                }
            }
            return (float)result;
        }

        public float maxLuaValue
        {
            get
            {
                if (m_maxLuaValue < 0)
                {
                    m_maxLuaValue = FindMaxLuaValue();
                }
                return m_maxLuaValue;
            }
        }

        private float FindMaxLuaValue()
        {
            int result = 0;
            int imax = m_luaMemoryHistory.Count - 1;
            int imin = Mathf.Max(imax - RECORD_FRAME_COUNT, 0);
            for (int i = imax; i >= imin; --i)
            {
                if (result < m_luaMemoryHistory[i])
                {
                    result = m_luaMemoryHistory[i];
                }
            }
            return (float)result;
        }

        public bool TryGetLuaMemory(int index, out float result)
        {
            if (index < 0 || index >= m_luaMemoryHistory.Count)
            {
                result = 0;
                return false;
            }

            if (setting.isRecord && !setting.isStartRecord)
            {
                result = m_luaMemoryHistory[index];
                return true;

            }
            else
            {
                int firstIndex = Mathf.Max(0, m_luaMemoryHistory.Count - RECORD_FRAME_COUNT);
                result = m_luaMemoryHistory[firstIndex + index];
                return true;
            }
        }

        public int GetLuaRecordLength()
        {
            if (setting.isRecord && !setting.isStartRecord)
            {
                return m_luaMemoryHistory.Count;
            }
            else
            {
                return Mathf.Min(m_luaMemoryHistory.Count, RECORD_FRAME_COUNT);
            }
        }

        public int GetLuaRecordCount(out float split)
        {
            split = 1;
            if (setting.isRecord && !setting.isStartRecord)
            {
                int count = m_luaMemoryHistory.Count;
                split = (float)count / 1000;
                return (int)((float)count / split);
            }
            else
            {
                return Mathf.Min(m_luaMemoryHistory.Count, RECORD_FRAME_COUNT);
            }
        }

        public void SlotLuaMemory(int value)
        {
            addLuaCount++;
            if (value < m_minLuaValue || m_minLuaValue < 0)
            {
                m_minLuaValue = value;
            }
            if (value > m_maxLuaValue || m_maxLuaValue < 0)
            {
                m_maxLuaValue = value;
            }

            if (setting.isRecord && !setting.isStartRecord)
            {
                m_luaMemoryHistory.Add(value);
            }
            else if (addLuaCount % splitCount == 0)
            {
                m_luaMemoryHistory.Add(value);
                if (m_luaMemoryHistory.Count >= 2 * RECORD_FRAME_COUNT)
                {
                    m_luaMemoryHistory.RemoveRange(0, RECORD_FRAME_COUNT - 1);
                }
            }
        }

        public bool IsLuaEmpty()
        {
            return m_luaMemoryHistory.Count <= 0;
        }
        #endregion

        #region mono
        private float m_minMonoValue = int.MaxValue;
        public float minMonoValue
        {
            get
            {
                if (m_minMonoValue < 0)
                {
                    m_minMonoValue = FindMinMonoValue();
                }

                return m_minMonoValue;
            }
        }
        private float m_maxMonoValue = 0;
        private float FindMinMonoValue()
        {
            int result = int.MaxValue;
            int imax = m_monoMemoryHistory.Count - 1;
            int imin = Mathf.Max(imax - RECORD_FRAME_COUNT, 0);
            for (int i = imax; i >= imin; --i)
            {
                if (result > m_monoMemoryHistory[i])
                {
                    result = m_monoMemoryHistory[i];
                }
            }
            return (float)result;
        }
        public float maxMonoValue
        {
            get
            {
                if (m_maxMonoValue < 0)
                {
                    m_maxMonoValue = FindMaxMonoValue();
                }
                return m_maxMonoValue;
            }
        }
        private float FindMaxMonoValue()
        {
            int result = 0;
            int imax = m_monoMemoryHistory.Count - 1;
            int imin = Mathf.Max(imax - RECORD_FRAME_COUNT, 0);
            for (int i = imax; i >= imin; --i)
            {
                if (result < m_monoMemoryHistory[i])
                {
                    result = m_monoMemoryHistory[i];
                }
            }
            return (float)result;
        }
        public bool TryGetMonoMemory(int index, out float result)
        {
            if (index < 0 || index >= m_monoMemoryHistory.Count)
            {
                result = 0;
                return false;
            }

            if (setting.isRecord && !setting.isStartRecord)
            {
                result = m_monoMemoryHistory[index];
                return true;

            }
            else
            {
                int firstIndex = Mathf.Max(0, m_monoMemoryHistory.Count - RECORD_FRAME_COUNT);
                result = m_monoMemoryHistory[firstIndex + index];
                return true;
            }
        }
        public int GetMonoRecordLength()
        {
            if (setting.isRecord && !setting.isStartRecord)
            {
                return m_monoMemoryHistory.Count;
            }
            else
            {
                return Mathf.Min(m_monoMemoryHistory.Count, RECORD_FRAME_COUNT);
            }
        }

        public int GetMonoRecordCount(out float split)
        {
            split = 1;
            if (setting.isRecord && !setting.isStartRecord)
            {
                int count = m_monoMemoryHistory.Count;
                split = (float)count / 1000;
                return (int)((float)count / split);
            }
            else
            {
                return Mathf.Min(m_monoMemoryHistory.Count, RECORD_FRAME_COUNT);
            }
        }

        public void SlotMonoMemory(int value)
        {
            addMonoCount++;
            if (value < m_minMonoValue || m_minMonoValue < 0)
            {
                m_minMonoValue = value;
            }
            if (value > m_maxMonoValue || m_maxMonoValue < 0)
            {
                m_maxMonoValue = value;
            }

            if (setting.isRecord && !setting.isStartRecord)
            {
                m_monoMemoryHistory.Add(value);
            }
            else if (addMonoCount % splitCount == 0)
            {
                m_monoMemoryHistory.Add(value);
                if (m_monoMemoryHistory.Count >= 2 * RECORD_FRAME_COUNT)
                {
                    m_monoMemoryHistory.RemoveRange(0, RECORD_FRAME_COUNT - 1);
                }
            }
        }

        public bool IsMonoEmpty()
        {
            return m_monoMemoryHistory.Count <= 0;
        }
        #endregion

        #region fps
        private float m_minFpsValue = 0;
        public float minFpsValue
        {
            get
            {
                if (m_minFpsValue < 0)
                {
                    m_minFpsValue = FindMinFpsValue();
                }

                return m_minFpsValue;
            }
        }
        private float m_maxFpsValue = 90;
        private float FindMinFpsValue()
        {
            float result = float.MaxValue;
            int imax = m_fpsHistory.Count - 1;
            int imin = Mathf.Max(imax - RECORD_FRAME_COUNT, 0);
            for (int i = imax; i >= imin; --i)
            {
                if (result > m_fpsHistory[i])
                {
                    result = m_fpsHistory[i];
                }
            }
            return result;
        }


        public float maxFpsValue
        {
            get
            {
                if (m_maxFpsValue < 0)
                {
                    m_maxFpsValue = FindMaxFpsValue();
                }
                return m_maxFpsValue;
            }
        }

        private float FindMaxFpsValue()
        {
            float result = 0;
            int imax = m_fpsHistory.Count - 1;
            int imin = Mathf.Max(imax - RECORD_FRAME_COUNT, 0);
            for (int i = imax; i >= imin; --i)
            {
                if (result < m_fpsHistory[i])
                {
                    result = m_fpsHistory[i];
                }
            }
            return result;
        }

        public bool TryGetFpsMemory(int index, out float result)
        {
            if (index < 0 || index >= m_fpsHistory.Count)
            {
                result = 0;
                return false;
            }

            if (setting.isRecord && !setting.isStartRecord)
            {
                result = m_fpsHistory[index];
                return true;

            }
            else
            {
                int firstIndex = Mathf.Max(0, m_fpsHistory.Count - RECORD_FRAME_COUNT);
                result = m_fpsHistory[firstIndex + index];
                return true;
            }
        }

        public int GetFpsRecordLength()
        {
            if (setting.isRecord && !setting.isStartRecord)
            {
                return m_fpsHistory.Count;
            }
            else
            {
                return Mathf.Min(m_fpsHistory.Count, RECORD_FRAME_COUNT);
            }
        }

        public int GetFpsRecordCount(out float split)
        {
            split = 1;
            if (setting.isRecord && !setting.isStartRecord)
            {
                int count = m_fpsHistory.Count;
                split = (float)count / 1000;
                return (int)((float)count / split);
            }
            else
            {
                return Mathf.Min(m_fpsHistory.Count, RECORD_FRAME_COUNT);
            }
        }

        public void SlotFpsMemory(float value)
        {
            addFpsCount++;
            value = Mathf.Min(90, value);
            if (setting.isRecord && !setting.isStartRecord)
            {
                m_fpsHistory.Add(value);
            }
            else if (addFpsCount % splitCount == 0)
            {
                m_fpsHistory.Add(value);
                if (m_fpsHistory.Count >= 2 * RECORD_FRAME_COUNT)
                {
                    m_fpsHistory.RemoveRange(0, RECORD_FRAME_COUNT - 1);
                }
            }
        }

        public bool IsFpsEmpty()
        {
            return m_fpsHistory.Count <= 0;
        }
        #endregion

        #region pss
        private float m_minPssValue = uint.MaxValue;
        public float minPssValue
        {
            get
            {
                if (m_minPssValue < 0)
                {
                    m_minPssValue = FindMinPssValue();
                }

                return m_minPssValue;
            }
        }
        private float m_maxPssValue = 0;
        private float FindMinPssValue()
        {
            uint result = uint.MaxValue;
            int imax = m_pssHistory.Count - 1;
            int imin = Mathf.Max(imax - RECORD_FRAME_COUNT, 0);
            for (int i = imax; i >= imin; --i)
            {
                if (result > m_pssHistory[i])
                {
                    result = m_pssHistory[i];
                }
            }
            return (float)result;
        }


        public float maxPssValue
        {
            get
            {
                if (m_maxPssValue < 0)
                {
                    m_maxPssValue = FindMaxPssValue();
                }
                return m_maxPssValue;
            }
        }

        private float FindMaxPssValue()
        {
            uint result = 0;
            int imax = m_pssHistory.Count - 1;
            int imin = Mathf.Max(imax - RECORD_FRAME_COUNT, 0);
            for (int i = imax; i >= imin; --i)
            {
                if (result < m_pssHistory[i])
                {
                    result = m_pssHistory[i];
                }
            }
            return (float)result;
        }

        public bool TryGetPssMemory(int index, out float result)
        {
            if (index < 0 || index >= m_pssHistory.Count)
            {
                result = 0;
                return false;
            }

            if (setting.isRecord && !setting.isStartRecord)
            {
                result = m_pssHistory[index];
                return true;

            }
            else
            {
                int firstIndex = Mathf.Max(0, m_pssHistory.Count - RECORD_FRAME_COUNT);
                result = m_pssHistory[firstIndex + index];
                return true;
            }
        }

        public int GetPssRecordLength()
        {
            if (setting.isRecord && !setting.isStartRecord)
            {
                return m_pssHistory.Count;
            }
            else
            {
                return Mathf.Min(m_pssHistory.Count, RECORD_FRAME_COUNT);
            }
        }

        public int GetPssRecordCount(out float split)
        {
            split = 1;
            if (setting.isRecord && !setting.isStartRecord)
            {
                int count = m_pssHistory.Count;
                split = (float)count / 1000;
                return (int)((float)count / split);
            }
            else
            {
                return Mathf.Min(m_pssHistory.Count, RECORD_FRAME_COUNT);
            }
        }

        public void SlotPssMemory(uint value)
        {
            addPssCount++;
            if (value < m_minPssValue || m_minPssValue < 0)
            {
                m_minPssValue = value;
            }
            if (value > m_maxPssValue || m_maxPssValue < 0)
            {
                m_maxPssValue = value;
            }

            if (setting.isRecord && !setting.isStartRecord)
            {
                m_pssHistory.Add(value);
            }
            else if (addPssCount % splitCount == 0)
            {
                m_pssHistory.Add(value);
                if (m_pssHistory.Count >= 2 * RECORD_FRAME_COUNT)
                {
                    m_pssHistory.RemoveRange(0, RECORD_FRAME_COUNT - 1);
                }
            }
        }

        public bool IsPssEmpty()
        {
            return m_pssHistory.Count <= 0;
        }
        #endregion

        #region power
        private float m_minPowerValue = 0;
        public float minPowerValue
        {
            get
            {
                if (m_minPowerValue < 0)
                {
                    m_minPowerValue = FindMinPowerValue();
                }

                return m_minPowerValue;
            }
        }
        private float m_maxPowerValue = 100;
        private float FindMinPowerValue()
        {
            float result = float.MaxValue;
            int imax = m_powerHistory.Count - 1;
            int imin = Mathf.Max(imax - RECORD_FRAME_COUNT, 0);
            for (int i = imax; i >= imin; --i)
            {
                if (result > m_powerHistory[i])
                {
                    result = m_powerHistory[i];
                }
            }
            return result;
        }


        public float maxPowerValue
        {
            get
            {
                if (m_maxPowerValue < 0)
                {
                    m_maxPowerValue = FindMaxPowerValue();
                }
                return m_maxPowerValue;
            }
        }

        private float FindMaxPowerValue()
        {
            float result = 0;
            int imax = m_powerHistory.Count - 1;
            int imin = Mathf.Max(imax - RECORD_FRAME_COUNT, 0);
            for (int i = imax; i >= imin; --i)
            {
                if (result < m_powerHistory[i])
                {
                    result = m_powerHistory[i];
                }
            }
            return result;
        }

        public bool TryGetPowerMemory(int index, out float result)
        {
            if (index < 0 || index >= m_powerHistory.Count)
            {
                result = 0;
                return false;
            }

            if (setting.isRecord && !setting.isStartRecord)
            {
                result = m_powerHistory[index];
                return true;

            }
            else
            {
                int firstIndex = Mathf.Max(0, m_powerHistory.Count - RECORD_FRAME_COUNT);
                result = m_powerHistory[firstIndex + index];
                return true;
            }
        }

        public int GetPowerRecordLength()
        {
            if (setting.isRecord && !setting.isStartRecord)
            {
                return m_powerHistory.Count;
            }
            else
            {
                return Mathf.Min(m_powerHistory.Count, RECORD_FRAME_COUNT);
            }
        }

        public int GetPowerRecordCount(out float split)
        {
            split = 1;
            if (setting.isRecord && !setting.isStartRecord)
            {
                int count = m_powerHistory.Count;
                split = (float)count / 1000;
                return (int)((float)count / split);
            }
            else
            {
                return Mathf.Min(m_powerHistory.Count, RECORD_FRAME_COUNT);
            }
        }

        public void SlotPowerMemory(float value)
        {
            addPowerCount++;
            value = Mathf.Min(100, value);
            if (setting.isRecord && !setting.isStartRecord)
            {
                m_powerHistory.Add(value);
            }
            else if (addPowerCount % splitCount == 0)
            {
                m_powerHistory.Add(value);
                if (m_powerHistory.Count >= 2 * RECORD_FRAME_COUNT)
                {
                    m_powerHistory.RemoveRange(0, RECORD_FRAME_COUNT - 1);
                }
            }
        }

        public bool IsPowerEmpty()
        {
            return m_powerHistory.Count <= 0;
        }
        #endregion

        public void Clear()
        {
            m_monoMemoryHistory.Clear();
            m_luaMemoryHistory.Clear();
            m_fpsHistory.Clear();
            m_pssHistory.Clear();
            m_powerHistory.Clear();

            addLuaCount = 0;
            addMonoCount = 0;
            addFpsCount = 0;
            addPssCount = 0;
            addPowerCount = 0;

            m_maxLuaValue = -1;
            m_minLuaValue = -1;

            m_maxMonoValue = -1;
            m_minMonoValue = -1;

            m_minFpsValue = 0;
            m_maxFpsValue = 90;

            m_minPssValue = -1;
            m_maxPssValue = -1;

            m_minPowerValue = 0;
            m_maxPowerValue = 100;
        }

    }
}
#endif