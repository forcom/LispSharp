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
        public class TooMuchArguments : RuntimeError
        {
            public TooMuchArguments(Node token) : base(token, "There are too much arguments") { }
        }
        public class DividedZero : RuntimeError
        {
            public DividedZero(Node token) : base(token, "Divided by 0!") { }
        }
        public class NotNumber : RuntimeError
        {
            public NotNumber(Node token) : base(token, "Cannot calculate with non-number") { }
        }
        public class NotFunctionApply : RuntimeError
        {
            public NotFunctionApply(Node token) : base(token, "Cannot apply " + token.Value + " as function") { }
        }
        public class Uncomparable : RuntimeError
        {
            public Uncomparable(Node token) : base(token, "Cannot compare arguments") { }
        }
        public class ImcompatibleCalculation : RuntimeError
        {
            public ImcompatibleCalculation(Node token) : base(token, "Not compatible calculation") { }
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

        static dynamic Add(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            if (list.Count < 2)
                throw new NotEnoughArguments(symbol);

            decimal x = 0;
            foreach (Node c in list)
            {
                decimal arg;
                try
                {
                    arg = Evaluate(c, environment);
                }
                catch
                {
                    throw new NotNumber(symbol);
                }
                x += arg;
            }
            return x;
        }
        static dynamic Subtract(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            if (list.Count == 0)
                throw new NotEnoughArguments(symbol);

            decimal x;

            if (list.Count == 1)
            {
                try
                {
                    x = -Evaluate(list[0], environment);
                    return x;
                }
                catch
                {
                    throw new NotNumber(symbol);
                }
            }

            try
            {
                x = Evaluate(list[0], environment);
            }
            catch
            {
                throw new NotNumber(list[0] as Node);
            }

            for (int i = 1; i < list.Count; ++i)
            {
                decimal arg;
                try
                {
                    arg = Evaluate(list[i], environment);
                }
                catch
                {
                    throw new NotNumber(symbol);
                }
                x -= arg;
            }
            return x;
        }
        static dynamic Multiply(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            if (list.Count < 2)
                throw new NotEnoughArguments(symbol);

            decimal x = 1;
            foreach (Node c in list)
            {
                decimal arg;
                try
                {
                    arg = Evaluate(c, environment);
                }
                catch
                {
                    throw new NotNumber(symbol);
                }
                x *= arg;
            }
            return x;
        }
        static dynamic Divide(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            if (list.Count < 2)
                throw new NotEnoughArguments(symbol);

            decimal x;

            try
            {
                x = Evaluate(list[0], environment);
            }
            catch
            {
                throw new NotNumber(symbol);
            }

            for (int i = 1; i < list.Count; ++i)
            {
                decimal arg;
                try
                {
                    arg = Evaluate(list[i], environment);
                    if (arg == 0)
                        throw new DividedZero(symbol);
                }
                catch
                {
                    throw new NotNumber(symbol);
                }
                x /= arg;
            }
            return x;
        }
        static dynamic Modular(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            if (list.Count < 2)
                throw new NotEnoughArguments(symbol);

            decimal x;

            try
            {
                x = Evaluate(list[0], environment);
            }
            catch
            {
                throw new NotNumber(symbol);
            }

            for ( int i = 1 ; i < list.Count ; ++ i )
            {
                decimal arg;
                try
                {
                    arg = Evaluate(list[i], environment);

                    if (arg == 0)
                        throw new DividedZero(symbol);
                }
                catch
                {
                    throw new NotNumber(symbol);
                }
                x %= arg;
            }
            return x;
        }
        static dynamic Equal(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            if (list.Count < 2)
                throw new NotEnoughArguments(symbol);

            dynamic prev = Evaluate(list[0], environment);
            for (int i = 1; i < list.Count; ++i)
            {
                try
                {
                    dynamic cur = Evaluate(list[i], environment);
                    if (prev != cur) return false;
                    prev = cur;
                }
                catch
                {
                    throw new Uncomparable(symbol);
                }
            }
            return true;
        }
        static dynamic Not(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            if (list.Count != 1)
                throw new TooMuchArguments(symbol);
            try
            {
                bool res = Evaluate(list, environment);
                return !res;
            }
            catch
            {
                throw new ImcompatibleCalculation(symbol);
            }
        }

        static dynamic Car(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            if (list.Count == 0)
                throw new NotEnoughArguments(symbol);
            return list[0];
        }
        static dynamic Cdr(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            if (list.Count < 2)
                throw new NotEnoughArguments(symbol);
            List<object> res = new List<object>(list);
            res.RemoveAt(0);
            return res;
        }
        static dynamic Cons(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            List<object> res = new List<object>();
            foreach (object i in list)
            {
                res.Add(i);
            }
            return res;
        }
        static dynamic IsNull(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            if (list.Count == 0) return true;
            return false;
        }

        public static dynamic Define(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            return null;
        }

        public static dynamic If(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            return null;
        }

        public static dynamic Apply(Node symbol, List<object> list, IDictionary<Node, object> environment)
        {
            if (symbol.NodeType != TokenType.Symbol) throw new NotFunctionApply(symbol);
            return null;
        }

        public static dynamic Evaluate(object expression, IDictionary<Node, object> environment)
        {
            if (expression is Node)
            {
                Node exp = expression as Node;
                if (exp.NodeType != TokenType.Symbol)
                    throw new NotFunctionApply(exp);

                if (!environment.ContainsKey(exp))
                    throw new UndefinedSymbol(exp);
                if (environment[exp] is Delegate)
                {
                    return ((Delegate)environment[exp]).DynamicInvoke();
                }
                else if (environment[exp] is Node)
                {
                    return Evaluate(environment[exp], environment);
                }
                else
                    throw new UnknownElement(exp);
            }
            else if(expression is List<object>){
                List<object> lst = expression as List<object>;
                Node first;
                try
                {
                    first = lst[0] as Node;
                }
                catch
                {
                    throw new UnknownElement();
                }

                if (first.NodeType != TokenType.Symbol)
                    throw new NotFunctionApply(first);

                //Special Forms Processing

                switch (first.Value.ToString())
                {
                    case "define":
                        return null;
                    case "if":
                        return null;
                    case "quote":
                        return null;
                    case "lambda":
                        return null;
                }

                if (!environment.ContainsKey(first))
                    throw new UndefinedSymbol(first);

                List<object> args = new List<object>();
                args.RemoveAt(0);

                foreach (object i in lst)
                {
                    args.Add(Evaluate(i, environment));
                }

                if (environment[first] is Delegate)
                {
                    (environment[first] as Delegate).DynamicInvoke(new object[]{ first, args });
                }
                else if (environment[first] is List<object>)
                {
                }
                else
                {
                    throw new UnknownElement();
                }

                return Apply(first, args, environment);
            }
            else
                throw new UnknownElement();
        }

        public static void SetInitialEnvironment(IDictionary<Node, object> environment)
        {
            environment.Add(new Node(TokenType.Symbol, "+"), new apply(Add));
            environment.Add(new Node(TokenType.Symbol, "-"), new apply(Subtract));
            environment.Add(new Node(TokenType.Symbol, "*"), new apply(Multiply));
            environment.Add(new Node(TokenType.Symbol, "/"), new apply(Divide));
            environment.Add(new Node(TokenType.Symbol, "%"), new apply(Modular));
            environment.Add(new Node(TokenType.Symbol, "="), new apply(Equal));

            environment.Add(new Node(TokenType.Symbol, "null?"), new apply(IsNull));
            environment.Add(new Node(TokenType.Symbol, "eval"), new evaluate(Evaluate));
            environment.Add(new Node(TokenType.Symbol, "apply"), new apply(Apply));

            environment.Add(new Node(TokenType.Symbol, "t"), true);
            environment.Add(new Node(TokenType.Symbol, "f"), false);
            environment.Add(new Node(TokenType.Symbol, "nil"), null);
        }

        public static void StartEvaluate(List<object> expression, IDictionary<Node, object> customEnvironment = null)
        {
            Dictionary<Node, object> env ;
            if (customEnvironment != null)
                env = new Dictionary<Node, object>(customEnvironment);
            else
                env = new Dictionary<Node, object>();
            SetInitialEnvironment(env);

            for (int i = 0; i < expression.Count; ++i)
            {
                dynamic res;
                res = Evaluate(expression[i], env);
                Console.WriteLine(res);
            }
        }
    }
}
