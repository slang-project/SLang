using System;
using System.Collections.Generic;

namespace SLang
{
    public interface iSCOPE
    {
     // iSCOPE enclosing { get; set; }
        DECLARATION find_in_scope(string id);
        void add(ENTITY d);
        ENTITY self { get; }
    }

    public static class Context
    {
        #region Structure

        static iSCOPE[] display;
        static int currentLevel;

        #endregion

        #region Initialization

        static Context()
        {
            display = new iSCOPE[32];
            currentLevel = 0;

         // currentScope = null;
        }

        #endregion

        #region Output


        #endregion

        #region Entering and exiting scopes

        // public static iSCOPE currentScope { get; private set; }
        
        public static void enter(iSCOPE scope)
        {
            display[currentLevel] = scope;
            currentLevel++;

         // scope.enclosing = currentScope;
         // currentScope = scope;
        }
        public static void exit()
        {
            currentLevel--;
         // currentScope = currentScope.enclosing;
        }

        /// <summary>
        /// The function returns the nearest enclosing unit declaration,
        /// or null if the current context doesn't include a unit
        /// (that is, the current context is either a global context
        /// or a context of a standalone routine).
        /// </summary>
        /// <returns></returns>
        public static UNIT unit()
        {
            for ( int i=currentLevel-1; i>=0; i-- )
            {
                if ( display[i] is UNIT ) return display[i] as UNIT;
            }
            return null;
        }

        public static ROUTINE routine()
        {
            for ( int i=currentLevel-1; i>=0; i-- )
            {
                if ( display[i] is ROUTINE ) return display[i] as ROUTINE;
            }
            return null;
        }

        #endregion

        #region Name look-up

        public static DECLARATION find(string id)
        {
            for ( int i=currentLevel-1; i>=0; i-- )
            {
                DECLARATION d = display[i].find_in_scope(id);
                if ( d != null ) return d;
            }
            return null;

         // iSCOPE scope = currentScope;
         // while ( scope != null )
         // {
         //     DECLARATION d = scope.find_in_scope(id);
         //     if ( d != null ) return d;
         //     scope = scope.enclosing;
         // }
         // return null;
        }
        public static DECLARATION find ( Token id )
        {
            return find(id.image);
        }
        public static DECLARATION find(IDENTIFIER id)
        {
            return find(id.identifier);
        }

        #endregion
    }
}
