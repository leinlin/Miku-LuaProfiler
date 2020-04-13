#include <LLex.h>

LLex::LLex(StringLoadInfo* loadinfo, const char* name) 
{
    m_loadInfo = loadinfo;
    m_lineNumber = 1;
    m_lastLine = 1;
    m_token = nullptr;
    m_lookAhead = nullptr;
    m_saved.empty();
    m_source = nullptr;

    _Next();
}

LLex::~LLex()
{
    if (m_loadInfo != nullptr) delete m_loadInfo;
}

void LLex::Next()
{
    m_lastLine = m_lineNumber;
    if (m_lookAhead != nullptr)
    {
        m_token = m_lookAhead;
        m_lookAhead = nullptr;
    }
    else
    {
        m_token = _Lex();
    }
}

Token* LLex::GetLookAhead()
{
    m_lookAhead = _Lex();
    return m_lookAhead;
}

void LLex::_Next()
{
    auto c = m_loadInfo->ReadByte();
    m_current = (c == -1) ? EOZ : c;
}

void LLex::_SaveAndNext()
{
    m_saved.append(1, (char)m_current);
    _Next();
}

void LLex::_Save(char c)
{
    m_saved.append(1, c);
}

std::string LLex::_GetSavedString()
{
    return std::string(m_saved);
}

void LLex::_ClearSaved()
{
    m_saved = "";
}

bool LLex::_CurrentIsNewLine()
{
    return m_current == '\n' || m_current == '\r';
}

bool LLex::_CurrentIsDigit()
{
    return m_current >= '0' && m_current <= '9';
}

bool LLex::_CurrentIsXDigit()
{
    return _CurrentIsDigit() ||
        ('A' <= m_current && m_current <= 'F') ||
        ('a' <= m_current && m_current <= 'f');    
}

bool LLex::_CurrentIsSpace()
{
    char c = (char)m_current;
    return c == ' ' || (c >= '\t' && c <= '\r') || c == '\u00a0' || c == '\u0085';
}

bool LLex::_CurrentIsAlpha()
{
    char c = (char)m_current;
    c |= ' ';
    return c >= 'a' && c <= 'z';
}

bool LLex::_IsReserved(std::string& identifier, TK& type)
{
    auto itr = m_reservedWordDict.find(identifier);
    if (itr != m_reservedWordDict.end()) 
    {
        type = (TK)(itr->second);
        return true;
    }
    else
    {
        return false;
    } 
}

bool LLex::IsReservedWord(std::string& identifier)
{
    auto itr = m_reservedWordDict.find(identifier);
    return itr != m_reservedWordDict.end();
}

void LLex::_IncLineNumber()
{
    auto old = m_current;
    _Next();
    if (_CurrentIsNewLine() && m_current != old)
        _Next();
    if (++m_lineNumber >= INT32_MAX)
        _Error("chunk has too many lines");
}

std::string& LLex::_ReadLongString(int sep, bool isComment)
{
    _SaveAndNext();

    if (_CurrentIsNewLine())
        _IncLineNumber();

    while (true)
    {
        switch (m_current)
        {
            case EOZ:
                _LexError(_GetSavedString(),
                    "unfinished long string/comment",
                    (int)EOS);
                break;

            case '[':
                {
                    if (_SkipSep() == sep)
                    {
                        _SaveAndNext();
                        if (sep == 0 && !isComment)
                        {
                            _LexError(_GetSavedString(),
                                "nesting of [[...]] is deprecated",
                                (int)EOS);
                        }
                    }
                    break;
                }

            case ']':
                {
                    if (_SkipSep() == sep)
                    {
                        _SaveAndNext();
                        goto endloop;
                    }
                    break;
                }

            case '\n':
            case '\r':
                {
                    _Save('\n');
                    _IncLineNumber();
                    break;
                }

            default:
                {
                    _SaveAndNext();
                    break;
                }
        }
    }
endloop:
    auto r = _GetSavedString();
    int len = r.length() - 2 * (2 + sep);
    if (len <= 0)
    {
        return std::string("");
    }
    else
    {
        return r.substr(2 + sep, len);
    }
}

void _EscapeError(std::string& info, const char* msg)
{
    _LexError(std::string("\\" + info), msg, (int)STRING);
}