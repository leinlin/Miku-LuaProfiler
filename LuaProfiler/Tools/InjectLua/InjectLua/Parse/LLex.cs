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
* Filename: LLex
* Created:  2018/7/2 11:36:16
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
#if UNITY_EDITOR || USE_LUA_PROFILER
using System;
using System.Collections.Generic;
using System.Text;
using NumberStyles = System.Globalization.NumberStyles;

namespace MikuLuaProfiler
{
    public enum TK
    {
        // reserved words
        AND = 257,
        BREAK,
        CONTINUE,
        DO,
        ELSE,
        ELSEIF,
        END,
        FALSE,
        FOR,
        FUNCTION,
        GOTO,
        IF,
        IN,
        LOCAL,
        NIL,
        NOT,
        OR,
        REPEAT,
        RETURN,
        THEN,
        TRUE,
        UNTIL,
        WHILE,
        // other terminal symbols
        CONCAT,
        DOTS,
        EQ,
        GE,
        LE,
        NE,
        DBCOLON,
        NUMBER,
        STRING,
        NAME,
        EOS,
    }

    public class StringLoadInfo
    {
        public StringLoadInfo(string s)
        {
            Str = new StringBuilder(s);
            Str.Append(' ');
            Pos = 0;
        }

        public int ReadByte()
        {
            if (Pos >= Str.Length)
                return -1;
            else
                return Str[Pos++];
        }

        public int PosChar
        {
            get
            {
                if (Pos >= Str.Length)
                    return (int)TK.EOS;
                else
                    return Str[Pos];
            }
        }

        public int Length
        {
            get
            {
                return Str.Length;
            }
        }

        public int PeekByte()
        {
            if (Pos >= Str.Length)
                return -1;
            else
                return Str[Pos];
        }

        public void Replace(int start, int len, string value)
        {
            Str = Str.Remove(start, len);
            Str = Str.Insert(start, value);
            if ((start + len) <= Pos)
            {
                Pos = Pos - (len - value.Length);
            }
        }
        public string ReadString(int startPos, int len)
        {
            char[] chars = new char[len];
            for (int i = 0, imax = len; i < imax; i++)
            {
                chars[i] = Str[i + startPos];
            }

            return new string(chars);
        }

        public void InsertString(int startPos, string value)
        {
            Str = Str.Insert(startPos, value);
            Pos += value.Length;
        }
        public string code
        {
            get
            {
                return Str.ToString();
            }
        }
        private StringBuilder Str;
        public int Pos;
    }

    public class BytesLoadInfo
    {
        public BytesLoadInfo(byte[] bytes)
        {
            Bytes = bytes;
            Pos = 0;
        }

        public int ReadByte()
        {
            if (Pos >= Bytes.Length)
                return -1;
            else
                return Bytes[Pos++];
        }

        public int PeekByte()
        {
            if (Pos >= Bytes.Length)
                return -1;
            else
                return Bytes[Pos];
        }

        private byte[] Bytes;
        private int Pos;
    }

    public abstract class Token
    {
        public abstract int TokenType { get; }

        public bool EqualsToToken(Token other)
        {
            return TokenType == other.TokenType;
        }

        public bool EqualsToToken(int other)
        {
            return TokenType == other;
        }

        public bool EqualsToToken(TK other)
        {
            return TokenType == (int)other;
        }
    }

    public class JumpToken : Token
    {
        public JumpToken(int pos)
        {
            Pos = pos;
        }
        public int Pos;
        public override int TokenType
        {
            get { return -1; }
        }
    }
    public class LiteralToken : Token
    {
        private int _Literal;

        public LiteralToken(int literal)
        {
            _Literal = literal;
        }

        public override int TokenType
        {
            get { return _Literal; }
        }

        public override string ToString()
        {
            return string.Format("LiteralToken: {0}", (char)_Literal);
        }
    }
    public class TypedToken : Token
    {
        private TK _Type;

        public TypedToken(TK type)
        {
            _Type = type;
        }

        public override int TokenType
        {
            get { return (int)_Type; }
        }

