using System;
using System.Collections.Generic;
using System.Diagnostics;

using SLang;

namespace SLangTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Reader reader = new Reader((Message)null,@"c:\Zouev\SLang\SLang Tests\T010.slang");
        //  Reader reader = new Reader((Message)null,@"c:\Zouev\SLang\SLang Config Min\Version.slang");
        //  Reader reader = new Reader((Message)null, @"c:\Zouev\SLang\SLang Tests\Core ver 0.72.slang");
        //  Reader reader = new Reader((Message)null,@"c:\Zouev\SLang\SLang Tests\Core ver 0.6.2.slang");
        //  Reader reader = new Reader((Message)null,@"c:\Zouev\SLang\SLang Tests\Core ver 0.7 Updated.slang");
        //  Reader reader = new Reader((Message)null,@"c:\Zouev\SLang\SLang Tests\Comparable.slang");
        //  Reader reader = new Reader((Message)null,@"c:\Zouev\SLang\SLang Tests\Any.slang");
        //  Reader reader = new Reader(@"unit Integer is end");
            Options options = new Options();
            Message messagePool = new Message(options);
            Tokenizer tokenizer = new Tokenizer(reader,options,messagePool);

            ENTITY.init(tokenizer,0,messagePool,options);
            COMPILATION compilation = COMPILATION.parse();

            compilation.report(4);
        }
    }
}
