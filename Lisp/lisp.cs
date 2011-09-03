using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Lisp {
    public class SyntaxError : Exception { }
    public class AtomizingError : SyntaxError { }
    public class SymbolizingError : SyntaxError { }

    public class ParenthesisError : SymbolizingError { }
    public class NumberError : SymbolizingError { }
    public class UnexpectedTokenError : SymbolizingError { }

    public enum TokenType {
        Unknown ,
        OpenParenthesis ,
        CloseParenthesis ,
        Number ,
        String ,
        Symbol
    }

    public class Symbol {
        public object Value { get; set; }
        public int Offset { get; set; }
        public int TokenType { get; set; }
        public LinkedList<Symbol> List { get; private set; }

        public Symbol ( ) {
            Value = null;
            List = new LinkedList<Symbol> ( );
        }

        public Symbol ( object value = null ) {
            Value = value;
            List = new LinkedList<Symbol> ( );
        }

        public override string ToString ( ) {
            return Value.ToString();
        }
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

    public class LispParser {
        public Symbol Data { get; set; }

        public string Expression { get; set; }

        public void TreeAnalyze ( Symbol Item , Token [ ] expression , int cur ) {
            int i;
            for ( i = cur ; ; ++i ) {
                switch ( expression [ i ].Type ) {
                    case TokenType.OpenParenthesis:
                        break;
                    case TokenType.CloseParenthesis:
                        break;
                    case TokenType.Number:
                        break;
                    case TokenType.String:
                        break;
                    case TokenType.Symbol:
                        break;
                    case TokenType.Unknown:
                        break;
                }
            }
        }

        public Token [ ] PreProcess ( string expression ) {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex ( @"(?<OpenParenthesis>[\(\{\[])|(?<CloseParenthesis>[\)\]\}])|(?<Number>[0-9]+([\.]?[0-9]*)?)|(?<String>""[^\""]*""|'[^\']*')|(?<Symbol>[^\(\{\[\)\}\]\s\\]+)" , System.Text.RegularExpressions.RegexOptions.ExplicitCapture );
            /*
             */
            MatchCollection matches = reg.Matches ( expression );

            Token [ ] result = new Token [ matches.Count ];
            String [ ] groupName = reg.GetGroupNames ( );

            for ( int i = 0 ; i < matches.Count ; ++i ) {
                result [ i ] = new Token ( matches [ i ].Value , matches [ i ].Index );
                result [ i ].Offset = matches [ i ].Index;
                for ( int j = 0 ; j < groupName.Length ; ++j ) {
                    if ( matches [ i ].Groups [ j ].Success ) {
                        result [ i ].Type = ( TokenType ) j;
                    }
                }
            }
            return result;
        }

        public void Parse ( string expression = null ) {
            Symbol root = new Symbol ( );
            Token [ ] equ = null;

            if ( expression == null ) expression = Expression;

            equ = PreProcess ( expression );

            TreeAnalyze ( root , equ , 0 );
        }
    }

    public class Lisp {
        LispParser parser = new LispParser ( );
        public void Start ( ) {
            parser.Parse ( @"(define (factorial x)
  (cond ((<= x 1) 1)
	(else (* x (factorial (- x 1))))))

(factorial 5)" );
            while(true){
            }
        }
    }
}
