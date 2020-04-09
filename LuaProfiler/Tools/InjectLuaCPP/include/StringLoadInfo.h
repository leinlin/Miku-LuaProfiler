#pragma onece

#include <iostream>
#include <Enums.h>

class StringLoadInfo
{
private:
    std::string m_str;
    int m_pos;

public:
    StringLoadInfo(const char *s);
    virtual ~StringLoadInfo();

public:
    int ReadByte();
    int GetPosChar();
    int PeekByte();
    void Replace(int start, int len, const char *value);
    const char *ReadString(int startPos, int len);
    void InsertString(int startPos, const char *value);

    inline int GetLength() { return m_str.length(); }
    inline int GetPos() { return m_pos; }
    inline void SetPos(int value) { m_pos = value; }
    inline const char* GetCode() { return m_str.c_str(); }
};