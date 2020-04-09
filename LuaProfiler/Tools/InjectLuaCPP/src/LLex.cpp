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