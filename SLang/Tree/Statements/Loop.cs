using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SLang
{
    /// <summary>
    /// 
    /// </summary>
    /// <syntax>
    /// Loop    : while Expression
    ///           loop
    ///           [ invariant PredicatesList ]
    ///           [ StatementList ]
    ///           [ variant PredicatesList ]
    ///           end [ loop ]
    ///           
    ///         | loop
    ///           [ invariant PredicatesList ]
    ///           [ StatementList ]
    ///           [ variant PredicatesList ]
    ///           [ while Expression ]
    ///           end [ loop ]
    /// </syntax>
    public class LOOP : STATEMENT, iSCOPE
    {
        #region Structure

        /// <summary>
        /// Null, if there is no counter and/or no while-prefix (eg, infinite loop).
        /// </summary>
        public VARIABLE loop_counter { get; private set; }

        /// <summary>
        /// If true then the while-prefix is at the beginning of the loop.
        /// Otherwise there is while-postfix at the end (or no while at all).
        /// </summary>
        public bool prefix { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public EXPRESSION while_clause { get; private set; }
        public void addw(EXPRESSION w) { while_clause = w; }

        /// <summary>
        /// 
        /// </summary>
        public List<EXPRESSION> invariants { get; private set; }
        public void addi(EXPRESSION e) { invariants.Add(e); }

        /// <summary>
        /// 
        /// </summary>
        public BODY body { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public List<EXPRESSION> variants { get; private set; }
        public void addv(EXPRESSION e) { variants.Add(e); }

        #endregion

        #region Constructors

        public LOOP() : base()
        {
            invariants = new List<EXPRESSION>();
            body = new BODY();
            variants = new List<EXPRESSION>();
        }

        #endregion

        #region iSCOPE implementation

     // public iSCOPE enclosing { get { return parent as iSCOPE; } set { } }

        public DECLARATION find_in_scope(string id)
        {
            if ( loop_counter == null ) return null;
            if ( loop_counter.name.identifier == id ) return loop_counter;
            return null;
        }

        public void add(ENTITY d) { loop_counter = d as VARIABLE; d.parent = this; }

        public ENTITY self { get { return this; } }

        #endregion

        #region Parser

        /// <summary>
        /// 
        /// </summary>
        /// <syntax>
        /// Loop    : while BooleanExpression
        ///           loop
        ///           [ invariant PredicateList ]
        ///           [ StatementList ]
        ///           [ variant PredicateList ]
        ///           end [ loop ]
        ///           
        ///         | loop
        ///           [ invariant PredicateList ]
        ///           [ StatementList ]
        ///           [ variant PredicateList ]
        ///           [ while BooleanExpression ]
        ///           end [ loop ]
        /// </syntax>
        /// <returns></returns>
        public static void parse(Token id, iSCOPE context)
        {
            Debug.Indent();
            Debug.WriteLine("Entering LOOP.parse");

            // If id != null, then 'id' is a loop label.
            // ':' after label is already parsed.
            LOOP loop = new LOOP();
            Context.enter(loop);

            Token token = get();
            Token begin = token;

            if ( token.code == TokenCode.While )
            {
                forget();
                loop.prefix = true;
                EXPRESSION whileExpr = EXPRESSION.parse(null,context);
                token = get();
                if ( token.code == TokenCode.Loop )
                {
                    forget();
                    loop.addw(whileExpr);
                    whileExpr.parent = loop;
                }
                else // Syntax error
                    error(token,"no-loop");

                token = get();
                if ( token.code == TokenCode.Invariant )
                {
                    forget();
                    parseInvariant(loop);
                }

                BODY body = new BODY();
                body.parent = loop;
                loop.body = body;

                Context.enter(body);
                token = get();
                BODY.parse(TokenCode.End,TokenCode.Variant,TokenCode.ERROR,body);
                body.setSpan(token, get());
                Context.exit();

                token = get();
                if ( token.code == TokenCode.Variant )
                {
                    forget();
                    parseVariant(loop);
                }
            }
            else if ( token.code == TokenCode.Loop )
            {
                loop.prefix = false;
                forget();
                token = get();
                if ( token.code == TokenCode.Invariant )
                {
                    forget();
                    parseInvariant(loop);
                }

                BODY body = new BODY();
                body.parent = loop;
                loop.body = body;

                Context.enter(body);
                token = get();
                BODY.parse(TokenCode.End, TokenCode.Variant, TokenCode.While, body);
                body.setSpan(token,get());
                Context.exit();

                token = get();
                if ( token.code == TokenCode.Variant )
                {
                    forget();
                    parseVariant(loop);
                }
                token = get();
                if ( token.code == TokenCode.While )
                {
                    forget();
                    EXPRESSION whileExpr = EXPRESSION.parse(null,context);
                    loop.addw(whileExpr);
                    whileExpr.parent = loop;
                }
            }
            else // Compiler error
                error(token,"system-bug");

            token = get();
            if ( token.code != TokenCode.End ) // Syntax error
                error(token,"no-end","loop");
            else
            {
                forget();
                token = get();
                if ( token.code == TokenCode.Loop )
                    forget();
            }

            if ( loop != null )
                loop.setSpan(begin,token);

            context.add(loop);
            Context.exit();

            Debug.WriteLine("Exiting LOOP.parse");
            Debug.Unindent();
        }

        private static void parseInvariant(LOOP result)
        {
            while ( true )
            {
                EXPRESSION inv = EXPRESSION.parse(null,result);
                if ( inv == null ) return;
                result.addi(inv);
                inv.parent = result;

                Token token = get();
                if ( token.code == TokenCode.Comma ) forget();
            }
        }

        private static void parseVariant(LOOP result)
        {
            while ( true )
            {
                EXPRESSION inv = EXPRESSION.parse(null,result);
                if ( inv == null ) return;
                result.addv(inv);
                inv.parent = result;

                Token token = get();
                if ( token.code == TokenCode.Comma || wasEOL ) forget();
            }
        }

        #endregion

        #region Verification

        public override bool check()
        {
            if ( while_clause != null && !while_clause.check() ) return false;
            foreach ( EXPRESSION e in invariants )
                if ( !e.check() ) return false;
            if ( !body.check() ) return false;
            foreach ( EXPRESSION e in variants )
                if ( !e.check() ) return false;
            return true;
        }

        public override bool verify()
        {
            if ( while_clause != null && !while_clause.verify() ) return false;
            foreach ( EXPRESSION e in invariants )
                if ( !e.verify() ) return false;
            if (!body.verify()) return false;
            foreach ( EXPRESSION e in variants )
                if ( !e.verify() ) return false;
            // Anything else to verify?
            return true;
        }

        #endregion

        #region Generation

        public override bool generate() { return true; }

        public override string ToJSON()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string common = commonAttrs();
            string r = common + shift(sh);
            if ( prefix )
            {
                System.Console.WriteLine(r + "WHILE");
                while_clause.report(sh+constant);
            }
            else
            {
                System.Console.WriteLine(r + "LOOP");
            }
            if ( invariants.Count > 0 )
            {
                System.Console.WriteLine(common.Length+sh+"INVARIANTS");
                foreach ( EXPRESSION expression in invariants )
                    expression.report(sh+constant);
            }

            body.report(sh+constant);

            if ( variants.Count > 0 )
            {
                System.Console.WriteLine(common.Length+sh+"VARIANTS");
                foreach ( EXPRESSION expression in variants )
                    expression.report(sh+constant);
            }
            if ( !prefix && while_clause != null )
            {
                System.Console.WriteLine(common.Length+sh+"WHILE");
                while_clause.report(sh+constant);
            }
        }

        #endregion
    }
}
