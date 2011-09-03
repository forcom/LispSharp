using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lisp {
    class Program {
        public class Lisp {
            LispParser parser = new LispParser ( );
            public void Start ( ) {
                List<object> code;
                code = parser.Parse ( @"(define (append list1 list2)
    (if (null? list1)
        list2
        (cons (car list1) (append (cdr list1) list2))))

(define (looknsayseq x seq)
    (display seq)
    (newline)
    (cond ((> x 1) (looknsayseq (- x 1) (make-seq seq)))))

(define (make-seq cur-seq)
    (make-seq-iter (car cur-seq) 1 (cdr cur-seq)))

(define (make-seq-iter num count seq)
	(cond ((null? seq) (list num count))
		  ((= (car seq) num) (make-seq-iter num (+ count 1) (cdr seq)))
		  (else (append (list num count) (make-seq-iter (car seq) 1 (cdr seq))))))

(looknsayseq 10 (list 1))
" );

                Console.WriteLine ( LispParser.TestParsed ( code ) );
                Console.WriteLine ( );
                Console.WriteLine ( LispParser.TestParsedType ( code ) );
                while ( true ) {
                }
            }
        }

        static void Main ( string [ ] args ) {
            new Lisp ( ).Start ( );
        }
    }
}
