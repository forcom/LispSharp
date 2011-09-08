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
                test.StartTest(ParserTest: true, EvaluatorTest: true);
#endif
            }

            public void Start ( ) {
                CallTest ( );
                while ( true ) {
                }
            }
        }

        static void Main ( string [ ] args ) {
            new Lisp ( ).Start ( );
        }
    }
}
