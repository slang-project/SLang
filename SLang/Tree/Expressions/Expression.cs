using SLang.Service;
using System;
using System.Collections.Generic;

namespace SLang
{
    /*

    primary:
        literal
        this
        identifier
        OLD identifier
        ( expression )

    name:
        primary
        primary { . member }
        primary { ( [ expression { , expression } ] ) }

    unary:
            name
        +   name
        -   name
        NOT name
        NEW name
        IF expression THEN expression ELSE expression

    multiplicative:
        unary { ( * | / | % ) unary }

    additive:
        multiplicative { ( + | - ) multiplicative }

    relational:
        additive [ ( < | <= | > | >= | = | /= ) additive ]

    logical:
        relational { (AND | OR | XOR ) relational }

    expression:
        logical
     */

    public abstract class EXPRESSION : ENTITY
    {
        /// <summary>
        /// 
        /// </summary>
        public TYPE type { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <returns></returns>
        public static EXPRESSION parse(Token first, iSCOPE context)
        {
            return LOGICAL.parse(first,context);
        }

        public override JsonIr ToJSON()
        {
            return new JsonIr(GetType())
                // TODO: make sure if TYPE is not required in EXPRESSION IR
                //.AppendChild(type.ToJSON())
                ;
        }

        public abstract void calculateType();
    }

