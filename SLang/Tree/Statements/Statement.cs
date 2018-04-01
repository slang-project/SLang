using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SLang
{
    /// <summary>
    /// 
    /// </summary>
    /// <syntax>
    /// Statement
    ///     : Assignment
    ///     | LocalAttributeDeclaration
    ///     | ( [ Label ":" ] IfCase )
    ///     | ( [ Label ":" ] Loop )
    ///     | break [ Label ]
    ///     | FeatureCallOrCreation       
    ///     | ? Identifier
    ///     | check PredicatesList end
    ///     | return Expression   
    ///     | Try 
    ///     | raise [Expression]
    /// </syntax>
    public abstract class STATEMENT : ENTITY
    {
        /// <summary>
        /// The function processes all kinds of statements including
        /// assignments, calls, and variable/constant declarations.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>true, if any construct (a statement, or a simple declaration)
        /// was processed, and false otherwise.</returns>
        public static bool parse(iSCOPE context, TokenCode stop1, TokenCode stop2, TokenCode stop3)
        {
            Debug.Indent();
            Debug.WriteLine("Entering STATEMENT.parse");

            bool result = true;

            Token token = get();
            Token begin = token;
            if ( token.code == stop1 && stop1 != TokenCode.ERROR ) goto Finish;
            if ( token.code == stop2 && stop2 != TokenCode.ERROR ) goto Finish;
            if ( token.code == stop3 && stop3 != TokenCode.ERROR ) goto Finish;

            switch ( token.code )
            {
             // case TokenCode.Pure: -- doesn't make any sense
                case TokenCode.Safe:
                case TokenCode.Routine:
                    ROUTINE.parse(token,false,false,false,0,context);
                    break;

                case TokenCode.If:
                    IF.parse(context);
                    break;
                
                case TokenCode.While:
                case TokenCode.Loop:
                    LOOP.parse(null,context);
                    break;

                // Break
                //     : break [ Label ]
                //
                // Label
                //     : Identifier
                case TokenCode.Break:
                    BREAK.parse(context);
                    break;

                // Statement
                //     : ...
                //     | raise [ Expression ]
                case TokenCode.Raise :
                    forget();
                    RAISE.parse(token.span,context);
                    break;

                // Statement
                //     : ...
                //     | check PredicatesList end
                //     | ...
                case TokenCode.Check:
                    CHECK.parse(context);
                    break;

                // Statement
                //     : ...
                //     | return [ Expression ]
                //     | ...
                case TokenCode.Return:
                    forget();
                    EXPRESSION expr = EXPRESSION.parse(null,context); // can be null
                    RETURN ret = new RETURN(expr);
                    if ( expr != null )
                    {
                        expr.parent = ret;
                        ret.setSpan(begin.span, expr.span);
                    }
                    else
                        ret.setSpan(begin);

                    context.add(ret);
                    break;

                // Statement
                //     : ...
                //     | Try
                //     | ...
                //
                // Try
                //     : try { Statement } Catches [ Else ] end
                //
                // Catches
                //     : catch [ "(" [ Identifier ":" ] Type ")" ] { Statement }
                //
                // Else
                //     : else { Statement }
                case TokenCode.Try:
                    TRY.parse(context);
                    break;

                // Statement
                //     : ...
                //     | ? Identifier
                //     | ...
                //
                case TokenCode.Question:

                    break;

                case TokenCode.Init:
                    // Initializer call
                    forget();
                    Token start = token;

                    DECLARATION init = Context.find(INITIALIZER.initName);
                    EXPRESSION initRef;
                    if ( init != null ) initRef = new REFERENCE(init);
                    else                initRef = new UNRESOLVED(context,new IDENTIFIER(INITIALIZER.initName));

                    CALL call = new CALL(initRef);
                    token = expect(TokenCode.LParen);
                    while ( true )
                    {
                        EXPRESSION argument = EXPRESSION.parse(null,context);
                        call.add(argument);
                        argument.parent = call;

                        token = get();
                        if ( token.code == TokenCode.Comma ) { forget(); continue; }
                        break;
                    }
                    token = expect(TokenCode.RParen);
                    call.setSpan(start.span, token.span);
                    context.add(call);
                    break;

                case TokenCode.Identifier:
                {
                    // Several different cases:
                    //   - a label in front of while/loop
                    //   - a declaration
                    //   - a statement

                    EXPRESSION attempt = EXPRESSION.parse(null,context);
                    if ( attempt is UNRESOLVED )
                    {
                        // Might be a label of a declaration...
                        Token idd = new Token(attempt.span, TokenCode.Identifier,
                                              (attempt as UNRESOLVED).name.identifier,
                                              new Category(CategoryCode.identifier));
                        token = get();
                        switch ( token.code )
                        {
                            case TokenCode.Is :
                            case TokenCode.Comma :
                                // This is definitely a declaration
                                forget();
                                VARIABLE.parse(false,false,false,false,idd,token,context);
                                break;
                            case TokenCode.Colon :
                                forget();
                                Token token2 = get();
                                if ( token2.code == TokenCode.While || token2.code == TokenCode.Loop )
                                    // This is a real label! Don't 'forget()'.
                                    LOOP.parse(idd,context);
                                else
                                    // This is definitely a variable declaration.
                                    // Don't forget()
                                    VARIABLE.parse(false,false,false,false,idd,token,context);
                                break;
                            default:
                                // Nothing to do; just going further
                                break;
                        }
                    }
                    // 'attempt' is something else: a call or the left part of an assignment
                    token = get();
                    if ( token.code == TokenCode.Assign )
                    {
                        forget();
                        EXPRESSION right = EXPRESSION.parse(null,context);
                        ASSIGNMENT res = new ASSIGNMENT(attempt,right);
                        res.setSpan(attempt.span,right.span);
                        context.add(res);
                    }
                    else
                    {
                        if ( !(attempt is CALL) ) // something's wrong
                            result = false;
                        context.add(attempt);
                    }
                    break;
                }
                case TokenCode.Const:
                    // Something like
                    //     const a is 5... 
                    // OR
                    //     const is a, b, ... end
                    forget(); token = get();
                    if ( token.code == TokenCode.Is )
                    {
                        forget();
                        CONSTANT.parse(context);
                    }
                    else
                        VARIABLE.parse(false,false,true,false,null,null,context);
                    break;

                default:
                    // Something else, e.g., (a... or this...
                    // Either a function call or an assignment.
                    //
                    //     this := ...
                    //     (if cond then a else b).f ...
                    //     ^

                    EXPRESSION e = EXPRESSION.parse(null,context);
                    if ( e == null )
                    {
                        // Either an error or just the end of statement sequence
                        result = false;
                        break;
                    }
                    token = get();
                    if ( token.code == TokenCode.Assign )
                    {
                        forget();
                        EXPRESSION right = EXPRESSION.parse(null,context);
                        ASSIGNMENT assignment = new ASSIGNMENT(e,right);
                        assignment.setSpan(e.span,right.span);
                        context.add(assignment);
                    }
                    else
                    {
                        context.add(e);
                    }
                    break;
            }
         Finish:
            Debug.WriteLine("Exiting STATEMENT.parse");
            Debug.Unindent();

            return result;
        }
    }

    /// <summary>
    /// The class represents sequences of language constructs
    /// that can be bodies of routines, loop bodies, tren- and else-parts
    /// of if statements etc.
    /// </summary>
    public class BODY : STATEMENT, iSCOPE
    {
        #region Structure

        /// <summary>
        /// Actually, this is the list of DECLARATIONs and/or STATEMENTs.
        /// </summary>
        public List<ENTITY> body { get; private set; }
        public void add(ENTITY e) { body.Add(e); e.parent = this; }

        #endregion

        #region Constructors

        public BODY() { body = new List<ENTITY>(); }

        #endregion

        #region iSCOPE implementation

        public iSCOPE enclosing { get { return parent as iSCOPE; } set { parent = value as ENTITY; } }

        public void add ( DECLARATION d) { body.Add(d); }

        public DECLARATION find_in_scope(string id)
        {
            foreach ( ENTITY e in body )
            {
                if ( !(e is DECLARATION) ) continue;
                if ( (e as DECLARATION).name.identifier == id ) return e as DECLARATION;
            }
            return null;
        }

        public ENTITY self { get { return this; } }

        #endregion

        #region Parser

        public static void parse ( TokenCode stop1, TokenCode stop2, TokenCode stop3, iSCOPE context )
        {
            Debug.Indent();
            Debug.WriteLine("Entering BODY.parse");

            Token token;
            Token start = get();
            while (true)
            {
                bool res = STATEMENT.parse(context,stop1,stop2,stop3);

                if ( !res )
                {
                    // Neither a statement nor a simple declaration.
                    // Perhaps, a nested/local function?
                    token = get();
                    switch ( token.code )
                    {
                        case TokenCode.Routine:
                        case TokenCode.Safe:
                        case TokenCode.Pure:
                            forget();
                            int pure_safe = 0;
                            switch ( token.code )
                            {
                                case TokenCode.Pure: pure_safe = 1; break;
                                case TokenCode.Safe: pure_safe = 2; break;
                            }
                            ROUTINE.parse(null,false,false,false,pure_safe,context);
                            break;

                        case TokenCode.Unit:
                        case TokenCode.Ref:
                        case TokenCode.Val:
                            // A _local_ unit???
                            UNIT.parse(context);
                            break;

                        default:
                            // What's this?
                            break;
                    }
                }

                token = get();
                if ( token.code == TokenCode.Semicolon /*|| wasEOL*/ ) forget();
                if ( token.code == stop1 ) break; // don't 'forget()'
                if ( stop2 != TokenCode.ERROR && token.code == stop2 ) break; // don't 'forget()'
                if ( stop3 != TokenCode.ERROR && token.code == stop3 ) break; // don't 'forget()'
            }

            BODY body = context as BODY;
            if ( body == null ) { /* A system error */ }
            else body.setSpan(start.span,token.span);

            Debug.WriteLine("Exiting BODY.parse");
            Debug.Unindent();
        }

        #endregion

        public override bool check()
        {
            throw new NotImplementedException();
        }

        public override bool verify()
        {
            throw new NotImplementedException();
        }

        #region Reporting

        public override void report(int sh)
        {
            System.Console.WriteLine(commonAttrs() + shift(sh) + "BODY");
            foreach (ENTITY entity in body)
                entity.report(sh+constant);
        }

        #endregion

        #region Generation

        public override bool generate()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class STMT_IF_THEN : STATEMENT
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public EXPRESSION condition { get; private set; }

        public void add(EXPRESSION c) { condition = c; condition.parent = this; }

        /// <summary>
        /// 
        /// </summary>
        public BODY thenPart { get; private set; }

        public void add(BODY body) { thenPart = body; thenPart.parent = this; }

        #endregion

        #region Creation

        public STMT_IF_THEN() { }

        #endregion

        public override bool check()
        {
            throw new NotImplementedException();
        }
        public override bool verify()
        {
            throw new NotImplementedException();
        }

        #region Reporting

        /// <summary>
        /// We don't need this common version for the class
        /// </summary>
        /// <param name="sh"></param>
        public override void report(int sh) { }

        public int reportIfThen(int sh, string ElseIf)
        {
            string common = null;
            if (ElseIf != null) common = ElseIf;
            else                common = commonAttrs();
            string r = common + shift(sh) + (ElseIf == null ? "ELSE IF " : "IF ");
            System.Console.WriteLine(r);

            condition.report(sh+constant);

            System.Console.WriteLine(shift(common.Length+sh) + "THEN");
            thenPart.report(sh+constant);

            return common.Length;
        }

        #endregion

        #region Generation

        public override bool generate()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class IF : STATEMENT
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public List<STMT_IF_THEN> ifThenParts { get; private set; }
        public void add(STMT_IF_THEN it) { ifThenParts.Add(it); it.parent = this; }

        /// <summary>
        /// 
        /// </summary>
        public BODY elsePart { get; private set; }
        public void add(BODY ep) { elsePart = ep; elsePart.parent = this; }

        #endregion

        #region Creation

        public IF() { ifThenParts = new List<STMT_IF_THEN>(); }

        #endregion

        #region Parser

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public static void parse(iSCOPE context)
        {
            Debug.Indent();
            Debug.WriteLine("Entering IF.parse");

            IF if_stmt = new IF();

            Token token = get(); // 'if'
            Token start = token;

            forget();
            EXPRESSION condition = EXPRESSION.parse(null, context);
            expect(TokenCode.Then);

            BODY body = new BODY();
            Context.enter(body);
            token = get();
            BODY.parse(TokenCode.Else,TokenCode.Elsif,TokenCode.End,body);
            body.setSpan(token,get());
            Context.exit();

            STMT_IF_THEN it = new STMT_IF_THEN();
            it.add(body);
            it.add(condition);

            if_stmt.add(it);

            while ( true )
            {
                token = get();
                if ( token.code != TokenCode.Elsif ) break;
                forget();

                condition = EXPRESSION.parse(null, context);
                expect(TokenCode.Then);

                body = new BODY();
                Context.enter(body);
                token = get();
                BODY.parse(TokenCode.Else, TokenCode.Elsif, TokenCode.End, body);
                body.setSpan(token,get());
                Context.exit();

                it = new STMT_IF_THEN();
                it.add(body);
                it.add(condition);

                if_stmt.add(it);
            }
            token = get();
            if ( token.code == TokenCode.Else )
            {
                forget();
                body = new BODY();
                Context.enter(body);
                token = get();
                BODY.parse(TokenCode.End,TokenCode.ERROR,TokenCode.ERROR,body);
                body.setSpan(token,get());
                Context.exit();

                if_stmt.add(body);
            }
            token = expect(TokenCode.End);
            context.add(if_stmt);

            if_stmt.parent = context.self;
            if_stmt.setSpan(start,token);

            Debug.WriteLine("Exiting IF.parse");
            Debug.Unindent();
        }

        #endregion

        public override bool check() { return true; }
        public override bool verify() { return true; }

        #region Generation

        public override bool generate() { return true; }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string ElseIf = commonAttrs();
            int sh2 = 0;
            foreach ( STMT_IF_THEN IfThenPart in ifThenParts )
            {
                sh2 = IfThenPart.reportIfThen(sh,ElseIf);
                ElseIf = null;
            }
            if ( elsePart != null )
            {
                System.Console.WriteLine(shift(sh2+sh) + "ELSE");
                elsePart.report(sh+constant);
            }
        }

        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    /// <syntax>
    /// Statement
    ///     : ...
    ///     | check PredicateList end
    ///     | ...
    /// </syntax>
    public class CHECK : STATEMENT
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public List<EXPRESSION> predicates { get; private set; }
        public void add(EXPRESSION e) { predicates.Add(e); }

        #endregion

        #region Constructors

        public CHECK()
        {
            predicates = new List<EXPRESSION>();
        }

        #endregion

        #region Parser

        // Statement
        //     : ...
        //     | check PredicateList end
        //     | ...
        public static CHECK parse(iSCOPE context)
        {
            Debug.Indent();
            Debug.WriteLine("Entering CHECK.parse");

            CHECK result = new CHECK();

            Token token = get();
            Token begin = token;
            if ( token.code != TokenCode.Check ) { /* compiler error */ return null; }
            forget();

            while ( true )
            {
                EXPRESSION predicate = EXPRESSION.parse(null,context);
                if ( predicate != null )
                {
                    result.add(predicate);
                    predicate.parent = result;
                }
                else
                {
                    token = get();
                    if ( token.code == TokenCode.End ) { forget(); break; }
                }
                token = get();
                switch (token.code)
                {
                    case TokenCode.Comma:
                    case TokenCode.Semicolon: forget(); break;
                    default: break;
                }
            }

            Debug.WriteLine("Exiting CHECK.parse");
            Debug.Unindent();

            result.setSpan(begin,token);
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

        public override bool generate() { return true; }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string r = commonAttrs() + shift(sh) + "CHECK";
            System.Console.WriteLine(r);

            foreach ( EXPRESSION predicate in predicates )
                predicate.report(sh + constant);
        }

        #endregion

    }

    public class RAISE : STATEMENT
    {
        /// <summary>
        /// 
        /// </summary>
        public EXPRESSION expression { get; private set; }
        
        public RAISE(EXPRESSION e) : base() { expression = e; }

        #region Parser

        public static void parse ( Span begin, iSCOPE context )
        { 
            EXPRESSION expr = EXPRESSION.parse(null,context); // can be null
            RAISE raise = new RAISE(expr);
            if ( expr != null )
            {
                expr.parent = raise;
                raise.setSpan(begin,expr.span);
            }
            else
                raise.setSpan(begin);

            context.add(raise);
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
            string r = commonAttrs() + shift(sh) + "RAISE";
            System.Console.WriteLine(r);

            if ( expression != null )
                expression.report(sh+constant);
        }

        #endregion

    }

    public class RETURN : STATEMENT
    {
        /// <summary>
        /// 
        /// </summary>
        public EXPRESSION expression { get; private set; }

        public RETURN(EXPRESSION e) : base() { expression = e;  }

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
            string r = commonAttrs() + shift(sh) + "RETURN";
            System.Console.WriteLine(r);

            if ( expression != null )
                expression.report(sh+constant);
        }

        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    /// <syntax>
    /// Statement
    ///     : ...
    ///     | break [ Label ]
    ///     | ...
    /// </syntax>
    public class BREAK : STATEMENT
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public string label { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public STATEMENT labeled { get; private set; }

        #endregion

        #region Constructors

        public BREAK(string label) : base()
        {
            this.label = label;
        }

        #endregion

        #region Parser

        public static BREAK parse ( iSCOPE context )
        {
            return null;
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
            string r = commonAttrs() + shift(sh) + "BREAK ";
            if ( label != null ) r += label;
            if ( labeled != null ) r += ":" + labeled.unique;
            System.Console.WriteLine(r);
        }

        #endregion
    }

    public class ASSIGNMENT : STATEMENT
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

        public ASSIGNMENT(EXPRESSION l, EXPRESSION r)
        {
            left = l; left.parent = this;
            right = r; right.parent = this;
        }

        #endregion

        public override bool check()
        {
            throw new NotImplementedException();
        }
        public override bool verify()
        {
            throw new NotImplementedException();
        }

        #region Reporting

        public override void report(int sh)
        {
            string common = commonAttrs();
            string r = common + shift(sh) + "ASSIGNMENT";
            System.Console.WriteLine(r);

            System.Console.WriteLine(shift(common.Length+sh)+"LEFT");
            left.report(sh + constant);

            System.Console.WriteLine(shift(common.Length+sh) + "RIGHT");
            right.report(sh + constant);
        }

        #endregion

        #region Generation

        public override bool generate()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
