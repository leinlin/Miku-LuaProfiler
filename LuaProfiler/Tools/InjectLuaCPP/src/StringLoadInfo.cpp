#include <StringLoadInfo.h>

StringLoadInfo::StringLoadInfo(const char *s)
{
    m_str = std::string(s);
    m_str.append(" ");
    m_pos = 0;
}

StringLoadInfo::~StringLoadInfo()
{
}

int StringLoadInfo::ReadByte()
{
    if (m_pos >= m_str.length())
        return -1;
    else
        return m_str[m_pos++];
}

int StringLoadInfo::GetPosChar()
{
    if (m_pos >= m_str.length())
        return (int)EOS;
    else
        return m_str[m_pos];
}

int StringLoadInfo::PeekByte()
{
    if (m_pos >= m_str.length())
        return -1;
    else
        return m_str[m_pos];
}

void StringLoadInfo::Replace(int start, int len, const char *value)
{
    m_str.erase(start, len);
    m_str.insert(start, value);

    if ((start + len) <= m_pos)
    {
        m_pos = m_pos - (len - strlen(value));
    }
}

const char* StringLoadInfo::ReadString(int startPos, int len)
{
    std::string chars = new char[len];
    for (int i = 0, imax = len; i < imax; i++)
    {
        chars[i] = m_str[i + startPos];
    }

    return chars.c_str();
}

void StringLoadInfo::InsertString(int startPos, const char *value)
{
    m_str.insert(startPos, value);
    m_pos += strlen(value);
}
