#pragma onece

#include <iostream>
#include <Enums.h>

class Token
{
public:
    virtual int GetTokenType() = 0;
    virtual const char* ToString() = 0;
};

class JumpToken : Token
{
private:
    int m_pos;
public:
    JumpToken(int pos);
public:
    inline int GetPos() { return m_pos; }
    int GetTokenType();
    const char* ToString();
};

class LiteralToken : Token
{
private:
    int _Literal;
public:
    LiteralToken(int literal);
public:
    int GetTokenType();
    const char* ToString();
};

class TypedToken : Token
{
private:
    TK _Type;
public:
     TypedToken(TK type);
public:
    int GetTokenType();
    const char* ToString();
};

class StringToken : TypedToken
{
private:
    const char* m_semInfo;
public:
    StringToken(const char* seminfo);
    const char* ToString(); 
};

class NameToken : TypedToken
{
private:
    const char* m_semInfo;
public:
    NameToken(const char* seminfo);
    const char* ToString(); 
};

class NumberToken : TypedToken
{
private:
    double m_semInfo;
public:
    NumberToken(double seminfo);
    const char* ToString(); 
};