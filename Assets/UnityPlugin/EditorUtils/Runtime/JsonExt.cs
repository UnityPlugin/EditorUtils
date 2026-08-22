using System;
using System.Text;

#if USE_BRIDGE
using UnityPlugin.Bridge;
#endif

namespace UnityPlugin.EditorUtils
{
    public class JsonExt
    {
        static readonly char[] TOKEN_INDENT_START = { '{', '[' };
        static readonly char[] TOKEN_INDENT_END = { '}', ']' };
        static readonly char[] TOKEN_PROPERTY_START = { ':' };
        static readonly char[] TOKEN_PROPERTY_END = { '{', '}', '[', ']', ',' };
        static readonly char[] TOKEN_TRIM = { ' ', '\t', '\r', '\n' };
        static readonly char[] TOKEN_STRING = { '"' };
        static readonly char[] TOKEN_STRING_SKIP = { '\\' };
        const string NEWLINE = "\n";

        public static string GetFormatedJson(string jsonStr, int indentLevel = 2, string indentStr = "    ")
        {
            if (string.IsNullOrEmpty(jsonStr)) return null;

            var isString = false;
            var ignoreNewLine = false;
            var indent = 0;

#if USE_BRIDGE
            var sb = UnityGenericPool<StringBuilder>.Get();
#else
            var sb = new StringBuilder();
#endif
            sb.Clear().Append(jsonStr);

            var i = 0;
            while (i < sb.Length)
            {
                if (!isString && IsToken(TOKEN_STRING, sb[i]))
                {
                    isString = true;

                    if (!ignoreNewLine && indent <= indentLevel)
                    {
                        InsertNewLineIndent(sb, ref i, indentStr, indent);
                    }

                    i++;
                }

                if (isString)
                {
                    while (i < sb.Length)
                    {
                        if (IsToken(TOKEN_STRING_SKIP, sb[i])) i += 2;
                        if (IsToken(TOKEN_STRING, sb[i]))
                        {
                            isString = false;
                            i++;
                            break;
                        }

                        i++;
                    }

                    ignoreNewLine = false;
                }

                if (!isString)
                {
                    if (IsToken(TOKEN_TRIM, sb[i]))
                    {
                        while (IsToken(TOKEN_TRIM, sb[i])) sb.Remove(i, 1);
                        continue;
                    }

                    if (IsToken(TOKEN_PROPERTY_START, sb[i]))
                    {
                        ignoreNewLine = true;
                        i++;
                        continue;
                    }

                    if (IsToken(TOKEN_INDENT_START, sb[i]))
                    {
                        if (i > 0 && !ignoreNewLine && indent <= indentLevel)
                        {
                            InsertNewLineIndent(sb, ref i, indentStr, indent);
                        }

                        ignoreNewLine = false;
                        indent++;
                        i++;
                        continue;
                    }

                    if (IsToken(TOKEN_INDENT_END, sb[i]))
                    {
                        indent--;
                        if (indent + 1 <= indentLevel)
                        {
                            InsertNewLineIndent(sb, ref i, indentStr, indent);
                        }

                        ignoreNewLine = false;
                        i++;
                        continue;
                    }

                    if (IsToken(TOKEN_PROPERTY_END, sb[i])) ignoreNewLine = false;
                    i++;

                    var a = false;
                    if (a) break;
                }
            }
            jsonStr = sb.ToString();

#if USE_BRIDGE
            UnityGenericPool<StringBuilder>.Release(sb);
#endif

            return jsonStr;
        }

        static bool IsToken(char[] tokenList, char value)
        {
            if (tokenList == null || tokenList.Length < 1) return false;
            return Array.IndexOf(tokenList, value) >= 0;
        }

        static void InsertNewLineIndent(StringBuilder strBuilder, ref int index, string indentStr, int count)
        {
            strBuilder.Insert(index, indentStr, count).Insert(index, NEWLINE);
            index = index + indentStr.Length * count + NEWLINE.Length;
        }
    }
}
