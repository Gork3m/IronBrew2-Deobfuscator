using System;
using System.Collections.Generic;
using System.Text;

namespace IronBrew2_Deobfuscator
{
    public class Recognizers {
        public static class VM {
            public static string Interpreter() {
                return @"while true do \w+=\w+\[\w+\];? ?\w+=\w+\[[0-9]\];? ?(.+?)\w+=\w+\+1;? ?end;? ?end\)?;? ?end;? ?return \w+\(";
            }
            public static string InterpreterEnumerator() {
                return @"if ?\(?(\w+)[<>=]{1,2}[0-9] ?\)?then";
            }
            public static string DeserializerFunction() {
                return @"end ?;?(return \w+\((\w+)\(\),\{\}.{2,20}$)";
            }

            public static string InstName() {
                return @"local (\w+?) ?;?local \w+? ?;?while true do";
            }

            public static string EnvName() {
                return @"local function \w+?\(\w, ?\w, ?(\w)\)(?:.{1,100})(?:return\(? ?function\(\.\.\.\))";

            }
            public static string StackName() {
                return @"#',\.\.\.\)-1 ?;?local .{3,10}(?:local (\w+) ?= ?\{\} ?;?for) \w+?=0";
            }

        }


        public class Opcodes
        {
            public static string OP_NextInstruction(Deobfuscator.IronBrewVM vm)
            {
                return @"(\w+?=\w+?\+1;? ?\w+?=\w+?\[\w+?\];?)";
            }
            public static string GetGlobal(Deobfuscator.IronBrewVM vm)
            {
                return $@"{vm.StackName}\[{vm.InstructionName}\[[0-9]\]\] *?= *?{vm.EnvName}\[{vm.InstructionName}\[[0-9]\]\];";
            }

            public static string LoadK(Deobfuscator.IronBrewVM vm)
            {
                return $@"{vm.StackName}\[{vm.InstructionName}\[[0-9]\]\] *?= *?{vm.InstructionName}\[[0-9]\];";
            }

            public static string Move(Deobfuscator.IronBrewVM vm)
            {
                return $@"{vm.StackName}\[{vm.InstructionName}\[[0-9]\]\] *?= *?{vm.StackName}\[{vm.InstructionName}\[[0-9]\]\];?";
            }

            public static string Return()
            {
                return "do return end;?";
            }

            public static string LoadNil()
            {
                return @"for \w+ *?=\w+\[[0-9]\],\w+\[[0-9]\] do \w+\[\w+]*?=nil;?end;?";
            }

            public static string GetUpVal(Deobfuscator.IronBrewVM vm)
            {
                return $@"{vm.StackName}\[{vm.InstructionName}\[2\]\]=\w+\[{vm.InstructionName}\[3\]\];?";
            }

            public static string SetGlobal()
            {
                return @"\w+\[\w+\[\w+\[[0-9]\]\]\]*?=\w+\[\w+\[[0-9]\]\];?";
            }

            public static string SetUpVal()
            {
                return @"\w+\[\w+\w\[[0-9]\]\]*?=\w+\[\w+\[[0-9]\]\];?";
            }

            public static string Unm()
            {
                return @"\w+\[\w+\[[0-9]\]\]=\-\w+\[\w+\[[0-9]\]\];?";
            }

            public static string Not()
            {
                return @"\w+\[\w+\[[0-9]\]\]=\(?not \w+\[\w+\[[0-9]\]\]\)?;?";
            }

            public static string Len(Deobfuscator.IronBrewVM vm)
            {
                return $@"{vm.StackName}\[{vm.InstructionName}\[[0-9]\]\]=#{vm.StackName}\[{vm.InstructionName}\[[0-9]\]\];?";
            }

            public static string Concat(Deobfuscator.IronBrewVM vm)
            {
                
                return $@"(local )?\w+={vm.InstructionName}\[[0-9]\];? ?(local )?\w+={vm.StackName}\[\w+\];? ?for \w+=\w+\+1,{vm.InstructionName}\[[0-9]\]do \w+=\w+\.\.{vm.StackName}\[\w\];? ?end;? ?{vm.StackName}\[{vm.InstructionName}\[[0-9]\]\]=a;? ?";
            }

