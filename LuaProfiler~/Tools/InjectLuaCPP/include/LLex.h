#pragma onece

#include <Token.h>
#include <StringLoadInfo.h>
#include <unordered_map>

class LLex
{
private:
    static std::unordered_map<std::string, int> m_reservedWordDict;
    static const char EOZ = 255;
public:
    static void InitLLex()
    {
        m_reservedWordDict["and"] = AND;
        m_reservedWordDict["break"] = BREAK;
        m_reservedWordDict["continue"] = CONTINUE;
        m_reservedWordDict["do"] = DO;
        m_reservedWordDict["else"] = ELSE;
        m_reservedWordDict["elseif"] = ELSEIF;
        m_reservedWordDict["end"] = END;
        m_reservedWordDict["false"] = FALSE;
        m_reservedWordDict["for"] = FOR;
        m_reservedWordDict["function"] = FUNCTION;
        m_reservedWordDict["goto"] = GOTO;
        m_reservedWordDict["if"] = IF;
        m_reservedWordDict["in"] = IN;
        m_reservedWordDict["local"] = LOCAL;
        m_reservedWordDict["nil"] = NIL;
        m_reservedWordDict["not"] = NOT;
        m_reservedWordDict["or"] = OR;
        m_reservedWordDict["repeat"] = REPEAT;
        m_reservedWordDict["return"] = RETURN;
        m_reservedWordDict["then"] = THEN;
        m_reservedWordDict["true"] = TRUE;
        m_reservedWordDict["until"] = UNTIL;
        m_reservedWordDict["while"] = WHILE;
    }
public:
    LLex(StringLoadInfo* loadinfo, const char* name);
    ~LLex();
private:
    Token* m_token;
    Token* m_lookAhead;
    int m_current;
    int m_lineNumber;
    int m_lastLine;
    StringLoadInfo* m_loadInfo;
    std::string m_saved;
    const char* m_source;
 public:
    void Next();
    Token* GetLookAhead();
private:
    void _Next();
    void _SaveAndNext();
    void _Save(char c);
    std::string _GetSavedString();
    void _ClearSaved();
    bool _CurrentIsNewLine();
    bool _CurrentIsDigit();
    bool _CurrentIsXDigit();
    bool _CurrentIsSpace();
    bool _CurrentIsAlpha();
    bool _IsReserved(std::string& identifier, TK& type);
    bool IsReservedWord(std::string& name);
    void _IncLineNumber();
    std::string&_ReadLongString(int sep, bool isComment);
    void _EscapeError(std::string& info, std::string& msg);
    uint8_t _ReadHexEscape();
    uint8_t _ReadDecEscape();
    std::string& _ReadString();
    double _ReadNumber();
    bool O_Str2Decimal(const char* s, double& result);
    void _Error(std::string& error);
    void _LexError(std::string& info, const char* msg, int tokenType);
    int _SkipSep();
    Token* _Lex();
public:
    inline Token* GetToken() {
        return m_token;
    }
    inline const char* GetCode() {
        return m_loadInfo->GetCode();
    }
    inline void InsertString(int startPos, const char* value) {
        return m_loadInfo->InsertString(startPos, value);
    }
    inline int GetLength() {
        return m_loadInfo->GetLength();
    }
    inline void Replace(int start, int end, const char* value) {
        m_loadInfo->Replace(start, end + 1 - start, value);
    }
    inline const char* ReadString(int start, int end) {
        int len = end - start + 1;
         return m_loadInfo->ReadString(start, len);
    }
};