using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SLang
{
    /// <summary>
    /// 
    /// <syntax>
    /// Единица-компиляции
    ///    : { Директива-использования } { Программная-единица }
    ///
    /// Программная-единица
    ///    : Анонимная-подпрограмма | Контейнер | Подпрограмма
    /// </syntax>
    /// </summary>
    public class COMPILATION : ENTITY, iSCOPE
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public List<USE> uses { get; private set; }
        public void add(USE u) { uses.Add(u); }

        /// <summary>
        /// 
        /// </summary>
        public List<DECLARATION> units_and_standalones { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ROUTINE anonymous { get; private set; }

        #endregion

        #region Constructors

        public COMPILATION() : base()
        {
            uses = new List<USE>();
            units_and_standalones = new List<DECLARATION>();

            // Creating the top-most (anonymous) routine
            Token anon = new Token(null,TokenCode.Identifier,"$anonymous",new Category());
            anonymous = new ROUTINE(0,false,anon);
            units_and_standalones.Add(anonymous);
            anonymous.parent = this;
        }

        #endregion

        #region iSCOPE interface

        public ENTITY self { get { return this; } }

     // public iSCOPE enclosing { get { return null; } set { } }

        public void add(ENTITY d) { units_and_standalones.Add(d as DECLARATION); }

        public DECLARATION find_in_scope ( string id )
        {
            foreach ( DECLARATION d in units_and_standalones )
            {
                if ( d.name != null && d.name.identifier == id ) return d;
            }
            return null;
        }

        public void show(int sh)
        {
            string indentation = "";
            for (int i=1; i<=sh; i++) indentation += " ";
            System.Console.WriteLine("{0}{1}",indentation,"COMPILATION");
            foreach(DECLARATION d in units_and_standalones)
                System.Console.WriteLine("    {0}{1}",indentation,d.name.identifier);
        }

        #endregion

        #region Parser

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static COMPILATION parse()
        {
            Debug.Indent();
            Debug.WriteLine("Entering COMPILATION.parse");

            COMPILATION compilation = new COMPILATION();
            Context.enter(compilation);

            Token start = get();
            Token startAnon = null;
            Token endAnon = null;

            int pure_safe = 0;

            while ( true )
            {
                Token token = get();
                Token begin = token;
                switch (token.code)
                {
                    case TokenCode.Final:
                        forget();
                        token = get();
                        switch ( token.code )
                        {
                            case TokenCode.Unit:
                            case TokenCode.Package:
                            case TokenCode.Ref:
                            case TokenCode.Val:
                            case TokenCode.Concurrent:
                                // Don't forget()
                                UNIT.parse(false,true,false,compilation);
                                break;
                            case TokenCode.Abstract:
                                forget();
                                UNIT.parse(false,true,true,compilation);
                                break;
                            case TokenCode.Safe:
                            case TokenCode.Pure:
                            case TokenCode.Routine:
                                forget();
                                switch ( token.code )
                                {
                                    case TokenCode.Pure: pure_safe = 1; break;
                                    case TokenCode.Safe: pure_safe = 2; break;
                                }
                                ROUTINE.parse(null,false,true,false,pure_safe,compilation);
                                pure_safe = 0;
                                break;
                        }
                        break;

                    case TokenCode.Ref:
                    case TokenCode.Val:
                    case TokenCode.Concurrent:
                    case TokenCode.Unit:
                    case TokenCode.Package:
                        UNIT.parse(false,false,false,compilation);
                        break;

                    case TokenCode.Abstract:
                        forget();
                        UNIT.parse(false,false,true,compilation);
                        break;

                    case TokenCode.Use:
                        USE.parse(compilation);
                        break;

                    case TokenCode.Routine:
                    case TokenCode.Safe:
                    case TokenCode.Pure:
                        forget();
                        switch ( token.code )
                        {
                            case TokenCode.Pure: pure_safe = 1; break;
                            case TokenCode.Safe: pure_safe = 2; break;
                        }
                        ROUTINE.parse(null,false,false,false,pure_safe,compilation);
                        pure_safe = 0;
                        break;

                    case TokenCode.EOS:
                        goto Finish;

                    case TokenCode.EOL:
                        forget();
                        break;

                    default:
                        // A call/assignment statement, or an error
                        if ( startAnon == null ) startAnon = get();
                        bool result = STATEMENT.parse(compilation.anonymous.routineBody,
                                                      TokenCode.EOS,TokenCode.ERROR,TokenCode.ERROR);
                        if ( !result )
                        {
                            // There was not a statement:
                            // apparently, this is a syntax error
                            goto Finish;
                        }
                        endAnon = get();
                        break;
                }
            }
         Finish:
            compilation.setSpan(start,get());
            if ( startAnon != null && endAnon != null )
                compilation.anonymous.setSpan(startAnon,endAnon);

            Debug.WriteLine("Exiting COMPILATION.parse");
            Debug.Unindent();

            Context.exit();
            return compilation;
        }

        #endregion

        #region Verification

        public override bool check()
        {
            throw new NotImplementedException();
        }

        public override bool verify()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Code generation

        public override bool generate() { return true; }

        public override string ToJSON()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string r = commonAttrs() + shift(sh) + "COMPILATION";
            System.Console.WriteLine(r);

            foreach (USE u in uses) u.report(sh + constant);
            foreach (ENTITY e in units_and_standalones) e.report(sh + constant);
        }

        #endregion
    }
}
