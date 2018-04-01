using System;
using System.Collections.Generic;

namespace SLang
{
    public class Token
    {
        #region Structure

        /// <summary>
        /// Token coordinates.
        /// </summary>
        public Span span { get; private set; }

        /// <summary>
        /// Token code.
        /// </summary>
        public TokenCode code { get; private set; }

        /// <summary>
        /// Token image: exactly as it appears in the source code.
        /// </summary>
        public string image { get; private set; }

        /// <summary>
        /// Token's lexical category.
        /// </summary>
        public Category category { get; private set; }

        /// <summary>
        /// For literals: an integer of floating value.
        /// </summary>
        public object value { get; private set; }

        #endregion

        #region Constructor

        public Token(Span s, TokenCode c, string i, Category ct, object v=null)
        {
            span = s;
            code = c;
            image = i;
            category = ct;
            value = v;
        }

        #endregion

        #region Output

        public override string ToString()
        {
            string result = span.ToString() + " " + code + " " + category.ToString() + " ";
            string img = "";
            switch ( code )
            {
                case TokenCode.EOL:
                    img = "\\n"; goto Output;
                case TokenCode.Tab:
                    img = "\\t"; goto Output;
                case TokenCode.Blank:
                    img = ".";
                Output:
                    for ( int i=1; i<=image.Length; i++ )
                        result += img;
                    break;
                case TokenCode.EOS:
                    result += "EOS";
                    break;
                default:
                    result += image;
                    break;
            }
            return result;
        }

        public string ToSource()
        {
            return image;
        }

        public LITERAL ToNode()
        {
            if ( !category.isLiteral() ) return null;
            LITERAL result = new LITERAL(this.value,span,code);
         // result.setSpan(this);
         // result.code = this.code;
            return result;
        }

        #endregion
    }

}
