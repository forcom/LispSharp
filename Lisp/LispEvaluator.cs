using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lisp
{
    public class LispEvaluator
    {
        public class RuntimeError : Exception {
            /// <summary>
            /// RuntimeError Exception Construtor
            /// </summary>
            /// <param name="message">Messages to show.</param>
            public RuntimeError(string message = "Syntax error has occured.") : base(message) { }
        }

        public class DividedZero : RuntimeError
        {
            public DividedZero(Symbol token = null) : base("Divided by 0!") { }
        }
        public class NotNumber : RuntimeError
        {
            public NotNumber(Symbol token = null) : base("Cannot calculate with non-number") { }
        }
        public class NotFunctionApply : RuntimeError
        {
            public NotFunctionApply(Symbol token) : base("Cannot apply " + token + " as function") { }
        }
        public class Uncomparable : RuntimeError
        {
            public Uncomparable(Symbol token = null) : base("Cannot compare arguments") { }
        }
        public class ImcompatibleCalculation : RuntimeError
        {
            public ImcompatibleCalculation(Symbol token = null) : base("Not compatible calculation") { }
        }
        public class UnknownElement : RuntimeError
        {
            public UnknownElement(Symbol token = null) : base("Unknown Elements") { }
        }
        public class UndefinedSymbol : RuntimeError
        {
            public UndefinedSymbol(Symbol token) : base(token + " is not defined") { }
        }

        delegate dynamic apply(List<object> list, IDictionary<Symbol, object> environment);
        delegate dynamic evaluate(object expression, IDictionary<Symbol, object> environment);

        public static dynamic Define(List<object> list, IDictionary<Symbol, object> environment)
        {
            return null;
        }

        public static dynamic If(List<object> list, IDictionary<Symbol, object> environment)
        {
            if (list.Count != 3 && list.Count != 4)
                throw new ArgumentException();
            if (Evaluate(list[1], environment))
                return Evaluate(list[2], environment);
            else if (list.Count == 4)
                return Evaluate(list[3], environment);
            return null;
        }

        public static dynamic Apply(List<object> list, IDictionary<Symbol, object> environment)
        {
            return null;
        }

        public static dynamic Evaluate(object expression, IDictionary<Symbol, object> environment)
        {
            if (expression is Symbol)
            {
                Symbol exp = expression as Symbol;
                
                if (!environment.ContainsKey(exp))
                    throw new UndefinedSymbol(exp);
                if (environment[exp] is Delegate)
                {
                    return ((Delegate)environment[exp]).DynamicInvoke(new object[] { new List<object>(), environment });
                }
                else if (environment[exp] is Symbol)
                {
                    return Evaluate(environment[exp], environment);
                }
                else
                    return environment[exp];
            }
            else if(expression is List<object>){
                List<object> lst = expression as List<object>;
                Symbol first;
                try
                {
                    first = lst[0] as Symbol;
                }
                catch
                {
                    throw new UnknownElement();
                }
                //Special Forms Processing

                switch (first.Name)
                {
                    case "define":
                        return null;
                    case "if":
                        return If(lst, environment);
                    case "quote":
                        return null;
                    case "lambda":
                        return null;
                }

                if (!environment.ContainsKey(first))
                    throw new UndefinedSymbol(first);

                if (environment[first] is Delegate)
                {
                    return (environment[first] as Delegate).DynamicInvoke(new object[] { lst, environment });
                }
                else if (environment[first] is List<object>)
                {
                    return new apply(Apply).Invoke(lst, environment);
                }
                else
                {
                    throw new NotFunctionApply(first);
                }
            }
            return expression;
        }

        public static void SetInitialEnvironment(IDictionary<Symbol, object> environment)
        {
            environment.Add(new Symbol("+"), (apply)((x, y) =>
                (from n in x.GetRange(1, x.Count - 1) select Evaluate(n, y)).Aggregate((a, b) => a + b)));
            environment.Add(new Symbol("-"), (apply)((x, y) =>
                (from n in x.GetRange(1, x.Count - 1) select Evaluate(n, y)).Aggregate((a, b) => a - b)));
            environment.Add(new Symbol("*"), (apply)((x, y) =>
                (from n in x.GetRange(1, x.Count - 1) select Evaluate(n, y)).Aggregate((a, b) => a * b)));
            environment.Add(new Symbol("/"), (apply)((x, y) =>
                (from n in x.GetRange(1, x.Count - 1) select Evaluate(n, y)).Aggregate((a, b) => a / b)));
            environment.Add(new Symbol("%"), (apply)((x, y) =>
                (from n in x.GetRange(1, x.Count - 1) select Evaluate(n, y)).Aggregate((a, b) => a % b)));
            environment.Add(new Symbol("="), (apply)((x, y) =>
                (from n in x.GetRange(1, x.Count - 1) select Evaluate(n, y)).Distinct().Count() <= 1));
            environment.Add(new Symbol(">"), (apply)((x, y) =>
            {
                if (x.Count != 3) throw new ArgumentException();
                return Evaluate(x[1], y) > Evaluate(x[2], y);
            }));
            environment.Add(new Symbol("<"), (apply)((x, y) =>
            {
                if (x.Count != 3) throw new ArgumentException();
                return Evaluate(x[1], y) < Evaluate(x[2], y);
            }));
            environment.Add(new Symbol(">="), (apply)((x, y) =>
            {
                if (x.Count != 3) throw new ArgumentException();
                return Evaluate(x[1], y) >= Evaluate(x[2], y);
            }));
            environment.Add(new Symbol("<="), (apply)((x, y) =>
            {
                if (x.Count != 3) throw new ArgumentException();
                return Evaluate(x[1], y) <= Evaluate(x[2], y);
            }));
            environment.Add(new Symbol("!"), (apply)((x, y) =>
            {
                if (x.Count != 2) throw new ArgumentException();
                return !(bool)Evaluate(x[1], y);
            }));

            environment.Add(new Symbol("car"), (apply)((x, y) => Evaluate(x[1], y)));
            environment.Add(new Symbol("cdr"), (apply)((x, y) =>
                from n in x.GetRange(2, x.Count - 2) select Evaluate(n, y)));
            environment.Add(new Symbol("cons"), (apply)((x, y) =>
            {
                if (x.Count != 3) throw new ArgumentException();
                dynamic a = Evaluate(x[1], y);
                dynamic b = Evaluate(x[2], y);
                if (a is List<object> && b is List<object>)
                    return a.Concat(b);
                else if (a is List<object>)
                    return a.Concat(new List<object>(new object[] { b }));
                else if (b is List<object>)
                    return new List<object>(new object[] { a }).Concat(b as List<object>);
                else
                    return new List<object>(new object[] { a, b });
            }));
            environment.Add(new Symbol("list"), (apply)((x, y) =>
                new List<object>(from n in x.GetRange(1, x.Count - 1) select Evaluate(n, y))));

            environment.Add(new Symbol("null?"), (apply)((x, y) => Evaluate(x, y).Count == 1));
            environment.Add(new Symbol("eval"), (apply)((x, y) =>
            {
                if (x.Count != 2) throw new ArgumentException();
                return Evaluate(from n in x.GetRange(1, x.Count - 1) select Evaluate(n, y), y);
            }));
            environment.Add(new Symbol("apply"), (apply)((x, y) =>
            {
                if (x.Count != 3) throw new ArgumentException();
                return Apply(new List<object>(new object[] { x[1], from n in x.GetRange(2, x.Count - 2) select Evaluate(n, y) }), y);
            }));

            environment.Add(new Symbol("display"), (apply)((x, y) =>
            {
                Console.Write(from n in x.GetRange(1, x.Count - 1) select Evaluate(n, y));
                return null;
            }));
            environment.Add(new Symbol("newline"), (apply)((x, y) =>
            {
                Console.WriteLine();
                return null;
            }));

            environment.Add(new Symbol("t"), true);
            environment.Add(new Symbol("f"), false);
            environment.Add(new Symbol("nil"), null);
        }

        public static void StartEvaluate(List<object> expression, IDictionary<Symbol, object> customEnvironment = null)
        {
            Dictionary<Symbol, object> env ;
            if (customEnvironment != null)
                env = new Dictionary<Symbol, object>(customEnvironment);
            else
                env = new Dictionary<Symbol, object>();
            SetInitialEnvironment(env);

            for (int i = 0; i < expression.Count; ++i)
            {
                Evaluate(expression[i], env);
            }
        }
    }
}
