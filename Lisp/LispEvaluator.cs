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

        public static IDictionary<Symbol, object> SpecialForms = new Dictionary<Symbol, object>();

        public static dynamic Apply(List<object> list, IDictionary<Symbol, object> environment)
        {
            if (!(list[0] is Symbol)) throw new ArgumentException();
            Symbol name = list[0] as Symbol;
            if (!(list[1] is List<object>)) throw new ArgumentException();
            List<object> args = list[1] as List<object>;

            if (!environment.ContainsKey(name)) throw new UndefinedSymbol(name);

            if (environment[name] is Delegate)
                return (environment[name] as apply).Invoke(new List<object>(new object[] { name }).Concat(args).ToList(), environment);

            List<object> defFun = (environment[name] as List<object>)[0] as List<object>;
            List<object> proc = (environment[name] as List<object>)[1] as List<object>;

            if (defFun.Count - 1 != args.Count) throw new ArgumentException();

            Dictionary<Symbol, object> parent = new Dictionary<Symbol, object>();
            for (int i = 1; i < defFun.Count; ++i)
            {
                if (environment.ContainsKey(defFun[i] as Symbol))
                {
                    parent.Add(defFun[i] as Symbol, environment[defFun[i] as Symbol]);
                    environment[defFun[i] as Symbol] = args[i - 1];
                }
                else
                {
                    environment.Add(defFun[i] as Symbol, args[i - 1]);
                }
            }

            dynamic res = Evaluate(proc, environment);

            for (int i = 1; i < defFun.Count; ++i)
            {
                environment.Remove(defFun[i] as Symbol);
                if (parent.ContainsKey(defFun[i] as Symbol))
                    environment.Add(defFun[i] as Symbol, parent[defFun[i] as Symbol]);
            }

            return res;
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
                    return (environment[exp] as apply).Invoke(new List<object>(new object[] { exp }), environment);
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
                if (!(lst[0] is Symbol)) throw new UnknownElement();
                first = lst[0] as Symbol;

                if (SpecialForms.ContainsKey(first))
                    return (SpecialForms[first] as apply).Invoke(lst, environment);

                if (!environment.ContainsKey(first))
                    throw new UndefinedSymbol(first);

                if (environment[first] is Delegate)
                {
                    return (environment[first] as apply).Invoke(lst, environment);
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
            if (SpecialForms.Count == 0)
            {
                SpecialForms.Add(new Symbol("if"), (apply)((x, y) =>
                {
                    if (x.Count != 3 && x.Count != 4)
                        throw new ArgumentException();
                    if (Evaluate(x[1], y))
                        return Evaluate(x[2], y);
                    else if (x.Count == 4)
                        return Evaluate(x[3], y);
                    return null;
                }));
                SpecialForms.Add(new Symbol("define"), (apply)((x, y) =>
                {
                    if (x.Count != 3) throw new ArgumentException();
                    if (x[1] is Symbol)
                    {
                        if (x[2] is Symbol) environment.Add(x[1] as Symbol, environment[x[2] as Symbol]);
                        else environment.Add(x[1] as Symbol, Evaluate(x[2], y));
                        return null;
                    }
                    else if (x[1] is List<object>)
                    {
                        List<object> defun = x[1] as List<object>;
                        if (!(defun[0] is Symbol))
                            throw new ArgumentException();
                        if (defun.Count(a => !(a is Symbol)) > 0)
                            throw new ArgumentException();
                        y.Add(defun[0] as Symbol, new List<object>(new object[] { defun, x[2] }));
                        return null;
                    }
                    throw new UnknownElement();
                }));
                SpecialForms.Add(new Symbol("quote"), (apply)((x, y) =>
                {
                    if (x.Count != 2) throw new ArgumentException();
                    return x[1];
                }));
                SpecialForms.Add(new Symbol("lambda"), (apply)((x, y) =>
                {
                    return null;
                }));
            }

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
                return Evaluate(Evaluate(x[1], y), y);
            }));
            environment.Add(new Symbol("apply"), (apply)((x, y) =>
            {
                if (x.Count != 3) throw new ArgumentException();
                if (!(x[1] is Symbol)) throw new ArgumentException();
                return Apply(new List<object>(new object[] { x[1] as Symbol, Evaluate(x[2], y) }), y);
            }));

            environment.Add(new Symbol("display"), (apply)((x, y) =>
            {
                if (x.Count != 2) throw new ArgumentException();
                Console.Write(Evaluate(x[1], y));
                return null;
            }));
            environment.Add(new Symbol("newline"), (apply)((x, y) =>
            {
                Console.WriteLine();
                return null;
            }));

            environment.Add(new Symbol("#t"), true);
            environment.Add(new Symbol("#f"), false);
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
