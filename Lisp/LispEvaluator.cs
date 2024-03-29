﻿/*
 * LispEvaluator.cs
 *  - Lisp# Evaluate Module
 * 
 * Created by SeungYong, Yoon
 * 
 * Created in 2011.09.05
 * Last Modified in 2011.09.08
 */

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

        public static IDictionary<Symbol, Func<List<object>, Func<Environment, dynamic>>> SpecialForms = CreateSpecialForms();

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
            List<Func<Environment, dynamic>> analyzed = (from n in expression as List<object> select Analyze(n)).ToList();
            dynamic res = null;
            for (int i = 0; i < analyzed.Count; ++i)
            {
                res = analyzed[i](environment);
            }
            return res;
        }

        public static Func<Environment, dynamic> Analyze(object expression) {
            if (expression is Symbol)
            {
                return (Func<Environment, dynamic>)(environment =>
                {
                    Symbol exp = expression as Symbol;
                    if (!environment.ContainsKey(exp))
                        throw new UndefinedSymbol(exp);
                    return environment[exp];
                });
            }
            else if (expression is List<object>)
            {
                List<object> lst = expression as List<object>;

                if (lst[0] is Symbol)
                {
                    if (SpecialForms.ContainsKey(lst[0] as Symbol))
                    {
                        Func<Environment, dynamic> res = SpecialForms[lst[0] as Symbol](lst.GetRange(1, lst.Count - 1));
                        return (Func<Environment, dynamic>)(environment => res(environment as Environment));
                    }
                }

                List<Func<Environment, dynamic>> args = (from n in lst select Analyze(n)).ToList();
                
                return (Func<Environment, dynamic>)(environment =>
                {
                    List<object> procArgs = (from n in args select n(environment)).ToList();
                    if (procArgs[0] is Delegate)
                    {
                        return (procArgs[0] as Func<List<object>, dynamic>)(procArgs.GetRange(1, procArgs.Count - 1));
                    }
                    else
                    {
                        throw new NotFunctionApply();
                    }
                });
            }
            return (Func<Environment, dynamic>)(x => expression);
        }

        public static Dictionary<Symbol, Func<List<object>, Func<Environment, dynamic>>> CreateSpecialForms()
        {
            var SpecialForms = new Dictionary<Symbol, Func<List<object>, Func<Environment, dynamic>>>();
            SpecialForms.Add(new Symbol("if"), (Func<List<object>, Func<Environment, dynamic>>)(x =>
            {
                if (x.Count != 2 && x.Count != 3)
                    throw new ArgumentException();
                if (x.Count == 2) x.Add(null);

                Func<Environment, dynamic> res = Analyze(x[0]);
                Func<Environment, dynamic> trueValue = Analyze(x[1]);
                Func<Environment, dynamic> falseValue = Analyze(x[2]);

                return (Func<Environment, dynamic>)(y => res(y) ? trueValue(y) : falseValue(y));
            }));
            SpecialForms.Add(new Symbol("define"), (Func<List<object>, Func<Environment, dynamic>>)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                if (!(x[0] is Symbol)) throw new ArgumentException();

                if (x[1] is Symbol)
                {
                    return (Func<Environment, dynamic>)(y =>
                    {
                        y.Add(x[0] as Symbol, y[x[1] as Symbol]);
                        return null;
                    });
                }
                else
                {
                    Func<Environment, dynamic> res = Analyze(x[1]);
                    return (Func<Environment, dynamic>)(y =>
                    {
                        y.Add(x[0] as Symbol, res(y));
                        return null;
                    });
                }
            }));
            SpecialForms.Add(new Symbol("quote"), (Func<List<object>, Func<Environment, dynamic>>)(x =>
            {
                if (x.Count != 1) throw new ArgumentException();
                return (Func<Environment, dynamic>)(y => x[0]);
            }));
            SpecialForms.Add(new Symbol("lambda"), (Func<List<object>, Func<Environment, dynamic>>)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                if (!(x[0] is List<object>)) throw new ArgumentException();

                Func<Environment, dynamic> res = Analyze(x[1]);

                return (Func<Environment, dynamic>)(y =>
                {
                    Func<List<object>, dynamic> lambda = a =>
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
                        return res(child);
                    };
                    return lambda;
                });
            }));
            SpecialForms.Add(new Symbol("let"), (Func<List<object>, Func<Environment, dynamic>>)(x =>
            {
                if (x.Count <= 2) throw new ArgumentException();
                if (!(x[0] is List<object>)) throw new ArgumentException();

                var args = (from n in x.GetRange(1, x.Count - 1) select Analyze(n)).ToList();

                return (Func<Environment, dynamic>)(y =>
                {
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
                    return (from n in args select n(child)).Last();
                });
            }));
            SpecialForms.Add(new Symbol("setf!"), (Func<List<object>, Func<Environment, dynamic>>)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                if (!(x[0] is Symbol)) throw new ArgumentException();
                Func<Environment, dynamic> res = Analyze(x[1]);
                return (Func<Environment, dynamic>)(y => y[x[0] as Symbol] = res(y));
            }));
            return SpecialForms;
        }

        public static Environment CreateInitialEnvironment()
        {
            Environment environment = new Environment();

            environment.Add(new Symbol("+"), (Func<List<object>, dynamic>)(x =>
                x.Aggregate((dynamic a, dynamic b) => a + b)));
            environment.Add(new Symbol("-"), (Func<List<object>, dynamic>)(x =>
                x.Aggregate((dynamic a, dynamic b) => a - b)));
            environment.Add(new Symbol("*"), (Func<List<object>, dynamic>)(x =>
                x.Aggregate((dynamic a, dynamic b) => a * b)));
            environment.Add(new Symbol("/"), (Func<List<object>, dynamic>)(x =>
                x.Aggregate((dynamic a, dynamic b) => a / b)));
            environment.Add(new Symbol("%"), (Func<List<object>, dynamic>)(x =>
                x.Aggregate((dynamic a, dynamic b) => a % b)));
            environment.Add(new Symbol("="), (Func<List<object>, dynamic>)(x =>
                x.Distinct().Count() == 1));
            environment.Add(new Symbol(">"), (Func<List<object>, dynamic>)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                return (dynamic)x[0] > (dynamic)x[1];
            }));
            environment.Add(new Symbol("<"), (Func<List<object>, dynamic>)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                return (dynamic)x[0] < (dynamic)x[1];
            }));
            environment.Add(new Symbol(">="), (Func<List<object>, dynamic>)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                return (dynamic)x[0] >= (dynamic)x[1];
            }));
            environment.Add(new Symbol("<="), (Func<List<object>, dynamic>)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                return (dynamic)x[0] <= (dynamic)x[1];
            }));
            environment.Add(new Symbol("!"), (Func<List<object>, dynamic>)(x =>
            {
                if (x.Count != 1) throw new ArgumentException();
                return !(bool)x[0];
            }));

            environment.Add(new Symbol("car"), (Func<List<object>, dynamic>)(x =>
            {
                if (x.Count != 1) throw new ArgumentException();
                if (!(x[0] is List<object>)) throw new ArgumentException();
                if ((x[0] as List<object>).Count == 0) throw new ArgumentNullException();

                return (x[0] as List<object>)[0];
            }));
            environment.Add(new Symbol("cdr"), (Func<List<object>, dynamic>)(x =>
            {
                if (x.Count != 1) throw new ArgumentException();
                if (!(x[0] is List<object>)) throw new ArgumentException();
                if ((x[0] as List<object>).Count == 0) throw new ArgumentNullException();
                return (x[0] as List<object>).GetRange(1, (x[0] as List<object>).Count - 1);
            }));
            environment.Add(new Symbol("cons"), (Func<List<object>, dynamic>)(x =>
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
            environment.Add(new Symbol("list"), (Func<List<object>, dynamic>)(x => x));

            environment.Add(new Symbol("null?"), (Func<List<object>, dynamic>)(x =>
            {
                if (x.Count != 1) throw new ArgumentException();
                if (!(x[0] is List<object>)) throw new ArgumentException();
                return (x[0] as List<object>).Count == 1;
            }));
            environment.Add(new Symbol("eval"), (Func<List<object>, dynamic>)(x =>
            {
                if (x.Count != 1) throw new ArgumentException();
                return Evaluate(x[0], environment);
            }));
            environment.Add(new Symbol("apply"), (Func<List<object>, dynamic>)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                if (!(x[0] is Delegate)) throw new ArgumentException();
                return Apply(x);
            }));

            environment.Add(new Symbol("display"), (Func<List<object>, dynamic>)(x =>
            {
                if (x.Count != 1) throw new ArgumentException();
                if (x[0] is List<object>) Console.Write(ParsedList(x[0]) + "\n");
                else Console.Write(x[0] + "\n");
                return null;
            }));
            environment.Add(new Symbol("newline"), (Func<List<object>, dynamic>)(x =>
            {
                Console.WriteLine();
                return null;
            }));

            environment.Add(new Symbol("#t"), true);
            environment.Add(new Symbol("#f"), false);
            environment.Add(new Symbol("nil"), null);

            return environment;
        }
    }
}