            public static string Jmp(Deobfuscator.IronBrewVM vm)
            {
                return $@"\w+={vm.InstructionName}\[[0-9]\];?";
            }

            public static class Closure {
                public static string ClosureActual(Deobfuscator.IronBrewVM vm) {
                    return $@"(local )?\w+=\w+\[{vm.InstructionName}\[[0-9]\]\];? ?(local \w+;?)? ?;?(local )?\w+=\{{\}} ?;?\w+=\w+\(\{{\}},.{{20,}}?#\w+\+1\]=\w+ ?;?end;? ?{vm.StackName}\[{vm.InstructionName}\[[0-9]\]\]=\w+\(\w+,\w+,{vm.EnvName}\);? ?";

                }
                public static string ClosureNU(Deobfuscator.IronBrewVM vm) {
                    return $@"{vm.StackName}\[{vm.InstructionName}\[[0-9]\]\]=\w+\(\w+\[{vm.InstructionName}\[[0-9]\]\],nil,{vm.EnvName}\);? ?";
                }
            }

            public static string ForLoop()
            {
                return @"local \w+=\w+\[\d];local \w+=\w+\[\w+\+2];local \w+=\w+\[\w+]\+\w+;\w+\[\w+\]=\w+;if \w+>0 then if \w+<=\w+\[\w+\+1] then \w+=\w+\+\w+\[\d];\w+\[\w+\+3]=\w+;end;elseif \w+>=\w+\[\w+\+1] then \w+=\w+\+\w+\[\d];\w+\[\w+\+3]=\w+;end;?";
            }

            public static string ForPrep()
            {
                return @"local \w+=\w+\[\d];\w+\[\w+]=\w+\[\w+]-\w+\[\w+\+2];\w+=\w+\+\w+\[\d];";
            }

            public static string TForLoop()
            {
                return @"local \w+=\w+\[\d];local \w+=\w+\[\d];local \w+=\w+\+2;local \w+={\w+\[\w+]\(\w+\[\w+\+1],\w+\[\w+\]\)};for \w+=1,\w+ do \w+\[\w+\+\w+]=\w+\[\w+];end;local \w+=\w+\[\w+\+3];if \w+ then \w+\[\w+\]=\w+ else \w+=\w+\+1;end;?";
            }

            public static string Close()
            {
                return @"local \w+=\w+\[\d];local \w+={};for \w+=1,#\w+ do local \w+=\w+\[\w+];for \w+=0,#\w+ do local \w+=\w+\[\w+];local \w+=\w+\[1];local \w+=\w+\[2]; if \w+==\w+ and \w+>=\w+ then \w+\[\w+]=\w+\[\w+];\w+\[1]=\w+;end;end;end;?";
            }
            
            public static class LoadBool
            {
                public static string LoadBoolA()
                {
                    return @"\w+?\[\w+?\[[0-9]\]\] *?= *?\(\w+?\[[0-9]\]\~=[0-9]\);?";
                }

                public static string LoadBoolC()
                {

                    return @"\w+?\[\w+?\[[0-9]\]\] *?= *?\(\w+?\[[0-9]\]\~=[0-9]\);?\w+? ?=\w+?\+1;?";
                }
            }

            public static class GetTable
            {
                public static string GetTableA()
                {
                    return @"\w+\[\w+\[[0-9]\]\]*?=\w+\[\w+\[[0-9]\]\]\[\w+\[\w+\[[0-9]\]\]\];?";
                }

                public static string GetTableConst()
                {
                    return @"\w+\[\w+\[[0-9]\]\]*?=\w+\[\w+\[[0-9]\]\]\[\w+\[\w+\[[0-9]\]\]\];?";
                }
            }

            public static class SetTable
            {
                public static string SetTableA()
                {
                    return @"\w+\[\w+\[[0-9]\]\]\[\w+\[\w+\[[0-9]\]\]\]*?=\w+\[\w+\[[0-9]\]\];?";
                }

