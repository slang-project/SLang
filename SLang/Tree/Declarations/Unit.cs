using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SLang
{
    /// <summary>
    /// 
    /// <syntax><code>
    /// Объявление-контейнера
    ///         :   [ Спецификатор-контейнера ] unit Имя-контейнера [ FormalGenerics ]
    ///                 { Директива-контейнера }
    ///             is 
    ///                 Тело-контейнера
    ///                 [ invariant Список-предикатов ]
    ///             end
    ///             
    /// Спецификатор-контейнера
    ///         : ref | val | concurrent | abstract
    ///         
    /// Директива-контейнера
    ///         : Директива-наследования
    ///         | Директива-использования
    ///         
    /// Директива-наследования
    ///         : extend Базовый-контейнер { , Базовый-контейнер }
    ///         
    /// Базовый-контейнер
    ///         : [ ~ ] UnitTypeName
    ///         
    /// Тело-контейнера
    ///         : { Объявление }
    /// </code></syntax>
    /// </summary>
    public class UNIT : DECLARATION, iSCOPE
    {
        #region iSCOPE interface

    //  public iSCOPE enclosing
    //  {
    //      get { return this.parent as iSCOPE; }
    //      set { this.parent = value as ENTITY; }
    //  }

        public ENTITY self { get { return this; } }

        public void add(ENTITY d) { declarations.Add(d as DECLARATION); }

        public DECLARATION find_in_scope(string id)
        {
            foreach ( DECLARATION d in declarations)
            {
                if ( d.name == null ) continue;
                if ( d.name.identifier == id ) return d;
            }
            foreach ( DECLARATION d in this.generics )
            {
                if ( d.name == null ) continue;
                if ( d.name.identifier == id ) return d;
            }
            return null;
        }

        #endregion

        #region Structure

        public bool RefVal { get; private set; }
        public bool Abstract { get; private set; }
        public bool Concurrent { get; private set; }

        public IDENTIFIER alias { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <syntax>
        /// FormalGenerics : "[" FormalGeneric { "," FormalGeneric } "]"
        /// </syntax>
        public List<FORMAL_GENERIC> generics;
        public void add(FORMAL_GENERIC g) { generics.Add(g); g.parent = this; }

        public bool isGeneric() { return generics.Count > 0; }
        public int numGenerics() { return generics.Count; }

        /// <summary>
        /// 
        /// </summary>
        /// <syntax>
        /// InheritDirective : extend Parent { "," Parent }
        /// 
        /// Parent  : [ "~" ] UnitType 
        /// </syntax>
        public List<PARENT> inherits;
        public void add(PARENT i) { inherits.Add(i); i.parent = this; }

        /// <summary>
        /// 
        /// </summary>
        public List<USE> uses;
        public void add(USE u) { uses.Add(u); u.parent = this; }

        /// <summary>
        /// 
        /// </summary>
        public List<DECLARATION> declarations;

        /// <summary>
        /// 
        /// </summary>
        public List<EXPRESSION> invariants;
        public void add(EXPRESSION e) { invariants.Add(e); e.parent = this; }

        #endregion

        #region Constructors

        public UNIT(string n, bool rv, bool a = false, bool c = false) : base(n)
        {
            RefVal = rv;
            Abstract = a;
            Concurrent = c;

            generics = new List<FORMAL_GENERIC>();
            inherits = new List<PARENT>();
            uses = new List<USE>();
            declarations = new List<DECLARATION>();
            invariants = new List<EXPRESSION>();
        }

        #endregion

        #region Parser

        private static TokenCode getUnitKeyword()
        {
            Token token = get();
            TokenCode code = token.code;
            forget();
            return code;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <syntax>
        /// Объявление-контейнера
        ///         :   [ Спецификатор-контейнера ] unit Имя-контейнера [ FormalGenerics ]
        ///                 { Директива-контейнера }
        ///             is 
        ///                 Тело-контейнера
        ///                 [ invariant Список-предикатов ]
        ///             end
        ///             
        /// Спецификатор-контейнера
        ///         : ref | val | concurrent | abstract
        ///         
        /// Имя-контейнера
        ///         : Составное-имя
        ///         
        /// Директива-контейнера
        ///         : Директива-наследования
        ///         | Директива-использования
        ///         
        /// Директива-наследования
        ///         : extend Базовый-контейнер { , Базовый-контейнер }
        ///         
        /// Базовый-контейнер
        ///         : [ ~ ] UnitTypeName
        ///         
        /// Тело-контейнера
        ///         : { Объявление }
        /// </syntax>
        /// <returns></returns>
        public static void parse(bool hidden, bool final, iSCOPE context)
        {
            Debug.Indent();
            Debug.WriteLine("Entering UNIT.parse");

            bool ref_val = false;     // unit is reference by default
            bool abstr = false;
            bool concurrent = false;

            UNIT unit = null;

            Token token = get();
            Token begin = token;

            TokenCode code = TokenCode.Unit;

            switch ( token.code )
            {
                case TokenCode.Ref:
                    ref_val = true;
                    forget();
                    code = getUnitKeyword();
                    break;

                case TokenCode.Val:
                    ref_val = false;
                    forget();
                    code = getUnitKeyword();
                    break;

                case TokenCode.Abstract:
                    abstr = true;
                    forget();
                    code = getUnitKeyword();
                    break;

                case TokenCode.Concurrent:
                    concurrent = true;
                    forget();
                    code = getUnitKeyword();
                    break;

                case TokenCode.Unit:
                    code = TokenCode.Unit;
                    forget();
                    break;
            }

            // 1. Unit header

            token = expect(TokenCode.Identifier);
            Token compoundName = IDENTIFIER.parseCompoundName(token);

            if ( code == TokenCode.Package )
            {
                if ( !ENTITY.options.optConfig )
                {
                    warning(token,"no-config");
                    unit = new UNIT(compoundName.image,ref_val,abstr,concurrent);
                }
                else
                    unit = new PACKAGE(compoundName.image,ref_val,abstr,concurrent);
            }
            else
            {
                unit = new UNIT(compoundName.image,ref_val,abstr,concurrent);
            }

            Debug.WriteLine("======================" + compoundName.image);

            unit.parent = context.self;
            unit.setSpecs(hidden,final);
            Context.enter(unit);

            // 2. Generic parameters

            token = get();
            if ( token.code == TokenCode.LBracket )
            {
                forget();
                while ( true )
                {
                    var generic = FORMAL_GENERIC.parse(unit);
                    unit.add(generic);

                    token = get();
                    switch ( token.code )
                    {
                        case TokenCode.Comma:
                        case TokenCode.Semicolon: forget(); continue;
                        case TokenCode.RBracket:  forget(); goto Finish;
                        default: { /* Syntax error */ break; }
                    }
                }
              Finish:
                ;
            }

            // Possible unit alias

            token = get();
            if ( token.code == TokenCode.Alias )
            {
                forget();
                token = expect(TokenCode.Identifier);
                unit.alias = new IDENTIFIER(token);
            }

            // 3. Unit directives: inheritance

            token = get();
            if (token.code == TokenCode.Extend)
            {
                forget();
                while ( true )
                {
                    PARENT parent = PARENT.parse(unit);
                    if (parent == null) {  /* Syntax error */ break; }
                    unit.add(parent);

                    token = get();
                    switch (token.code)
                    {
                        case TokenCode.Comma:
                        case TokenCode.Semicolon:
                        case TokenCode.EOL:
                            forget();
                            continue;
                        default:
                            goto Use;
                    }
                }
            }

            // 4. Unit directives: use
        Use:
            token = get();
            if (token.code == TokenCode.Use)
            {
                forget();
                USE.parse(unit);
            }

            // 5. Optional 'is'

            token = get();
            if (token.code == TokenCode.Is) forget();

            // 6. Unit body

            while ( true )
            {
                bool result = DECLARATION.parse(unit);
                if ( !result ) break;

                token = get();
                switch ( token.code )
                {
                    case TokenCode.Semicolon:
                    case TokenCode.EOL:
                        forget(); token = get();
                        continue;
                    default:
                        if (wasEOL) continue;
                        break;
                }
            }

            // 7. Unit invariants

            token = get();
            if ( token.code == TokenCode.Invariant )
            {
                forget();
                while ( true )
                {
                    EXPRESSION invariant = EXPRESSION.parse(null,unit);
                    if ( invariant == null ) { /* Syntax error */ break; }
                    unit.add(invariant);

                    token = get();
                    switch ( token.code )
                    {
                        case TokenCode.Comma:
                        case TokenCode.Semicolon:
                        case TokenCode.EOL:
                            forget();
                            continue;
                        case TokenCode.End:
                            goto Out;
                        default:
                            continue;
                    }
                }
             Out:
                ;
            }
            token = expect(TokenCode.End);
            Context.exit();

            unit.setSpan(begin, token);
            context.add(unit);

            Debug.WriteLine("Exiting UNIT.parse");
            Debug.Unindent();
        }

        #endregion

        #region Verification

        public override bool check()
        {
            foreach ( FORMAL_GENERIC g in generics ) if ( !g.check() ) return false;
            foreach ( PARENT p in inherits )  if ( !p.check() ) return false;
            foreach ( DECLARATION d in declarations ) if ( !d.check() ) return false;
            foreach ( EXPRESSION e in invariants ) if ( !e.check() ) return false;
            return true;
        }

        public override bool verify()
        {
            if ( Abstract && Concurrent ) { error(null,"abstr-concurr"); return false; }

            foreach ( FORMAL_GENERIC g in generics ) if ( !g.verify() ) return false;
            foreach ( PARENT p in inherits )  if ( !p.verify() ) return false;
            foreach ( DECLARATION d in declarations ) if ( !d.verify() ) return false;
            foreach ( EXPRESSION e in invariants ) if ( !e.verify() ) return false;

            // Something else to verify...

            return true;
        }

        #endregion

        #region Code generation

        public override bool generate() { return true; }

        #endregion

        #region Reporting

        public override void report (int sh )
        {
            reportInternal(sh,"UNIT");
        }

        protected void reportInternal ( int sh, string title )
        {
            string common = commonAttrs();
            string a =  Concurrent ? "CONCURRENT " : (Abstract ? "ABSTRACT " : ( RefVal ? "REF " : "VAL "));
            string r = common + shift(sh) + a + (isGeneric() ? "GENERIC " : "") + title + " " + name.identifier;
            if ( alias != null ) r += " ALIAS " + alias.identifier;
            System.Console.WriteLine(r);

            if ( generics.Count > 0 )
            {
                System.Console.WriteLine(shift(common.Length+sh)+"GENERIC PARAMETERS");
                foreach (FORMAL_GENERIC g in generics ) g.report(sh + constant);
            }
            if ( inherits.Count > 0 )
            {
                System.Console.WriteLine(shift(common.Length + sh) + "BASE CLASSES");
                foreach (PARENT p in inherits) p.report(sh + constant);
            }
            if ( uses.Count > 0 )
            {
                System.Console.WriteLine(shift(common.Length+sh)+"USES");
                foreach (USE u in uses) u.report(sh + constant);
            }

            System.Console.WriteLine(shift(common.Length+sh)+"MEMBERS");
            foreach (DECLARATION d in declarations) d.report(sh + constant);
            
            if ( invariants.Count > 0 )
            {
                System.Console.WriteLine(shift(common.Length+sh)+"INVARIANTS");
                foreach (EXPRESSION e in invariants) e.report(sh + constant);
            }
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <syntax>
    /// Parent : [ "~" ] UnitTypeName
    /// </syntax>
    public class PARENT : ENTITY
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public bool conformance { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public TYPE unit_type { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public PARENT(bool c) : base() { conformance = c; }

        #endregion

        #region Parser

        /// <summary>
        /// 
        /// </summary>
        /// <syntax>
        /// Parent : [ "~" ] UnitTypeName 
        /// </syntax>
        /// <returns></returns>
        public static PARENT parse(iSCOPE context)
        {
            bool c = false;
            Token token = get();
            if ( token.code == TokenCode.Tilde )
            {
                c = true; forget();
            }

            PARENT parent = new PARENT(c);

            token = get();
            if ( token.code == TokenCode.LParen )
            {
                TYPE tuple = TUPLE_TYPE.parse(context);
                parent.unit_type = tuple;
                parent.setSpan(token.span,tuple.span);
                tuple.parent = parent;
            }
            else
            {
                UNIT_REF unit_type = UNIT_REF.parse(null,false,context);
                parent.unit_type = unit_type;
                parent.setSpan(token.span,unit_type.span);
                unit_type.parent = parent;
            }
            return parent;
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

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string r = this.commonAttrs() + shift(sh) + (conformance ? "NONCONF " : "") + "EXTEND";
            System.Console.WriteLine(r);

            unit_type.report(sh + constant);
        }

        #endregion

    }

    public class PACKAGE : UNIT
    {
        #region Constructors

        public PACKAGE(string n, bool rv, bool a = false, bool c = false) : base(n,rv,a,c)
        {
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            base.reportInternal(sh,"PACKAGE");
        }

        #endregion

    }

}
