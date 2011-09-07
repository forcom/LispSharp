/*
 * Program.cs
 *  - Lisp# Entry Point
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

namespace Lisp {
    class Program {
        /// <summary>
        /// <c>Lisp</c> class is a entry class.
        /// </summary>
        public class Lisp {

            public void CallTest ( ) {
#if DEBUG
                Test test = new Test ( );
                test.StartTest ( ParserTest: true );
#endif
            }

            /*
(define account
    (lambda (n)
        (lambda (incr)
            (setf! n (+ n incr))
             n)))
(define a (account 1))
(a 0) #1
(a 12) #13
(a 5) #18
(define b (account 1))
(b 0) #1
(b 1) #2
(a 1) #19
             * 
(define x 123)
(display x) #123
(let ((x 456) (y 123))
    (display x) #456
    (display y)) #123
(display x) #123
(display y) #Name Error
*/

            public void Start ( ) {
                //CallTest ( );
                List<object> parsed = LispParser.Parse(@"(define args (quote (1 ""hello""))) (display (cdr args))");
                LispEvaluator.StartEvaluate(parsed);
                while ( true ) {
                }
            }
        }

        static void Main ( string [ ] args ) {
            new Lisp ( ).Start ( );
        }
    }
}
