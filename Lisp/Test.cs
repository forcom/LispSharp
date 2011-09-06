/*
 * Test.cs
 *  - Lisp# Test Module
 * 
 * Created by SeungYong, Yoon
 * 
 * Created in 2011.09.04
 * Last Modified in 2011.09.04
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Lisp {
#if DEBUG
    /// <summary>
    /// Test Lisp# Parser Module
    /// </summary>
    public class ParserTest {

        /// <summary>
        /// Test Lisp# Parser
        /// </summary>
        /// <param name="codeString">Lisp Code</param>
        /// <param name="resultString">The result should-be</param>
        /// <param name="ConsoleWrite">Dump result</param>
        /// <returns>True if pass the test, otherwise false.</returns>
        /// <remarks><c>TestParser</c> method would test a Lisp# parser.
        /// 
        /// With passed Lisp code, <c>TestParser</c> checks the result is equals to expected results.
        /// When the exception occurs, <c>TestParser</c> will check the exception message, instead.
        /// </remarks>
        /// <example>
        /// - Input Code
        /// (a (b (c) (d e)) f)
        /// - Output Result
        /// ((a (b (c) (d e)) f))\n\na : [Symbol]\nb : [Symbol]\nc : [Symbol]\nd : [Symbol]\ne : [Symbol]\nf : [Symbol]
        /// </example>
        public bool TestParser ( string codeString = "(a (b (c) (d e)) f)" , string resultString = "((a (b (c) (d e)) f))\n\na : [Symbol]\nb : [Symbol]\nc : [Symbol]\nd : [Symbol]\ne : [Symbol]\nf : [Symbol]" , bool ConsoleWrite = false ) {
            List<object> code;
            try {
                code = LispParser.Parse ( codeString );
            } catch ( Exception e ) {
                return resultString != e.Message;
            }

            string [ ] result = new string [ ] { LispParser.TestParsed ( code ) , "" , LispParser.TestParsedType ( code ) };
            string resultCombined = string.Join ( "\n" , result );

            if ( ConsoleWrite ) {
                Console.WriteLine ( resultCombined );
            }

            bool testResult = resultString != resultCombined;

            return testResult;
        }

        /// <summary>
        /// Start the parser test.
        /// </summary>
        public void StartTest ( ) {
            Debug.WriteLineIf ( TestParser ( ) , "[ParserTest:1] [Fail]" );
            Debug.WriteLineIf (
            TestParser ( "(define (append list1 list2)\n    (if (null? list1)\n        list2\n        (cons (car list1) (append (cdr list1) list2))))\n\n(define (looknsayseq x seq)\n    (display seq)\n    (newline)\n    (cond ((> x 1) (looknsayseq (- x 1) (make-seq seq)))))\n\n(define (make-seq cur-seq)\n    (make-seq-iter (car cur-seq) 1 (cdr cur-seq)))\n\n(define (make-seq-iter num count seq)\n	(cond ((null? seq) (list num count))\n		  ((= (car seq) num) (make-seq-iter num (+ count 1) (cdr seq)))\n		  (else (append (list num count) (make-seq-iter (car seq) 1 (cdr seq))))))\n\n(looknsayseq 10 (list 1))" , "((define (append list1 list2) (if (null? list1) list2 (cons (car list1) (append (cdr list1) list2)))) (define (looknsayseq x seq) (display seq) (newline) (cond ((> x 1) (looknsayseq (- x 1) (make-seq seq))))) (define (make-seq cur-seq) (make-seq-iter (car cur-seq) 1 (cdr cur-seq))) (define (make-seq-iter num count seq) (cond ((null? seq) (list num count)) ((= (car seq) num) (make-seq-iter num (+ count 1) (cdr seq))) (else (append (list num count) (make-seq-iter (car seq) 1 (cdr seq)))))) (looknsayseq 10 (list 1)))\n\ndefine : [Symbol]\nappend : [Symbol]\nlist1 : [Symbol]\nlist2 : [Symbol]\nif : [Symbol]\nnull? : [Symbol]\nlist1 : [Symbol]\nlist2 : [Symbol]\ncons : [Symbol]\ncar : [Symbol]\nlist1 : [Symbol]\nappend : [Symbol]\ncdr : [Symbol]\nlist1 : [Symbol]\nlist2 : [Symbol]\ndefine : [Symbol]\nlooknsayseq : [Symbol]\nx : [Symbol]\nseq : [Symbol]\ndisplay : [Symbol]\nseq : [Symbol]\nnewline : [Symbol]\ncond : [Symbol]\n> : [Symbol]\nx : [Symbol]\n1 : [Number]\nlooknsayseq : [Symbol]\n- : [Symbol]\nx : [Symbol]\n1 : [Number]\nmake-seq : [Symbol]\nseq : [Symbol]\ndefine : [Symbol]\nmake-seq : [Symbol]\ncur-seq : [Symbol]\nmake-seq-iter : [Symbol]\ncar : [Symbol]\ncur-seq : [Symbol]\n1 : [Number]\ncdr : [Symbol]\ncur-seq : [Symbol]\ndefine : [Symbol]\nmake-seq-iter : [Symbol]\nnum : [Symbol]\ncount : [Symbol]\nseq : [Symbol]\ncond : [Symbol]\nnull? : [Symbol]\nseq : [Symbol]\nlist : [Symbol]\nnum : [Symbol]\ncount : [Symbol]\n= : [Symbol]\ncar : [Symbol]\nseq : [Symbol]\nnum : [Symbol]\nmake-seq-iter : [Symbol]\nnum : [Symbol]\n+ : [Symbol]\ncount : [Symbol]\n1 : [Number]\ncdr : [Symbol]\nseq : [Symbol]\nelse : [Symbol]\nappend : [Symbol]\nlist : [Symbol]\nnum : [Symbol]\ncount : [Symbol]\nmake-seq-iter : [Symbol]\ncar : [Symbol]\nseq : [Symbol]\n1 : [Number]\ncdr : [Symbol]\nseq : [Symbol]\nlooknsayseq : [Symbol]\n10 : [Number]\nlist : [Symbol]\n1 : [Number]" ) , "[ParserTest:2] [Fail]" );
            Debug.WriteLineIf ( TestParser ( "(define (factorial x)\n  (cond ((<= x 1) 1)\n	(else (* x (factorial (- x 1))))))\n\n(factorial 5)" , "((define (factorial x) (cond ((<= x 1) 1) (else (* x (factorial (- x 1)))))) (factorial 5))\n\ndefine : [Symbol]\nfactorial : [Symbol]\nx : [Symbol]\ncond : [Symbol]\n<= : [Symbol]\nx : [Symbol]\n1 : [Number]\n1 : [Number]\nelse : [Symbol]\n* : [Symbol]\nx : [Symbol]\nfactorial : [Symbol]\n- : [Symbol]\nx : [Symbol]\n1 : [Number]\nfactorial : [Symbol]\n5 : [Number]" ) , "[ParserTest:3] [Fail]" );
            Debug.WriteLineIf ( TestParser ( "(a (b (c (d e)) f)" , "Source code does not reach to the end. Close parenthesises may be missed.\nCode at [1:17]\n(a (b (c (d e)) f)\n                 ^" ) , "[ParserExceptionTest:1] [Fail]" );
        }
    }

    /// <summary>
    /// Lisp# Test Module
    /// </summary>
    public class Test {

        /// <summary>
        /// Start Lisp# Module Test
        /// </summary>
        /// <param name="ParserTest">Test Lisp# Parser</param>
        public void StartTest ( bool ParserTest = true ) {
            if ( ParserTest ) {
                ParserTest parseTest = new ParserTest ( );
                parseTest.StartTest ( );
            }
        }
    }
#endif
}