        public override string ToString()
        {
            return string.Format("TypedToken: {0}", _Type);
        }
    }

    public class StringToken : TypedToken
    {
        public string SemInfo;

        public StringToken(string seminfo) : base(TK.STRING)
        {
            SemInfo = seminfo;
        }

        public override string ToString()
        {
            return string.Format("StringToken: {0}", SemInfo);
        }
    }

    public class NameToken : TypedToken
    {
        public string SemInfo;

        public NameToken(string seminfo) : base(TK.NAME)
        {
            SemInfo = seminfo;
        }

        public override string ToString()
        {
            return string.Format("NameToken: {0}", SemInfo);
        }
    }
    public class NumberToken : TypedToken
    {
        public double SemInfo;

        public NumberToken(double seminfo) : base(TK.NUMBER)
        {
            SemInfo = seminfo;
        }

        public override string ToString()
        {
            return string.Format("NumberToken: {0}", SemInfo);
        }
    }

    public class LLex
    {
        public const char EOZ = Char.MaxValue;

        public int pos
        {
            get
            {
                return LoadInfo.Pos;
            }
        }
        private int Current;
        public int LineNumber;
        public int LastLine;
        private StringLoadInfo LoadInfo;
        public string Source;

        public Token Token;
        private Token LookAhead;

        private StringBuilder _Saved;
        private StringBuilder Saved
        {
            get
            {
                if (_Saved == null) { _Saved = new StringBuilder(); }
                return _Saved;
            }
        }

        private static Dictionary<string, TK> ReservedWordDict;
        static LLex()
        {
            ReservedWordDict = new Dictionary<string, TK>();
            ReservedWordDict.Add("and", TK.AND);
            ReservedWordDict.Add("break", TK.BREAK);
            ReservedWordDict.Add("continue", TK.CONTINUE);
            ReservedWordDict.Add("do", TK.DO);
            ReservedWordDict.Add("else", TK.ELSE);
            ReservedWordDict.Add("elseif", TK.ELSEIF);
            ReservedWordDict.Add("end", TK.END);
            ReservedWordDict.Add("false", TK.FALSE);
            ReservedWordDict.Add("for", TK.FOR);
            ReservedWordDict.Add("function", TK.FUNCTION);
            ReservedWordDict.Add("goto", TK.GOTO);
            ReservedWordDict.Add("if", TK.IF);
            ReservedWordDict.Add("in", TK.IN);
            ReservedWordDict.Add("local", TK.LOCAL);
            ReservedWordDict.Add("nil", TK.NIL);
            ReservedWordDict.Add("not", TK.NOT);
            ReservedWordDict.Add("or", TK.OR);
            ReservedWordDict.Add("repeat", TK.REPEAT);
            ReservedWordDict.Add("return", TK.RETURN);
            ReservedWordDict.Add("then", TK.THEN);
            ReservedWordDict.Add("true", TK.TRUE);
            ReservedWordDict.Add("until", TK.UNTIL);
            ReservedWordDict.Add("while", TK.WHILE);
        }

        public LLex(StringLoadInfo loadinfo, string name)
        {
            LoadInfo = loadinfo;
            LineNumber = 1;
            LastLine = 1;
            Token = null;
            LookAhead = null;
            _Saved = null;
            Source = name;

            _Next();
        }

        public string code
        {
            get
            {
                return LoadInfo.code;
            }
        }

        public void InsertString(int startPos, string value)
        {
            LoadInfo.InsertString(startPos, value);
        }

        public void Next()
        {
            LastLine = LineNumber;
            if (LookAhead != null)
            {
                Token = LookAhead;
                LookAhead = null;
            }
            else
            {
                Token = _Lex();
            }
        }

        public Token GetLookAhead()
        {
            Utl.Assert(LookAhead == null);
            LookAhead = _Lex();
            return LookAhead;
        }

        public int Length
        {
            get
            {
                return LoadInfo.Length;
            }
        }

        public void Replace(int start, int end, string value)
        {
            LoadInfo.Replace(start, end + 1 - start, value);
        }

