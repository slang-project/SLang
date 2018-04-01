using System;
using System.Collections.Generic;

namespace SLang
{
    public enum TokenCode
    {
        EOS,          //  end of source
        EOL,          //  end of line

        Tab,          //  horizontal tabulation
        Blank,        //  blank (whitespace)

        SComment,     //  short comment
        LComment,     //  long comment
        DComment,     //  documenting comment

        ERROR,        //  illegal token

        Identifier,   //
        Integer,      //
        Real,         //
        String,       //
        Character,    //

        Comma,        //  ,
        Dot,          //  .
        DotDot,       //  ..
        Semicolon,    //  ;
        Colon,        //  :
        LParen,       //  (
        RParen,       //  )
        LBracket,     //  [
        RBracket,     //  ]
//      LBrace,       //  {
//      RBrace,       //  }
        Arrow,        //  =>
        Arrow2,       //  ->
        Assign,       //  :=
        Equal,        //  =
        EqualEqual,   //  ==
        NotEqual,     //  /=
        NotEqualDeep, //  /==
        Less,         //  <
        LessEqual,    //  <=
        Greater,      //  >
        GreaterEqual, //  >=
        Tilde,        //  ~
        Question,     //  ?
        Vertical,     //  |
        VertVert,     //  ||
        Caret,        //  ^
        Ampersand,    //  &
        AmpAmp,       //  &&
        Plus,         //  +
        Minus,        //  -
        Multiply,     //  *
        Power,        //  **
        Divide,       //  /
     // Remainder,    //  %
        Remainder,    //  \
     // Call,         //  ()

        Abstract,
        Alias,
   //   And,
        As,
        Break,
        Case,
        Catch,
        Check,
        Concurrent,
        Const,
        Else,
        Elsif,
        End,
        Ensure,
        Extend,
     // External,
        Final,
        Foreign,
        Hidden,
        If,
        In,
        Init,
        Invariant,
        Is,
        Lambda,
        Loop,
        New,
   //   Not,
        Old,
   //   Or,
        Override,
        Package,
        Private,
        Public,
        Pure,
        Raise,
        Ref,
        Require,
        Return,
        Routine,
        Safe,
        Then,
        This,
        Try,
        Unit,
        Use,
        Val,
        Variant,
        While,
   //   Xor
    }
}
