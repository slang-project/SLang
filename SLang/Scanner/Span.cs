using System;
using System.Collections.Generic;

namespace SLang
{
    public class Position
    {
        #region Structure

        /// <summary>
        /// Line number of the source code (starting from 1)
        /// </summary>
        public int line { get; private set; }

        /// <summary>
        /// Position number within the line (starting from 1)
        /// </summary>
        public int pos { get; private set; }

        #endregion

        #region Constructors

        public Position(int l, int p) { line = l; pos = p; }
        public Position(Position p) { line = p.line; pos = p.pos; }

        #endregion

        #region Output

        public override string ToString() { return line.ToString("000") + ":" + pos.ToString("000"); }

        #endregion
    }

    public class Span
    {
        #region Structure

        public Position begin { get; private set; }
        public Position end { get; private set; }
        
        #endregion

        #region Constructors

        public Span(Span s) { begin = new Position(s.begin); end = new Position(s.end); }
        public Span(Span b, Span e) { begin = b.begin; end = e.end; }
        public Span(Position b, Position e) { begin = b;  end = e; }
        public Span(Token b, Token e ) { begin = b.span.begin; end = e.span.end; }
        public Span(Position p) { begin = p;  end = new Position(p.line,p.pos+1); }

        #endregion

        #region Output

        public override string ToString() { return "(" + begin.ToString() + "," + end.ToString() + ")"; }

        #endregion
    }
}
