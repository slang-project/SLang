using System;
using System.Collections.Generic;
using System.IO;

namespace SLang
{
    public class Reader
    {
        /// <summary>
        /// 
        /// </summary>
        private Message messages;

        /// <summary>
        /// 
        /// </summary>
        private string sourcePath;

        /// <summary>
        /// 
        /// </summary>
        private string sourceCode;

        /// <summary>
        /// 
        /// </summary>
        public bool wasOpen { get; private set; }

        #region Constructors

        /// <summary>
        /// Constructor. Reads source text from the disk file
        /// performing some trivial transormations on it.
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="path"></param>
        /// <param name="options"></param>
        public Reader(Message messages, string path, Options options = null)
        {
            if ( options == null ) options = new Options();
            if ( messages == null ) messages = new Message(options);
            this.messages = messages;

            sourcePath = path;

            // Reading from the file to the string
            // removing system-specific sequences like \n\r...
            StreamReader reader;
            bool toClose = false;
            try
            {
                reader = new StreamReader(sourcePath);
            }
            catch (ArgumentNullException) // Path is null
            {
                messages.error(null,"null-path"); return;
            }
            catch (ArgumentException) // Path is empty
            {
                messages.error(null,"empty-path"); return;
            }
            catch (FileNotFoundException) // Cannot find the path specified
            {
                messages.error(null,"no-file",sourcePath); return;
            }
            catch (DirectoryNotFoundException) // Incorrect path
            {
                messages.error(null,"wrong-path",sourcePath); return;
            }
            catch (IOException) // Illegal path syntax
            {
                messages.error(null,"wrong-path-stx",sourcePath); return;
            }
            toClose = true;
            sourceCode = null;
            try
            {
                sourceCode = reader.ReadToEnd();
            }
            catch (OutOfMemoryException) // Not enough memory
            {
                messages.error(null,"no-memory",sourcePath); return;
            }
            catch (IOException) // IO error
            {
                messages.error(null,"io-error",sourcePath); return;
            }
            // We remove all pairs \r\n with single \n character,
            // and all \r's with \n's.
            // The terminating zero character will indicate end-of-source.
            sourceCode = sourceCode.Replace("\r\n", "\n").Replace("\r", "\n") + "\0";
            if ( toClose ) { reader.Close(); reader.Dispose(); }

            wasOpen = true;
            currentPos = 0;

            lineNo = 1;
            posNo = 1;

            forgetChar();
        }

        /// <summary>
        /// Constructor. Takes the list of source lines as the parameter
        /// and transforms it to the standard internal representation - 
        /// as a single string of lines separated by single <c>'\n'</c> characters.
        /// </summary>
        /// <param name="lines">The list of lines comprising the source code to be compiled.</param>
        public Reader(List<string> lines)
        {
            wasOpen = true;
            currentPos = 0;

            lineNo = 1;
            posNo = 1;

            forgetChar();

            sourcePath = "List of code lines specified explicitly";
            sourceCode = "";
            foreach ( string s in lines )
                sourceCode += s + "\n";
            sourceCode += '\0';
        }

        /// <summary>
        /// Constructor. Takes the single string as the parameter.
        /// It is assumed that the string represents rh source code
        /// to be compiled.
        /// </summary>
        /// <param name="source"></param>
        public Reader(string source)
        {
            sourcePath = "The single source line specified explicitly";
            sourceCode = source + '\0';

            wasOpen = true;
            currentPos = 0;

            lineNo = 1;
            posNo = 1;

            forgetChar();
        }

        #endregion

        //   public void adjust(int start, int end = 0)
        //   {
        //       if (!wasOpen) return; // no source by some reason
        //       if ( end > 0 )
        //       {
        //           if ( end <= start ) return; // illegal argument(s)
        //       }
        //       else // end==0
        //           end = codeLength;
        //       codeLength = end - start + 1;
        //       currentPos = start;
        //   }

        /// <summary>
        /// Position of the source character string that is not read yet.
        /// Starts from 0 and enumerates character across the whole string.
        /// </summary>
        private int currentPos;

        #region Logical position within the source text

        /// <summary>
        /// Current logical line number (counting via '\n's).
        /// Starts from 1.
        /// </summary>
        private int lineNo;

        /// <summary>
        /// Current position within the current line (from one '\n' to the next one).
        /// The very first position within the line is 1.
        /// </summary>
        private int posNo;

        public Position currPos()
        {
            return new Position(lineNo,posNo);
        }

        #endregion

        private char getNextChar()
        {
            char result = sourceCode[currentPos];

            // Zero indicates end-of-source.
            // This character was appended to the source line
            // while reading it from the file.
            if ( result == '\0' ) return '\0';

            if ( result == '\n' ) { lineNo++; posNo = 1; } else posNo++;
            currentPos++;
            return result;
        }

        #region Simple mechanism for walking through the source

        /// <summary>
        /// Current character that is taken from the source string,
        /// if it is not forgotten yet. If it is, then it contains
        /// the special code <c>'\x1'</c>.
        /// </summary>
        private char currentChar = '\x1';

        /// <summary>
        /// The function returns next character from the source string.
        /// The function does not "forget" it after returning, so the next
        /// call to the function will return the same character.
        /// </summary>
        /// <returns></returns>
        public char getChar()
        {
            if (currentChar == '\x1') currentChar = getNextChar();
            return currentChar;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool checkNextFor(char n)
        {
            return sourceCode[currentPos] == n;
        }

        public char tryNext()
        {
            return sourceCode[currentPos];
        }

        /// <summary>
        /// The function "forgets" the last character taken from the
        /// source string, so the next call to <c>getChar</c> will
        /// return a new character.
        /// </summary>
        public void forgetChar() { currentChar = '\x1'; }

        #endregion
    }
}
