using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace SLang
{
    public class Message
    {
        private Options options;

        public int numErrors = 0;

        public Message(Options o) { options = o; }

        private Dictionary<string,string> patterns = new Dictionary<string,string>()
        {
            // Info messages
            { "compiler-title", "SLang Compiler version {0}" },
            { "end-compilation","Compilation completed" },
            { "max-errs",       "Maximal number of error exceeded: {0}" },
            { "system-bug",     "Internal compiler error" },

            // Option errors
            { "null-path",      "source path is null" },
            { "empty-path",     "source path is empty" },
            { "no-file",        "cannot find the source path specified: {0}" },
            { "wrong-path",     "incorrect path specified: {0}" },
            { "wrong-path-stx", "illegal source path syntax; {0}" },
            { "no-memory",      "not enough memory for reading from {0}" },
            { "io-error",       "i/o error" },

            // Lexical errors

            // Syntax errors
            { "syntax-error",   "syntax error: {0}" },
            { "not-found",      "{0} expected but not found" },

            // Semantic errors
            { "illegal-spec",  "{0}: illegal specifier(s) for {1}; ignored" },
            { "no-config",     "use /config option to compile configuration file" },

        };

        private List<string> messagePool = new List<string>();
        public void dump()
        {
            foreach ( string msg in messagePool )
                System.Console.WriteLine(msg);
        }

        private void message(Position position, string kind, string title, params object[] args)
        {
            string msg = position != null ? position.ToString() + " " : "";
            string messageBody = "";
            bool res = patterns.TryGetValue(title,out messageBody);
            if (!res)
                msg += " internal error: no message with title '" + title + "'";
            else
            {
                if ( kind != null && kind != "" )
                    msg += kind + ": " + String.Format(messageBody,args);
                else
                    msg += String.Format(messageBody,args);
            }
            messagePool.Add(msg);
            // In debug mode we issue the message immediately after
            // encountering an error.
            Debug.WriteLine(msg);
        }

        public void warning(Position position, string title, params object[] args)
        {
            message(position,"warning",title,args);
        }

        public void error(Position position,string title, params object[] args)
        {
            message(position,"error",title,args);
            numErrors++;

            if ( numErrors >= options.optMaxErrors )
            {
                info("max-errs",options.optMaxErrors);
                throw new TerminateSLangCompiler(position);
            }
        }

        public void info(string title, params object[] args)
        {
            message(null,null,title,args);
        }
    }
}
