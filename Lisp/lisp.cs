/*
 * Lisp.cs
 *  - Lisp# Implement Module
 * 
 * Created by SeungYong, Yoon
 * 
 * Created in 2011.09.04
 * Last Modified in 2011.09.06
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Lisp
{
    /// <summary>
    /// TokenTypes
    /// </summary>
    /// <remarks>When the lisp code is tokened, to identify the token's information.</remarks>
    public enum TokenType
    {
        /// <summary>
        /// Unknown Token Type
        /// </summary>
        Unknown,
        /// <summary>
        /// OpenParenthesis. ({[
        /// </summary>
        OpenParenthesis,
        /// <summary>
        /// CloseParenthesis. )}]
        /// </summary>
        CloseParenthesis,
        /// <summary>
        /// Number. 0-9
        /// </summary>
        Number,
        /// <summary>
        /// String
        /// </summary>
        String,
        /// <summary>
        /// Symbol
        /// </summary>
        Symbol
    }

    /// <summary>
    /// Node.
    /// </summary>
    /// <remarks>After the tokenizing is over, <c>LispParser</c> would make a lisp tree.
    /// To make lisp tree, <c>Node</c> will be used</remarks>
    public class Node
    {
        /// <summary>
        /// Node's Value
        /// </summary>
        /// <remarks>It can be Number, String, or Symbol.
        /// It is defined by <c>TokenType</c>.</remarks>
        public object Value { get; set; }
        /// <summary>
        /// Type of <c>Node</c>
        /// </summary>
        public TokenType NodeType { get; set; }
        /// <summary>
        /// Original souce code line.
        /// </summary>
        public DebugInfo OriginalCode { get; private set; }

        /// <summary>
        /// Node Constructor
        /// </summary>
        /// <param name="type">Type of <c>Node</c></param>
        /// <param name="debugInfo">Debugging Information</param>
        /// <param name="value">Value of <c>Node</c></param>
        /// <remarks>Default value of <c>Node</c> is a Unknown typed NULL Node.
        /// Default source code position is -1.</remarks>
        public Node(TokenType type = global::Lisp.TokenType.Unknown, object value = null, DebugInfo debugInfo = null)
        {
            NodeType = type;
            Value = value;
            OriginalCode = debugInfo;
        }

        /// <summary>
        /// (Override) ToString Method
        /// </summary>
        /// <returns>Returns value of <c>Node</c></returns>
        public override string ToString()
        {
            return Value.ToString();
        }
    }

    /// <summary>
    /// Debugging Information
    /// </summary>
    /// <remarks>When the exception has occured, this information such as line number, column, souce code line will be shown.</remarks>
    public class DebugInfo
    {
        /// <summary>
        /// Line number
        /// </summary>
        public int Line { get; set; }
        /// <summary>
        /// Column number
        /// </summary>
        public int Offset { get; set; }
        /// <summary>
        /// Original source code line
        /// </summary>
        public string ThisCodeLine { get; set; }

        /// <summary>
        /// DebugInfo constructor
        /// </summary>
        /// <param name="line">Line number</param>
        /// <param name="offset">Column number</param>
        /// <param name="thisLine">Original source code line</param>
        public DebugInfo(int line = -1, int offset = -1, string thisLine = "")
        {
            Line = line;
            Offset = offset;
            ThisCodeLine = thisLine;
        }
    }

    /// <summary>
    /// Lisp# Parser Implements
    /// </summary>
    /// <example>
    /// Usage:
    ///  - To parse lisp# code
    /// <code>
    ///     LispParser parser = new LispParser( );
    ///     List&lt;object&gt; parsedLisp = parser.parse ( "(a ((b c) d e))" );
    /// </code>
    /// 
    ///  - [Debug Mode] To identify parsed code.
    /// <code>
    ///     //This code will recreate lisp# code.
    ///     Console.WriteLine( LispParser.TestParsed ( parsedLisp ) );
    ///     //This code will show the type of Lisp# members.
    ///     Console.WriteLine( LispParser.TestParsedType ( parsedLisp ) );
    /// </code>
    /// </example>
    public class LispParser
    {

#if DEBUG
        /// <summary>
        /// [Debug Mode] Recreate Lisp# code.
        /// </summary>
        /// <param name="node">Parsed Lisp# lists. List&lt;object&gt; is required.</param>
        /// <returns>Recreated Lisp# code</returns>
        /// <example>
        /// Input >> { a , { { b , c } , d } , e }
        /// Output >> "(a ((b c) d) e)"
        /// </example>
        public static string TestParsed(object node)
        {
            if (node is List<object>)
            {
                return "(" + (from n in (node as List<object>) select TestParsed(n)).Aggregate((x, y) => x + " " + y) + ")";
            }
            else if (node is Node && (node as Node).NodeType == TokenType.String)
            {
                return "\"" + node.ToString().Replace("\"", "\\\"") + "\"";
            }
            return node.ToString();
        }

        /// <summary>
        /// [Debug Mode] Shows the type of Lisp# members.
        /// </summary>
        /// <param name="node">Parsed Lisp# lists. List&lt;object&gt; is required.</param>
        /// <returns>Information String</returns>
        /// <example>
        /// Input >> { a , { { b , "hello" } , 3.14 } , d }
        /// Output >> 
        ///     a : [Symbol]
        ///     b : [Symbol]
        ///     "hello" : [String]
        ///     3.14 : [Number]
        ///     d : [Symbol]
        /// </example>
        public static string TestParsedType(object node)
        {
            if (node is List<object>)
            {
                return (from n in (node as List<object>) select TestParsedType(n)).Aggregate((x, y) => x + "\n" + y);
            }
            else if (node is Node && (node as Node).NodeType == TokenType.String)
            {
                return "\"" + node.ToString().Replace("\"", "\\\"") + "\" : [String]";
            }
            return node.ToString() + " : [" + (node as Node).NodeType + "]";
        }
#endif

        /// <summary>
        /// SyntaxError Exception
        /// </summary>
        public class SyntaxError : Exception
        {
            static string GetErrorMessage(Token token, string message)
            {
                StringBuilder blankcursor = new StringBuilder(token.OriginalCode.Offset + 1);
                for (int i = 0; i < token.OriginalCode.Offset; ++i) blankcursor.Append(' ');
                blankcursor.Append('^');

                return string.Join("\n", message, "Code at [" + token.OriginalCode.Line + ":" + token.OriginalCode.Offset + "]", token.OriginalCode.ThisCodeLine, blankcursor.ToString());
            }

            /// <summary>
            /// SyntaxError Exception Construtor
            /// </summary>
            /// <param name="token">The token that occurs the error.</param>
            /// <param name="message">Messages to show.</param>
            public SyntaxError(Token token, string message = "Syntax error has occured.") : base(GetErrorMessage(token, message)) { }
        }
        /// <summary>
        /// TokenizingError Exception
        /// </summary>
        /// <remarks><c>TokenizingError</c> Exception would occur in <c>PreProcess</c> method.</remarks>
        public class TokenizingError : SyntaxError
        {
            /// <summary>
            /// TokenizingError Exception Constructor
            /// </summary>
            /// <param name="token">The token that occurs the error.</param>
            /// <param name="message">Messages to show.</param>
            public TokenizingError(Token token, string message = "Tokenizing error has occured.")
                : base(token, message) { }
        }
        /// <summary>
        /// SymbolizingError Exception
        /// </summary>
        /// <remarks><c>SymbolizingError</c> Exception would occur in <c>TreeAnalyze</c> method.</remarks>
        public class SymbolizingError : SyntaxError
        {
            /// <summary>
            /// SymbolizingError Exception Constructor
            /// </summary>
            /// <param name="token">The token that occurs the error.</param>
            /// <param name="message">Messages to show.</param>
            public SymbolizingError(Token token, string message = "Symbolizing error has occured.")
                : base(token, message) { }
        }

        /// <summary>
        /// ParenthesisError Exception
        /// </summary>
        /// <remarks><c>ParenthesisError</c> Exception would occur when the pair of Open-Close parenthesis is not correctly associated.</remarks>
        public class ParenthesisError : SymbolizingError
        {
            /// <summary>
            /// ParenthesisError Exception Constructor
            /// </summary>
            /// <param name="token">The token that occurs the error.</param>
            public ParenthesisError(Token token)
                : base(token, "Unexpected parenthesis has been detected.") { }
        }
        /// <summary>
        /// NumberError Exception
        /// </summary>
        /// <remarks><c>NumberError</c> Exception would occur when the number is not correctly parsed.</remarks>
        public class NumberError : SymbolizingError
        {
            /// <summary>
            /// NumberError Exception Constructor
            /// </summary>
            /// <param name="token">The token that occurs the error.</param>
            public NumberError(Token token)
                : base(token, "Number parsing error has occured.") { }
        }
        /// <summary>
        /// UnexpectedTokenError Exception
        /// </summary>
        /// <remarks><c>UnexpectedTokenError</c> Exception would occur when wrong symbol name is detected or unacceptable token is found.</remarks>
        public class UnexpectedTokenError : SymbolizingError
        {
            /// <summary>
            /// UnexpectedTokenError Exception Constructor
            /// </summary>
            /// <param name="token">The token that occurs the error.</param>
            public UnexpectedTokenError(Token token)
                : base(token, "Unexpected Token has been detected.") { }
        }

        /// <summary>
        /// Token
        /// </summary>
        /// <remarks><c>Token</c> is used in <c>PreProcess</c> method.
        /// <c>Token</c> is created from a raw string source code </remarks>
        public class Token
        {
            /// <summary>
            /// Type of Token
            /// </summary>
            public TokenType Type { get; set; }
            /// <summary>
            /// Value of Token
            /// </summary>
            public string Value { get; set; }
            /// <summary>
            /// Original source code line.
            /// </summary>
            public DebugInfo OriginalCode { get; set; }

            /// <summary>
            /// Token Constructor
            /// </summary>
            /// <param name="value">Value of Token</param>
            /// <param name="type">Type of Token</param>
            /// <param name="debugInfo">Original source code information</param>
            /// <remarks>Default value of <c>Token</c> is a Unknown-typed NULL token.
            /// Its position in the source code is -1.</remarks>
            public Token(string value = "", TokenType type = TokenType.Unknown, DebugInfo debugInfo = null)
            {
                Value = value;
                Type = type;
                OriginalCode = debugInfo;
            }

            /// <summary>
            /// (Override) ToString
            /// </summary>
            /// <returns>Returns value of Token</returns>
            public override string ToString()
            {
                return Value;
            }

            /// <summary>
            /// Set the debug information
            /// </summary>
            /// <param name="offset">Index of token in the entire source code</param>
            /// <param name="expression">Original source code</param>
            public void SetThisCodeLine(int offset, string expression)
            {
                int lines = expression.Substring(0, offset).Count(x => x == '\n') + 1;

                int curLinePos = expression.LastIndexOf('\n', offset);
                curLinePos = (curLinePos == -1) ? 0 : curLinePos + 1;

                int nextLinePos = expression.IndexOf('\n', offset);
                nextLinePos = (nextLinePos == -1) ? expression.Length - 1 : nextLinePos - 1;

                int position = offset - curLinePos;

                string curLine = expression.Substring(curLinePos, nextLinePos - curLinePos + 1).Replace('\t', ' ');

                OriginalCode = new DebugInfo(lines, position, curLine);
            }
        }

        /// <summary>
        /// Check the symbol name is acceptable.
        /// </summary>
        /// <param name="name">Symbol name</param>
        /// <returns>True if the name is acceptable, otherwise false.</returns>
        /// <remarks>
        /// Acceptable Symbol name
        ///  - At the first letter, only number, character, =, +, *, &lt;, &gt;, /, -, ! is acceptable.
        ///  - In the Symbol name, only number, character, _, ., ?, =, &lt;, &gt;, - is acceptable.
        /// </remarks>
        bool IsSymbolNameOk(string name)
        {
            Regex reg = new Regex(@"^[\w=\+\*\<\>/\-!%]([\w_\.\?=\<\>\-])*$");
            return reg.IsMatch(name);
        }

        /// <summary>
        /// Create List# Tree
        /// </summary>
        /// <param name="Item">Current list</param>
        /// <param name="expression">Tokenized source code.</param>
        /// <param name="cur">Current parsing position in tokenized source code.</param>
        /// <param name="sourceCode">Original source code.</param>
        /// <returns>Completed parsing position in tokenized source code.</returns>
        /// <exception cref="SyntaxError" />
        /// <exception cref="SymbolizingError" />
        /// <exception cref="ParenthesisError" />
        /// <exception cref="NumberError" />
        /// <exception cref="UnexpectedTokenError" />
        int TreeAnalyze(List<object> Item, Token[] expression, int cur, string sourceCode)
        {
            int i;
            if (expression[cur].Type != TokenType.OpenParenthesis)
                throw new ParenthesisError(expression[cur]);

            for (i = cur + 1; i < expression.Length; ++i)
            {
                switch (expression[i].Type)
                {
                    case TokenType.OpenParenthesis:
                        List<object> child = new List<object>();
                        Item.Add(child);
                        i = TreeAnalyze(child, expression, i, sourceCode);
                        break;
                    case TokenType.CloseParenthesis:
                        return i;
                    case TokenType.Number:
                        Item.Add(new Node(expression[i].Type, decimal.Parse(expression[i].Value), expression[i].OriginalCode));
                        break;
                    case TokenType.String:
                        Item.Add(new Node(expression[i].Type, expression[i].Value, expression[i].OriginalCode));
                        break;
                    case TokenType.Symbol:
                        if (!IsSymbolNameOk(expression[i].Value))
                            throw new UnexpectedTokenError(expression[i]);
                        Item.Add(new Node(expression[i].Type, expression[i].Value, expression[i].OriginalCode));
                        break;
                    case TokenType.Unknown:
                        throw new UnexpectedTokenError(expression[i]);
                }
            }
            return expression.Length;
        }

        /// <summary>
        /// Tokenize original source code.
        /// </summary>
        /// <param name="expression">Original source code</param>
        /// <returns>Tokenized source code.</returns>
        Token[] PreProcess(string expression)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"(?<OpenParenthesis>[\(\{\[])|(?<CloseParenthesis>[\)\]\}])|(?<Number>[0-9]+([\.]?[0-9]*)?)|(?<String>""[^\""]*""|'[^\']*')|(?<Symbol>[^\(\{\[\)\}\]\s\\]+)", System.Text.RegularExpressions.RegexOptions.ExplicitCapture);
            MatchCollection matches = reg.Matches(expression);

            Token[] result = new Token[matches.Count + 2];
            String[] groupName = reg.GetGroupNames();

            result[0] = new Token("", TokenType.OpenParenthesis);
            result[0].SetThisCodeLine(0, expression);
            for (int i = 0; i < matches.Count; ++i)
            {
                result[i + 1] = new Token(matches[i].Value);
                result[i + 1].SetThisCodeLine(matches[i].Index, expression);
                for (int j = 0; j < groupName.Length; ++j)
                {
                    if (matches[i].Groups[j].Success)
                    {
                        result[i + 1].Type = (TokenType)j;
                        if (j == (int)TokenType.String)
                        {
                            result[i + 1].Value = result[i + 1].Value.Substring(1, result[i + 1].Value.Length - 2);
                        }
                    }
                }
            }
            result[matches.Count + 1] = new Token("", TokenType.CloseParenthesis);
            result[matches.Count + 1].SetThisCodeLine(expression.Length - 1, expression);
            return result;
        }

        /// <summary>
        /// Parse Lisp# source code
        /// </summary>
        /// <param name="expression">Lisp# source code</param>
        /// <returns>Parsed Lisp# lists</returns>
        /// <example>Usage:
        /// <code>
        ///     LispParser parser = new LispParser( );
        ///     List&lt;object&gt; parsed = parser.Parse( "(a ((b c) d) e)" );
        /// </code>
        /// </example>
        public List<object> Parse(string expression)
        {
            List<object> root = new List<object>();
            Token[] equ = null;
            int res;

            equ = PreProcess(expression);

            res = TreeAnalyze(root, equ, 0, expression);
            if (res + 1 < equ.Length) throw new SymbolizingError(equ[res - 1], "Source code cannot reach to the end. Open parenthesises may be missed.");
            else if (res + 1 > equ.Length) throw new SymbolizingError(equ[res - 1], "Source code does not reach to the end. Close parenthesises may be missed.");

            return root;
        }
    }
}
