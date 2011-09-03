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
                code = parser.Parse ( @"(define (factorial x)
  (cond ((<= x 1.1) 1)
	(else (* x (""factorial' (- x 1))))))

(factorial 5)" );

                Console.WriteLine ( LispParser.TestParsed ( code ) );
                while ( true ) {
                }
            }
        }

        static void Main ( string [ ] args ) {
            new Lisp ( ).Start ( );
        }
    }
}
