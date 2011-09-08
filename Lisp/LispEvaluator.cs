using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lisp
{
    public class LispEvaluator
    {
        public class RuntimeError : Exception
        {
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
            public NotFunctionApply(Symbol token = null) : base("Cannot apply" + (token == null ? "" : token.Name + " ") + " as function") { }
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

        public class Environment : Dictionary<Symbol, object>
        {
            public Environment Parent { get; set; }
            public Environment(Environment parent = null)
            {
                Parent = parent;
            }

            public bool ContainsKey(Symbol key)
            {
                return base.ContainsKey(key) || ((Parent != null) ? Parent.ContainsKey(key) : false);
            }

            public object this[Symbol key]
            {
                get
                {
                    return base.ContainsKey(key) ? base[key] : (Parent != null ? (Parent.ContainsKey(key) ? Parent[key] : null) : null);
                }
                set
                {
                    if (base.ContainsKey(key)) base[key] = value;
                    else if (Parent.ContainsKey(key)) Parent[key] = value;
                    else throw new KeyNotFoundException();
                }
            }
        }

        delegate dynamic apply(List<object> list);
        delegate dynamic special(List<object> list, Environment environment);

        public static Environment SpecialForms = new Environment();

        public static string ParsedList(object node)
        {
            if (node is List<object>)
            {
                return "(" + (from n in (node as List<object>) select ParsedList(n)).Aggregate((x, y) => x + " " + y) + ")";
            }
            else if (node is string)
            {
                return "\"" + node.ToString().Replace("\"", "\\\"") + "\"";
            }
            return node.ToString();
        }

        public static dynamic Apply(List<object> list)
        {
            if (list.Count != 2) throw new ArgumentException();
            if (!(list[0] is Delegate)) throw new NotFunctionApply();
            return (list[0] as Delegate).DynamicInvoke(list[1]);
        }

        public static dynamic Evaluate(object expression, Environment environment)
        {
            if (expression is Symbol)
            {
                Symbol exp = expression as Symbol;

                if (!environment.ContainsKey(exp))
                    throw new UndefinedSymbol(exp);
                if (environment[exp] is Delegate)
                {
                    return (environment[exp] as apply).Invoke(new List<object>(new object[] { }));
                }
                else if (environment[exp] is Symbol)
                {
                    return Evaluate(environment[exp], environment);
                }
                else
                    return environment[exp];
            }
            else if (expression is List<object>)
            {
                List<object> lst = expression as List<object>;

                if (lst[0] is Symbol)
                {
                    if (SpecialForms.ContainsKey(lst[0] as Symbol))
                        return (SpecialForms[lst[0] as Symbol] as special).Invoke(lst.GetRange(1, lst.Count - 1), environment);
                }

                List<object> args = new List<object>(lst.Count);
                for (int i = 0; i < lst.Count; ++i)
                {
                    if (lst[i] is Symbol && environment[lst[i] as Symbol] is Delegate)
                        args.Add(environment[lst[i] as Symbol]);
                    else args.Add(Evaluate(lst[i], environment));
                }

                //if (!environment.ContainsKey(first))throw new UndefinedSymbol(first);

                if (args[0] is Delegate)
                {
                    return (args[0] as apply).Invoke(args.GetRange(1, args.Count - 1));
                }
                else
                {
                    throw new NotFunctionApply();
                }
            }
            return expression;
        }

        public static void SetInitialEnvironment(Environment environment)
        {
            if (SpecialForms.Count == 0)
            {
                SpecialForms.Add(new Symbol("if"), (special)((x, y) =>
                {
                    if (x.Count != 2 && x.Count != 3)
                        throw new ArgumentException();
                    if (Evaluate(x[0], y))
                        return Evaluate(x[1], y);
                    else if (x.Count == 3)
                        return Evaluate(x[2], y);
                    return null;
                }));
                SpecialForms.Add(new Symbol("define"), (special)((x, y) =>
                {
                    if (x.Count != 2) throw new ArgumentException();
                    if (!(x[0] is Symbol)) throw new ArgumentException();

                    if (x[1] is Symbol)
                    {
                        environment.Add(x[0] as Symbol, environment[x[1] as Symbol]);
                    }
                    else
                    {
                        environment.Add(x[0] as Symbol, Evaluate(x[1], y));
                    }
                    return null;
                }));
                SpecialForms.Add(new Symbol("quote"), (special)((x, y) =>
                {
                    if (x.Count != 1) throw new ArgumentException();
                    return x[0];
                }));
                SpecialForms.Add(new Symbol("lambda"), (special)((x, y) =>
                {
                    if (x.Count != 2) throw new ArgumentException();
                    if (!(x[0] is List<object>)) throw new ArgumentException();
                    apply lambda = a =>
                    {
                        Environment child = new Environment(y);
                        List<object> args = x[0] as List<object>;
                        for (int i = 0; i < args.Count; ++i)
                        {
                            if (!(args[i] is Symbol)) throw new ArgumentException();
                            child.Add(args[i] as Symbol, null);
                        }

                        if (a.Count != args.Count) throw new ArgumentException();
                        for (int i = 0; i < args.Count; ++i)
                        {
                            child[args[i] as Symbol] = a[i];
                        }
                        return Evaluate(x[1], child);
                    };
                    return lambda;
                }));
                SpecialForms.Add(new Symbol("let"), (special)((x, y) =>
                {
                    if (x.Count <= 2) throw new ArgumentException();
                    if (!(x[0] is List<object>)) throw new ArgumentException();
                    Environment child = new Environment(y);
                    foreach (List<object> i in x[0] as List<object>)
                    {
                        if (i.Count != 2) throw new ArgumentException();
                        if (!(i[0] is Symbol)) throw new ArgumentException();
                        if (i[1] is Symbol)
                        {
                            if (!y.ContainsKey(i[1] as Symbol)) throw new ArgumentException();
                            child.Add(i[0] as Symbol, y[i[1] as Symbol]);
                        }
                        else
                        {
                            child.Add(i[0] as Symbol, i[1]);
                        }
                    }
                    dynamic res = null;
                    for (int i = 1; i < x.Count; ++i)
                    {
                        res = Evaluate(x[i], child);
                    }
                    return res;
                }));
                SpecialForms.Add(new Symbol("setf!"), (special)((x, y) =>
                {
                    if (x.Count != 2) throw new ArgumentException();
                    if (!(x[0] is Symbol)) throw new ArgumentException();
                    if (!y.ContainsKey(x[0] as Symbol)) throw new UndefinedSymbol(x[0] as Symbol);
                    if (x[1] is Symbol)
                    {
                        if (!y.ContainsKey(x[1] as Symbol)) throw new UndefinedSymbol(x[1] as Symbol);
                        y[x[0] as Symbol] = y[x[1] as Symbol];
                    }
                    else
                    {
                        y[x[0] as Symbol] = Evaluate(x[1], y);
                    }
                    return y[x[0] as Symbol];
                }));
            }

            environment.Add(new Symbol("+"), (apply)(x =>
                x.Aggregate((dynamic a, dynamic b) => a + b)));
            environment.Add(new Symbol("-"), (apply)(x =>
                x.Aggregate((dynamic a, dynamic b) => a - b)));
            environment.Add(new Symbol("*"), (apply)(x =>
                x.Aggregate((dynamic a, dynamic b) => a * b)));
            environment.Add(new Symbol("/"), (apply)(x =>
                x.Aggregate((dynamic a, dynamic b) => a / b)));
            environment.Add(new Symbol("%"), (apply)(x =>
                x.Aggregate((dynamic a, dynamic b) => a % b)));
            environment.Add(new Symbol("="), (apply)(x =>
                x.Distinct().Count() == 1));
            environment.Add(new Symbol(">"), (apply)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                return (dynamic)x[0] > (dynamic)x[1];
            }));
            environment.Add(new Symbol("<"), (apply)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                return (dynamic)x[0] < (dynamic)x[1];
            }));
            environment.Add(new Symbol(">="), (apply)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                return (dynamic)x[0] >= (dynamic)x[1];
            }));
            environment.Add(new Symbol("<="), (apply)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                return (dynamic)x[0] <= (dynamic)x[1];
            }));
            environment.Add(new Symbol("!"), (apply)(x =>
            {
                if (x.Count != 1) throw new ArgumentException();
                return !(bool)x[0];
            }));

            environment.Add(new Symbol("car"), (apply)(x =>
            {
                if (x.Count != 1) throw new ArgumentException();
                if (!(x[0] is List<object>)) throw new ArgumentException();
                if ((x[0] as List<object>).Count == 0) throw new ArgumentNullException();

                return (x[0] as List<object>)[0];
            }));
            environment.Add(new Symbol("cdr"), (apply)(x =>
            {
                if (x.Count != 1) throw new ArgumentException();
                if (!(x[0] is List<object>)) throw new ArgumentException();
                if ((x[0] as List<object>).Count == 0) throw new ArgumentNullException();
                return (x[0] as List<object>).GetRange(1, (x[0] as List<object>).Count - 1);
            }));
            environment.Add(new Symbol("cons"), (apply)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                dynamic a = x[0];
                dynamic b = x[1];
                if (a is List<object> && b is List<object>)
                    return a.Concat(b);
                else if (a is List<object>)
                    return a.Concat(new List<object>(new object[] { b }));
                else if (b is List<object>)
                    return new List<object>(new object[] { a }).Concat(b as List<object>);
                else
                    return new List<object>(new object[] { a, b });
            }));
            environment.Add(new Symbol("list"), (apply)(x => x));

            environment.Add(new Symbol("null?"), (apply)(x =>
            {
                if (x.Count != 1) throw new ArgumentException();
                if (!(x[0] is List<object>)) throw new ArgumentException();
                return (x[0] as List<object>).Count == 1;
            }));
            environment.Add(new Symbol("eval"), (apply)(x =>
            {
                if (x.Count != 1) throw new ArgumentException();
                return Evaluate(x[0], environment);
            }));
            environment.Add(new Symbol("apply"), (apply)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                if (!(x[0] is Delegate)) throw new ArgumentException();
                return Apply(x);
            }));

            environment.Add(new Symbol("display"), (apply)(x =>
            {
                if (x.Count != 1) throw new ArgumentException();
                if (x[0] is List<object>) Console.Write(ParsedList(x[0]) + "\n");
                else Console.Write(x[0] + "\n");
                return null;
            }));
            environment.Add(new Symbol("newline"), (apply)(x =>
            {
                Console.WriteLine();
                return null;
            }));

            environment.Add(new Symbol("#t"), true);
            environment.Add(new Symbol("#f"), false);
            environment.Add(new Symbol("nil"), null);
        }

        public static dynamic StartEvaluate(List<object> expression, Environment customEnvironment = null)
        {
            Environment env;
            dynamic res = null;
            if (customEnvironment != null)
                env = new Environment(customEnvironment);
            else
                env = new Environment();
            SetInitialEnvironment(env);

            for (int i = 0; i < expression.Count; ++i)
            {
                res = Evaluate(expression[i], env);
            }
            return res;
        }
    }
}
