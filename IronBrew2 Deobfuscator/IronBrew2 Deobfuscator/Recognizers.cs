﻿using System;
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
            public static string InterpreterEnumerator()
            {
                return @"if ?\(?(\w+)[<>=]{1,2}[0-9] ?\)?then";
            }
            public static string DeserializedTables()
            {
                return @"local function \w+?\((\w), ?\w, ?\w\)(.{1,100})(return\(? ?function\(\.\.\.\))";
            }
            
        }

        public static class Opcodes
        {
            public static string OP_NextInstruction()
            {
                return @"(\w+?=\w+?\+1;? ?\w+?=\w+?\[\w+?\];?)";
            }
            public static string GetGlobal()
            {
                return @"\w+\[\w+\[[0-9]\]\] *?= *?\w+\[\w+\[[0-9]\]\];";
            }

            public static string LoadK()
            {
                return @"\w+\[\w+\[[0-9]\]\] *?= *?\w+\[[0-9]\];";
            }

            public static string Return()
            {
                return "do return end;?";
            }

            public static class Call
            {
                public static string Call_B2_C1() {
                    return @"(local)? ?\w+=\w+\[[0-9]\] ?;?\w+\[\w+\]\(\w+\[\w+\+[0-9]\]\);? ?";
                }

                public static string Call_B2_C2() {
                    return @"(local)? ?\w+=\w+\[[0-9]\] ?;?\w+\[\w\] ?= ?\w+\[\w+\]\(\w+\[\w+\+[0-9]\]\);? ?";
                }

            }
        }
    }
}