    /// <summary>
    /// 
    /// <syntax>
    /// primary:
    ///     literal
    ///     this
    ///     return
    ///     identifier
    ///     IF expression THEN expression ELSE expression
    ///     ( expression )
    /// </syntax>
    /// 
    /// </summary>
    public abstract class PRIMARY : EXPRESSION
    {
        #region Parser

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static new EXPRESSION parse(Token first, iSCOPE context)
        {
            EXPRESSION result = null;
            Token token = (first == null) ? get() : first;
            Token begin = token;
            switch ( token.code )
            {
                case TokenCode.This:
                    forget();
                    UNIT unit = Context.unit();
                    if (unit == null)
                    { 
                        // Error message!
                        result = new THIS(null);
                    }
                    else
                    {
                        result = new THIS(unit);
                        result.setSpan(token);
                    }
                    break;

                case TokenCode.Return:
                    if ( !ENTITY.weAreWithinEnsure ) break;
                    forget();
                    ROUTINE routine = Context.routine();
                    if (routine == null)
                    { } // error
                    else
                    {
                        result = new RETURN_EXPR(routine);
                        result.setSpan(token);
                    }
                    break;

                case TokenCode.Old:
                    forget();
                    result = EXPRESSION.parse(null,context);
                    Span end = result.span;
                    OLD old = new OLD(result);
                    result.parent = old;
                    result = old;
                    result.setSpan(begin.span,end);
                    break;

                case TokenCode.If:
                    result = CONDITIONAL.parse(context);
                    break;

                case TokenCode.LParen:
                    forget();
                    Span start_tuple = token.span;
                    result = EXPRESSION.parse(null,context);
                    token = get();
                    if ( token.code == TokenCode.Comma )
                    {
                        // Seems to be a tuple
                        forget();
                        TUPLE_EXPR tuple = new TUPLE_EXPR();
                        tuple.add(result);
                        while ( true )
                        {
                            EXPRESSION expr = EXPRESSION.parse(null,context);
                            tuple.add(expr);

                            token = get();
                            if ( token.code != TokenCode.Comma ) break;
                            forget();
                        }
                        result = tuple;
                    }
                    end = expect(TokenCode.RParen).span;
                    result.setSpan(token.span,end);
                    break;

                case TokenCode.Identifier:
                    if ( first == null ) forget(); ////// perhaps the same condition should be added for all cases?
                    DECLARATION d = Context.find(token);
                    if ( d == null )
                        result = new UNRESOLVED(context, new IDENTIFIER(token));
                    else
                        result = new REFERENCE(d);

                    Token token2 = get();
                    if ( token2.code == TokenCode.LBracket )
                    {
                        UNIT_REF unitRef = UNIT_REF.parse(token,false,context);
                        result = new NEW(unitRef);
                        result.setSpan(unitRef.span);
                    }
                    else
                        result.setSpan(token);
                    break;

                case TokenCode.Integer:
                    result = new LITERAL(token.value,token.span,TokenCode.Integer);
                    result.setSpan(token);
                    forget();
                    break;

                case TokenCode.Real:
                    result = new LITERAL(token.value, token.span, TokenCode.Real);
                    result.setSpan(token);
                    forget();
                    break;

                case TokenCode.String:
                    result = new LITERAL(token.value, token.span, TokenCode.String);
                    result.setSpan(token);
                    forget();
                    break;

                default:
                    return null;
            }
         // result.setSpan(token);
            return result;
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

        public override bool generate()
        {
            throw new NotImplementedException();
        }

        public override JsonIr ToJSON()
        {
            return base.ToJSON();
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class COND_IF_THEN : EXPRESSION
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public EXPRESSION condition { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        public EXPRESSION thenPart { get; private set; }

        #endregion

        #region Creation

        public COND_IF_THEN(EXPRESSION c, EXPRESSION t)
        {
            condition = c;
            thenPart = t;
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

        public override bool generate()
        {
            throw new NotImplementedException();
        }

        public override JsonIr ToJSON()
        {
            return base.ToJSON()
                .AppendChild(condition.ToJSON())
                .AppendChild(thenPart.ToJSON());
        }

        #endregion

        #region Reporting

        public override void report(int sh) { }

        public void reportIfThen(int sh, string ElseIf)
        {
            string common = null;
            if ( ElseIf != null ) common = ElseIf;
            else                  common = commonAttrs();
            string r = common + shift(sh) + (ElseIf == null ? "ELSE IF " : "IF ");
            System.Console.WriteLine(r);

            condition.report(sh + constant);

            System.Console.WriteLine(shift(common.Length + sh) + "THEN");
            thenPart.report(sh + constant);
        }

        #endregion

        public override void calculateType()
        {
            throw new NotImplementedException();
        }
    }

    public class CONDITIONAL : PRIMARY
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public List<COND_IF_THEN> ifThenParts { get; private set; }

        public void add(COND_IF_THEN cit) { ifThenParts.Add(cit); cit.parent = this; }
        
        /// <summary>
        /// 
        /// </summary>
        public EXPRESSION elsePart { get; private set; }

        public void add(EXPRESSION e) { elsePart = e; e.parent = this; }

        #endregion

        #region Creation

        public CONDITIONAL()
        {
            ifThenParts = new List<COND_IF_THEN>();
        }

        #endregion

        #region Parser

        public static EXPRESSION parse(iSCOPE context)
        {
            Token token = get();
            forget();
            Span begin = token.span;

            CONDITIONAL result = new CONDITIONAL();
            EXPRESSION elsePart = null;

            EXPRESSION condition = EXPRESSION.parse(null, context);
            expect(TokenCode.Then);
            EXPRESSION thenPart = EXPRESSION.parse(null, context);

            COND_IF_THEN cit = new COND_IF_THEN(condition, thenPart);
            condition.parent = result;
            thenPart.parent = result;
            cit.parent = result;
            result.add(cit);

            token = get();
            while ( true )
            {
                if ( token.code == TokenCode.Elsif )
                {
                    forget();
                    condition = EXPRESSION.parse(null, context);
                    expect(TokenCode.Then);
                    thenPart = EXPRESSION.parse(null, context);

                    cit = new COND_IF_THEN(condition, thenPart);
                    cit.setSpan(token.span,thenPart.span);
                    
                    condition.parent = result;
                    thenPart.parent = result;
                    cit.parent = result;
                    
                    result.add(cit);
                    token = get();
                    // go to next iteration
                }
                else
                    break;
            }
            token = expect(TokenCode.Else);
            elsePart = EXPRESSION.parse(null,context);
            elsePart.parent = result;
            result.add(elsePart);
            result.setSpan(begin,elsePart.span);

         // context.add(result);  ?? why ??
            return result;
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string ElseIf = commonAttrs();
            foreach (COND_IF_THEN IfThenPart in ifThenParts)
            {
                IfThenPart.reportIfThen(sh,ElseIf);
                ElseIf = null;
            }
            if ( elsePart != null )
            {
                System.Console.WriteLine(shift(sh) + "ELSE");
                elsePart.report(sh + constant);
            }
        }

        #endregion

        #region Code generation

        public override JsonIr ToJSON()
        {
            return base.ToJSON()
                .AppendChild(JsonIr.ListToJSON(ifThenParts))
                .AppendChild(elsePart.ToJSON());
        }

        #endregion

        public override void calculateType()
        {
            throw new NotImplementedException();
        }
    }

    public class THIS : PRIMARY
    {
        #region Structure

        public UNIT unit;

        #endregion

        #region Creation

        public THIS(UNIT u) : base()
        {
            unit = u;
            type = new UNIT_REF(u);
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            System.Console.WriteLine(commonAttrs() + shift(sh) + "THIS:" + unit.unique);
        }

        #endregion

        public override void calculateType()
        {
            throw new NotImplementedException();
        }
    }

    public class RETURN_EXPR : PRIMARY
    {
        #region Structure

        public ROUTINE routine;

        #endregion

        #region Creation

        public RETURN_EXPR(ROUTINE r) : base()
        {
            routine = r;
            type = routine.type;
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            System.Console.WriteLine(commonAttrs() + shift(sh) + "RETURN:" + routine.unique);
        }

        #endregion

        public override void calculateType()
        {
            throw new NotImplementedException();
        }
    }

    public class OLD : PRIMARY
    {
        #region structure

        public EXPRESSION old { get; private set; }

        #endregion

        #region Creation

        public OLD(EXPRESSION o) : base()
        {
            old = o;
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            System.Console.WriteLine(commonAttrs() + shift(sh) + "OLD");
            old.report(sh + constant);
        }

        #endregion

        #region Code generation

        public override JsonIr ToJSON()
        {
            return base.ToJSON()
                .AppendChild(old.ToJSON());
        }

        #endregion

        public override void calculateType()
        {
            throw new NotImplementedException();
        }
    }

    public class REFERENCE : PRIMARY
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public DECLARATION declaration { get; private set; }

        #endregion

        #region Creation

        public REFERENCE(DECLARATION d)
        {
            declaration = d;
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string r = "??:??";
            if ( declaration != null )
            {
                r = "REF " + declaration.name.identifier + ":" + declaration.unique;
            }
            System.Console.WriteLine(commonAttrs()+shift(sh)+r);
        }

        #endregion

        #region Code generation

        public override JsonIr ToJSON()
        {
            return base.ToJSON()
                .AppendChild(declaration.ToJSON());
        }

        #endregion

        public override void calculateType()
        {
            throw new NotImplementedException();
        }
    }

    public class UNRESOLVED : PRIMARY
    {
        #region Structure

        public iSCOPE ownScope { get; private set; }
        public IDENTIFIER name { get; private set; }

        #endregion

        #region Creation

        public UNRESOLVED(iSCOPE context, IDENTIFIER n)
        {
            ownScope = context;
            name = n;
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string r = "??:??";
            if ( name != null )
            {
                r = "UNRESOLVED " + name.identifier + ":??";
            }
            System.Console.WriteLine(commonAttrs() + shift(sh) + r);
        }

        #endregion

        #region Code generation

        public override JsonIr ToJSON()
        {
            return base.ToJSON()
                .AppendChild(name.ToJSON());
        }

        #endregion

        public override void calculateType()
        {
            throw new NotImplementedException();
        }

    }

    public class LITERAL : PRIMARY
    {
        #region Structure
        /// <summary>
        /// 
        /// </summary>
        public object value { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public TokenCode code;

        #endregion

        #region Creation

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        public LITERAL(object v, Span s, TokenCode c)
        {
            this.value = v;
            this.setSpan(s);
            this.code = c;
        }

        #endregion

        #region Type

        public override void calculateType()
        {
            switch ( code )
            {
             // case TokenCode.Integer:
             //     type = Intege
                case TokenCode.Real:
                    break;
            }
            return;
        }

        #endregion

        #region Verification

        public override bool check() { throw new NotImplementedException(); }
        public override bool verify() { throw new NotImplementedException(); }

        #endregion

        #region Code generation

        public override bool generate() { throw new NotImplementedException(); }

        public override JsonIr ToJSON()
        {
            return new JsonIr(GetType(), value.ToString());  // TODO: check
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string r = commonAttrs() + shift(sh) + "LITERAL ";
            System.Console.WriteLine(r + value.ToString() + ":" + this.unique);
        }

        #endregion
    }

    public class TUPLE_EXPR : PRIMARY
    {
        #region Structure

        public List<EXPRESSION> expressions { get; private set; }
        public void add(EXPRESSION e) { expressions.Add(e); if ( e != null ) e.parent = this; }

        #endregion

        #region Creation

        public TUPLE_EXPR() { expressions = new List<EXPRESSION>(); }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            System.Console.WriteLine(commonAttrs() + shift(sh) + "TUPLE");

            foreach ( EXPRESSION e in expressions )
                e.report(sh+constant);
        }

        #endregion

        #region Verification

        public override bool check()
        {
            return base.check();
        }
        public override bool verify()
        {
            return base.verify();
        }

        #endregion

        #region Code generation

        public override bool generate()
        {
            return base.generate();
        }

        public override JsonIr ToJSON()
        {
            return base.ToJSON()
                .AppendChild(JsonIr.ListToJSON(expressions));
        }

        #endregion

        public override void calculateType()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 
    /// <syntax>
    ///     secondary:
    ///         primary
    ///         secondary { . member }
    ///         secondary { ( [expression { , expression } ] ) }
    /// </syntax>
    /// 
    /// </summary>
    public class SECONDARY : EXPRESSION
    {
        #region Parser

        public static new EXPRESSION parse ( Token first, iSCOPE context )
        {
            EXPRESSION result = PRIMARY.parse(first,context);
            if ( result == null ) return null;

            Span begin = result.span;

            while ( true )
            {
                Token token = get();
                switch ( token.code )
                {
                    case TokenCode.Dot:
                        forget();
                        IDENTIFIER identifier;
                        token = get();
                        if ( token.code == TokenCode.Init )
                        {
                            forget();
                            // Initializer call in full form
                            identifier = new IDENTIFIER(INITIALIZER.initName);
                        }
                        else if ( token.code == TokenCode.Identifier )
                        {
                            forget();
                            identifier = new IDENTIFIER(token);
                        }
                        else // syntax error
                        {
                            identifier = new IDENTIFIER("ERROR");
                        }
                        result = new MEMBER(result,identifier);
                        result.setSpan(begin,token.span);
                        break;

                    case TokenCode.LParen:
                        forget();
                        result = new CALL(result);
                        while ( true )
                        {
                            token = get();
                            if ( token.code == TokenCode.RParen ) { forget(); break; }
                            EXPRESSION actual = EXPRESSION.parse(null, context);
                            (result as CALL).add(actual);
                            token = get();
                            if ( token.code == TokenCode.Comma ) { forget(); continue; }
                            token = expect(TokenCode.RParen);
                            break;
                        }
                        result.setSpan(begin,token.span);
                        break;

                    default:
                        goto Out;
                }
            }
         Out:
            return result;
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

        public override bool generate()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override void calculateType()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 
    /// <syntax>
    ///     secondary:
    ///         primary
    ///   ==>   secondary { . member }
    ///         secondary { ( [expression { , expression } ] ) }
    /// </syntax>
    /// 
    /// </summary>
    public class MEMBER : SECONDARY
    {
        #region Structure

        public EXPRESSION secondary { get; private set; }

        public IDENTIFIER member { get; private set; }

        #endregion

        #region Creation

        public MEMBER(EXPRESSION prefix, IDENTIFIER member)
        {
            this.secondary = prefix; prefix.parent = this;
            this.member = member;    member.parent = this;
        }

        #endregion

        #region Code generation

        public override JsonIr ToJSON()
        {
            return base.ToJSON()
                .AppendChild(secondary.ToJSON())
                .AppendChild(member.ToJSON());
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string r = commonAttrs() + shift(sh) + "MEMBER " + member.identifier + " FROM";
            System.Console.WriteLine(r);

            secondary.report(sh+constant);
        }

        #endregion

        public override void calculateType()
        {
            throw new NotImplementedException();
        }

    }

    /// <summary>
    /// 
    /// <syntax>
    ///     name:
    ///         primary
    ///         secondary { . member }
    ///    ==>  secondary { ( [expression { , expression } ] ) }
    /// </syntax>
    /// 
    /// </summary>
    public class CALL : SECONDARY
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public EXPRESSION secondary { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public List<EXPRESSION> actuals { get; private set; }

        public void add(EXPRESSION a) { actuals.Add(a); a.parent = this; }

        #endregion

        #region Creation

        public CALL(EXPRESSION s)
        {
            secondary = s;
            s.parent = this;
            actuals = new List<EXPRESSION>();
        }

        #endregion

        #region Parser

        public static CALL parse ( EXPRESSION reference, iSCOPE context )
        {
            CALL call = new CALL(reference);

            Token token;
            while ( true )
            {
                EXPRESSION arg = EXPRESSION.parse(null,context);
                call.add(arg);
                token = get();
                if ( token.code == TokenCode.Comma ) { forget(); continue; }
                break;
            }
            token = expect(TokenCode.RParen);
            call.setSpan(reference.span,token.span);

            return call;
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string common = commonAttrs();
            string r = common + shift(sh) + "CALL";
            System.Console.WriteLine(r);

            secondary.report(sh+constant);

            if ( actuals.Count > 0 )
            {
                System.Console.WriteLine(shift(common.Length+sh) + "ARGUMENTS");
                foreach ( EXPRESSION argument in actuals )
                    argument.report(sh+constant);
            }
        }

        #endregion

        #region Code generation

        public override JsonIr ToJSON()
        {
            return base.ToJSON()
                .AppendChild(secondary.ToJSON())
                .AppendChild(JsonIr.ListToJSON(actuals));
        }

        #endregion

        public override void calculateType()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 
    /// <syntax>
    /// unary:
    ///         secondary
    ///     +   unary
    ///     -   unary
    ///     ~   unary
    ///     NEW unary
    /// </syntax>
    /// </summary>
    public class UNARY : EXPRESSION
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public EXPRESSION primary { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Token unaryOp;

        #endregion

        #region Creation

        public UNARY(Token op, EXPRESSION p)
        {
            this.primary = p;
            this.unaryOp = op;
        }

        #endregion

        #region Parser

        public static new EXPRESSION parse ( Token first, iSCOPE context )
        {
            EXPRESSION result;
            Token token = (first != null) ? first : get();

            switch (token.code)
            {
                case TokenCode.Plus:
                case TokenCode.Minus:
                case TokenCode.Tilde:
                    forget();
                    EXPRESSION second = UNARY.parse(null,context);
                    result = new UNARY(token,second);
                    result.setSpan(token.span,second.span);
                    break;
                case TokenCode.New:
                    forget();
                    UNIT_REF unitRef = UNIT_REF.parse(null,false,context);
                    result = new NEW(unitRef);
                    result.setSpan(token.span,unitRef.span);
                    break;
                default:
                    result = POWER.parse(token,context);
                    break;
            }
            return result;
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

        public override bool generate()
        {
            throw new NotImplementedException();
        }

        public override JsonIr ToJSON()
        {
            return new JsonIr(GetType(), unaryOp.value.ToString())
                .AppendChild(primary.ToJSON());
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string sign = "";
            switch ( unaryOp.code )
            {
                case TokenCode.Plus  : sign = "UNARY +"; break;
                case TokenCode.Minus : sign = "UNARY -"; break;
                case TokenCode.Tilde : sign = "NEGATION ~"; break;
             // case TokenCode.New   : sign = "NEW"; break;
             // case TokenCode.In    : sign = "IN";  break;
            }
            System.Console.WriteLine(commonAttrs() + shift(sh) + sign);
            primary.report(sh+constant);
        }

        #endregion

        public override void calculateType()
        {
            throw new NotImplementedException();
        }
    }

    public class NEW : UNARY
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public UNIT_REF unitRef { get; private set; }

        /// <summary>
        /// 
        /// </summary>
     // public Token unaryOp; -- from the base class

        #endregion

        #region Creation

        public NEW(UNIT_REF unitRef): base(null,null)
        {
            base.unaryOp = new Token(null,TokenCode.New,"new",new Category(CategoryCode.operatorr));
            this.unitRef = unitRef;
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

        #region Reporting

        public override void report(int sh)
        {
            System.Console.WriteLine(commonAttrs()+ shift(sh)+"NEW");
            unitRef.report(sh+constant);
        }

        #endregion

        public override void calculateType()
        {
            throw new NotImplementedException();
        }
    }

    public class IN_EXPRESSION : UNARY
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public RANGE_TYPE range { get; private set; }

        #endregion

        #region Creation

        public IN_EXPRESSION(Token t, EXPRESSION v, RANGE_TYPE r) : base(t, v)
        {
            range = r;
            range.parent = this;
        }

        #endregion

        #region Code generation

        public override JsonIr ToJSON()
        {
            return new JsonIr(GetType())
                .AppendChild(range.ToJSON());
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            System.Console.WriteLine(commonAttrs() + shift(sh) + "IN RELATION");
            primary.report(sh + constant);
            range.report(sh + constant);
        }

        #endregion
    }

    public class BINARY : EXPRESSION
    {
        #region Structure

        public EXPRESSION left { get; private set; }
        public EXPRESSION right { get; private set; }

        #endregion

        #region Creation

        public BINARY(EXPRESSION op1, EXPRESSION op2)
        {
            left = op1;  left.parent = this;
            right = op2; right.parent = this;
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

        public override bool generate()
        {
            throw new NotImplementedException();
        }

        public override JsonIr ToJSON()
        {
            return new JsonIr(GetType())
                .AppendChild(left.ToJSON())
                .AppendChild(right.ToJSON());
        }

        #endregion

        #region Reporting

        public override void report(int sh) { }

        protected void reportCommon ( int sh, string sign )
        {
            string common = commonAttrs();
            System.Console.WriteLine(common + shift(sh) + sign);

            System.Console.WriteLine(shift(common.Length + sh) + "LEFT");
            left.report(sh + constant);

            System.Console.WriteLine(shift(common.Length + sh) + "RIGHT");
            right.report(sh + constant);
        }

        #endregion

        public override void calculateType()
        {
            throw new NotImplementedException();
        }
    }

    public class POWER : BINARY
    {
        #region Creation

        public POWER(EXPRESSION l, EXPRESSION r) : base(l, r) { }

        #endregion

        #region Parser

        public static new EXPRESSION parse(Token token, iSCOPE context)
        {
            EXPRESSION result = SECONDARY.parse(token, context);
            while (true)
            {
                token = get();
                if (token.code == TokenCode.Power)
                {
                    forget();
                    EXPRESSION right = UNARY.parse(null, context);
                    Span begin = result.span;
                    result = new POWER(result, right);
                    result.setSpan(begin, right.span);
                }
                else
                    goto Out;
            }
        Out:
            return result;
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            reportCommon(sh, "POWER **");
        }

        #endregion
    }

    /// <summary>
    /// 
    /// <syntax>
    /// multiplicative:
    ///     unary { ( * | / | % ) unary }
    /// </syntax>
    /// </summary>
    public class MULTIPLICATIVE : BINARY
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public Token multOp { get; private set; } // *, /, %

        #endregion

        #region Creation

        public MULTIPLICATIVE(Token op, EXPRESSION op1, EXPRESSION op2) : base(op1,op2)
        {
            this.multOp = op;
        }

        #endregion

        #region Parser

        public static new EXPRESSION parse(Token first, iSCOPE context)
        {
            EXPRESSION result = UNARY.parse(first, context);
            while ( true )
            {
                Token token = get();
                switch (token.code)
                {
                    case TokenCode.Multiply  :
                    case TokenCode.Divide    :
                    case TokenCode.Remainder :
                        forget(); break;
                    default:
                        goto Out;
                }
                EXPRESSION second = UNARY.parse(null,context);
                Span begin = result.span;
                result = new MULTIPLICATIVE(token,result,second);
                result.setSpan(begin,second.span);
            }
         Out:
            return result;
        }

        #endregion

        #region Code generation

        public override JsonIr ToJSON()
        {
            return base.ToJSON()
                .AppendChild(new JsonIr("OP", multOp.value.ToString()));
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string sign = "";
            switch ( multOp.code )
            {
                case TokenCode.Multiply  : sign = "MULTIPLY *";  break;
                case TokenCode.Divide    : sign = "DIVIDE /";    break;
                case TokenCode.Remainder : sign = "REMAINDER %"; break;
            }
            reportCommon(sh, sign);
        }

        #endregion
    }

    /// <summary>
    /// 
    /// <syntax>
    /// additive:
    ///     multiplicative { ( + | - ) multiplicative }
    /// </syntax>
    /// </summary>
    public class ADDITIVE : BINARY
    {
        #region Structure

        /// <summary>
        /// + | -
        /// </summary>
        public Token addOp { get; private set; }

        #endregion

        #region Creation

        public ADDITIVE ( Token op, EXPRESSION op1, EXPRESSION op2 ) : base(op1,op2)
        {
            this.addOp = op;
        }

        #endregion

        #region Parser

        public static new EXPRESSION parse ( Token first, iSCOPE context )
        {
            EXPRESSION result = MULTIPLICATIVE.parse(first,context);
            while (true)
            {
                Token token = get();
                switch ( token.code )
                {
                    case TokenCode.Plus:
                    case TokenCode.Minus:
                        forget(); break;
                    default:
                        goto Out;
                }
                EXPRESSION second = MULTIPLICATIVE.parse(null,context);
                Span start = result.span;
                result = new ADDITIVE(token,result,second);
                result.setSpan(start,second.span);
            }
         Out:
            return result;
        }

        #endregion

        #region Code generation

        public override JsonIr ToJSON()
        {
            return base.ToJSON()
                .AppendChild(new JsonIr("OP", addOp.value.ToString()));
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string sign = "";
            switch ( addOp.code )
            {
                case TokenCode.Plus  : sign = "PLUS +";  break;
                case TokenCode.Minus : sign = "MINUS -"; break;
            }
            reportCommon(sh, sign);
        }

        #endregion
    }

    /// <summary>
    /// 
    /// <syntax>
    /// relational:
    ///     additive [ ( @lt; | @lt;= | > | >= | = | == | /= | /== ) additive ]
    ///     additive IN expression .. expression
    ///     additive IN range-type
    /// </syntax>
    /// </summary>
    public class RELATIONAL : BINARY
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public Token relOp { get; private set; }

        #endregion

        #region Creation

        public RELATIONAL(Token op, EXPRESSION op1, EXPRESSION op2) : base(op1,op2)
        {
            this.relOp = op;
        }

        #endregion

        #region Parser

        public static new EXPRESSION parse(Token first, iSCOPE context)
        {
            EXPRESSION result = ADDITIVE.parse(first, context);
            Token token = get();
            switch ( token.code )
            {
                case TokenCode.Less         :
                case TokenCode.LessEqual    :
                case TokenCode.Greater      :
                case TokenCode.GreaterEqual :
                case TokenCode.Equal        :
                case TokenCode.NotEqual     :
                case TokenCode.EqualEqual   :
                case TokenCode.NotEqualDeep :
                    forget(); break;
                case TokenCode.In :
                    forget();
                    EXPRESSION left = EXPRESSION.parse(null,context);
                    token = get();
                    if ( token.code == TokenCode.DotDot )
                    {
                        forget();
                        EXPRESSION right = EXPRESSION.parse(null,context);
                        RANGE_TYPE range = new RANGE_TYPE(left,right);
                        range.setSpan(left.span,right.span);
                        Span start = result.span;
                        IN_EXPRESSION r = new IN_EXPRESSION(token,result,range);
                        result.parent = r;
                        result = r;
                        result.setSpan(start,range.span);
                        goto Out;
                    }
                    else
                    {
                        // Something's wrong: right part of the in-expression
                        // should always be a range-type
                        goto Out;
                    }
                default:
                    goto Out;
            }
            EXPRESSION second = ADDITIVE.parse(null,context);
            Span start2 = result.span;
            result = new RELATIONAL(token,result,second);
            result.setSpan(start2,second.span);
         Out:
            return result;
        }

        #endregion

        #region Code generation

        public override JsonIr ToJSON()
        {
            return base.ToJSON()
                .AppendChild(new JsonIr("OP", relOp.value.ToString()));
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string sign = "";
            switch ( relOp.code )
            {
                case TokenCode.Less         : sign = "LESS <";             break;
                case TokenCode.LessEqual    : sign = "LESS EQUAL <=";      break;
                case TokenCode.Greater      : sign = "GREATER >";          break;
                case TokenCode.GreaterEqual : sign = "GREATER EQUAL >=";   break;
                case TokenCode.Equal        : sign = "EQUAL =";            break;
                case TokenCode.EqualEqual   : sign = "DEEP EQUAL ==";      break;
                case TokenCode.NotEqual     : sign = "NOT EQUAL /=";       break;
                case TokenCode.NotEqualDeep : sign = "DEEP NOT EQUAL /=="; break;
             // case TokenCode.In           : sign = "IN";                 break;
             // 'in' operator is treated as unary!!
            }
            reportCommon(sh,sign);
        }

        #endregion
    }

    /// <summary>
    /// 
    /// <syntax>
    /// logical:
    ///     relational { ( && (and) | & (and then) | || (or) | || or else) | ^ ) relational }
    /// </syntax>
    /// </summary>
    public class LOGICAL : BINARY
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public Token logOp { get; private set; }

        #endregion

        #region Creation

        public LOGICAL(Token op, EXPRESSION op1, EXPRESSION op2) : base(op1,op2)
        {
            this.logOp = op;
        }

        #endregion

        #region Parser

        public static new EXPRESSION parse(Token first, iSCOPE context)
        {
            EXPRESSION result = RELATIONAL.parse(first, context);
            if (result == null) return null;
            Span begin = result.span;

            while (true)
            {
                Token token = get();
                switch (token.code)
                {
                    case TokenCode.Ampersand :
                    case TokenCode.AmpAmp:
                    case TokenCode.Vertical :
                    case TokenCode.VertVert:
                        forget(); break;

                    case TokenCode.Identifier :
                        Category cat = new Category(CategoryCode.operatorr);
                        if ( token.image == "or" )
                        {
                            forget(); Token second = get();
                            if ( second.code == TokenCode.Else )
                            {
                                forget();
                                token = new Token(new Span(token,second),TokenCode.Vertical,"or else",cat);
                            }
                            else
                            {
                                token = new Token(token.span,TokenCode.VertVert,"or",cat);
                            }
                            break;
                        }
                        else if ( token.image == "and" )
                        {
                            forget(); Token second = get();
                            if ( second.code == TokenCode.Then )
                            {
                                forget();
                                token = new Token(new Span(token,second),TokenCode.Ampersand,"and then",cat);
                            }
                            else
                            {
                                token = new Token(token.span,TokenCode.AmpAmp,"and",cat);
                            }
                            break;
                        }
                        else
                            goto Out;
                    default:
                        goto Out;
                }
                EXPRESSION right = RELATIONAL.parse(null,context);
                result = new LOGICAL(token,result,right);
                result.setSpan(begin,right.span);
            }
         Out:
            return result;
        }

        #endregion

        #region Code generation

        public override JsonIr ToJSON()
        {
            return base.ToJSON()
                .AppendChild(new JsonIr("OP", logOp.value.ToString()));
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string sign = logOp.image.ToUpper();
            reportCommon(sh,sign);
        }

        #endregion
    }
}
