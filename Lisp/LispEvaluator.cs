using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lisp
{
    public class LispEvaluator
    {
        public class RuntimeError : Exception {
            static string GetErrorMessage(Node token, string message)
            {
                string result;

                if (token != null)
                {
                    StringBuilder blankcursor = new StringBuilder(token.OriginalCode.Offset + token.Value.ToString().Length);
                    for (int i = 0; i < token.OriginalCode.Offset; ++i) blankcursor.Append(' ');
                    for (int i = 0; i < token.Value.ToString().Length; ++i) blankcursor.Append('~');
                    result = string.Join("\n", message, "Code at [" + token.OriginalCode.Line + ":" + token.OriginalCode.Offset + "]", token.OriginalCode.ThisCodeLine, blankcursor.ToString());
                }
                else
                {
                    result = message;
                }
                return result;
            }

            /// <summary>
            /// RuntimeError Exception Construtor
            /// </summary>
            /// <param name="token">The token that occurs the error.</param>
            /// <param name="message">Messages to show.</param>
            public RuntimeError(Node token, string message = "Syntax error has occured.") : base(GetErrorMessage(token, message)) { }
        }

        public class NotEnoughArguments : RuntimeError
        {
            public NotEnoughArguments(Node token) : base(token, (string)token.Value + " needs more arguments.") { }
        }
        public class DividedZero : RuntimeError
        {
            public DividedZero(Node token) : base(token, "Divided by 0!") { }
        }
        public class NotNumber : RuntimeError
        {
            public NotNumber(Node token) : base(token, "Cannot calculate with non-number") { }
        }

        public class UnknownElement : RuntimeError
        {
            public UnknownElement(Node token = null) : base(token, "Unknown Elements") { }
        }

        public class UndefinedSymbol : RuntimeError
        {
            public UndefinedSymbol(Node token) : base(token, token.Value + " is not defined") { }
        }

        delegate object apply(Node symbol, List<object> list, IDictionary<Node, object> environment);
        delegate dynamic evaluate(object expression, IDictionary<Node, object> environment);

        static decimal Add(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            if (list.Count < 2)
                throw new NotEnoughArguments(symbol);

            decimal x = 0;
            foreach (Node c in list)
            {
                decimal arg;
                try
                {
                    arg = new evaluate(Evaluate).Invoke(c, environment);
                }
                catch
                {
                    throw new NotNumber(symbol);
                }
                x += arg;
            }
            return x;
        }

        static decimal Subtract(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            if (list.Count < 2)
                throw new NotEnoughArguments(symbol);

            decimal x;

            try
            {
                x = new evaluate(Evaluate).Invoke(list[0], environment) * 2;
            }
            catch
            {
                throw new NotNumber(list[0] as Node);
            }

            foreach (Node c in list)
            {
                decimal arg;
                try
                {
                    arg = new evaluate(Evaluate).Invoke(c, environment);
                }
                catch
                {
                    throw new NotNumber(symbol);
                }
                x -= arg;
            }
            return x;
        }

        static decimal Multiply(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            if (list.Count < 2)
                throw new NotEnoughArguments(symbol);

            decimal x = 1;
            foreach (Node c in list)
            {
                decimal arg;
                try
                {
                    arg = new evaluate(Evaluate).Invoke(c, environment);
                }
                catch
                {
                    throw new NotNumber(symbol);
                }
                x *= arg;
            }
            return x;
        }

        static decimal Divide(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            if (list.Count < 2)
                throw new NotEnoughArguments(symbol);

            decimal x;

            try
            {
                x = new evaluate(Evaluate).Invoke(list[0], environment);
                x *= x;
            }
            catch
            {
                throw new NotNumber(symbol);
            }

            foreach (Node c in list)
            {
                decimal arg;
                try
                {
                    arg = new evaluate(Evaluate).Invoke(c, environment);

                    if (arg == 0 && x != 0)
                        throw new DividedZero(c);
                }
                catch
                {
                    throw new NotNumber(symbol);
                }
                x /= arg;
            }
            return x;
        }

        static decimal Modular(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            if (list.Count < 2)
                throw new NotEnoughArguments(symbol);

            decimal x;

            try
            {
                x = new evaluate(Evaluate).Invoke(list[0], environment);
                x *= x;
            }
            catch
            {
                throw new NotNumber(symbol);
            }

            foreach (Node c in list)
            {
                decimal arg;
                try
                {
                    arg = new evaluate(Evaluate).Invoke(c, environment);

                    if (arg == 0 && x != 0)
                        throw new DividedZero(c);
                }
                catch
                {
                    throw new NotNumber(symbol);
                }
                x %= arg;
            }
            return x;
        }

        public static dynamic Apply(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            return null;
        }

        public static dynamic Evaluate(object expression, IDictionary<Node, object> environment)
        {
            dynamic curEq;
            if (expression is List<object>)
            {
                curEq = expression as List<object>;

                if (curEq.Count == 0) return expression;

                if (curEq[0] is List<object>)
                {
                    List<object> res = new List<object>();
                    foreach (object i in curEq)
                    {
                        res.Add(new evaluate(Evaluate).Invoke(curEq, environment));
                    }
                    return res;
                }
                else if (curEq[0] is Node)
                {
                    switch ((curEq[0] as Node).NodeType)
                    {
                        case TokenType.Symbol:
                            if (environment[curEq[0]] is Delegate)
                            {
                                List<object> args = new List<object>(curEq);
                                args.RemoveAt(0);
                                return new apply(environment[curEq]).Invoke(curEq, args, environment);
                            }
                            else
                            {
                                // ToDo : redo
                                return environment[curEq[0]];
                            }
                    }
                }
            }
            else if (expression is Node)
            {
                curEq = expression as Node;

                switch ((curEq as Node).NodeType)
                {
                    case TokenType.Number:
                    case TokenType.String:
                        return curEq.Value;
                    case TokenType.Symbol:
                        if (!environment.ContainsKey(curEq))
                            throw new UndefinedSymbol(curEq);
                        return environment[curEq];
                }
            }
            else {
                throw new UnknownElement();
            }
            return null;
        }

        public static void StartEvaluate(object expression, IDictionary<Node, object> customEnvironment = null)
        {
            Dictionary<Node, object> env ;
            if (customEnvironment != null)
                env = new Dictionary<Node, object>(customEnvironment);
            else
                env = new Dictionary<Node, object>();
        }
    }
}