                public static string SetTableB()
                {
                    return @"\w+\[\w+\[[0-9]\]\]\[\w+\[\w+\[[0-9]\]\]\]*?=\w+\[\w+\[[0-9]\]\];?";
                }

                public static string SetTableC()
                {
                    return @"\w+\[\w+\[[0-9]\]\]\[\w+\[\w+\[[0-9]\]\]\]*?=\w+\[\w+\[[0-9]\]\];?";
                }

                public static string SetTableD()
                {
                    return @"\w+\[\w+\[[0-9]\]\]\[\w+\[\w+\[[0-9]\]\]\]*?=\w+\[\w+\[[0-9]\]\];?";
                }
            }

            public static class NewTable
            {
                public static string NewTableA()
                {
                    return @"\w+\[\w+\[[0-9]\]\]*?={};?";
                }

                public static string NewTableB()
                {

                    return @"\w+\[\w+\[[0-9]\]\]*?={};?";
                }

                public static string NewTableC()
                {
                    return @"\w+\[\w+\[[0-9]\]\]*?={unpack\({}, *?1, *?\w+\[[0-9]\]\)};?";
                }
            }

            public static class Self
            {
                public static string SelfA()
                {
                    return @"local *?\w+=\w+\[[0-9]\];?local *?\w+=\w+\[\w+\[[0-9]\]\];?\w+\[\w+\+1\]=\w+;?\w+\[\w+\]=\w+\[\w+\[\w+\[[0-9]\]\]\];?";
                }

                public static string SelfB()
                {
                    return @"local \w+=\w+\[[0-9]\];local \w+=\w+\[\w+\[[0-9]\]\];?\w+\[\w+\+1\]=\w+;?\w+\[\w+\]=\w+\[\w+\[\w+\[[0-9]\]\]\];?";
                }
            }

            public static class Add
            {
                public static string AddA()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]\+\w+\[\w+\[[0-9]\]\];?";
                }

