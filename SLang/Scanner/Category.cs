using System;
using System.Collections.Generic;

namespace SLang
{
    [Flags]
    public enum CategoryCode
    {
        no_category =   0,

        delimiter   =   1,
        operatorr   =   2,
        identifier  =   4,
        keyword     =   8,
        trivia      =  16,

        specifier   =  32,
        statement   =  64,
        literal     = 128
    }

    public class Category
    {
        public CategoryCode code;

        public Category() { code = 0; }
        public Category(CategoryCode c) { code = c; }

        public void setDelimiter()  { code |= CategoryCode.delimiter; }
        public void setOperator()   { code |= CategoryCode.operatorr; }
        public void setIdentifier() { code |= CategoryCode.identifier; }
        public void setKeyword()    { code |= CategoryCode.keyword; }
        public void setTrivia()     { code |= CategoryCode.trivia; }
        public void setSpecifier()  { code |= CategoryCode.specifier; }
        public void setStatement()  { code |= CategoryCode.statement; }
        public void setLiteral()    { code |= CategoryCode.literal; }

        public override string ToString() { return code.ToString(); }

        public bool isDelimiter()  { return (code & CategoryCode.delimiter) == CategoryCode.delimiter; }
        public bool isOperator()   { return (code & CategoryCode.operatorr) == CategoryCode.operatorr; }
        public bool isIdentifier() { return (code & CategoryCode.identifier) == CategoryCode.identifier; }
        public bool isKeyword()    { return (code & CategoryCode.keyword) == CategoryCode.keyword; }
        public bool isTrivia()     { return (code & CategoryCode.trivia) == CategoryCode.trivia; }
        public bool isStatement()  { return (code & CategoryCode.statement) == CategoryCode.statement; }
        public bool isLiteral()    { return (code & CategoryCode.literal) == CategoryCode.literal; }

    }
}
