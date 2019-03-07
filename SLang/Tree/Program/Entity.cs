using System;
using System.Collections.Generic;

namespace SLang
{
    public abstract class ENTITY
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public ENTITY parent;

        /// <summary>
        /// 
        /// </summary>
        public Span span { get; private set; }
        public void setSpan(Token t) { span = t.span; }
        public void setSpan(Span s) { span = s; }
        public void setSpan(Token b, Token e) { span = new Span(b.span, e.span); }
        public void setSpan(Span b, Span e) { span = new Span(b, e); }

        /// <summary>
        /// 
        /// </summary>
        public long unique { get; private set; }
        private static long nodeCount;

        #endregion

        #region Constructors

        static ENTITY()
        {
            weAreWithinEnsure = false;
        }

        public ENTITY()
        {
            unique = ++nodeCount;
        }

        #endregion

        #region Verification

        public abstract bool check();
        public abstract bool verify();

        #endregion

        #region Code generation

        public abstract bool generate();

     // public abstract string ToJSON();

        #endregion

        #region Technical stuff

        /// <summary>
        /// 
        /// </summary>
        public static bool weAreWithinEnsure;
        
        /// <summary>
        /// 
        /// </summary>
        private static Queue<Token> buffer = new Queue<Token>();

        public static TokenCode saveTokensUntilRightParenth(Token left)
        {
            int balance = 1;
            buffer.Enqueue(left);
            
            Token token;
            while ( true )
            {
                token = get(anyway:true);
                buffer.Enqueue(token);
                forget();
                if (token.code == TokenCode.LParen)
                {
                    balance++;
                }
                else if (token.code == TokenCode.RParen)
                {
                    balance--;
                    if ( balance == 0 ) break;
                }
             // else
             //     -- Consider error case: EOS
            }
            token = get(anyway:true);
            forget();
            buffer.Enqueue(token);
            return token.code;
        }

        #endregion

        #region Service for parsing

        private static Tokenizer scanner;
        public static Token current;  // made public for getting info in case of abnormal termination

        public static bool wasEOL { get; private set; }

        public static Token get(bool anyway=false)
        {
            if ( current == null )
            {
                wasEOL = false;
                while ( true )
                {
                    if ( buffer.Count == 0 || anyway )
                        current = scanner.getNextToken();
                    else
                        current = buffer.Dequeue();

                    switch ( current.code )
                    {
                        case TokenCode.EOL: wasEOL = true; continue;
                        case TokenCode.EOS: goto Found;
                        default:
                            if ( current.category.isTrivia() ) continue;
                            goto Found;
                    }
                }
            }
          Found:
            return current;
        }

        public static void forget()
        {
            current = null;
            wasEOL = false;
        }

        public static Token expect(TokenCode code)
        {
            Token token = get();
            if ( token.code == code ) { forget(); return token; }
            // Else error: token 'code' expected but not found
            error(token,"not-found",code.ToString());
            return null;
        }

        #endregion

        #region Service for messaging

        private static Message messages;

        public static void warning(Token token, string title, params object[] args)
        {
            messages.warning(token.span.begin,title, args);
        }

        public static void error(Token token, string title, params object[] args)
        {
            messages.error(token.span.begin,title, args);
        }

        public static void info(string title, params object[] args)
        {
            messages.info(null,title, args);
        }

        #endregion

        #region Service for options

        public static Options options;

        #endregion

        #region Service initialization

        public static void init(Tokenizer s, long start = 0, Message m = null, Options o = null)
        {
            scanner = s;
            current = null;
            nodeCount = start;

            messages = m;

            if (o != null) options = o; else options = new Options();
            if (m == null) messages = new Message(options);

            wasEOL = false;
        }

        #endregion

        #region Reporting

        public string commonAttrs()
        {
            string p = (parent != null) ? parent.unique.ToString("0000") : "---";
            string s = (span != null) ? span.ToString() : "(----:----,----:----)";
            return "N:" + unique.ToString("0000") + " P:" + p + " S:" + s;
        }

        public string shift(int sh)
        {
            string r = "";
            for (int i = 1; i <= sh; i++)
                r += " ";
            return r;
        }

        public static int constant = 4;
        public abstract void report(int sh);

        #endregion
    }

    public class IDENTIFIER : ENTITY
    {
        #region Structure

        public string identifier { get; private set; }

        #endregion

        #region Creation

        public IDENTIFIER(string n) { identifier = n; }
        public IDENTIFIER(Token t) { identifier = t.image; setSpan(t); }
        #endregion

        #region Parsing

        public static Token parseCompoundName(Token firstId)
        {
            if (firstId.code != TokenCode.Identifier)
            {
                // Syntax error
                error(firstId,"not-found",TokenCode.Identifier.ToString());
                return null;
            }

            string resultId = firstId.image;
            Token token = null;
            while (true)
            {
                token = get();
                if (token.code != TokenCode.Dot) break;

                // else: compound name
                forget(); resultId += ".";
                token = expect(TokenCode.Identifier);
                if ( token == null ) break;
                resultId += token.image;
            }
            return new Token(new Span(firstId, token),
                             TokenCode.Identifier,
                             resultId,
                             new Category(CategoryCode.identifier)); 
        }

        #endregion

        public override bool check() { return true; }
        public override bool verify() { return true; }
        public override bool generate() { return true; }
        public override void report(int sh) { }
    }
}
