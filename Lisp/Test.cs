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
        public bool TestParser(string testFile = "Test/Parser/test00.in", string resultFile = "Test/Parser/test00.out", bool ConsoleWrite = false)
        {
            System.IO.StreamReader fr = new System.IO.StreamReader(testFile);
            string codeString = fr.ReadToEnd();
            fr.Close();
            fr = new System.IO.StreamReader(resultFile);
            string resultString = fr.ReadToEnd();
            fr.Close();

            List<object> code;
            try
            {
                code = LispParser.Parse(codeString);
            }
            catch (Exception e)
            {
                return resultString != e.Message;
            }

            string[] result = new string[] { LispParser.TestParsed(code), "", LispParser.TestParsedType(code) };
            string resultCombined = string.Join("\n", result);

            if (ConsoleWrite)
            {
                Debug.WriteLine(resultCombined);
            }

            bool testResult = resultString != resultCombined;

            return testResult;
        }

        /// <summary>
        /// Start the parser test.
        /// </summary>
        public void StartTest()
        {
            for (int i = 0; i <= 4; ++i)
            {
                string inName = "./Test/Parser/test" + i.ToString("00") + ".in";
                string outName = "./Test/Parser/test" + i.ToString("00") + ".out";
                Debug.WriteLineIf(TestParser(inName, outName, ConsoleWrite: false), "[ParserTest:" + (i + 1).ToString() + "] [Fail]");
            }
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
