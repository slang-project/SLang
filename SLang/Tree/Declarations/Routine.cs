using SLang.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SLang
{
    public class ROUTINE : DECLARATION, iSCOPE
    {
        #region Structure

     // public IDENTIFIER name -- from the base class

        /// <summary>
        /// 
        /// </summary>
        public IDENTIFIER alias { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        private int PureSafe;
        public bool isPure { get { return PureSafe == 1; } }
        public bool isSafe { get { return PureSafe == 2; } }

        public bool isAbstract { get; private set; }

        public bool isForeign { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool isOverride { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public TYPE type { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public List<FORMAL_GENERIC> genericParameters { get; private set; }
        public void add(FORMAL_GENERIC g) { genericParameters.Add(g); }

        /// <summary>
        /// 
        /// </summary>
        public List<ENTITY> parameters { get; private set; }
        // public void add(DECLARATION p) { parameters.Add(p); } -- see iSCOPE interface above

        /// <summary>
        /// 
        /// </summary>
        public List<EXPRESSION> preconditions { get; private set; }
        public void addPre(EXPRESSION pre) { preconditions.Add(pre); }

        public bool requireElse { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public List<EXPRESSION> postconditions { get; private set; }
        public void addPost(EXPRESSION post) { postconditions.Add(post); }

        public bool ensureThen { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public BODY routineBody;

        #endregion

        #region iSCOPE implementation

        public ENTITY self { get { return this; } }

        public void add(ENTITY d) { parameters.Add(d /*as DECLARATION*/); }

        public DECLARATION find_in_scope ( string id )
        {
            foreach(ENTITY d in parameters)
            {
                if ( !(d is DECLARATION) ) continue;
                DECLARATION dd = d as DECLARATION;
                if ( dd.name.identifier == id ) return dd;
            }
            foreach(FORMAL_GENERIC g in genericParameters)
                if ( g.name.identifier == id ) return g;
            return null;
        }

    //  public void show(int sh)
    //  {
    //      string indentation = shift(sh);
    //      System.Console.WriteLine("{0}ROUTINE {1}",indentation,name.identifier);
    //      foreach ( FORMAL_GENERIC g in genericParameters )
    //          System.Console.WriteLine("    {0}{1}",indentation,g.name.identifier);
    //      foreach (ENTITY p in parameters)
    //          if ( p is VARIABLE )
    //             System.Console.WriteLine("    {0}{1}", indentation, (p as VARIABLE).name.identifier);
    //   // routineBody.show(sh+4); -- not necessary
    //  }

        #endregion

        #region Creation

        public ROUTINE(int ps, bool o, Token name) : this(ps, o, name.image) 
        {
        }

        public ROUTINE(int ps, bool o, string name) : base(name)
        {
            PureSafe = ps;
            isOverride = o;

            this.name = new IDENTIFIER(name);
            genericParameters = new List<FORMAL_GENERIC>();
            parameters = new List<ENTITY>();
            preconditions = new List<EXPRESSION>();
            postconditions = new List<EXPRESSION>();
            requireElse = false;
            ensureThen = false;
            routineBody = new BODY();
        }

        #endregion

        #region Parser

        public static Token detectOperatorSign ( Token token )
        {
            Span span;
            string image;
            TokenCode code = TokenCode.Identifier;

            switch ( token.code )
            {
                case TokenCode.Assign:       //  :=
                case TokenCode.Equal:        //  =
                case TokenCode.EqualEqual:   //  ==
                case TokenCode.NotEqual:     //  /=
                case TokenCode.NotEqualDeep: //  /==
                case TokenCode.Less:         //  <
                case TokenCode.LessEqual:    //  <=
                case TokenCode.Greater:      //  >
                case TokenCode.GreaterEqual: //  >=
                case TokenCode.Tilde:        //  ~
                case TokenCode.Question:     //  ?
                case TokenCode.Vertical:     //  |
                case TokenCode.Caret:        //  ^
                case TokenCode.Plus:         //  +
                case TokenCode.Minus:        //  -
                case TokenCode.Multiply:     //  *
                case TokenCode.Divide:       //  /
                case TokenCode.Remainder:    //  %
                case TokenCode.Ampersand:    //  &
             // case TokenCode.Call:         //  ()
                case TokenCode.Power:        //  **
                    forget();
                    span = new Span(token.span);
                    image = token.image;
                    break;

                // Additional (reserved) operator signs

             // case TokenCode.BackSlash:       //  \
             // case TokenCode.BackBackSlash:   //  \\
             // case TokenCode.DblArrow:        //  <=>
             // case TokenCode.PlusEqual:       //  +=
             // case TokenCode.MinusEqual:      //  -=
             // case ...

                case TokenCode.LParen:        //  (, and ) is expected after it
                    forget();
                    Token right = expect(TokenCode.RParen);
                    span = new Span(token.span,right.span);
                    image = "()";
                    break;

                default:
                    // Not an operator sign
                    span = new Span(token.span);
                    image = token.image;
                    code = TokenCode.ERROR;
                    break;
            }
            return new Token(span,code,image,new Category(CategoryCode.identifier));
        }

        public static void parse(Token routineName, bool hidden, bool final, bool isOverride, int pure_safe, iSCOPE context)
        {
            ROUTINE routine = null;

            if ( routineName.code == TokenCode.Init )
            {
                Debug.Indent();
                Debug.WriteLine("Entering INITIALIZER.parse");

                routine = new INITIALIZER();
                goto Init;
            }

            Debug.Indent();
            Debug.WriteLine("Entering ROUTINE.parse (" + routineName.image + ")");

            string image = routineName.image;
            Token token = get(); // What's after the routine name?

            switch ( routineName.image )
            {
                case "and" :
                    if ( token.code == TokenCode.Then )
                    {
                        forget();
                        image += " then";
                        Span span = new Span(routineName.span,token.span);
                        routineName = new Token(span,TokenCode.Identifier,image,new Category(CategoryCode.identifier));
                    }
                    break;

                case "or" :
                    token = get();
                    if ( token.code == TokenCode.Else )
                    {
                        forget();
                        image += " else";
                        Span span = new Span(routineName.span,token.span);
                        routineName = new Token(span,TokenCode.Identifier,image,new Category(CategoryCode.identifier));
                    }
                    break;
            }
            routine = new ROUTINE(pure_safe,isOverride,routineName);
         Init:
            routine.setSpecs(hidden,final);
            Context.enter(routine);

            if ( routineName.code == TokenCode.Init ) goto Init2;

            token = get();
            if ( token.code == TokenCode.Alias )
            {
                forget();
                token = expect(TokenCode.Identifier);
                routine.alias = new IDENTIFIER(token.image);
            }

            if ( token.code == TokenCode.LBracket )
            {
                // Generic routine
                forget();
                while ( true )
                {
                    var generic = FORMAL_GENERIC.parse(context);
                    routine.add(generic);

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
         
         Init2:
            token = get();
            if ( token.code == TokenCode.LParen )
            {
                forget(); token = get();
                if ( token.code == TokenCode.RParen )
                {
                    // Empty parameter list
                    forget();
                    goto Weiter;
                }
                while ( true )
                { 
                    VARIABLE.parse(false,false,false,false,null,null,routine);

                    token = get();
                    if ( token.code == TokenCode.Comma ) { forget(); continue; }
                    if ( token.code == TokenCode.Semicolon ) { forget(); continue; }
                    break;
                }
                expect(TokenCode.RParen);
            }

          Weiter:
            token = get();
            if ( token.code == TokenCode.Colon )
            {
                forget();

                bool ref_val, conc;  // TEMP SOLUTION
                Span span2;
                routine.type = VARIABLE.parseTypeSpecifier(routine, out ref_val, out conc, out span2);
                if ( routine.type != null )
                {
                    routine.type.parent = routine;
                    routine.type.setSpan(span2);
                }
            }

            token = get();
            if ( token.code == TokenCode.Require )
            {
                forget(); token = get();
                if ( token.code == TokenCode.Else )
                {
                    forget(); routine.requireElse = true;
                }
                while ( true )
                {
                    EXPRESSION precondition = EXPRESSION.parse(null,routine);
                    routine.addPre(precondition);
                    precondition.parent = routine;

                    token = get();
                    if ( token.code == TokenCode.Is || token.code == TokenCode.Arrow )
                        break;
                }
            }

            if ( token.code == TokenCode.Arrow )
            {
                forget();

                BODY body = new BODY();
                routine.routineBody = body;
                body.parent = routine;

                Context.enter(body);
                EXPRESSION expression = EXPRESSION.parse(null,body);
                RETURN ret = new RETURN(expression);
                expression.parent = ret;
                ret.setSpan(expression.span);

                ret.parent = body;
                body.add(ret);
                body.setSpan(ret.span);

                Context.exit();
            }
            else if ( token.code == TokenCode.Is )
            {
                forget(); token = get();
                if ( token.code == TokenCode.Abstract )
                {
                    forget();
                    routine.isAbstract = true;
                }
                else if ( token.code == TokenCode.Foreign )
                {
                    forget();
                    routine.isForeign = true;
                }
                else
                {
                    BODY body = new BODY();
                    body.parent = routine;
                    routine.routineBody = body;

                    Context.enter(body);
                    BODY.parse(TokenCode.End,TokenCode.Ensure,TokenCode.ERROR,body);
                    Context.exit();
                }
                token = get();
                if ( token.code == TokenCode.Ensure )
                {
                    forget();
                    token = get();
                    if ( token.code == TokenCode.Then )
                    {
                        forget(); routine.ensureThen = true;
                    }
                    ENTITY.weAreWithinEnsure = true;
                    while ( true )
                    {
                        EXPRESSION postcondition = EXPRESSION.parse(null,routine);
                        routine.addPre(postcondition);
                        postcondition.parent = routine;

                        token = get();
                        if ( token.code == TokenCode.End )
                        {
                            forget(); break;
                         }
                    }
                    ENTITY.weAreWithinEnsure = false;
                }
                else if ( !routine.isAbstract && !routine.isForeign )
                    expect(TokenCode.End);
            }
            token = get();
            if ( token.code == TokenCode.Semicolon ) forget();

            Context.exit();
            context.add(routine);

            routine.parent = context.self;
            routine.setSpan(routineName,token);

            if ( routineName.code == TokenCode.Init )
                Debug.WriteLine("Exiting INITIALIZER.parse");
            else
                Debug.WriteLine("Exiting ROUTINE.parse");
            Debug.Unindent();
        }

        #endregion

        #region Verify

        public override bool check()
        {
            return true;
        }

        public override bool verify()
        {
            return true;
        }

        #endregion

        #region Code generation

        public override bool generate() { return true; }

        public override JsonIr ToJSON()
        {
            return base.ToJSON()
                .AppendChild(alias.ToJSON())
                //.AppendChild(new JsonIr("PURE_SAFE_SPEC", isPure ? "pure" : isSafe ? "safe" : null))
                //.AppendChild(new JsonIr("ABSTRACT_SPEC", isAbstract ? "abstract" : null))
                .AppendChild(new JsonIr("FOREIGN_SPEC", isForeign ? "foreign" : null))
                //.AppendChild(new JsonIr("OVERRIDE_SPEC", isOverride ? "override" : null))
                .AppendChild(type.ToJSON())
                //.AppendChild(JsonIr.ListToJSON(genericParameters))
                .AppendChild(JsonIr.ListToJSON(parameters))
                .AppendChild(
                    new JsonIr("PRECONDITION", requireElse ? "require else" : null)
                        .AppendChild(JsonIr.ListToJSON(preconditions))
                    )
                .AppendChild(
                    new JsonIr("POSTCONDITION", ensureThen ? "ensure then" : null)
                        .AppendChild(JsonIr.ListToJSON(postconditions))
                    )
                .AppendChild(routineBody.ToJSON());
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string n = (this is INITIALIZER) ? "INIT " : "ROUTINE ";
            string a = ( genericParameters.Count > 0 ) ? "GENERIC " : "";
            a += isPure ? "PURE " : "";
            a += isSafe ? "SAFE " : "";
            a += isAbstract ? "ABSTRACT " : "";
            a += isForeign ? "FOREIGN " : "";
            a += isOverride ? "OVERRIDE " : "";
            string common = commonAttrs();
            string r = common + shift(sh) + a + n + name.identifier;
            if ( alias != null ) r += " ALIAS " + alias.identifier;
            System.Console.WriteLine(r);

            if ( type != null )
            {
                type.report(sh+constant);
            }
            if ( genericParameters.Count > 0 )
            {
                System.Console.WriteLine(shift(common.Length+sh)+"GENERIC PARAMETERS");
                foreach ( FORMAL_GENERIC generic in genericParameters )
                    generic.report(sh+constant);
            }
            if ( parameters.Count > 0 )
            {
                System.Console.WriteLine(shift(common.Length+sh)+"PARAMETERS");
                foreach ( ENTITY parameter in parameters )
                    parameter.report(sh+constant);
            }
            if ( preconditions.Count > 0 )
            {
                string pe = "PRECONDITIONS require";
                if ( this.requireElse ) pe += " else";
                System.Console.WriteLine(shift(common.Length+sh)+pe);
                foreach ( EXPRESSION precondition in preconditions )
                    precondition.report(sh+constant);
            }

            if ( routineBody != null && routineBody.body.Count > 0 )
            {
                routineBody.report(sh);
            }

            if ( postconditions.Count > 0 )
            {
                string pt = "POSTCONDITIONS ensure";
                if ( this.ensureThen ) pt += " then";
                System.Console.WriteLine(shift(common.Length + sh) + pt);
                foreach ( EXPRESSION postcondition in postconditions )
                    postcondition.report(sh+constant);
            }
        }

        #endregion
    }

    public class INITIALIZER : ROUTINE
    {
        public static string initName = "$init";

        #region Creation

        public INITIALIZER() : base(0, false, initName) { }

        #endregion
    }

}