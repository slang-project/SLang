using System;
using System.Collections.Generic;
using System.Diagnostics;

using SLang;
using SLang.Service;

namespace SLangCompiler
{
    public class SLang
    {
        static void Main(string[] args)
        {
            if ( args.Length == 0 )
            {
                // Output the quick guide
                Options.QuickGuide();
                goto NoActions;
            }

            // Parsing command line arguments
            Dictionary<string,string> opts = CommandLineArgs.parse(args);
            // The first queue element should be the source file name/path
            string fileName = opts["file-name"];

            // Setting compilation options
            Options options = new Options(opts);
            // Opening message pool
            Message messagePool = new Message(options);

            // Output compiler title & version
            if ( options.optPrintVersion )
                messagePool.info("compiler-title", "0.0.0.1 prototype");

            // Opening the source file
            if ( fileName == "" )
            {
                messagePool.error(null, "empty-path");
                goto Finish;
            }

            if ( !System.IO.File.Exists(fileName) )
            {
                messagePool.error(null, "no-file", fileName);
                goto Finish;
            }

            // Initializing parsing process
            Reader reader = new Reader((Message)null,fileName);
            Tokenizer tokenizer = new Tokenizer(reader,options,messagePool);
            ENTITY.init(tokenizer,0,messagePool,options);
            if ( messagePool.numErrors > 0 ) goto Finish;

            // Phase 1: parsing
            // ================

            COMPILATION compilation = null;
            try
            {
                try
                {
                    compilation = COMPILATION.parse();
                    if ( options.optDumpAST )
                        compilation.report(4);
                }
                catch ( Exception )
                {
                    throw new TerminateSLangCompiler(ENTITY.current);
                }
            }
            catch ( TerminateSLangCompiler exc )
            {
                Position pos = (exc == null) ? null : exc.position;
                messagePool.error(pos, "system-bug");
                goto Finish;
            }

            // TODO: semantic analysis call?

            // Phase 2: code generation
            // ===============================

            if ( options.optDumpJSON )
            {
                compilation.ToJSON().WriteToFile(fileName + ".json");
            }

            if ( !options.optGenerate ) goto Finish;

        Finish:
            messagePool.info("end-compilation");
        NoActions:
            ;
        }
    }

    public class CommandLineArgs
    {
        public static Dictionary<string,string> parse(string[] args)
        {
            Dictionary<string, string> CommandLineOptions = new Dictionary<string, string>
            {
                ["file-name"] = "" // reserve the position for the source file name
            };

            foreach (string arg in args)
            {
                int colon;
                string key;
                string par;

                // If no leading '/' then this should be a source file name
                if ( arg[0] != '/' )
                {
                    CommandLineOptions["file-name"] = arg;
                    continue;
                }

                // Take parameter kind and parameter value.
                // Assume that compiler parameters are of the following form:
                //
                //     /key:arg
                //
                // where 'key' is the parameter kind (out, xml, main, ref)
                // and 'arg' is its value. Values depend on the parameter kind.

                colon = arg.IndexOf(":",0,arg.Length);
                if ( colon == -1 )
                {
                    key = arg.Substring(1);
                    par = "";
                }
                else
                {
                    key = arg.Substring(1,colon-1);
                    par = arg.Substring(colon+1);
                }
                CommandLineOptions[key.ToLower()] = par;
            }
            return CommandLineOptions;
        }
    }
}
