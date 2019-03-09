using System;
using System.Collections.Generic;

namespace SLang
{
    /// <summary>
    /// 
    /// </summary>
    /// <syntax>
    /// Try : try 
    ///          StatementList
    ///       catch ( [ Identifier : ] UnitType )
    ///          [ StatementList ]
    ///     { catch ( [ Identifier : ] UnitType ) 
    ///          [ StatementList ]
    ///     } 
    ///     [ else [ StatementsList ] ]
    ///       end
    /// </syntax>
    public class TRY : STATEMENT, iSCOPE
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public List<ENTITY> body { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public List<CATCH> handlers;
        public void add(CATCH c) { handlers.Add(c); }

        /// <summary>
        /// 
        /// </summary>
        public List<ENTITY> else_part { get; private set; }
        public void adde(ENTITY s) { else_part.Add(s); }

        #endregion

        #region Constructors

        public TRY() : base()
        {
            body = new List<ENTITY>();
            handlers = new List<CATCH>();
            else_part = new List<ENTITY>();
        }

        #endregion

        #region iSCOPE implementation

        public iSCOPE enclosing { get { return parent as iSCOPE; } set { } }
        public DECLARATION find_in_scope(string id)
        {
            foreach ( ENTITY e in body )
            {
                if ( !(e is DECLARATION) ) continue;
                if ( (e as DECLARATION).name.identifier == id ) return e as DECLARATION;
            }
            return null;
        }
        public void add(ENTITY d) { body.Add(d); }
        public ENTITY self { get { return this; } }

        public void show(int sh) { }

        #endregion

        #region Parser

        /// <summary>
        /// 
        /// </summary>
        /// <syntax>
        /// Try : try 
        ///          StatementList
        ///       catch ( [ Identifier : ] UnitType )
        ///          [ StatementList ]
        ///     { catch ( [ Identifier : ] UnitType ) 
        ///          [ StatementList ]
        ///     } 
        ///     [ else [ StatementsList ] ]
        ///       end
        /// </syntax>
        /// <returns></returns>
        public static void parse(iSCOPE context)
        {
            Token token = get();
            Token begin = token;

            if ( token.code != TokenCode.Try ) // Compiler error
            {
            }
            forget();

            TRY result = new TRY();
            Context.enter(result);

            BODY.parse(TokenCode.Catch,TokenCode.ERROR,TokenCode.ERROR,result);

            while ( true )
            {
                token = get();
                if ( token.code != TokenCode.Catch ) break;
                CATCH.parse(result);
            }
            token = get();
            if ( token.code == TokenCode.Else )
            {
                forget();
                BODY.parse(TokenCode.End,TokenCode.ERROR,TokenCode.ERROR,context);
            }
            token = get();
            if ( token.code != TokenCode.End ) // Syntax error
            { }
            forget();

            result.setSpan(begin,token);
            context.add(result);
            Context.exit();
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
/*          string a = Concurrent ? "CONCURRENT " : (Abstract ? "ABSTRACT " : (RefVal ? "REF " : "VAL "));
            string r = commonAttrs() + shift(sh) + a + (isGeneric() ? "GENERIC " : "") + "UNIT " + name;
            System.Console.WriteLine(r);

            foreach (GENERIC g in generics) g.report(sh + constant);
            foreach (PARENT p in inherits) p.report(sh + constant);
            foreach (USE u in uses) u.report(sh + constant);
            foreach (DECLARATION d in declarations) d.report(sh + constant);
            foreach (EXPRESSION e in invariants) e.report(sh + constant);
  */    }

        #endregion

    }

    public class CATCH : STATEMENT, iSCOPE
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public VARIABLE catchVar { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public UNIT_REF unit_ref { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public BODY body { get; private set; }

        #endregion

        #region Constructors

        public CATCH() : base()
        {
            body = new BODY();
        }

        #endregion

        #region iSCOPE implementation

        public iSCOPE enclosing { get { return parent as iSCOPE; } set { } }  // fix!!!

        public DECLARATION find_in_scope(string id)
        {
            if ( catchVar == null ) return null;
            if ( catchVar.name == null ) return null;
            if ( catchVar.name.identifier == id ) return catchVar;
            return null;
        }

        public void add(ENTITY d) { catchVar = d as VARIABLE; }

        public ENTITY self { get { return this; } }

        #endregion

        #region Parser

        public static void parse(iSCOPE context) // REWRITE!!
        {
            Token token = get();
            Token begin = token;
            if ( token.code != TokenCode.Catch ) // Compiler error
            { }
            forget();

            CATCH handler = new CATCH();
            Context.enter(handler);

            token = get();
            if ( token.code != TokenCode.LParen ) // Syntax error
            { }
            else
                forget();

            token = get();
            if ( token.code != TokenCode.Identifier ) // Syntax error
            { }
            else
            {
                Token id = token;
                forget();
                token = get();
                if ( token.code == TokenCode.Colon )
                {
                    forget();
                 // handler.identifier = id.image;

                    token = get();
                    if ( token.code != TokenCode.Identifier ) // Syntax error
                    { }
                    forget();
                    token = id;
                }
                UNIT_REF unit_ref = UNIT_REF.parse(null,false,context); // CHECK!!
                handler.unit_ref = unit_ref;
                unit_ref.parent = handler;
            }

            token = get();
            if ( token.code != TokenCode.RParen ) // Syntax error
            { }
            forget();

            BODY.parse(TokenCode.Catch,TokenCode.Else,TokenCode.End,handler);

            token = get(); // just to get the span...
            handler.setSpan(begin,token);

            Context.exit();
            context.add(handler);
        }

        #endregion // Parser

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
/*          string a = Concurrent ? "CONCURRENT " : (Abstract ? "ABSTRACT " : (RefVal ? "REF " : "VAL "));
            string r = commonAttrs() + shift(sh) + a + (isGeneric() ? "GENERIC " : "") + "UNIT " + name;
            System.Console.WriteLine(r);

            foreach (GENERIC g in generics) g.report(sh + constant);
            foreach (PARENT p in inherits) p.report(sh + constant);
            foreach (USE u in uses) u.report(sh + constant);
            foreach (DECLARATION d in declarations) d.report(sh + constant);
            foreach (EXPRESSION e in invariants) e.report(sh + constant);
  */    }

        #endregion
    }
}
