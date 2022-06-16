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
* Filename: Parse
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
#if UNITY_EDITOR_WIN || USE_LUA_PROFILER
using System;
using System.Collections.Generic;

namespace MikuLuaProfiler
{
    public static class Parse
    {
        // MikuSample = {BeginMikuSample, EndMikuSample, miku_unpack_return_value}
        public static readonly string LOCAL_PROFILER =
            "local MikuSample = {rawget(_G, 'MikuLuaProfiler').LuaProfiler.BeginSample, rawget(_G, 'MikuLuaProfiler').LuaProfiler.EndSample, rawget(_G, 'miku_unpack_return_value')}"
            + " return (function(...) ";
        #region parse
        public static string InsertSample(string value, string name)
        {
            LLex l = new LLex(new StringLoadInfo(value), name);
            string sampleStr = string.Format("{0}MikuSample[1](\"[lua]:require {1},{1}&line:1\")", LOCAL_PROFILER, name);
            l.InsertString(0, sampleStr);

            int lastPos = 0;
            int nextPos = l.pos;
            l.Next();
            int tokenType = l.Token.TokenType;

            lastPos = nextPos;
            nextPos = l.pos;

            InsertSample(l, ref lastPos, ref nextPos, tokenType, false);
            l.InsertString(l.Length, "\n end)(...)");

            return l.code;
        }

        static void InsertSample(LLex l, ref int lastPos, ref int nextPos, int tokenType, bool onlyFun)
        {
            Stack<int> tokens = new Stack<int>();
            List<Token> history = new List<Token>(16);

            bool needLastSample = true;
            bool hasReturn = false;
            int lastStackToken = -1;
            while (tokenType != (int)TK.EOS)
            {
                switch (tokenType)
                {
                    case (int)TK.FUNCTION:
                        hasReturn = false;
                        tokens.Push(tokenType);
                        lastStackToken = tokenType;
                        string funName = "";
                        bool isLeft = false;

                        bool isForward = false;
                        int index = history.Count - 2;
                        if (index >= 0)
                        {
                            var hisToken = history[index];
                            while (hisToken is JumpToken)
                            {
                                index--;
                                if (index < 0) break;
                                hisToken = history[index];
                            }
                            isForward = hisToken.TokenType == (int)'=';
                            index--;
                            if (isForward && index >= 0)
                            {
                                hisToken = history[index];
                                while (hisToken is JumpToken)
                                {
                                    index--;
                                    if (index < 0) break;
                                    hisToken = history[index];
                                }
                                while (!(hisToken is JumpToken))
                                {
                                    if (hisToken is NameToken)
                                    {
                                        funName = ((NameToken)hisToken).SemInfo + funName;
                                    }
                                    else if ((hisToken.TokenType == (int)':'))
                                    {
                                        funName = ':' + funName;
                                    }
                                    else if ((hisToken.TokenType == (int)'.')
                                        || (hisToken.TokenType == (int)'['))
                                    {
                                        funName = '.' + funName;
                                    }
                                    else if (hisToken is StringToken)
                                    {
                                        funName = ((StringToken)hisToken).SemInfo + funName;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    index--;
                                    if (index < 0) break;
                                    hisToken = history[index];
                                }
                            }
                        }

                        while (tokenType != (int)TK.EOS)
                        {
                            l.Next();
                            tokenType = l.Token.TokenType;

                            lastPos = nextPos;
                            nextPos = l.pos;


                            if (!isLeft && !isForward)
                            {
                                if (l.Token is NameToken)
                                {
                                    funName += ((NameToken)l.Token).SemInfo;
                                }
                                else if ((l.Token.TokenType == (int)':'))
                                {
                                    funName += ':';
                                }
                                else if ((l.Token.TokenType == (int)'.'))
                                {
                                    funName += '.';
                                }
                            }

                            if (tokenType == (int)'(')
                            {
                                isLeft = true;
                            }

                            if (tokenType == (int)')')
                            {
                                if (string.IsNullOrEmpty(funName))
                                {
                                    funName = "";
                                }
                                string profilerStr = string.Format(" MikuSample[1](\"[lua]:{0},{1}&line:{2}\") ", funName, l.Source, l.LineNumber);
                                l.InsertString(nextPos - 1, profilerStr);
                                nextPos = l.pos;
                                break;
                            }
                        }
                        break;
                    case (int)TK.IF:
                    case (int)TK.DO:
                        if (tokens.Count > 0)
                        {
                            tokens.Push(tokenType);
                            lastStackToken = tokenType;
                        }
                        break;
                    case (int)TK.RETURN:
                        int insertPos = lastPos - 1;

                        if (lastStackToken == (int)TK.FUNCTION && tokens.Count == 1)
                        {
                            hasReturn = true;
                        }

                        if (tokens.Count == 0)
                        {
                            needLastSample = false;
                        }
                        while (tokenType != (int)TK.EOS)
                        {
                            l.Next();

                            tokenType = l.Token.TokenType;
                            if (!(l.Token is JumpToken))
                            {
                                lastPos = nextPos;
                                nextPos = l.pos;
                            }

                            if (tokenType == (int)TK.FUNCTION)
                            {
                                InsertSample(l, ref lastPos, ref nextPos, tokenType, true);
                                tokenType = l.Token.TokenType;
                            }

                            if (tokenType == (int)TK.END
                                || tokenType == (int)TK.UNTIL
                                || tokenType == (int)TK.ELSEIF
                                || tokenType == (int)TK.ELSE
                                || tokenType == (int)TK.EOS)
                            {
                                lastPos = lastPos - 1;

                                string returnStr = l.ReadString(insertPos, lastPos - 1);
                                returnStr = " return MikuSample[3](" + returnStr.Substring(6, returnStr.Length - 6) + ") ";
                                l.Replace(insertPos, lastPos - 1, returnStr);

                                nextPos = l.pos;
                                if (tokenType == (int)TK.END)
                                {
                                    if (tokens.Count > 0)
                                        tokens.Pop();
                                    if (onlyFun && tokens.Count <= 0)
                                    {
                                        l.Next();
                                        if (!(l.Token is JumpToken))
                                        {
                                            lastPos = nextPos;
                                            nextPos = l.pos;
                                        }
                                        return;
                                    }
                                }
                                break;
                            }
                        }
                        break;
                    case (int)TK.END:
                        if (tokens.Count > 0)
                        {
                            int token = tokens.Pop();
                            if (token == (int)TK.FUNCTION)
                            {
                                if (!hasReturn)
                                {
                                    l.InsertString(lastPos - 1, " MikuSample[2]() ");
                                    lastPos = nextPos;
                                    nextPos = l.pos;
                                }
                                if (onlyFun && tokens.Count <= 0)
                                {
                                    l.Next();
                                    if (!(l.Token is JumpToken))
                                    {
                                        lastPos = nextPos;
                                        nextPos = l.pos;
                                    }
                                    return;
                                }
                            }
                            if (tokens.Count > 0)
                            {
                                //var tA = tokens.ToArray();
                                lastStackToken = tokens.Peek();
                            }
                            hasReturn = false;
                        }
                        break;
                }
                l.Next();
                history.Add(l.Token);
                if (history.Count >= 16)
                {
                    history.RemoveAt(0);
                }

                tokenType = l.Token.TokenType;
                lastPos = nextPos;
                nextPos = l.pos;
            }

            if (needLastSample)
            {
                l.InsertString(nextPos, "\n MikuSample[2]()");
            }
        }
        #endregion
    }
}
#endif