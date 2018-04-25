using System;
using System.Collections.Generic;

namespace SLang
{
    public class Tokenizer
    {
        private Reader reader;
        private Message messages;

        public Tokenizer(Reader reader, Options options, Message messages=null)
        {
            this.reader = reader;
            this.messages = messages;
            if ( this.messages == null ) this.messages = new Message(options);
        }

        public Token getNextToken()
        {
            char curr = reader.getChar();
            char ch;
            Position start = reader.currPos();
            Category category = new Category();
            string image = "";
            TokenCode code;

            switch ( curr )
            {
                case '\0':
                    category.setDelimiter();
                    category.setTrivia();
                    return new Token(new Span(start),TokenCode.EOS,"",category);
                case ' ' :
                case '\t':
                case '\n':
                {
                    while ( true )
                    {
                        ch = reader.getChar();
                        if ( ch != curr ) break;
                        image += ch;
                        reader.forgetChar();
                    }
                    switch ( curr )
                    {
                        case ' ' : code = TokenCode.Blank; break;
                        case '\t': code = TokenCode.Tab;   break;
                        case '\n': code = TokenCode.EOL;   break;
                        default  : code = TokenCode.ERROR; break;
                    }
                    category.setTrivia();
                    return new Token(new Span(start,reader.currPos()),code,image,category);
                }

                // One-character tokens
                case ',': code = TokenCode.Comma;     category.setDelimiter(); goto OneCharToken;
                case ';': code = TokenCode.Semicolon; category.setDelimiter(); goto OneCharToken;
                case '(': code = TokenCode.LParen;    category.setDelimiter(); goto OneCharToken;
                case ')': code = TokenCode.RParen;    category.setDelimiter(); goto OneCharToken;
                case '[': code = TokenCode.LBracket;  category.setDelimiter(); goto OneCharToken;
                case ']': code = TokenCode.RBracket;  category.setDelimiter(); goto OneCharToken;
            //  case '{': code = TokenCode.LBrace;    category.setDelimiter(); goto OneCharToken;
            //  case '}': code = TokenCode.RBrace;    category.setDelimiter(); goto OneCharToken;
                case '~': code = TokenCode.Tilde;     category.setDelimiter(); goto OneCharToken;
                case '+': code = TokenCode.Plus;      category.setOperator();  reader.forgetChar(); goto OneCharOperator;
                case '^': code = TokenCode.Caret;     category.setOperator();  reader.forgetChar(); goto OneCharOperator;
                case '\\': code = TokenCode.Remainder; category.setOperator(); reader.forgetChar(); goto OneCharOperator;
                case '?': code = TokenCode.Question;  category.setDelimiter(); goto OneCharToken;
                OneCharToken:
                    reader.forgetChar();
                    image = "" + curr;
                    return new Token(new Span(start),code,image,category);

                case '&': // &, &&
                    reader.forgetChar();
                    ch = reader.getChar();
                    if ( ch == '&' )
                    {
                        reader.forgetChar();
                        code = TokenCode.AmpAmp; image = "&&"; goto TwoCharOperator;
                    }
                    else
                    {
                        code = TokenCode.Ampersand; image = "&"; goto OneCharOperator;
                    }

                case '|': // |, ||
                    reader.forgetChar();
                    ch = reader.getChar();
                    if ( ch == '|' )
                    {
                        reader.forgetChar();
                        code = TokenCode.VertVert; image = "||"; goto TwoCharOperator;
                    }
                    else
                    {
                        code = TokenCode.Vertical; image = "|"; goto OneCharOperator;
                    }

                case '*': // *, **
                    reader.forgetChar();
                    ch = reader.getChar();
                    if ( ch == '*' )
                    {
                        reader.forgetChar();
                        code = TokenCode.Power; image = "**"; goto TwoCharOperator;
                    }
                    else
                    {
                        code = TokenCode.Multiply; image = "*"; goto OneCharOperator;
                    }
                case ':':   //  :, :=
                    reader.forgetChar();
                    ch = reader.getChar();
                    if ( ch == '=' )
                    {
                        reader.forgetChar();
                        code = TokenCode.Assign; image = ":="; goto TwoCharDelimiter;
                    }
                    else
                    {
                        code = TokenCode.Colon; image = ":"; goto OneCharDelimiter;
                    }

                case '.':   //  ., ..
                    reader.forgetChar();
                    ch = reader.getChar();
                    if ( ch == '.' )
                    {
                        reader.forgetChar();
                        code = TokenCode.DotDot; image = ".."; goto TwoCharDelimiter;
                    }
                    else
                    {
                        code = TokenCode.Dot; image = "."; goto OneCharDelimiter;
                    }

                case '=':   //  =, =>, ==
                    reader.forgetChar();
                    ch = reader.getChar();
                    switch ( ch )
                    {
                        case '>':
                            reader.forgetChar();
                            code = TokenCode.Arrow; image = "=>"; goto TwoCharDelimiter;

                        case '=':
                            reader.forgetChar();
                            code = TokenCode.EqualEqual; image = "=="; goto TwoCharOperator;

                        default:
                            code = TokenCode.Equal; image = "="; goto OneCharOperator;
                    }

                case '-': // -, ->
                    reader.forgetChar();
                    ch = reader.getChar();
                    if ( ch == '>' )
                    {
                        reader.forgetChar();
                        code = TokenCode.Arrow2; image = "->"; goto TwoCharDelimiter;
                    }
                    else
                    {
                        code = TokenCode.Minus; image = "-"; goto OneCharDelimiter;
                    }

                case '/':   //   /, /=, /==, /*, //
                    Position pos = reader.currPos();
                    reader.forgetChar();
                    ch = reader.getChar();
                    switch ( ch )
                    {
                        case '=':
                            reader.forgetChar();
                            curr = reader.getChar();
                            if ( curr == '=' )
                            {
                                reader.forgetChar();
                                code = TokenCode.NotEqualDeep; image = "/=="; category.setOperator();
                                return new Token(new Span(start,reader.currPos()),code,image,category);
                            }
                            else
                            {
                                code = TokenCode.NotEqual; image = "/="; goto TwoCharOperator;
                            }
                        case '/': // short or documenting comment
                            reader.forgetChar();
                            return scanShortComment(pos);
                        case '*': // long comment
                            reader.forgetChar();
                            return scanLongComment(pos);
                        default:
                            code = TokenCode.Divide; image = "/";
                            goto OneCharOperator;
                    }

                case '<':   //  <, <=
                    reader.forgetChar();
                    ch = reader.getChar();
                    if ( ch == '=' )
                    {
                        reader.forgetChar();
                        code = TokenCode.LessEqual; image = "<="; goto TwoCharOperator;
                    }
                    else
                    {
                        code = TokenCode.Less; image = "<"; goto OneCharOperator;
                    }

                case '>':   //  >, >=
                    reader.forgetChar();
                    ch = reader.getChar();
                    if ( ch == '=' )
                    {
                        reader.forgetChar();
                        code = TokenCode.GreaterEqual; image = ">="; goto TwoCharOperator;
                    }
                    else
                    {
                        code = TokenCode.Greater; image = ">"; goto OneCharOperator;
                    }

                case '\'':  // character literal
                    return scanCharacter();

                case '"':  // string literal
                    return scanString();

                TwoCharDelimiter:
                    category.setDelimiter();
                    return new Token(new Span(start,reader.currPos()),code,image,category);
                TwoCharOperator:
                    category.setOperator();
                    return new Token(new Span(start,reader.currPos()),code,image,category);
                OneCharDelimiter:
                    category.setDelimiter();
                    return new Token(new Span(start),code,image,category);
                OneCharOperator:
                 // reader.forgetChar();
                    image = "" + curr;
                    category.setOperator();
                    return new Token(new Span(start),code,image,category);

                default:
                    if ( Char.IsLetter(curr) )
                    {
                        // An identifier or a keyword
                        string identifier = "" + curr;
                        reader.forgetChar();
                        while ( true )
                        {
                            curr = reader.getChar();
                            if ( Char.IsLetter(curr) || Char.IsDigit(curr) || curr == '_' )
                            {
                                identifier += curr;
                                reader.forgetChar();
                            }
                            else
                                break;
                        }
                        return detectKeyword(identifier, new Span(start,reader.currPos()));
                    }
                    else if ( Char.IsDigit(curr) )
                    {
                        // A numeric literal
                        if ( curr != '0' )
                        {
                            Token t = scanNumeric(10);
                            curr = reader.getChar();
                            if ( curr == '.' )
                            {
                                if ( reader.checkNextFor('.') )
                                {
                                    // Oops... this is _range_: i.e., something like dd .. dd
                                    return t;
                                }
                                else
                                {
                                    char c = reader.tryNext();
                                    if ( Char.IsDigit(c) )
                                    {
                                        // Mantissa starts
                                        reader.forgetChar();
                                        Token mantissa = scanNumeric(10,true);
                                        category.setLiteral();
                                        return new Token(new Span(t.span, mantissa.span),
                                                         TokenCode.Real,t.image + "." + mantissa.image,
                                                         category,(double)(long)t.value + (double)mantissa.value);
                                    }
                                    else
                                    {
                                        // Just an ordinary standalone dot.
                                        // It will be taken on the next call.
                                        return t;
                                    }
                                }
                            }
                            else
                                return t;
                        }
                        else // starting '0'
                        {
                            reader.forgetChar();
                            curr = reader.getChar();
                            switch ( Char.ToUpper(curr) )
                            {
                                case 'B': reader.forgetChar(); return scanNumeric(2);
                                case 'O': reader.forgetChar(); return scanNumeric(8);
                                case 'X': reader.forgetChar(); return scanNumeric(16);
                                default:
                                    if ( Char.IsDigit(curr) )
                                    {
                                        // This is perhaps a decimal number probably followed by
                                        // a floating "tail"
                                        Token numeric = scanNumeric(10);
                                        curr = reader.getChar();
                                        if (curr == '.') // fixed-point floating
                                        {
                                            reader.forgetChar();
                                            Token mantissa = scanNumeric(10,true);
                                            category.setLiteral();
                                            numeric = new Token(new Span(numeric.span,mantissa.span),
                                                                TokenCode.Real,
                                                                numeric.image+"."+mantissa.image,
                                                                category,
                                                                (double)(long)numeric.value+(double)mantissa.value);
                                        }
                                        curr = reader.getChar();
                                        if ( Char.ToUpper(curr) == 'E' )
                                        {
                                            reader.forgetChar();
                                            curr = reader.getChar();
                                            bool minus = false;
                                            switch ( curr )
                                            {
                                                case '+': reader.forgetChar(); break;
                                                case '-': reader.forgetChar(); minus = true; break;
                                                default: break;
                                            }
                                            Token exponent = scanNumeric(10);
                                            double expo = Math.Pow((int)exponent.value, 10);
                                            if (minus) expo = 1 / expo;
                                            double fixedPart = numeric.code == TokenCode.Integer
                                                                ? (double)(int)numeric.value
                                                                : (double)numeric.value;
                                            category.setLiteral();
                                            return new Token(new Span(start, reader.currPos()), TokenCode.Real,
                                                             numeric.image+"E"+(minus ? "-" : "+")+exponent.image,
                                                             category,fixedPart+expo);
                                        }
                                        else
                                            return numeric;
                                    }
                                    else if (curr == '.')
                                    {
                                        char c = reader.tryNext();
                                        if ( Char.IsDigit(c) )
                                        {
                                            // This is '0' with the '.' after it
                                            if ( reader.checkNextFor('.') )
                                                // Oops... this is '..'. Don't forget the first '.'.
                                                return new Token(new Span(start),TokenCode.Integer,"0",category,(long)0);
                                            else
                                            {
                                                reader.forgetChar();
                                                // This is something like 0.dddd, where d's are decimal digits.
                                                Token mantissa = scanNumeric(10,true);
                                                category.setLiteral();
                                                return new Token(new Span(start,mantissa.span.end),
                                                                 TokenCode.Real,"0." + mantissa.image,
                                                                 category,(double)(long)0 + (double)mantissa.value);
                                            }
                                        }
                                        else
                                        {
                                            // Something like 0.function (which is OK)
                                            // This is just single zero!
                                            return new Token(new Span(start),TokenCode.Integer,"0",category,(long)0);
                                        }
                                    }
                                    else
                                    {
                                        // This is just single zero!
                                        return new Token(new Span(start),TokenCode.Integer,"0",category,(long)0);
                                    }
                            }
                        }
                    }
                    else  // Illegal character
                    {
                        reader.forgetChar();
                        category.setTrivia();
                        return new Token(new Span(start),TokenCode.ERROR,""+curr,category);
                    }
            } // switch
        } // getNextToken

