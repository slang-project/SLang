using SLang.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SLang
{
    public abstract class DECLARATION : ENTITY
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public bool isHidden { get;  private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool isFinal { get; private set; }

        public void setSpecs(bool h, bool f)
        {
            isHidden = h;
            isFinal = f;
        }

        /// <summary>
        /// Each declaration has a name.
        /// </summary>
        public IDENTIFIER name { get; protected set; }

        #endregion

        #region Constructors

        public DECLARATION ( Token n = null ) { if (n != null ) name = new IDENTIFIER(n.image); }
        public DECLARATION ( string n ) {  name = new IDENTIFIER(n); }
        public DECLARATION ( IDENTIFIER n ) { name = n; }

        #endregion

        public override JsonIr ToJSON()
        {
            return new JsonIr(GetType())
                .AppendChild(ToJSON(name));
        }

        public static bool parse(iSCOPE context)
        {
            Debug.Indent();
            Debug.WriteLine("Entering DECLARATION.parse");

            bool result = true;

            bool isHidden = false;
            bool isFinal = false;
            int pure_safe = 0;
            bool isOverride = false;
            bool isRoutine = false;
            bool isAbstract = false;

            Token token = get();
            Token begin = token;

            // Collecting specifiers
            while ( true )
            {
                switch ( token.code )
                {
                    case TokenCode.Abstract: isAbstract = true;                   forget(); break;
                    case TokenCode.Override: isOverride = true;                   forget(); break;
                    case TokenCode.Hidden  : isHidden   = true;                   forget(); break;
                    case TokenCode.Final   : isFinal    = true; isRoutine = true; forget(); break;
                    case TokenCode.Routine :                    isRoutine = true; forget(); break;
                    case TokenCode.Pure    : pure_safe = 1;     isRoutine = true; forget(); break;
                    case TokenCode.Safe    : pure_safe = 2;     isRoutine = true; forget(); break;
                    default: goto OutLoop;
                }
                token = get();
            }
         OutLoop:
            // Checking for a leading keyword
            switch ( token.code )
            {
                case TokenCode.Unit:
                    if ( isOverride || isRoutine || pure_safe > 0 )
                        // Illegal specifier for unit declaration
                        error(token,"illegal-spec",begin,"unit declaration");

                    UNIT.parse(isHidden,isFinal,isAbstract,context);
                    break;

                case TokenCode.Const:
                    if ( isRoutine || pure_safe > 0 )
                        // Illegal specifier for constant declaration
                        error(begin,"illegal-spec",begin,"object declaration");

                    Token start = token;
                    forget(); token = get();
                    if ( token.code == TokenCode.Is )
                    {
                        forget();
                        if ( isOverride )
                            // Illegal specifier for constant declaration
                            error(begin,"illegal-spec",begin,"constant declaration");
                        CONSTANT.parse(isHidden,isFinal,start,context);
                    }
                    else
                    {
                        if ( pure_safe > 0 )
                            // Illegal specifier for variable declaration
                            error(begin,"illegal-spec",begin,"variable declaration");
                        VARIABLE.parse(isHidden,isFinal,true,isOverride,null,null,context);
                    }
                    break;

                case TokenCode.Init:
                    forget();
                    if ( isOverride || isFinal || isRoutine || pure_safe>0 )
                        // Illegal specifier for initializer
                        error(begin,"illegal-spec",begin,"initializer declaration");
                    ROUTINE.parse(token,isHidden,isFinal,false,0,context);
                    break;

                case TokenCode.Identifier:
                    // An identifier just following (optional) specifier(s)
                    if ( isRoutine || pure_safe>0 )
                    {
                        // This is definitely a routine declaration
                        // (Some time after, 'isRoutine' might be removed...)
                        forget();
                        ROUTINE.parse(token,isHidden,isFinal,isOverride,pure_safe,context);
                    }
                    else
                    {
                        // Decide whether this is variable or routine declaration
                        if ( !isOverride || !isHidden || !isFinal )
                        {
                            if ( !(context is UNIT) )
                                // Tricky point:
                                // We are out of a unit context, i.e., within a routine
                                // or in a global scope, AND there is NO ONE specifier given.
                                // So, we conclude that identifier starts a statement
                                // but not a declaration.
                                // We do nothing and silently exit with result == false.
                                break;
                        }
                        Token id = token;
                        forget(); token = get(); // forget();
                        switch (token.code)
                        {
                            case TokenCode.LParen:
                            case TokenCode.LBracket:
                            case TokenCode.Then:
                            case TokenCode.Else:
                                ROUTINE.parse(id,isHidden,isFinal,isOverride,pure_safe, context);
                                break;
                            default:
                                forget();
                                VARIABLE.parse(isHidden,isFinal,false,isOverride,id,token,context);
                                break;
                        }
                        break;
                    }
                    break;

                default:
                    // Some other token after optional specifier(s).
                    // Perhaps this is an operator sign?
                    Token name = ROUTINE.detectOperatorSign(token);
                    if ( name.code != TokenCode.ERROR )
                        ROUTINE.parse(name,isHidden,isFinal,isOverride,pure_safe,context);
                    else if ( isRoutine || isOverride || isHidden )
                    {
                        // Syntax error in declaration
                        error(token,"syntax-error","wrong declaration");
                        result = false;
                    }
                    else
                    {
                        // What's this? -- something that is not a declaration.
                        // Don't issue a message
                        // error(token,"syntax-error");
                        result = false;
                    }
                    break;
            }

            Debug.WriteLine("Exiting DECLARATION.parse");
            Debug.Unindent();

            return result;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class VARIABLE : DECLARATION
    {
        #region Structure

     // public IDENTIFIER name; -- from the base class

        /// <summary>
        /// 
        /// </summary>
        public TYPE type { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public EXPRESSION initializer { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool isConst { get; private set; }
        public bool isRef { get; private set; }
        public bool isOverride { get; private set; }
        public bool isVal { get { return !isRef; } }
        public bool isAbstract { get; private set; }
        public bool isForeign { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool isConcurrent { get; private set; }

        public void setVarSpecs(bool cnst, bool overrid, bool rv, bool c, bool a, bool f)
        {
            isConst = cnst;
            isRef = rv;
            isConcurrent = c;
            isOverride = overrid;
            isAbstract = a;
            isForeign = f;
        }

        #endregion

        #region Constructors

        public VARIABLE(IDENTIFIER n=null, TYPE t = null, EXPRESSION i = null) : base(n)
        {
            type = t;
            initializer = i;
        }

        #endregion

        #region Parser

        /// <summary>
        /// 
        /// <syntax><code>
        /// Объявление-переменных
        ///         : Список-идентификаторов [ : Спецификатор-типа ] is Выражение
        ///         | Список-идентификаторов : Спецификатор-типа
        ///
        /// Спецификатор-типа
        ///         : [ ? | ref | val | concurrent ] Тип
        ///         | as Составное-имя
        /// </code></syntax>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static void parse ( bool hidden, bool ffinal, bool isConst, bool isOverride, Token id, Token afterId, iSCOPE context )
        {
            Debug.Indent();
            Debug.WriteLine("Entering VARIABLE.parse");

            TYPE type = null;
            EXPRESSION initializer = null;

            bool RefVal = false; // 'val' by default
            bool Conc = false;
            bool Abstr = false;
            bool Foreign = false;

            Token token = id;
            Span final = null;

            // If id==null, then we just start processing the declaration.
            // In this case, always afterId==null.
            //
            // If id != null then it contains an identifier.
            // In this case, afterId contains ':', ',' or 'is' - three
            // options starting a declaration.

            List<Token> identifiers = new List<Token>();
            if ( token == null )
            {
                // We just start parsing variable declaration
                token = get(); forget();
                identifiers.Add(token);

                // Parsing the rest of identifiers in the list (if any)
                while (true)
                {
                    token = get();
                    if (token.code != TokenCode.Comma) break;
                    forget();
                    token = expect(TokenCode.Identifier);
                    identifiers.Add(token);
                }
            }
            else
            {
                if ( afterId.code == TokenCode.Is )
                {
                    // id is ...
                    identifiers.Add(id);
                    token = afterId;
                }
                else if ( afterId.code == TokenCode.Colon )
                {
                    // 'token' contains an identifier.
                    // Check what's after this identifier?
                    //
                    //  id : ...
                    //       ^
                    identifiers.Add(id);
                    token = afterId;
                }
                else if ( afterId.code == TokenCode.Comma )
                {
                    // 'token' contains an identifier,
                    // and comma goes after it (comma was parsed already).
                    //
                    //  id, ...
                    //      ^
                    identifiers.Add(id);

                    // Parsing the rest of the list of identifiers.
                    while (true)
                    {
                        token = expect(TokenCode.Identifier);
                        identifiers.Add(token);
                        if (token.code != TokenCode.Comma) break;
                        forget();
                    }
                }
                else
                {
                    // Syntax error
                    error(afterId, "syntax-error","declaration");
                }
            }
            // Here, we have parsed the list of identifiers.
            // 'token' contains the token after the list.
            // Only two valid options are ':' or 'is'.

            if ( token.code == TokenCode.Colon )
            {
                // Type is expected.
                if ( afterId == null ) forget();
                type = parseTypeSpecifier(context,out RefVal, out Conc, out final);
                token = get();
            }
            if (token.code == TokenCode.Is)
            {
                forget(); token = get();
                if ( token.code == TokenCode.Abstract )
                {
                    forget();
                    Abstr = true;
                    final = token.span;
                }
                else if ( token.code == TokenCode.Foreign )
                {
                    forget();
                    Foreign = true;
                    final = token.span;
                }
                else
                {
                    forget();
                    initializer = EXPRESSION.parse(token,context);
                    if ( type == null )
                    {
                        // Type inference
                     // initializer.calculateType();
                        type = initializer.type;
                    }
                    final = initializer.span;
                }
            }

            // Here, we have the list of identifiers, their type,
            // and perhaps the initializer.
            // Create the final node(s) for variable declaration(s)
            // out of identifier(s), type and initializer.

            foreach (Token ident in identifiers)
            {
                VARIABLE varDecl = new VARIABLE(new IDENTIFIER(ident),type,initializer);
                varDecl.setSpecs(hidden,ffinal);
                varDecl.setVarSpecs(isConst,isOverride,RefVal,Conc,Abstr,Foreign);
                varDecl.setSpan(ident.span, final!=null ? final : ident.span);
                context.add(varDecl);

                varDecl.parent = context.self;

                if ( type != null ) type.parent = varDecl;
                if ( initializer != null ) initializer.parent = varDecl;
            }

            Debug.WriteLine("Exiting VARIABLE.parse");
            Debug.Unindent();
        }

        public static TYPE parseTypeSpecifier(iSCOPE context, out bool ref_val, out bool conc, out Span final)
        {
            TYPE type = null;

            bool opt = false;
            
            ref_val = false; conc = false;
            Token token = get();
            Token begin = token;
            if ( token.code == TokenCode.Question )
            {
                forget(); token = get();
                opt = true;
            }
            switch ( token.code )
            {
                case TokenCode.As:
                    forget();
                    EXPRESSION example = PRIMARY.parse(null,context);
                    type = example.type;
                    if ( type != null )
                    {
                        type.setSpan(begin.span,example.span);
                        if ( type is UNIT_REF ) (type as UNIT_REF).setSpecs(opt,true);

                    }
                    final = example.span;
                    break;

                case TokenCode.Ref:        ref_val = true; forget(); goto ParseType;
                case TokenCode.Concurrent: conc    = true; forget(); goto ParseType;
                case TokenCode.Val:                        forget(); goto ParseType;

                case TokenCode.LParen:
                    // Seems to a tuple type
                    type = TUPLE_TYPE.parse(context);
                    context.add(type);
                    final = type.span;
                    break;

                default:
                 // forget();
                ParseType:
                    UNIT_REF unitRef = UNIT_REF.parse(null,opt,context);
                    if (unitRef == null) /* An error was detected earlier */ { final = null; return null; }
                    final = unitRef.span;
                    token = get();
                    return unitRef;
            }
            return type;
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
            return base.ToJSON()
                //.AppendChild(new JsonIr("CONST_SPEC", isConst ? "const" : null))
                .AppendChild(new JsonIr("REF_VAL_SPEC", isRef ? "ref" : "val"))
                //.AppendChild(new JsonIr("OVERRIDE_SPEC", isOverride ? "override" : null))
                //.AppendChild(new JsonIr("ABSTRACT_SPEC", isAbstract ? "abstract" : null))
                .AppendChild(isConcurrent ? new JsonIr("CONCURRENT_SPEC") : null)
                .AppendChild(isForeign ? new JsonIr("FOREIGN_SPEC") : null)
                .AppendChild(ToJSON(type))
                .AppendChild(ToJSON(initializer));
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string a = (isHidden     ? "HIDDEN "     : "")
                     + (isConst      ? "CONST "      : "")
                     + (isConcurrent ? "CONCURRENT " : "")
                     + (isRef        ? "REF "        : "VAL ")
                     + (isOverride   ? "OVERRIDE "   : "")
                     + (isAbstract   ? "ABSTRACT "   : "")
                     + (isForeign    ? "FOREIGN "    : "");
            string r = commonAttrs() + shift(sh) + a + "VARIABLE " + name.identifier;
            System.Console.WriteLine(r);

            if ( type != null ) type.report(sh+constant);
            if ( initializer != null ) initializer.report(sh+constant);
        }

        #endregion
    }

    public class CONSTANT : DECLARATION
    {
        #region Structure

        /// <summary>
        /// Either common (single) EXPRESSION, or BINARY
        /// as two EXPRESSIONs.
        /// </summary>
        public List<EXPRESSION> constants { get; private set; }

        #endregion

        #region Creation

        public CONSTANT() : base((Token)null) { constants = new List<EXPRESSION>(); }

        #endregion

        #region Parsing

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static void parse(bool hidden, bool final, Token start, iSCOPE context)
        {
            Debug.Indent();
            Debug.WriteLine("Entering CONSTANT.parse");

            CONSTANT constant = new CONSTANT();

            // 'const' and 'is' keywords were already parsed
            Token token = get();
            if (token.code == TokenCode.End) { forget(); goto FinalActions; } // empty list

            while ( true )
            {
                EXPRESSION left = EXPRESSION.parse(null,context);
                token = get();
                if ( token.code == TokenCode.DotDot ) // range
                {
                    forget();
                    EXPRESSION right = EXPRESSION.parse(null, context);
                    BINARY range = new BINARY(left, right);
                    range.setSpan(left.span, right.span);
                    range.parent = constant;
                    constant.constants.Add(range);
                }
                else // single expression
                {
                    left.parent = constant;
                    constant.constants.Add(left);
                }
                token = get();
                if ( token.code == TokenCode.Comma ) { forget(); continue; }
                if ( token.code == TokenCode.End ) { forget(); break; }
                else { error(token,"syntax-error"); break; }
            }
         FinalActions:
            constant.setSpecs(hidden,final);
            context.add(constant);
            constant.parent = context.self;
            constant.setSpan(start,token);

            Debug.WriteLine("Exiting CONSTANT.parse");
            Debug.Unindent();
        }

        #endregion

        #region Validation

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
            return new JsonIr(GetType())  // do not use base.ToJSON(), no need
                .AppendChild(JsonIr.ListToJSON(constants));
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string common = commonAttrs();
            string r = common + shift(sh) + (isHidden ? "HIDDEN " : "" ) + "CONSTANT IS";
            System.Console.WriteLine(r);

            foreach ( EXPRESSION c in constants )
            {
                if ( c is BINARY )
                {
                    System.Console.WriteLine(shift(common.Length+sh+constant) + "RANGE");
                    (c as BINARY).left.report(sh + constant + constant);
                    (c as BINARY).right.report(sh + constant + constant);
                }
                else
                    c.report(sh+constant);
            }
        }

        #endregion
    }
}
