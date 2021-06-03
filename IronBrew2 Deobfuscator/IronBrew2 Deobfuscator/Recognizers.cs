using System;
using System.Collections.Generic;
using System.Text;

namespace IronBrew2_Deobfuscator
{
    public static class Recognizers
    {
        public static class VM
        {
            public static string Interpreter()
            {
                return @"while true do \w+=\w+\[\w+\];? ?\w+=\w+\[[0-9]\];? ?(.+?)\w+=\w+\+1;? ?end;? ?end\)?;? ?end;? ?return \w+\(";
            }
        }
    }
}