        public string ReadString(int start, int end)
        {
            int len = end - start + 1;
            return LoadInfo.ReadString(start, len);
        }

        private void _Next()
        {
            var c = LoadInfo.ReadByte();
            Current = (c == -1) ? EOZ : c;
        }

        private void _SaveAndNext()
        {
            Saved.Append((char)Current);
            _Next();
        }

        private void _Save(char c)
        {
            Saved.Append(c);
        }

        private string _GetSavedString()
        {
            return Saved.ToString();
        }

        private void _ClearSaved()
        {
            _Saved = null;
        }

        private bool _CurrentIsNewLine()
        {
            return Current == '\n' || Current == '\r';
        }

        private bool _CurrentIsDigit()
        {
            return Char.IsDigit((char)Current);
        }

        private bool _CurrentIsXDigit()
        {
            return _CurrentIsDigit() ||
                ('A' <= Current && Current <= 'F') ||
                ('a' <= Current && Current <= 'f');
        }

        private bool _CurrentIsSpace()
        {
            return Char.IsWhiteSpace((char)Current);
        }

        private bool _CurrentIsAlpha()
        {
            return Char.IsLetter((char)Current);
        }

        private bool _IsReserved(string identifier, out TK type)
        {
            return ReservedWordDict.TryGetValue(identifier, out type);
        }

        public bool IsReservedWord(string name)
        {
            return ReservedWordDict.ContainsKey(name);
        }

        private void _IncLineNumber()
        {
            var old = Current;
            _Next();
            if (_CurrentIsNewLine() && Current != old)
                _Next();
            if (++LineNumber >= Int32.MaxValue)
                _Error("chunk has too many lines");
        }

