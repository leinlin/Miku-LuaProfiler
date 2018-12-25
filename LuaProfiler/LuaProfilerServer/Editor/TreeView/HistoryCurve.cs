/*
* ==============================================================================
* Filename: HistoryCurve
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
using System.Collections.Generic;
using UnityEngine;

namespace MikuLuaProfiler
{
    public class HistoryCurve
    {
        #region field
        private int addLuaCount = 0;
        private int addMonoCount = 0;
        public const int splitCount = 60;
        public const int RECORD_FRAME_COUNT = 100;
        private readonly List<int> m_luaMemoryHistory;
        private readonly List<int> m_monoMemoryHistory;
        private LuaDeepProfilerSetting setting = LuaDeepProfilerSetting.Instance;
        #endregion

        public HistoryCurve(int count)
        {
            m_luaMemoryHistory = new List<int>(count);
            m_monoMemoryHistory = new List<int>(count);
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
            if (value < m_minLuaValue && value != 0)
            {
                m_minLuaValue = value;
            }
            if (value > m_maxLuaValue)
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
                    m_minLuaValue = -1;
                    m_maxLuaValue = -1;
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
            if (value < m_minMonoValue && value != 0)
            {
                m_minMonoValue = value;
            }
            if (value > m_maxMonoValue)
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
                    m_minMonoValue = -1;
                    m_maxMonoValue = -1;
                }
            }
        }

        public bool IsMonoEmpty()
        {
            return m_monoMemoryHistory.Count <= 0;
        }
        #endregion


        public void Clear()
        {
            m_monoMemoryHistory.Clear();
            m_luaMemoryHistory.Clear();
            addLuaCount = 0;
            addMonoCount = 0;

            m_maxLuaValue = 0;
            m_minLuaValue = 0;

            m_maxMonoValue = 0;
            m_minMonoValue = 0;
        }

    }
}

