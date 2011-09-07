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

        delegate dynamic apply(List<object> list);
        delegate dynamic special(List<object> list, IDictionary<Symbol, object> environment);

        public static IDictionary<Symbol, object> SpecialForms = new Dictionary<Symbol, object>();

        public static dynamic Apply(List<object> list)
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
                    return (environment[exp] as apply).Invoke(new List<object>(new object[] { exp }));
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
                    return (SpecialForms[first] as special).Invoke(lst, environment);

                for (int i = 1; i < lst.Count; ++i)
                {
                    lst[i] = Evaluate(lst[i], environment);
                }

                if (!environment.ContainsKey(first))
                    throw new UndefinedSymbol(first);

                if (environment[first] is Delegate)
                {
                    return (environment[first] as apply).Invoke(lst);
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
                SpecialForms.Add(new Symbol("if"), (special)((x, y) =>
                {
                    if (x.Count != 3 && x.Count != 4)
                        throw new ArgumentException();
                    if (Evaluate(x[1], y))
                        return Evaluate(x[2], y);
                    else if (x.Count == 4)
                        return Evaluate(x[3], y);
                    return null;
                }));
                SpecialForms.Add(new Symbol("define"), (special)((x, y) =>
                {
                    if (x.Count != 3) throw new ArgumentException();
                    if (!(x[1] is Symbol)) throw new ArgumentException();
                    
                    if (x[2] is Symbol) environment.Add(x[1] as Symbol, environment[x[2] as Symbol]);
                    else environment.Add(x[1] as Symbol, Evaluate(x[2], y));
                    return null;
                }));
                SpecialForms.Add(new Symbol("quote"), (special)((x, y) =>
                {
                    if (x.Count != 2) throw new ArgumentException();
                    return x[1];
                }));
                SpecialForms.Add(new Symbol("lambda"), (special)((x, y) =>
                {
                    return null;
                }));
                SpecialForms.Add(new Symbol("let"), (special)((x, y) =>
                {
                    return null;
                }));
            }

            environment.Add(new Symbol("+"), (apply)(x =>
                x.GetRange(1, x.Count - 1).Aggregate((dynamic a, dynamic b) => a + b)));
            environment.Add(new Symbol("-"), (apply)(x =>
                x.GetRange(1, x.Count - 1).Aggregate((dynamic a, dynamic b) => a - b)));
            environment.Add(new Symbol("*"), (apply)(x =>
                x.GetRange(1, x.Count - 1).Aggregate((dynamic a, dynamic b) => a * b)));
            environment.Add(new Symbol("/"), (apply)(x =>
                x.GetRange(1, x.Count - 1).Aggregate((dynamic a, dynamic b) => a / b)));
            environment.Add(new Symbol("%"), (apply)(x =>
                x.GetRange(1, x.Count - 1).Aggregate((dynamic a, dynamic b) => a % b)));
            environment.Add(new Symbol("="), (apply)(x =>
                x.GetRange(1, x.Count - 1).Distinct().Count() <= 1));
            environment.Add(new Symbol(">"), (apply)(x =>
            {
                if (x.Count != 3) throw new ArgumentException();
                return (dynamic)x[1] > (dynamic)x[2];
            }));
            environment.Add(new Symbol("<"), (apply)(x =>
            {
                if (x.Count != 3) throw new ArgumentException();
                return (dynamic)x[1]< (dynamic)x[2];
            }));
            environment.Add(new Symbol(">="), (apply)(x =>
            {
                if (x.Count != 3) throw new ArgumentException();
                return (dynamic)x[1] >= (dynamic)x[2];
            }));
            environment.Add(new Symbol("<="), (apply)(x=>
            {
                if (x.Count != 3) throw new ArgumentException();
                return (dynamic)x[1] <= (dynamic)x[2];
            }));
            environment.Add(new Symbol("!"), (apply)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                return !(bool)x[1];
            }));

            environment.Add(new Symbol("car"), (apply)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                if (!(x[1] is List<object>)) throw new ArgumentException();

                return (x[1] as List<object>)[0];
            }));
            environment.Add(new Symbol("cdr"), (apply)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                if (!(x[1] is List<object>)) throw new ArgumentException();
                return (x[1] as List<object>).GetRange(1, (x[1] as List<object>).Count - 1);
            }));
            environment.Add(new Symbol("cons"), (apply)(x =>
            {
                if (x.Count != 3) throw new ArgumentException();
                dynamic a = x[1];
                dynamic b = x[2];
                if (a is List<object> && b is List<object>)
                    return a.Concat(b);
                else if (a is List<object>)
                    return a.Concat(new List<object>(new object[] { b }));
                else if (b is List<object>)
                    return new List<object>(new object[] { a }).Concat(b as List<object>);
                else
                    return new List<object>(new object[] { a, b });
            }));
            environment.Add(new Symbol("list"), (apply)(x =>x.GetRange(1, x.Count - 1)));

            environment.Add(new Symbol("null?"), (apply)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                if (!(x[1] is List<object>)) throw new ArgumentException();
                return (x[1] as List<object>).Count == 1;
            }));
            environment.Add(new Symbol("eval"), (special)((x, y) =>
            {
                if (x.Count != 2) throw new ArgumentException();
                return Evaluate(x[1], y);
            }));
            environment.Add(new Symbol("apply"), (apply)(x =>
            {
                if (x.Count != 3) throw new ArgumentException();
                if (!(x[1] is Symbol)) throw new ArgumentException();
                return Apply(new List<object>(new object[] { x[1] as Symbol, x[2] }));
            }));

            environment.Add(new Symbol("display"), (apply)(x =>
            {
                if (x.Count != 2) throw new ArgumentException();
                if (x[1] is List<object>)
                {
                    Console.Write("( " + (from n in x[1] as List<object> select n).Aggregate((a, b) => a.ToString() + " , " + b.ToString()) + " )");
                }
                else Console.Write(x[1]);
                return null;
            }));
            environment.Add(new Symbol("newline"), (apply)(x =>
            {
                Console.WriteLine();
                return null;
            }));
            environment.Add(new Symbol("setf!"), (apply)(x =>
            {
                //부모 env로 올라가면서 값을 교체
                return null;
            }));

            environment.Add(new Symbol("#t"), true);
            environment.Add(new Symbol("#f"), false);
            environment.Add(new Symbol("nil"), null);
        }

        public static dynamic StartEvaluate(List<object> expression, IDictionary<Symbol, object> customEnvironment = null)
        {
            Dictionary<Symbol, object> env;
            dynamic res = null;
            if (customEnvironment != null)
                env = new Dictionary<Symbol, object>(customEnvironment);
            else
                env = new Dictionary<Symbol, object>();
            SetInitialEnvironment(env);

            for (int i = 0; i < expression.Count; ++i)
            {
                res = Evaluate(expression[i], env);
            }
            return res;
        }
    }
}
