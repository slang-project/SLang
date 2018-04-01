using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SLang
{
    /// <summary>
    /// 
    /// <syntax><code>
    /// FormalGenerics
    ///       : [ Обобщенный-параметр { , Обобщенный-параметр } ]
    ///       
    /// Обобщенный-параметр
    ///       : Обобщенный–типовой-параметр [ Уточнение-типового-параметра ]
    ///       | Обобщенный-нетиповой-параметр : Тип
    ///       
    /// Уточнение-типового-параметра
    ///       : -> Тип [ "init" [ "(" [ Тип { "," Тип } ] ")" ] ] ]
    ///       
    /// Обобщенный-типовой-параметр
    ///       : Идентификатор
    ///       
    /// Обобщенный-нетиповой-параметр
    ///       : Идентификатор
    /// </code></syntax>
    /// </summary>
    public class FORMAL_GENERIC : DECLARATION
    {
        #region Strucutre

     // IDENTIFIER name -- from the base class

        #endregion

        #region Creation

        public FORMAL_GENERIC(Token t) : base(t.image) { }
        public FORMAL_GENERIC(string n) : base(n) { }

        #endregion

        #region Parser

        public static new FORMAL_GENERIC parse(iSCOPE context)
        {
            Debug.Indent();
            Debug.WriteLine("Entering FORMAL_GENERIC.parse");

            Token id = expect(TokenCode.Identifier);
            if (id == null) return null; // error

            FORMAL_GENERIC result = null;

            Token token = get();
            if ( token.code == TokenCode.Colon )
            {
                forget();
                result = FORMAL_NONTYPE.parse(id,context);
            }
            else
                result = FORMAL_TYPE.parse(id,context);

            Debug.WriteLine("Exiting FORMAL_GENERIC.parse");
            Debug.Unindent();

            return result;
        }

        #endregion

        #region Verify

        public override bool check() { return true; }
        public override bool verify() { return true; }

        #endregion

        #region Code generation
        public override bool generate() { return true; }

        #endregion

        #region Reporting

        public override void report(int sh) { }

        #endregion
    }

    /// <summary>
    /// 
    /// <syntax><code>
    /// Обобщенный-параметр
    ///       : Обобщенный–типовой-параметр [ Уточнение-типового-параметра ]
    ///       | ...
    /// </code></syntax>
    /// </summary>
    public class FORMAL_TYPE : FORMAL_GENERIC
    {
        #region Structure

     // public IDENTIFIER name -- from the base-base class

        /// <summary>
        /// 
        /// </summary>
        public TYPE base_type { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public List<TYPE> init_param_types { get; private set; }

        public void add(TYPE t) { init_param_types.Add(t); t.parent = this; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        public FORMAL_TYPE(Token t) : base(t.image) { init_param_types = new List<TYPE>(); }

        #endregion

        #region Parser

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static FORMAL_TYPE parse(Token id, iSCOPE context)
        {
            Debug.Indent();
            Debug.WriteLine("Entering FORMAL_TYPE.parse");

            // Identifier was parsed before and is passed via 'id'.
            FORMAL_TYPE generic_type_par = new FORMAL_TYPE(id);

            Token token = get();
            if ( token.code != TokenCode.Arrow2 ) goto Finish;

            // ->
            TYPE base_type = null;
            forget(); token = get();
            if ( token.code == TokenCode.Identifier)
            {
                forget();
                base_type = UNIT_REF.parse(token,false,context);
            }
            else if ( token.code == TokenCode.LParen ) // T->(tuple)
            {
                base_type = TUPLE_TYPE.parse(context);
            }
            else
            {
                // Syntax error
            }
            generic_type_par.base_type = base_type;

            token = get();
            if ( token.code != TokenCode.Init ) goto Finish;
            forget(); token = get();

            // init
            if ( token.code != TokenCode.LParen ) goto Finish;
            forget(); token = get();
            if ( token.code == TokenCode.RParen ) { forget(); goto Finish; }

            while ( true )
            {
               TYPE init_param_type = TYPE.parse(context);
               generic_type_par.add(init_param_type);
               init_param_type.parent = generic_type_par;

               token = get();
               if ( token.code == TokenCode.Comma ) { forget(); continue; }
               break;
            }
            token = expect(TokenCode.RParen);

         Finish:
            Debug.WriteLine("Exiting FORMAL_TYPE.parse");
            Debug.Unindent();

            generic_type_par.setSpan(id,token);
            return generic_type_par;
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
            string r = commonAttrs() + shift(sh) + "FORMAL TYPE " + name.identifier;
            if ( init_param_types.Count > 0 ) r += " WITH RESTRICTIONS";
            System.Console.WriteLine(r);

            foreach ( TYPE t in init_param_types )
                t.report(sh+constant);
        }

        #endregion
    }

    /// <summary>
    /// 
    /// <syntax><code>
    /// Обобщенный-параметр
    ///       : ...
    ///       | Обобщенный-нетиповой-параметр : Тип
    /// </code></syntax>
    /// </summary>
    public class FORMAL_NONTYPE : FORMAL_GENERIC
    {
        #region Structure

     // public IDENTIFIER name -- from the base-base class

        /// <summary>
        /// 
        /// </summary>
        public TYPE type { get; private set; }

        #endregion

        #region Creation

        public FORMAL_NONTYPE(Token id) : base(id.image) { }
        public FORMAL_NONTYPE(string n) : base(n) { }

        #endregion

        #region Parser

        public static FORMAL_NONTYPE parse ( Token id, iSCOPE context )
        {
            Debug.Indent();
            Debug.WriteLine("Entering FORMAL_NONTYPE.parse");

            // parameter name is passed via 'id'.
            // ':' is already eaten.

            FORMAL_NONTYPE generic_nontype_par = new FORMAL_NONTYPE(id);

            TYPE type = TYPE.parse(context);
            generic_nontype_par.type = type;
            type.parent = generic_nontype_par;

            Debug.WriteLine("Exiting FORMAL_NONTYPE.parse");
            Debug.Unindent();

            generic_nontype_par.setSpan(id.span,type.span);
            return generic_nontype_par;
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string r = commonAttrs() + shift(sh) + "FORMAL NON-TYPE " + name.identifier;
            System.Console.WriteLine(r);

            type.report(sh + constant);
        }

        #endregion

        #region Verify

        public override bool check() { return true; }
        public override bool verify() { return true; }

        #endregion

        #region Code generation

        public override bool generate() { return true; }

        #endregion
    }
}
