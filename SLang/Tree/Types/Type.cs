using SLang.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SLang
{
    /// <summary>
    /// 
    /// </summary>
    /// <syntax>
    /// Type : UnitType | MultiType | TupleType | RoutineType
    /// </syntax>
    public abstract class TYPE : ENTITY
    {
        #region Parser

        public static TYPE parse(iSCOPE context)
        {
            Debug.Indent();
            Debug.WriteLine("Entering TYPE.parse");

            Token token = get();

            TYPE result = null;
            switch ( token.code )
            {
                case TokenCode.LParen :
                    result = TUPLE_TYPE.parse(context);
                    break;

                case TokenCode.Routine:
                    result = ROUTINE_TYPE.parse(context);
                    break;

                case TokenCode.Identifier :
                {
                 // Don't forget()
                    UNIT_REF unit_type = UNIT_REF.parse(null,false,context);
                    Token del = get();
                    if ( del.code == TokenCode.Vertical )
                        // Don't call forget()
                        result = MULTI_TYPE.parse(unit_type,context);
                    else
                        result = unit_type;
                    break;
                }
                default: // Syntax error
                {
                    break;
                }
            }

            Debug.WriteLine("Exiting TYPE.parse");
            Debug.Unindent();

            return result;
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <syntax>
    /// MultiType : UnitType { "|" UnitType }
    /// </syntax>
    public class MULTI_TYPE : TYPE
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public List<UNIT_REF> types { get; private set; }
        public void add(UNIT_REF u) { types.Add(u); }

        #endregion

        #region Constructors

        public MULTI_TYPE() : base() { types = new List<UNIT_REF>(); }

        #endregion

        #region Parser

        public static MULTI_TYPE parse ( UNIT_REF first, iSCOPE context )
        {
            Debug.Indent();
            Debug.WriteLine("Entering MULTI_TYPE.parse");

            MULTI_TYPE multi = new MULTI_TYPE();
            multi.add(first);

            Span begin = first.span;
            UNIT_REF elem_type = null;
            while ( true )
            {
                Token token = get();
                if ( token.code != TokenCode.Vertical ) break;
                forget();
                elem_type = UNIT_REF.parse(null,false,context);
                multi.add(elem_type);
            }
            multi.setSpan(begin, elem_type.span);

            Debug.WriteLine("Exiting MULTI_TYPE.parse");
            Debug.Unindent();

            return multi;
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

        public override JsonIr ToJSON()
        {
            return new JsonIr(GetType())
                .AppendChild(JsonIr.ListToJSON(types));
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string r = commonAttrs() + shift(sh) + "MULTI-TYPE ";
            System.Console.WriteLine(r);

            foreach ( TYPE type in types )
                type.report(sh+constant);
        }

        #endregion
    }

    public class RANGE_TYPE : TYPE
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public EXPRESSION left { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public EXPRESSION right { get; private set; }

        #endregion

        #region Creation

        public RANGE_TYPE(EXPRESSION l, EXPRESSION r)
        {
            left = l; left.parent = this;
            right = r; right.parent = this;
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

        public override JsonIr ToJSON()
        {
            return new JsonIr(GetType())
                .AppendChild(left.ToJSON())
                .AppendChild(right.ToJSON());
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string common = commonAttrs();
            System.Console.WriteLine(common+shift(sh)+"RANGE TYPE");

            System.Console.WriteLine(shift(common.Length+sh) + "LEFT");
            left.report(sh+constant);

            System.Console.WriteLine(shift(common.Length+sh) + "RIGHT");
            right.report(sh+constant);
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <syntax>
    /// TupleType  : "(" [TupleField { ( "," | ";" ) TupleField } ] ")"
    /// 
    /// TupleField : [ Identifier { "," Identifier } ":" ] UnitType [ is Expression ]
    ///            | Identifier { "," Identifier } is Expression
    /// </syntax>
    public class TUPLE_TYPE : TYPE
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, UNIT_REF> fields { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, EXPRESSION> inits { get; private set; }
        
        public bool exists(string n)
        {
            UNIT_REF t;
            return fields.TryGetValue(n, out t);
        }

        public void add(string n, UNIT_REF t) { fields.Add(n, t); }
        public void add(string n, EXPRESSION e) { inits.Add(n, e); }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public TUPLE_TYPE() : base() { fields = new Dictionary<string,UNIT_REF>(); }

        #endregion

        #region Parser

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        new public static TUPLE_TYPE parse(iSCOPE context)
        {
            Debug.Indent();
            Debug.WriteLine("Entering TUPLE_TYPE.parse");

            TUPLE_TYPE tuple = new TUPLE_TYPE();

            Token begin = expect(TokenCode.LParen);

            int count = 0;
            Token token = get();
            if ( token.code == TokenCode.RParen )
            {
                // Empty tuple
                forget(); goto OutLoop;
            }

            var ids = new List<Token>();
            UNIT_REF unit_type;

            while ( true )
            {
                token = expect(TokenCode.Identifier);

                Token delimiter = get();
                switch ( delimiter.code )
                {
                    case TokenCode.Comma :
                        // Identifier is the current name in the list of fields
                        forget();
                        ids.Add(token);
                        continue;

                    case TokenCode.Colon :
                        // Identifier is the last name in the field of fields
                        forget();
                        ids.Add(token);

                        // Now we treat collected ids as names of tuple fields,
                        // and the construct following ':' as unit name.
                        unit_type = UNIT_REF.parse(null,false,context);
                        unit_type.parent = tuple;

                        foreach ( Token id in ids )
                        {
                            if ( tuple.exists(id.image) ) // Error: duplicate name
                                { }
                            else
                                tuple.add(id.image, unit_type);
                        }
                        token = get();
                        if (token.code == TokenCode.Is)
                        {
                            forget();
                            EXPRESSION expr = EXPRESSION.parse(null,context);
                            foreach ( Token id in ids )
                            {
                                if (tuple.exists(id.image))
                                    { }
                                else
                                    tuple.add(id.image,expr);
                            }
                        }
                        ids.Clear();
                        break;

                    case TokenCode.Is :
                        ids.Add(token);
                        // No explicit type for the field(s) but only initializer
                        forget();
                        EXPRESSION expr2 = EXPRESSION.parse(null,context);
                        foreach (Token id in ids)
                        {
                            if ( tuple.exists(id.image) )  // Error: duplicate name
                                { }
                            else
                                tuple.add(id.image,expr2);
                        }
                        ids.Clear();
                        break;

                    case TokenCode.RParen :
                    case TokenCode.Semicolon :
                        forget();
                        Token stop = delimiter;
                        // ')': The end of the tuple type; this means that all previous ids were
                        //      actually simple type names.
                        // ';': The end of the current part of the tuple type. Again, this means
                        //      that all ids were type names.
                        // In both cases, field names were omitted, and we have to assign
                        // artifical names for those fields.
                        ids.Add(token);
                        foreach ( Token id in ids )
                        {
                            unit_type = UNIT_REF.parse(id,false,context);
                            count++;
                            string n = "$" + count.ToString();
                            tuple.add(n,unit_type);
                        }
                        ids.Clear();
                        if ( stop.code == TokenCode.Semicolon ) continue;
                        else goto OutLoop;

                    case TokenCode.LBracket:
                        ids.Add(token);
                        // 'ids' collected before, are actually unit names:
                        // all of them before the last one, were simple names,
                        // and the last one is like 'name[...'.
                        foreach ( Token id in ids )
                        {
                            unit_type = UNIT_REF.parse(id,false,context);
                            count++;
                            string n = "$" + count.ToString();
                            tuple.add(n,unit_type);
                        }
                        break;

                    } // switch

                } // while

         OutLoop:
            tuple.setSpan(begin,token);

            Debug.WriteLine("Exiting TUPLE_TYPE.parse");
            Debug.Unindent();

            return tuple;
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

        public override JsonIr ToJSON()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string common = commonAttrs();
            string r = common + shift(sh) + "TUPLE TYPE";
            System.Console.WriteLine(r);

            foreach ( KeyValuePair<string,UNIT_REF> field in fields )
            {
                string name = field.Key;
                UNIT_REF unit = field.Value;

                System.Console.WriteLine(shift(common.Length+sh+constant) + name + ":"
                                         + unit.unit_ref.name.identifier + ":" + unit.unit_ref.unique);
            }
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <syntax>
    /// UnitTypeName         : CompoundName [ GenericInstantiation ]
    /// 
    /// GenericInstantiation : "[" (Type|Expression) { "," (Type|Expression) } "]"
    /// </syntax>
    public class UNIT_REF : TYPE
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public bool opt { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool as_sign { get; private set; }

        public void setSpecs(bool o, bool a) { opt = o; as_sign = a; }

        /// <summary>
        /// The name of the referenced unit
        ///   -- For the 1st phase; later the name should be resolved
        ///      for the 'unit_ref', see below.
        /// </summary>
        public string name { get; private set; }

        /// <summary>
        /// The reference to the unit
        /// </summary>
        public DECLARATION unit_ref { get; private set; }

        /// <summary>
        /// The list of generic actual parameters: types and/or expressions.
        /// </summary>
        public List<ENTITY> generic_actuals { get; private set; }
        public void add(ENTITY t) { generic_actuals.Add(t); }

        #endregion

        #region Constructors

        private UNIT_REF() : base() { generic_actuals = new List<ENTITY>(); }
        public UNIT_REF(string n) : this() { name = n; }
        public UNIT_REF(UNIT u) : this() { name = u.name.identifier; unit_ref = u; }

        #endregion

        #region Parser

        /// <summary>
        /// 
        /// </summary>
        /// <syntax>
        /// UnitTypeName         : CompoundName [ GenericInstantiation ]
        /// 
        /// GenericInstantiation : "[" (Type|Expression) { "," (Type|Expression) } "]"
        /// </syntax>
        /// <returns></returns>
        public static UNIT_REF parse(Token id, bool opt, iSCOPE context)
        {
            Debug.Indent();
            Debug.WriteLine("Entering UNIT_REF.parse");

            Token token = null;
            // We assume that 'id' is 'identifier'.
            if ( id == null ) { token = get(); forget(); }
            else                token = id;

            token = IDENTIFIER.parseCompoundName(token);
            if ( token == null ) /* an error was detected earlier */ return null;
            Token start = token;
            
            UNIT_REF unit_ref = new UNIT_REF(token.image);
            unit_ref.opt = opt;
            unit_ref.as_sign = true;
            DECLARATION unit = Context.find(token);
            if ( unit != null && (unit is UNIT || unit is FORMAL_TYPE ) )
                unit_ref.unit_ref = unit;

            token = get();
            if ( token.code == TokenCode.LBracket )
            {
                forget();
                while ( true )
                {
                    TYPE type = null;
                    token = get();
                    if ( token.code == TokenCode.LParen )
                    {
                        type = TUPLE_TYPE.parse(context);
                        unit_ref.add(type);
                        goto Delimiter;
                    }
                    EXPRESSION expr = EXPRESSION.parse(null,context);
                    if ( expr is REFERENCE || expr is UNRESOLVED )
                    {
                        string name = null;
                        if ( expr is REFERENCE )
                        {
                            if ((expr as REFERENCE).declaration is UNIT)
                                name = (expr as REFERENCE).declaration.name.identifier;
                            else
                                goto NonType;
                        }
                        else // UNRESOLVED
                            name = (expr as UNRESOLVED).name.identifier;

                        id = new Token(expr.span,TokenCode.Identifier,name,new Category(CategoryCode.identifier));
                        type = UNIT_REF.parse(id,false,context); // Recursive call

                        unit_ref.add(type);
                        type.parent = unit_ref;
                        goto Delimiter;
                    }
                 // else -- expr is perhaps a non-type argument
                NonType:
                    token = get();
                    if ( token.code == TokenCode.DotDot )
                    {
                        // This is actually a range _type_
                        forget();
                        EXPRESSION right = EXPRESSION.parse(null,context);
                        RANGE_TYPE range = new RANGE_TYPE(expr, right);
                        range.setSpan(expr.span,right.span);
                        unit_ref.add(range);
                        range.parent = unit_ref;
                    }
                    else // Definitely a non-type argument
                    {
                        unit_ref.add(expr);
                        expr.parent = unit_ref;
                    }
                Delimiter:
                    token = get();
                    switch ( token.code )
                    {
                        case TokenCode.Comma:    forget(); continue;
                        case TokenCode.RBracket: forget(); goto Finish;
                        default: { /* Syntax error in generic actuals */ break; }
                    }
                }
             Finish:
                unit_ref.setSpan(start.span,token.span);
            }
            else
                unit_ref.setSpan(start);

            Debug.WriteLine("Exiting UNIT_REF.parse");
            Debug.Unindent();

            return unit_ref;
        }

        #endregion

        #region Verification

        public override bool check()
        {
            if (unit_ref == null) { error(null,"no-unit-ref"); return false; }
            foreach (TYPE t in generic_actuals )
                if ( !t.check() ) return false;
            return true;
        }

        public override bool verify()
        {
            // Check the genericity of the 'unit' and the correspondence
            // between number of formal and actual generic parameters.
            if ( unit_ref is UNIT && (unit_ref as UNIT).isGeneric() )
            {
                if (generic_actuals.Count != (unit_ref as UNIT).numGenerics())
                { error(null,"illegal-num-gen"); return false; }
            }
            else
            {
                if ( generic_actuals.Count != 0 )
                { error(null,"no-gen-actuals"); return false; }
            }
            // Check the actual generic parameters
            foreach ( TYPE t in generic_actuals )
                if ( !t.verify() ) return false;
            return true;
        }

        #endregion

        #region Code generation

        public override bool generate() { return true; }

        public override JsonIr ToJSON()
        {
            return new JsonIr(GetType(), name);
                //.AppendChild(new JsonIr("OPT_SPEC", opt ? "opt" : null))
                //.AppendChild(new JsonIr("AS_SPEC", as_sign ? "as" : null))
                //.AppendChild(JsonIr.ListToJSON(generic_actuals))
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string common = commonAttrs();
            string r = common + shift(sh) + (opt ? "OPT " : "") + "UNIT REF " + name;
            if ( unit_ref != null ) r += ":" + unit_ref.unique;
            else                    r += ":???";
            if ( as_sign ) r += " TAKEN FROM AS";

            if (generic_actuals.Count > 0) r += " GENERIC ACTUALS";
            System.Console.WriteLine(r);

            foreach ( ENTITY g in generic_actuals )
                g.report(sh+constant);
        }

        #endregion
    }

    public class ROUTINE_TYPE : TYPE
    {
        #region Parser

        new public static ROUTINE_TYPE parse(iSCOPE context)
        {
            Debug.Indent();
            Debug.WriteLine("Entering ROUTINE_TYPE.parse");


            Debug.WriteLine("Exiting ROUTINE_TYPE.parse");
            Debug.Unindent();

            return null;
        }

        #endregion

        #region Verification

        public override bool check() { return true; }
        public override bool verify() { return true; }

        #endregion

        #region Code generation

        public override bool generate() { return true; }

        public override JsonIr ToJSON()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Reporting

        public override void report(int sh) { }

        #endregion
    }
}