                public static string AddB()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]\+\w+\[\w+\[[0-9]\]\];?";
                }

                public static string AddC()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]\+\w+\[\w+\[[0-9]\]\];?";
                }

                public static string AddD()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]\+\w+\[\w+\[[0-9]\]\];?";
                }
            }
            
            public static class Sub
            {
                public static string SubA()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]-\w+\[\w+\[[0-9]\]\];?";
                }

                public static string SubB()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]-\w+\[\w+\[[0-9]\]\];?";
                }

                public static string SubC()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]-\w+\[\w+\[[0-9]\]\];?";
                }

                public static string SubD()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]-\w+\[\w+\[[0-9]\]\];?";
                }
            }
            
            public static class Mul
            {
                public static string MulA()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]*\w+\[\w+\[[0-9]\]\];?";
                }

                public static string MulB()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]*\w+\[\w+\[[0-9]\]\];?";
                }

                public static string MulC()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]*\w+\[\w+\[[0-9]\]\];?";
                }

                public static string MulD()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]*\w+\[\w+\[[0-9]\]\];?";
                }
            }
            
            public static class Div
            {
                public static string DivA()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]\/\w+\[\w+\[[0-9]\]\];?";
                }

                public static string DivB()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]\/\w+\[\w+\[[0-9]\]\];?";
                }

                public static string DivC()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]\/\w+\[\w+\[[0-9]\]\];?";
                }

                public static string DivD()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]\/\w+\[\w+\[[0-9]\]\];?";
                }
            }
            
            public static class Mod
            {
                public static string ModA()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]\%w+\[\w+\[[0-9]\]\];?";
                }

                public static string ModB()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]%\w+\[\w+\[[0-9]\]\];?";
                }

                public static string ModC()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]%\w+\[\w+\[[0-9]\]\];?";
                }

                public static string ModD()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]%\w+\[\w+\[[0-9]\]\];?";
                }
            }
            
            public static class Pow
            {
                public static string PowA()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]\^w+\[\w+\[[0-9]\]\];?";
                }

                public static string PowB()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]\^\w+\[\w+\[[0-9]\]\];?";
                }

                public static string PowC()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]\^\w+\[\w+\[[0-9]\]\];?";
                }

                public static string PowD()
                {
                    return @"\w+\[\w+\[[0-9]\]\]=\w+\[\w+\[[0-9]\]\]\^\w+\[\w+\[[0-9]\]\];?";
                }
            }

            public static class Eq
            {
                public static string EqA(Deobfuscator.IronBrewVM vm)
                {
                    return $@"if\({vm.StackName}\[{vm.InstructionName}\[[0-9]\]\]=={vm.StackName}\[{vm.InstructionName}\[[0-9]\]\]\) ?then \w+=\w+\+1;? ?else \w+={vm.InstructionName}\[[0-9]\];? ?end;? ?";
                }

                public static string EqB(Deobfuscator.IronBrewVM vm) {
                    return $@"if\({vm.InstructionName}\[[0-9]\]\=={vm.StackName}\[{vm.InstructionName}\[[0-9]\]\]\) ?then \w+=\w+\+1;? ?else \w+={vm.InstructionName}\[[0-9]\];? ?end;? ?";
                }

                public static string EqC(Deobfuscator.IronBrewVM vm)
                {
                    return $@"if\({vm.StackName}\[{vm.InstructionName}\[[0-9]\]\]=={vm.InstructionName}\[[0-9]\]\) ?then \w+=\w+\+1;? ?else \w+={vm.InstructionName}\[[0-9]\];? ?end;? ?";
                }

                public static string EqD(Deobfuscator.IronBrewVM vm) {
                    return $@"if\({vm.InstructionName}\[[0-9]\]=={vm.InstructionName}\[[0-9]\]\) ?then \w+=\w+\+1;? ?else \w+={vm.InstructionName}\[[0-9]\];? ?end;? ?";
                }
            }

            public static class Neq {
                public static string NeqA(Deobfuscator.IronBrewVM vm) {
                    return $@"if\({vm.StackName}\[{vm.InstructionName}\[[0-9]\]\]~={vm.StackName}\[{vm.InstructionName}\[[0-9]\]\]\) ?then \w+=\w+\+1;? ?else \w+={vm.InstructionName}\[[0-9]\];? ?end;? ?";
                }

                public static string NeqB(Deobfuscator.IronBrewVM vm) {
                    return $@"if\({vm.InstructionName}\[[0-9]\]\~={vm.StackName}\[{vm.InstructionName}\[[0-9]\]\]\) ?then \w+=\w+\+1;? ?else \w+={vm.InstructionName}\[[0-9]\];? ?end;? ?";
                }

                public static string NeqC(Deobfuscator.IronBrewVM vm) {
                    return $@"if\({vm.StackName}\[{vm.InstructionName}\[[0-9]\]\]~={vm.InstructionName}\[[0-9]\]\) ?then \w+=\w+\+1;? ?else \w+={vm.InstructionName}\[[0-9]\];? ?end;? ?";
                }

                public static string NeqD(Deobfuscator.IronBrewVM vm) {
                    return $@"if\({vm.InstructionName}\[[0-9]\]~={vm.InstructionName}\[[0-9]\]\) ?then \w+=\w+\+1;? ?else \w+={vm.InstructionName}\[[0-9]\];? ?end;? ?";
                }
            }

            public static class Lt
            {
                public static string LtA()
                {
                    return @"if\(?\w+\[\w+\[[0-9]\]\]<\w+\[\w+\[[0-9]\]\]\)?then \w+=\w+\+1;else \w+=\w+\+\w+\[[0-9]];end;";
                }

                public static string LtB()
                {
                    return @"if\(?\w+\[\w+\[[0-9]\]\]<\w+\[\w+\[[0-9]\]\]\)?then \w+=\w+\+1;else \w+=\w+\+\w+\[[0-9]\];end;?";
                }

                public static string LtC()
                {
                    return @"if\(?\w+\[\w+\[[0-9]\]\]<\w+\[\w+\[[0-9]\]\]\)?then \w+=\w+\+1;else \w+=\w+\+\w+\[[0-9]\];?end;?";
                }

                public static string LtD()
                {
                    return @"if\(?\w+\[\w+\[[0-9]\]\]<\w+\[\w+\[[0-9]\]\]\)?then \w+=\w+\+1;else \w+=\w+\+\w+\[[0-9]\];end?";
                }
            }

            public static class Le
            {
                public static string LeA()
                {
                    return
                        @"if\(?\w+\[\w+\[[0-9]\]\]<=\w+\[\w+\[[0-9]\]\]\)?then \w+=\w+\+1;else \w+=\w+\+\w+\[[0-9]\];end;? ";
                }

                public static string LeB()
                {
                    return
                        @"if\(?\w+\[\w+\[\w+\]\]<=\w+\[\w+\[[0-9]\]\]\)?then \w+=\w+\+1;else \w+=\w+\+\w+\[[0-9]\];end;?";
                }

                public static string LeC()
                {
                    return
                        @"if\(?\w+\[\w+\[[0-9]\]\]<=\w+\[\w+\[[0-9]\]\]\)?then \w+=\w+\+1;else \w+=\w+\+\w+\[[0-9]\];end;?";
                }

                public static string LeD()
                {
                    return
                        @"if\(?\w+\[\w+\[[0-9]\]\]<=\w+\[\w+\[[0-9]\]\]\)?then \w+=\w+\+1;else \w+=\w+\+\w+\[[0-9]\];end;?";
                }
            }

            public static class Test
            {
                public static string TestA()
                {
                    return @"if \w+\[\w+\[[0-9]\]\] then \w+=\w+\+1;else \w+=\w+\+\w+\[[0-9]\];end;?";
                }

                public static string TestB()
                {
                    return @"if not \w+\[\w+\[[0-9]\]\] then \w+=\w+\+1;else \w+=\w+\+\w+\[[0-9]\];end;?";
                }
            }

            public static class TestSet
            {
                public static string TestSetA()
                {
                    return @"local \w+=\w+\[\w+\[\d\]\];if \w+ then \w+=\w+\+1;else \w+\[\w+\[\d\]\]=\w+;\w+=\w+\+\w+\[\w+\+1\]\[\d\]\+1;end;?";
                }

                public static string TestSetB()
                {
                    return @"local \w+=\w+\[\w+\[\d\]\];if not \w+ then \w+=\w+\+1;else \w+\[\w+\[\d\]\]=\w+;\w+=\w+\+\w+\[\w+\+1\]\[\d\]\+1;end;?";
                }
            }

            public static class Call
            {
                public static string CallC2B2(Deobfuscator.IronBrewVM vm) {
                    return $@"(local )?\w+={vm.InstructionName}\[[0-9]\];? ?{vm.StackName}\[\w+\]={vm.StackName}\[\w+\]\({vm.StackName}\[\w+\+1\]\);? ?";
                }
                public static string CallC1B2(Deobfuscator.IronBrewVM vm) {
                    return $@"(local )?\w+={vm.InstructionName}\[[0-9]\];? ?{vm.StackName}\[\w+\]\({vm.StackName}\[\w+\+1\]\);? ?";
                }
                public static string CallC1B1(Deobfuscator.IronBrewVM vm) {
                    return $@"{vm.StackName}\[{vm.InstructionName}\[[0-9]\]\]\(\);? ?";
                }
                public static string CallA()
                {
                    return @"local \w+=\w+\[\d\];local \w+={};local \w+=0;local \w+=\w+\+\w+\[\d]-1;for \w+=\w+\+1,\w+ do \w+=\w+\+1;\w+\[\w+]=\w+\[\w+\];end;local \w+={\w+\[\w+\]\(?Unpack\(\w+,1,\w+-\w+\)?\)?};local \w+=\w+\+\w+\[\d]-2;\w+=0;for \w+=\w+,\w+ do \w+=\w+\+1;\w+\[\w+]=\w+\[\w+];end;\w+=\w+;?";
                }

                public static string CallB()
                {
                    return @"local \w+=\w+\[\d];local \w+={};local \w+=0;local \w+=\w+;for \w+=\w+\+1,\w+ do \w+=\w+\+1;\w+\[\w+\]=\w+\[\w+\];end;local \w+={\w+\[\w+]\(?Unpack\(\w+,1,\w+-\w+\)?\)?};?local \w+=\w+\+\w+\[\d]-2;\w+=0;for \w+=\w+,\w+ do \w+=\w+\+1;\w+\[\w+\]=\w+\[\w+\];end;\w+=\w+;?";
                }

                public static string CallC()
                {
                    return @"local \w+=\w+\[\d];local \w+,\w+={\w+\[\w+\]\(\)};local \w+=\w+\+\w+\[\d]-2;local \w+=0;for \w+=\w+,\w+ do \w+=\w+\+1;\w+\[\w+]=\w+\[\w+\];end;\w+=\w+;?";
                }

                public static string CallD()
                {
                    return @"local \w+=\w+\[\d];local \w+={};local \w+=0;local \w+=\w+\+\w+\[\d]-1;for \w+=\w+\+1,\w+ do \w+=\w+\+1;\w+\[\w+\]=\w+\[\w+\];end;local \w+,\w+=\w+\(\w+\[\w+\]\(Unpack\(\w+,1,\w+-\w+\)\)\);\w+=\w+\+\w+-1;\w+=0;for \w+=\w+,\w+ do \w+=\w+\+1;\w+\[\w+\]=\w+\[\w+\];end;\w+=\w+;?";
                }
            }

            public static class TailCall
            {
                public static string TailCallA()
                {
                    return @"local \w+=\w+\[\d];local \w+={};local \w+=\w+\+\w+\[\d]-1;for \w+=\w+\+1,\w+ do \w+\[#\w+\+1]=\w+\[\w+];end;do return \w+\[\w+]\(Unpack\(\w+,1,\w+-\w+\)\) end;?";
                }

                public static string TailCallB()
                {
                    return @"local \w+=\w+\[\d];local \w+={};local \w+=\w+;for \w+=\w+\+1,\w+ do \w+\[#\w+\+1]=\w+\[\w+];end;do return \w+\[\w+]\(Unpack\(\w+,1,\w+-\w+\)\) end;?";
                }

                public static string TailCallC()
                {
                    return @"local \w+=\w+\[\d];do return \w+\[\w+\]\(\); end;?";
                }
            }

            public static class VarArg
            {
                public static string VarArgA()
                {
                    return @"local \w+=\w+\[\d];local \w+=\w+\[\d];for \w+=\w+,\w+\+\w+-1 do \w+\[\w+]=\w+\[\w+-\w+];end;?";
                }

                public static string VarArgB()
                {
                    return @"local \w+=\w+\[\d];\w+=\w+\+\w+-1;for \w+=\w+,\w+ do local \w+=\w+\[\w+-\w+];\w+\[\w+]=\w+;end;?";
                }
            }

            public static class SetList
            {
                public static string SetListA()
                {
                    return @"";
                }

                public static string SetListB()
                {
                    return @"";
                }
            }
        }
    }
}

/*
0 MOVE ADDED
1 LOADK ADDED
2 LOADBOOL ADDED
3 LOADNIL ADDED
4 GETUPVAL ADDED
5 GETGLOBAL ADDED
6 GETTABLE ADDED
7 SETGLOBAL ADDED
8 SETUPVAL ADDED
9 SETTABLE ADDED
10 NEWTABLE ADDED
11 SELF ADDED
12 ADD ADDED
13 SUB ADDED
14 MUL ADDED
15 DIV ADDED
16 MOD ADDED
17 POW ADDED
18 UNM ADDED
19 NOT ADDED
20 LEN ADDED
21 CONCAT ADDED
22 JMP ADDED
23 EQ ADDED
24 LT ADDED
25 LE ADDED
26 TEST ADDED
27 TESTSET ADDED
28 CALL 4/9 ADDED
29 TAILCALL ADDED
30 RETURN ADDED
31 FORLOOP ADDED
32 FORPREP ADDED
33 TFORLOOP ADDED
34 SETLIST not added
35 CLOSE ADDED
36 CLOSURE ADDED
37 VARARG ADDED
*/
