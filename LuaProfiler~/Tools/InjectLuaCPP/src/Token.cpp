#include <Token.h>

JumpToken::JumpToken(int pos)
{
    m_pos = pos; 
}

int JumpToken::GetTokenType()
{
    return -1;
}

const char* JumpToken::ToString()
{
    return "";
}

LiteralToken::LiteralToken(int literal)
{
    _Literal = literal;
}

int LiteralToken::GetTokenType()
{
    return _Literal;
}

const char* LiteralToken::ToString()
{
    char* result = (char*)malloc(sizeof(char) * 32);
    sprintf(result, "LiteralToken: %c", _Literal);

    return result;
}

TypedToken::TypedToken(TK type)
{
    _Type = type;
}

int TypedToken::GetTokenType()
{
    return (int)_Type;
}

const char* TypedToken::ToString()
{
    char* result = (char*)malloc(sizeof(char) * 32);
    sprintf(result, "TypedToken: %d", _Type);

    return result;
}

StringToken::StringToken(const char* seminfo) : TypedToken(STRING)
{
    m_semInfo = seminfo;
}

const char* StringToken::ToString()
{
    char* result = (char*)malloc(sizeof(char) * (32 + strlen(m_semInfo)));
    sprintf(result, "StringToken: %s", m_semInfo);

    return result;
}

NameToken::NameToken(const char* seminfo) : TypedToken(NAME)
{
    m_semInfo = seminfo;
}

const char* NameToken::ToString()
{
    char* result = (char*)malloc(sizeof(char) * (32 + strlen(m_semInfo)));
    sprintf(result, "NameToken: %s", m_semInfo);

    return result;
}

NumberToken::NumberToken(double seminfo) : TypedToken(NAME)
{
    m_semInfo = seminfo;
}

const char* NameToken::ToString()
{
    char* result = (char*)malloc(sizeof(char) * (32 + strlen(m_semInfo)));
    sprintf(result, "NameToken: %s", m_semInfo);

    return result;
}
