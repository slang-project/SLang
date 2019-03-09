using System;
using System.Collections.Generic;

namespace SLang
{
    public class USE : ENTITY
    {
        #region Structure

        /// <summary>
        /// 
        /// </summary>
        public UNIT_REF unitRef { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool useConst { get; private set; }

        #endregion

        #region Creation

        public USE(UNIT_REF ur, bool c = false)
        {
            unitRef = ur;
            useConst = c;
        }

        #endregion

        #region Parser

        public static void parse(iSCOPE context)
        {
            bool useConst = false;
            
            Token token = get();
            Token begin = token;

            if ( token.code == TokenCode.Const )
            {
                forget();
                useConst = true;
            }
            while (true )
            {
                UNIT_REF ur = UNIT_REF.parse(null,false,context);
                USE result = new USE(ur, useConst);
                result.parent = context.self;
                result.setSpan(begin.span,ur.span);

                if ( context is UNIT )
                    (context as UNIT).add(result);
                else if ( context is COMPILATION )
                    (context as COMPILATION).add(result);
             // else
             //     -- Some other use of 'use'

                token = get();
                if ( token.code != TokenCode.Comma )
                    break;
                forget();
            }
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

        public override string ToJSON()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Reporting

        public override void report(int sh)
        {
            string common = commonAttrs();
            string r = common + shift(sh) + "USE " + (useConst ? "CONST " : "");
            System.Console.WriteLine(r);

            unitRef.report(sh);
        }

        #endregion
    }
}
