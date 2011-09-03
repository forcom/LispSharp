using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Lisp {
    public enum TokenType {
        Unknown ,
        OpenParenthesis ,
        CloseParenthesis ,
        Number ,
        String ,
        Symbol
    }

    public class Node {
        public object Value { get; set; }
        public int Offset { get; set; }
        public TokenType TokenType { get; set; }

        public Node ( ) {
            Offset = -1;
            TokenType = Lisp.TokenType.Unknown;
        }

        public Node ( TokenType type = global::Lisp.TokenType.Unknown , int offset = 0 , object value = null ) {
            TokenType = type;
            Offset = offset;
            Value = value;
        }

        public override string ToString ( ) {
            return Value.ToString ( );
        }
    }

    public class LispParser {

        public static string TestParsed ( object node ) {
            if ( node is List<object> ) {
                return "(" + ( from n in ( node as List<object> ) select TestParsed ( n ) ).Aggregate ( ( x , y ) => x + " " + y ) + ")";
            } else if ( node is Node && ( node as Node ).TokenType == TokenType.String ) {
                return "\"" + node.ToString ( ).Replace ( "\"" , "\\\"" ) + "\"";
            }
            return node.ToString ( );
        }

        public static string TestParsedType ( object node ) {
            if ( node is List<object> ) {
                return ( from n in ( node as List<object> ) select TestParsedType ( n ) ).Aggregate ( ( x , y ) => x + "\n" + y );
            } else if ( node is Node && ( node as Node ).TokenType == TokenType.String ) {
                return "\"" + node.ToString ( ).Replace ( "\"" , "\\\"" ) + "\" : [String]";
            }
            return node.ToString ( ) + " : [" + ( node as Node ).TokenType + "]";
        }

        public class SyntaxError : Exception {
            public static string GetErrorMessage ( Token token , string code , string message ) {
                int lines = code.Substring ( 0 , token.Offset ).Count ( x => x == '\n' ) + 1;

                int curLinePos = code.LastIndexOf ( '\n' , token.Offset );
                curLinePos = ( curLinePos == -1 ) ? 0 : curLinePos + 1;

                int nextLinePos = code.IndexOf ( '\n' , token.Offset );
                nextLinePos = ( nextLinePos == -1 ) ? code.Length - 1 : nextLinePos - 1;

                int position = token.Offset - curLinePos;

                string curLine = code.Substring ( curLinePos , nextLinePos - curLinePos + 1 ).Replace ( '\t' , ' ' );

                StringBuilder blankcursor = new StringBuilder ( position + 1 );
                for ( int i = 0 ; i < position ; ++i ) blankcursor.Append ( ' ' );
                blankcursor.Append ( '^' );

                string result = string.Join ( "\n" , message , "Code at [" + lines + ":" + position + "]" , curLine , blankcursor.ToString ( ) );

                return result;
            }

            public SyntaxError ( Token token , string code = "" , string message = "Syntax error has occured." ) : base ( GetErrorMessage ( token , code , message ) ) { }
        }
        public class TokenizingError : SyntaxError {
            public TokenizingError ( Token token , string code = "" , string message = "Tokenizing error has occured." )
                : base ( token , code , message ) { }
        }
        public class SymbolizingError : SyntaxError {
            public SymbolizingError ( Token token , string code = "" , string message = "Symbolizing error has occured." )
                : base ( token , code , message ) { }
        }

        public class ParenthesisError : SymbolizingError {
            public ParenthesisError ( Token token , string code = "" , string message = "Unexpected parenthesis has been detected." )
                : base ( token , code , message ) { }
        }
        public class NumberError : SymbolizingError {
            public NumberError ( Token token , string code = "" , string message = "Number parsing error has occured." )
                : base ( token , code , message ) { }
        }
        public class UnexpectedTokenError : SymbolizingError {
            public UnexpectedTokenError ( Token token , string code = "" , string message = "Unexpected Token has been detected." )
                : base ( token , code , message ) { }
        }

        public class Token {
            public TokenType Type { get; set; }
            public string Value { get; set; }
            public int Offset { get; set; }

            public Token ( ) { Value = ""; Offset = 0; }
            public Token ( string value , int offset = 0 , TokenType type = TokenType.Unknown ) {
                Value = value;
                Offset = offset;
                Type = type;
            }

            public override string ToString ( ) {
                return Value;
            }
        }

        private string sourceCode;

        bool IsSymbolNameOk ( string name ) {
            Regex reg = new Regex ( @"^[\w=\+\*\<\>/\-!]([\w_\.\?=\<\>\-])*$" );
            return reg.IsMatch ( name );
        }

        public int TreeAnalyze ( List<object> Item , Token [ ] expression , int cur ) {
            int i;
            if ( expression [ cur ].Type != TokenType.OpenParenthesis )
                throw new ParenthesisError ( expression [ cur ] , sourceCode );

            for ( i = cur + 1 ; i < expression.Length ; ++i ) {
                switch ( expression [ i ].Type ) {
                    case TokenType.OpenParenthesis:
                        List<object> child = new List<object> ( );
                        Item.Add ( child );
                        i = TreeAnalyze ( child , expression , i );
                        break;
                    case TokenType.CloseParenthesis:
                        return i;
                    case TokenType.Number:
                    case TokenType.String:
                        Item.Add ( new Node ( expression [ i ].Type , expression [ i ].Offset , expression [ i ].Value ) );
                        break;
                    case TokenType.Symbol:
                        if ( !IsSymbolNameOk ( expression [ i ].Value ) )
                            throw new UnexpectedTokenError ( expression [ i ] , sourceCode );
                        Item.Add ( new Node ( expression [ i ].Type , expression [ i ].Offset , expression [ i ].Value ) );
                        break;
                    case TokenType.Unknown:
                        throw new UnexpectedTokenError ( expression [ i ] , sourceCode );
                }
            }
            return expression.Length;
        }

        public Token [ ] PreProcess ( string expression ) {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex ( @"(?<OpenParenthesis>[\(\{\[])|(?<CloseParenthesis>[\)\]\}])|(?<Number>[0-9]+([\.]?[0-9]*)?)|(?<String>""[^\""]*""|'[^\']*')|(?<Symbol>[^\(\{\[\)\}\]\s\\]+)" , System.Text.RegularExpressions.RegexOptions.ExplicitCapture );
            MatchCollection matches = reg.Matches ( expression );

            Token [ ] result = new Token [ matches.Count + 2 ];
            String [ ] groupName = reg.GetGroupNames ( );

            result [ 0 ] = new Token ( "" , -1 , TokenType.OpenParenthesis );
            for ( int i = 0 ; i < matches.Count ; ++i ) {
                result [ i + 1 ] = new Token ( matches [ i ].Value , matches [ i ].Index );
                result [ i + 1 ].Offset = matches [ i ].Index;
                for ( int j = 0 ; j < groupName.Length ; ++j ) {
                    if ( matches [ i ].Groups [ j ].Success ) {
                        result [ i + 1 ].Type = ( TokenType ) j;
                        if ( j == ( int ) TokenType.String ) {
                            result [ i + 1 ].Value = result [ i + 1 ].Value.Substring ( 1 , result [ i + 1 ].Value.Length - 2 );
                        }
                    }
                }
            }
            result [ matches.Count + 1 ] = new Token ( "" , expression.Length - 1 , TokenType.CloseParenthesis );
            return result;
        }

        public List<object> Parse ( string expression ) {
            List<object> root = new List<object> ( );
            Token [ ] equ = null;
            int res;

            sourceCode = expression;

            equ = PreProcess ( expression );

            res = TreeAnalyze ( root , equ , 0 );
            if ( res + 1 < equ.Length ) throw new SymbolizingError ( equ [ res - 1 ] , sourceCode , "Source code cannot reach to the end. Open parenthesises may be missed." );
            else if ( res + 1 > equ.Length ) throw new SymbolizingError ( equ [ res - 1 ] , sourceCode , "Source code does not reach to the end. Close parenthesises may be missed." );

            return root;
        }
    }
}
