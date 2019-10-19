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
* Filename: Utl
* Created:  2018/7/2 11:36:16
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

#if UNITY_EDITOR || USE_LUA_PROFILER
#define API_CHECK
#define UNILUA_ASSERT

using System;

namespace MikuLuaProfiler
{
    using DebugS = System.Diagnostics.Debug;
    using NumberStyles = System.Globalization.NumberStyles;

    internal static class Utl
    {
        private static void Throw(params string[] msgs)
        {
            throw new Exception(String.Join("", msgs));
        }

        public static void Assert(bool condition)
        {
#if UNILUA_ASSERT
            if (!condition)
                Throw("assert failed!");
            DebugS.Assert(condition);
#endif
        }

        private static bool IsNegative(string s, ref int pos)
        {
            if (pos >= s.Length)
                return false;

            char c = s[pos];
            if (c == '-')
            {
                ++pos;
                return true;
            }
            else if (c == '+')
            {
                ++pos;
            }
            return false;
        }

        private static bool IsXDigit(char c)
        {
            if (Char.IsDigit(c))
                return true;

            if ('a' <= c && c <= 'f')
                return true;

            if ('A' <= c && c <= 'F')
                return true;

            return false;
        }

        private static double ReadHexa(string s, ref int pos, double r, out int count)
        {
            count = 0;
            while (pos < s.Length && IsXDigit(s[pos]))
            {
                r = (r * 16.0) + Int32.Parse(s[pos].ToString(), NumberStyles.HexNumber);
                ++pos;
                ++count;
            }
            return r;
        }

        private static double ReadDecimal(string s, ref int pos, double r, out int count)
        {
            count = 0;
            while (pos < s.Length && Char.IsDigit(s[pos]))
            {
                r = (r * 10.0) + Int32.Parse(s[pos].ToString());
                ++pos;
                ++count;
            }
            return r;
        }

        // following C99 specification for 'strtod'
        public static double StrX2Number(string s, ref int curpos)
        {
            int pos = curpos;
            while (pos < s.Length && Char.IsWhiteSpace(s[pos])) ++pos;
            bool negative = IsNegative(s, ref pos);

            // check `0x'
            if (pos >= s.Length || !(s[pos] == '0' && (s[pos + 1] == 'x' || s[pos + 1] == 'X')))
                return 0.0;

            pos += 2; // skip `0x'

            double r = 0.0;
            int i = 0;
            int e = 0;
            r = ReadHexa(s, ref pos, r, out i);
            if (pos < s.Length && s[pos] == '.')
            {
                ++pos; // skip `.'
                r = ReadHexa(s, ref pos, r, out e);
            }
            if (i == 0 && e == 0)
                return 0.0; // invalid format (no digit)

            // each fractional digit divides value by 2^-4
            e *= -4;
            curpos = pos;

            // exponent part
            if (pos < s.Length && (s[pos] == 'p' || s[pos] == 'P'))
            {
                ++pos; // skip `p'
                bool expNegative = IsNegative(s, ref pos);
                if (pos >= s.Length || !Char.IsDigit(s[pos]))
                    goto ret;

                int exp1 = 0;
                while (pos < s.Length && Char.IsDigit(s[pos]))
                {
                    exp1 = exp1 * 10 + Int32.Parse(s[pos].ToString());
                    ++pos;
                }
                if (expNegative)
                    exp1 = -exp1;
                e += exp1;
            }
            curpos = pos;

        ret:
            if (negative) r = -r;

            return r * Math.Pow(2.0, e);
        }

        public static double Str2Number(string s, ref int curpos)
        {
            int pos = curpos;
            while (pos < s.Length && Char.IsWhiteSpace(s[pos])) ++pos;
            bool negative = IsNegative(s, ref pos);

            double r = 0.0;
            int i = 0;
            int f = 0;
            r = ReadDecimal(s, ref pos, r, out i);
            if (pos < s.Length && s[pos] == '.')
            {
                ++pos;
                r = ReadDecimal(s, ref pos, r, out f);
            }
            if (i == 0 && f == 0)
                return 0.0;

            f = -f;
            curpos = pos;

            // exponent part
            double e = 0.0;
            if (pos < s.Length && (s[pos] == 'e' || s[pos] == 'E'))
            {
                ++pos;
                bool expNegative = IsNegative(s, ref pos);
                if (pos >= s.Length || !Char.IsDigit(s[pos]))
                    goto ret;

                int n;
                e = ReadDecimal(s, ref pos, e, out n);
                if (expNegative)
                    e = -e;
                f += (int)e;
            }
            curpos = pos;

        ret:
            if (negative) r = -r;

            return r * Math.Pow(10, f);
        }
    }

}

#endif