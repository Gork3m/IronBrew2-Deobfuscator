using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;


namespace IronBrew2_Deobfuscator
{
    public static class VMData
    {
        public class IB2Opcode {
            public int Enum { get; set; }
            public string Opcode { get; set; }
            public bool isSuperOperatorOpcode { get; set; }
            public IB2Opcode(int i, string opcode) {
                Opcode = opcode;
                Enum = i;
                isSuperOperatorOpcode = false;
            }
        }
        public static string FindInterpreter(string script)
        {
            Regex regex = new Regex(Recognizers.VM.Interpreter(), RegexOptions.Singleline);
            var matches = regex.Matches(script);

            if (matches.Count != 1)
            {
                Debug.Log("Unable to find interpreter loop!", ConsoleColor.Blue);
                throw new Exception("INTERPRETER_NOT_FOUND");
            }
            
            string IP = matches[0].Groups[1].Value;


            return IP;
        }

        public static string ReplaceOpcodes(string interpreter)
        {
            OpcodeIDs opcodeIDs = new OpcodeIDs()
            {
                opidx = 0
            };
            interpreter = ReplaceOp(interpreter, "GETGLOBAL", Recognizers.Opcodes.GetGlobal(),ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "LOADK", Recognizers.Opcodes.LoadK(),ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "RETURN", Recognizers.Opcodes.Return(),ref opcodeIDs);

            ////////////// Untested //////////////

            interpreter = ReplaceOp(interpreter, "MOVE", Recognizers.Opcodes.Move(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "LOADNIL", Recognizers.Opcodes.LoadNil(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "GETUPVAL", Recognizers.Opcodes.GetUpVal(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "SETGLOBAL", Recognizers.Opcodes.SetGlobal(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "SETUPVAL", Recognizers.Opcodes.SetUpVal(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "UNM", Recognizers.Opcodes.Unm(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "NOT", Recognizers.Opcodes.Not(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "LEN", Recognizers.Opcodes.Len(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "CONCAT", Recognizers.Opcodes.Concat(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "JMP", Recognizers.Opcodes.Jmp(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "CLOSURE", Recognizers.Opcodes.Closure(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "FORLOOP", Recognizers.Opcodes.ForLoop(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "FORPREP", Recognizers.Opcodes.ForPrep(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "TFORLOOP", Recognizers.Opcodes.TForLoop(), ref opcodeIDs);
            
            ///////////// Call opcode deserves its own section lol ///////////
            
            
            ///////////// opcodes with mutations or something /////////////
            interpreter = ReplaceOp(interpreter, "LOADBOOLA", Recognizers.Opcodes.LoadBool.LoadBoolA(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "LOADBOOLC", Recognizers.Opcodes.LoadBool.LoadBoolC(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "SETTABLEA", Recognizers.Opcodes.SetTable.SetTableA(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "SETTABLEB", Recognizers.Opcodes.SetTable.SetTableB(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "SETTABLEC", Recognizers.Opcodes.SetTable.SetTableC(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "SETTABLED", Recognizers.Opcodes.SetTable.SetTableD(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "GETTABLE", Recognizers.Opcodes.GetTable.GetTableA(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "GETTABLECONST", Recognizers.Opcodes.GetTable.GetTableConst(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "EQ", Recognizers.Opcodes.Eq.EqA(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "EQB", Recognizers.Opcodes.Eq.EqB(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "EQC", Recognizers.Opcodes.Eq.EqC(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "EQD", Recognizers.Opcodes.Eq.EqD(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "LT", Recognizers.Opcodes.Lt.LtA(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "LTB", Recognizers.Opcodes.Lt.LtB(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "LTC", Recognizers.Opcodes.Lt.LtC(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "LTD", Recognizers.Opcodes.Lt.LtD(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "LE", Recognizers.Opcodes.Le.LeA(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "LEB", Recognizers.Opcodes.Le.LeB(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "LEC", Recognizers.Opcodes.Le.LeC(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "LED", Recognizers.Opcodes.Le.LeD(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "TEST", Recognizers.Opcodes.Test.TestA(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "TESTB", Recognizers.Opcodes.Test.TestB(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "TESTSET", Recognizers.Opcodes.TestSet.TestSetA(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "TESTSETB", Recognizers.Opcodes.TestSet.TestSetB(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "TAILCALL", Recognizers.Opcodes.TailCall.TailCallA(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "TAILCALLB", Recognizers.Opcodes.TailCall.TailCallB(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "TAILCALLC", Recognizers.Opcodes.TailCall.TailCallC(), ref opcodeIDs);
            
            //////////// math opcodes //////////////
            // add
            interpreter = ReplaceOp(interpreter, "ADD", Recognizers.Opcodes.Add.AddA(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "ADDB", Recognizers.Opcodes.Add.AddB(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "ADDC", Recognizers.Opcodes.Add.AddC(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "ADDD", Recognizers.Opcodes.Add.AddD(), ref opcodeIDs);
            // sub
            interpreter = ReplaceOp(interpreter, "SUB", Recognizers.Opcodes.Sub.SubA(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "SUBB", Recognizers.Opcodes.Sub.SubB(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "SUBC", Recognizers.Opcodes.Sub.SubC(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "SUBD", Recognizers.Opcodes.Sub.SubD(), ref opcodeIDs);
            // mul
            interpreter = ReplaceOp(interpreter, "MUL", Recognizers.Opcodes.Mul.MulA(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "MULB", Recognizers.Opcodes.Mul.MulB(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "MULC", Recognizers.Opcodes.Mul.MulC(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "MULD", Recognizers.Opcodes.Mul.MulD(), ref opcodeIDs);
            // div
            interpreter = ReplaceOp(interpreter, "DIV", Recognizers.Opcodes.Div.DivA(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "DIVB", Recognizers.Opcodes.Div.DivB(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "DIVC", Recognizers.Opcodes.Div.DivC(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "DIVD", Recognizers.Opcodes.Div.DivD(), ref opcodeIDs);
            // mod
            interpreter = ReplaceOp(interpreter, "MOD", Recognizers.Opcodes.Mod.ModA(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "MODB", Recognizers.Opcodes.Mod.ModB(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "MODC", Recognizers.Opcodes.Mod.ModC(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "MODD", Recognizers.Opcodes.Mod.ModD(), ref opcodeIDs);
            // pow
            interpreter = ReplaceOp(interpreter, "POW", Recognizers.Opcodes.Pow.PowA(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "POWB", Recognizers.Opcodes.Pow.PowB(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "POWC", Recognizers.Opcodes.Pow.PowC(), ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "POWD", Recognizers.Opcodes.Pow.PowD(), ref opcodeIDs);
            
            // allah yok dinin yalan //

            interpreter = ReplaceOp(interpreter, "NO_OP", Recognizers.Opcodes.OP_NextInstruction(), ref opcodeIDs);

            return interpreter;
        }

        public class OpcodeIDs
        {
            public int opidx { get; set; }
        } 
        
        public static string Pad(string text)
        {
            string ret = text;
            for (int i = 0; i < (14 - text.Length); i++)
            {
                ret += " ";
            }
            return ret;
        }
        private static string ReplaceOp(string interpreter, string opcodename, string rgx, ref OpcodeIDs opcodeIDs)
        {
            Regex regex = new Regex(rgx, RegexOptions.Singleline);
            var matches = regex.Matches(interpreter);
            int diff = 0;
            foreach (Match m in matches)
            {
                string before = interpreter.Substring(0, diff + m.Index);
                string after = interpreter.Substring(m.Index + m.Length + diff);

                string _new = before + (opcodename == "NO_OP" ? " NO_OP_USED=true; " : (" appendOpcode('' .. enumval .. '|" + opcodename + "', "+opcodeIDs.opidx+"); ")) + after;
                opcodeIDs.opidx++;
                diff += _new.Length - interpreter.Length;

                interpreter = _new;
            }
            return interpreter;
        }

    }
}