        private Token detectKeyword(string identifier, Span s)
        {
            TokenCode code;
            Category category = new Category();

            switch ( identifier )
            {
                case "abstract"       : category.setDelimiter(); code = TokenCode.Abstract;   break;
                case "alias"          : category.setSpecifier(); code = TokenCode.Alias;      break;
           //   case "and"            : category.setOperator();  code = TokenCode.And;        break;
                case "as"             : category.setDelimiter(); code = TokenCode.As;         break;
                case "break"          : category.setStatement(); code = TokenCode.Break;      break;
                case "case"           : category.setStatement(); code = TokenCode.Case;       break;
                case "check"          : category.setStatement(); code = TokenCode.Check;      break;
                case "concurrent"     : category.setSpecifier(); code = TokenCode.Concurrent; break;
                case "const"          : category.setSpecifier(); code = TokenCode.Const;      break;
                case "else"           : category.setStatement(); code = TokenCode.Else;       break;
                case "elsif"          : category.setStatement(); code = TokenCode.Elsif;      break;
                case "end"            : category.setStatement(); code = TokenCode.End;        break;
                case "ensure"         : category.setSpecifier(); code = TokenCode.Ensure;     break;
                case "extend"         : category.setSpecifier(); code = TokenCode.Extend;     break;
             // case "external"       : category.setSpecifier(); code = TokenCode.External;   break;
                case "final"          : category.setSpecifier(); code = TokenCode.Final;      break;
                case "foreign"        : category.setSpecifier(); code = TokenCode.Foreign;    break;
                case "hidden"         : category.setSpecifier(); code = TokenCode.Hidden;     break;
                case "if"             : category.setStatement(); code = TokenCode.If;         break;
                case "in"             : category.setOperator();  code = TokenCode.In;         break;
                case "init"           : category.setSpecifier(); code = TokenCode.Init;       break;
                case "invariant"      : category.setSpecifier(); code = TokenCode.Invariant;  break;
                case "is"             : category.setOperator();  code = TokenCode.Is;         break;
                case "lambda"         : category.setSpecifier(); code = TokenCode.Lambda;     break;
                case "loop"           : category.setStatement(); code = TokenCode.Loop;       break;
                case "new"            : category.setOperator();  code = TokenCode.New;        break;
            //  case "not"            : category.setOperator();  code = TokenCode.Not;        break;
                case "old"            : category.setSpecifier(); code = TokenCode.Old;        break;
            //  case "or"             : category.setOperator();  code = TokenCode.Or;         break;
                case "override"       : category.setSpecifier(); code = TokenCode.Override;   break;
                case "package"        : category.setStatement(); code = TokenCode.Package;    break;
                case "private"        : category.setSpecifier(); code = TokenCode.Private;    break;
                case "public"         : category.setSpecifier(); code = TokenCode.Public;     break;
                case "pure"           : category.setSpecifier(); code = TokenCode.Pure;       break;
                case "raise"          : category.setStatement(); code = TokenCode.Raise;      break;
                case "ref"            : category.setSpecifier(); code = TokenCode.Ref;        break;
                case "require"        : category.setSpecifier(); code = TokenCode.Require;    break;
                case "return"         : category.setStatement(); code = TokenCode.Return;     break;
                case "routine"        : category.setSpecifier(); code = TokenCode.Routine;    break;
                case "safe"           : category.setSpecifier(); code = TokenCode.Safe;       break;
                case "then"           : category.setStatement(); code = TokenCode.Then;       break;
                case "this"           : category.setSpecifier(); code = TokenCode.This;       break;
                case "unit"           : category.setStatement(); code = TokenCode.Unit;       break;
                case "use"            : category.setSpecifier(); code = TokenCode.Use;        break;
                case "val"            : category.setSpecifier(); code = TokenCode.Val;        break;
                case "variant"        : category.setSpecifier(); code = TokenCode.Variant;    break;
                case "while"          : category.setStatement(); code = TokenCode.While;      break;
            //  case "xor"            : category.setOperator();  code = TokenCode.Xor;        break;

                default:
                    // Not a keyword but an identifier.
                    return new Token(s,TokenCode.Identifier,identifier,
                                     new Category(CategoryCode.identifier));
            }
            category.setKeyword();
            return new Token(s,code,identifier,category);
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <returns></returns>
        private Token scanCharacter()
        {
            string image = "";
            char value = '\0';
            Position begin = reader.currPos();

            reader.forgetChar(); // skip '
            char curr = reader.getChar();

            return new Token(new Span(begin,reader.currPos()),TokenCode.Character,
                             image,
                             new Category(CategoryCode.literal),
                             value);
        }

        /// <summary>
        /// Not completely implemented
        /// </summary>
        /// <returns></returns>
        private Token scanString()
        {
            string image = "";
            string value = "";
            Position begin = reader.currPos();

            reader.forgetChar(); // skip "
            while(true)
            {
                char curr = reader.getChar();
                if ( curr == '"' ) { reader.forgetChar(); break; }
                image += curr;
                reader.forgetChar();
            }

            return new Token(new Span(begin, reader.currPos()), TokenCode.String,
                             image,
                             new Category(CategoryCode.literal),
                             value);
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <returns></returns>
        private char scanEscape()
        {
            char curr = reader.getChar();
            switch ( curr )
            {
                case '\\': return '\\';
                case '\'': return '\'';
                case '"' : return '"';
                case 't' : return '\t';
                case 'n' : return '\n';
                case 'r' : return '\r';

                case 'x' :
                case 'u' : return '0'; // not implemented

                default: return '0';
            }

        }

        private Token scanNumeric ( int power, bool m = false )
        {
            string scale = "0123456789ABCDEF".Substring(0,power);

            Position begin = reader.currPos();
            long resultInt = 0;
            double resultDouble = 0.0;
            string image = "";
            while ( true )
            {
                char curr = reader.getChar();
                if ( curr == '_' ) { reader.forgetChar(); continue; }
                int i = scale.IndexOf(curr);
                if ( i == -1 ) break;
                image += curr;
                reader.forgetChar();
                if (m)
                {
                    resultDouble = resultDouble + ((double)i) / power;
                    power *= 10;
                }
                else
                    resultInt = resultInt * power + i;
            }
            object resValue = m ? (object)resultDouble : (object)resultInt;
            TokenCode code = m ? TokenCode.Real : TokenCode.Integer;
            return new Token(new Span(begin,reader.currPos()),
                             code,image,
                             new Category(CategoryCode.literal),
                             resValue);
        }

        private Token scanLongComment(Position begin)
        {
            // '/*' sequence is already scanned.
            string comment = "/*";
            while ( true )
            {
                char ch = reader.getChar();
                if ( ch == '*' )
                {
                    reader.forgetChar();
                    ch = reader.getChar();
                    if (ch == '/')
                    {
                        // End of long comment
                        reader.forgetChar();
                        comment += "*/";
                        break;
                    }
                    else
                        comment += "*";
                }
                else
                {
                    comment += ch;
                    reader.forgetChar();
                }
            }
            return new Token(new Span(begin, reader.currPos()),
                             TokenCode.LComment,comment,
                             new Category(CategoryCode.trivia));
        }

        private Token scanShortComment(Position begin)
        {
            // '//' sequence is already scanned.
            TokenCode code = TokenCode.SComment;
            char ch = reader.getChar();
            if ( ch == '/' ) { reader.forgetChar(); code = TokenCode.DComment;  }

            string comment = "//";
            while ( true )
            {
                ch = reader.getChar();
                if (ch == '\n' || ch == '\0') break;
                comment += ch;
                reader.forgetChar();
            }
            return new Token(new Span(begin,reader.currPos()),
                             code,comment,
                             new Category(CategoryCode.trivia));
        }
    }
}
 