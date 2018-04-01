using System;
using System.Collections.Generic;

namespace SLang
{
    public class Options
    {
        /// <summary>
        /// Call/exit tracing
        /// </summary>
        public bool optDebug { get; private set; }

        /// <summary>
        /// Print compiler title & version
        /// </summary>
        public bool optPrintVersion { get; private set; }

        /// <summary>
        /// Print parser's AST
        /// </summary>
        public bool optDumpAST { get; private set; }

        /// <summary>
        /// JSON IR generation
        /// </summary>
        public bool optToJSON { get; private set; }

        /// <summary>
        /// Print JSON IR
        /// </summary>
        public bool optDumpJSON { get; private set; }

        /// <summary>
        /// Generate both JSON & final code
        /// </summary>
        public bool optGenerate { get; private set; }

        /// <summary>
        /// Config mode: compilation unit is actually a configuration file
        /// </summary>
        public bool optConfig { get; private set; }

        /// <summary>
        /// ??
        /// </summary>
        public bool dump_source { get; private set; }

        /// <summary>
        /// Maximal number of errors: after exceeding the number
        /// compilation process will be terminated.
        /// </summary>
        public int optMaxErrors { get; private set; }

        /// <summary>
        /// Strict mode: treat warning as errors.
        /// </summary>
        public bool optWarningsAsErrors { get; private set; }

        /// <summary>
        /// Sets default option values.
        /// </summary>
        public Options()
        {
            optDebug = true; // false;
            optPrintVersion = true;
            optDumpAST = false;
            optToJSON = true;
            optDumpJSON = false;
            optGenerate = true;
            optConfig = true; //  false;
         // dump_source
            optMaxErrors = 50;
            optWarningsAsErrors = false;
        }

        /// <summary>
        /// Sets compiler options specified in the command line
        /// </summary>
        /// <param name="strOpt"></param>
        public Options(Dictionary<string,string> strOpt)
        {
            Options options = new Options();

            foreach ( KeyValuePair<string,string> opt in strOpt )
            {
                string value = opt.Value;
                bool boolVal = ( value == "" || value == "yes" || opt.Value == "true" ) ? true : false;

                switch ( opt.Key )
                {
                    case "v":
                    case "version":
                        optPrintVersion = boolVal;
                        break;

                    case "d":
                    case "debug" :
                        optDebug = boolVal;
                        break;

                    case "ast":
                        optDumpAST = boolVal;
                        break;

                    case "json":
                        optDumpJSON = boolVal;
                        break;

                    case "g":
                    case "gen":
                    case "generate":
                        optGenerate = boolVal;
                        break;

                    case "m":
                    case "max":
                        int result;
                        bool ok = int.TryParse(value,out result);
                        if ( ok ) optMaxErrors = result;
                        break;

                    case "w":
                        optWarningsAsErrors = boolVal;
                        break;

                    case "c":
                    case "config":
                        optConfig = boolVal;
                        break;

                    default:
                        // a wrong option; now just ignore...
                        break;
                }
            }

         // options.max_errors

        }

        public static void QuickGuide()
        {
            System.Console.WriteLine("\"SLang Compiler.exe\" <Options> <SourceFilePath>");
            System.Console.WriteLine("");
            System.Console.WriteLine("Option: /opt-name");
            System.Console.WriteLine("Option: /opt-name:yes");
            System.Console.WriteLine("Option: /opt-name:true");
            System.Console.WriteLine("");
            System.Console.WriteLine("Options:");
            System.Console.WriteLine("/opt-name");
            System.Console.WriteLine("");
            System.Console.WriteLine("/v");
            System.Console.WriteLine("/version     Output compiler title and version number (NO by default)");
            System.Console.WriteLine("/d");
            System.Console.WriteLine("/debug       Tracing compiler function calls & exits (NO by default)");
            System.Console.WriteLine("/ast         Dump the whole syntax tree after successful parsing (NO by default)");
            System.Console.WriteLine("/json        Generate IR in JSON format (YES by default)");
            System.Console.WriteLine("/g");
            System.Console.WriteLine("/gen");
            System.Console.WriteLine("/generate    Generate target code (YES by default)");
            System.Console.WriteLine("/m           Set the maximal number of error messages (not implemented)");
            System.Console.WriteLine("/max");
            System.Console.WriteLine("/w           Treat warnings as errors (not implemented)");
            System.Console.WriteLine("/c           Configuration mode ('package' keyword etc.) (NO by default)");
        }
    }
}
