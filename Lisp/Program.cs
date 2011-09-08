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
                /*
                (display ""--- Apply and Function as Value"")(define display2 display)(apply display2 (quote (""Hello"")))(display2 ""Hello"")(display ""--- Lexical Scope (let)"")(let ((+ -) (- +))     (display (+ 4 3))     (display (- 4 3))     (setf! - *)     (display (- 4 3)))(display (+ 4 3))(display (- 4 3))(display ""--- Lambda"")(display ((lambda (a b) (+ a b)) 1 2))(display ""--- Higher-Order Function"")(display ((lambda (op a b) (op a (op a b))) * 3 4))(display (((lambda () (lambda (a) (* a a)))) 3))(display ""--- Lexical Scope (lambda)"")(display (((lambda (a) (lambda (b) (/ a b))) 9) 3))
                (define account        (lambda (balance)                (lambda (incr)                        (setf! balance (+ balance incr)))))(define hong (account 10))(define choi (account 5))(display (hong 0))(display (choi 0))(display (hong 10))(display (choi 5))
                 */
                List<object> parsed = LispParser.Parse(@"(display ""--- Apply and Function as Value"")(define display2 display)(apply display2 (quote (""Hello"")))(display2 ""Hello"")(display ""--- Lexical Scope (let)"")(let ((+ -) (- +))     (display (+ 4 3))     (display (- 4 3))     (setf! - *)     (display (- 4 3)))(display (+ 4 3))(display (- 4 3))(display ""--- Lambda"")(display ((lambda (a b) (+ a b)) 1 2))(display ""--- Higher-Order Function"")(display ((lambda (op a b) (op a (op a b))) * 3 4))(display (((lambda () (lambda (a) (* a a)))) 3))(display ""--- Lexical Scope (lambda)"")(display (((lambda (a) (lambda (b) (/ a b))) 9) 3))
(define account        (lambda (balance)                (lambda (incr)                        (setf! balance (+ balance incr)))))(define hong (account 10))(define choi (account 5))(display (hong 0))(display (choi 0))(display (hong 10))(display (choi 5))");
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