        private string _ReadLongString(int sep, bool isComment)
        {
            _SaveAndNext();

            if (_CurrentIsNewLine())
                _IncLineNumber();

            while (true)
            {
                switch (Current)
                {
                    case EOZ:
                        _LexError(_GetSavedString(),
                            "unfinished long string/comment",
                            (int)TK.EOS);
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
                                        (int)TK.EOS);
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
            var r = _GetSavedString();
            int len = r.Length - 2 * (2 + sep);
            if (len <= 0)
            {
                return string.Empty;
            }
            else
            {
                return r.Substring(2 + sep, len);
            }
        }

        private void _EscapeError(string info, string msg)
        {
            _LexError("\\" + info, msg, (int)TK.STRING);
        }

        private byte _ReadHexEscape()
        {
            int r = 0;
            var c = new char[3] { 'x', (char)0, (char)0 };
            // read two hex digits
            for (int i = 1; i < 3; ++i)
            {
                _Next();
                c[i] = (char)Current;
                if (!_CurrentIsXDigit())
                {
                    _EscapeError(new String(c, 0, i + 1),
                        "hexadecimal digit expected");
                    // error
                }
                r = (r << 4) + Int32.Parse(Current.ToString(),
                    NumberStyles.HexNumber);
            }
            return (byte)r;
        }

        private byte _ReadDecEscape()
        {
            int r = 0;
            var c = new char[3];
            // read up to 3 digits
            int i = 0;
            for (i = 0; i < 3 && _CurrentIsDigit(); ++i)
            {
                c[i] = (char)Current;
                r = r * 10 + Current - '0';
                _Next();
            }
            if (r > Byte.MaxValue)
                _EscapeError(new String(c, 0, i),
                    "decimal escape too large");
            return (byte)r;
        }

        private string _ReadString()
        {
            var del = Current;
            _Next();
            while (Current != del)
            {
                switch (Current)
                {
                    case EOZ:
                        _Error("unfinished string");
                        continue;

                    case '\n':
                    case '\r':
                        _Error("unfinished string");
                        continue;

                    case '\\':
                        {
                            byte c;
                            _Next();
                            switch (Current)
                            {
                                case 'a': c = (byte)'\a'; break;
                                case 'b': c = (byte)'\b'; break;
                                case 'f': c = (byte)'\f'; break;
                                case 'n': c = (byte)'\n'; break;
                                case 'r': c = (byte)'\r'; break;
                                case 't': c = (byte)'\t'; break;
                                case 'v': c = (byte)'\v'; break;
                                case 'x': c = _ReadHexEscape(); break;

                                case '\n':
                                case '\r': _Save('\n'); _IncLineNumber(); continue;
                                case 'u' :
                                case '\\':
                                case '\"':
                                case '\'': c = (byte)Current; break;

                                case EOZ: continue;

                                // zap following span of spaces
                                case 'z':
                                    {
                                        _Next(); // skip `z'
                                        while (_CurrentIsSpace())
                                        {
                                            if (_CurrentIsNewLine())
                                                _IncLineNumber();
                                            else
                                                _Next();
                                        }
                                        continue;
                                    }
                                default:
                                    {
                                        if (!_CurrentIsDigit())
                                            _EscapeError(Current.ToString(),
                                                "invalid escape sequence");

                                        // digital escape \ddd
                                        c = _ReadDecEscape();
                                        _Save((char)c);
                                        continue;
                                        // {
                                        //     c = (char)0;
                                        //     for(int i=0; i<3 && _CurrentIsDigit(); ++i)
                                        //     {
                                        //         c = (char)(c*10 + Current - '0');
                                        //         _Next();
                                        //     }
                                        //     _Save( c );
                                        // }
                                        // continue;
                                    }
                            }
                            _Save((char)c);
                            _Next();
                            continue;
                        }

                    default:
                        _SaveAndNext();
                        continue;
                }
            }
            _Next();
            return _GetSavedString();
        }

        private double _ReadNumber()
        {
            var expo = new char[] { 'E', 'e' };
            Utl.Assert(_CurrentIsDigit());
            var first = Current;
            _SaveAndNext();
            if (first == '0' && (Current == 'X' || Current == 'x'))
            {
                expo = new char[] { 'P', 'p' };
                _SaveAndNext();
            }
            for (; ; )
            {
                if (Current == expo[0] || Current == expo[1])
                {
                    _SaveAndNext();
                    if (Current == '+' || Current == '-')
                        _SaveAndNext();
                }
                if (_CurrentIsXDigit() || Current == '.')
                    _SaveAndNext();
                else
                    break;
            }

            double ret;
            var str = _GetSavedString();
            if (O_Str2Decimal(str, out ret))
            {
                return ret;
            }
            else
            {
                _Error("malformed number: " + str);
                return 0.0;
            }
        }

        public static bool O_Str2Decimal(string s, out double result)
        {
            result = 0.0;

            if (s.Contains("n") || s.Contains("N")) // reject `inf' and `nan'
                return false;

            int pos = 0;
            if (s.Contains("x") || s.Contains("X"))
                result = Utl.StrX2Number(s, ref pos);
            else
                result = Utl.Str2Number(s, ref pos);

            if (pos == 0)
                return false; // nothing recognized

            while (pos < s.Length && Char.IsWhiteSpace(s[pos])) ++pos;
            return pos == s.Length; // OK if no trailing characters
        }

        // private float _ReadNumber()
        // {
        //     do
        //     {
        //         _SaveAndNext();
        //     } while( _CurrentIsDigit() || Current == '.' );
        //     if( Current == 'E' || Current == 'e' )
        //     {
        //         _SaveAndNext();
        //         if( Current == '+' || Current == '-' )
        //             _SaveAndNext();
        //     }
        //     while( _CurrentIsAlpha() || _CurrentIsDigit() || Current == '_' )
        //         _SaveAndNext();
        //     float ret;
        //     if( !Single.TryParse( _GetSavedString(), out ret ) )
        //         _Error( "malformed number" );
        //     return ret;
        // }

        private void _Error(string error)
        {
            throw new Exception(string.Format("{0}:{1}: {2}", Source, LineNumber, error));
        }

        private void _LexError(string info, string msg, int tokenType)
        {
            // TODO
            _Error(msg + ":" + info);
        }

        public void SyntaxError(string msg)
        {
            // TODO
            _Error(msg);
        }

        private int _SkipSep()
        {
            int count = 0;
            var boundary = Current;
            _SaveAndNext();
            while (Current == '=')
            {
                _SaveAndNext();
                count++;
            }
            return (Current == boundary ? count : (-count) - 1);
        }

        private Token _Lex()
        {
            _ClearSaved();
            while (true)
            {
                switch (Current)
                {
                    case '\n':
                    case '\r':
                        {
                            var token = new JumpToken(pos);
                            _IncLineNumber();
                            return token;
                        }

                    case '-':
                        {
                            _Next();
                            if (Current != '-') return new LiteralToken('-');

                            // else is a long comment
                            _Next();
                            if (Current == '[')
                            {
                                int sep = _SkipSep();
                                _ClearSaved();
                                if (sep >= 0)
                                {
                                    _ReadLongString(sep, true);
                                    _ClearSaved();
                                    return new JumpToken(pos);
                                }
                            }

                            // else is a short comment
                            while (!_CurrentIsNewLine() && Current != EOZ)
                                _Next();
                            return new JumpToken(pos);
                        }

                    case '[':
                        {
                            int sep = _SkipSep();
                            if (sep >= 0)
                            {
                                string seminfo = _ReadLongString(sep, false);
                                return new StringToken(seminfo);
                            }
                            else if (sep == -1) return new LiteralToken('[');
                            else _Error("invalid long string delimiter");
                            continue;
                        }

                    case '=':
                        {
                            _Next();
                            if (Current != '=') return new LiteralToken('=');
                            _Next();
                            return new TypedToken(TK.EQ);
                        }

                    case '<':
                        {
                            _Next();
                            if (Current != '=') return new LiteralToken('<');
                            _Next();
                            return new TypedToken(TK.LE);
                        }

                    case '>':
                        {
                            _Next();
                            if (Current != '=') return new LiteralToken('>');
                            _Next();
                            return new TypedToken(TK.GE);
                        }

                    case '~':
                        {
                            _Next();
                            if (Current != '=') return new LiteralToken('~');
                            _Next();
                            return new TypedToken(TK.NE);
                        }

                    case ':':
                        {
                            _Next();
                            if (Current != ':') return new LiteralToken(':');
                            _Next();
                            return new TypedToken(TK.DBCOLON); // new in 5.2 ?
                        }

                    case '"':
                    case '\'':
                        {
                            return new StringToken(_ReadString());
                        }
                    case '.':
                        {
                            _SaveAndNext();
                            if (Current == '.')
                            {
                                _SaveAndNext();
                                if (Current == '.')
                                {
                                    _SaveAndNext();
                                    return new TypedToken(TK.DOTS);
                                }
                                else
                                {
                                    return new TypedToken(TK.CONCAT);
                                }
                            }
                            else if (!_CurrentIsDigit())
                                return new LiteralToken('.');
                            else
                                return new NumberToken(_ReadNumber());
                        }
                    case EOZ:
                        {
                            return new TypedToken(TK.EOS);
                        }

                    default:
                        {
                            if (_CurrentIsSpace())
                            {
                                var token = new JumpToken(pos);
                                _Next();
                                return token;
                                //continue;
                            }
                            else if (Current == ';')
                            {
                                var token = new JumpToken(pos);
                                _Next();
                                return token;
                            }
                            else if (_CurrentIsDigit())
                            {
                                return new NumberToken(_ReadNumber());
                            }
                            else if (_CurrentIsAlpha() || Current == '_')
                            {
                                do
                                {
                                    _SaveAndNext();
                                } while (_CurrentIsAlpha() ||
                                         _CurrentIsDigit() ||
                                         Current == '_');

                                string identifier = _GetSavedString();
                                TK type;
                                if (_IsReserved(identifier, out type))
                                {
                                    return new TypedToken(type);
                                }
                                else
                                {
                                    return new NameToken(identifier);
                                }
                            }
                            else
                            {
                                var c = Current;
                                _Next();
                                return new LiteralToken(c);
                            }
                        }
                }
            }
        }

    }

}

#endif