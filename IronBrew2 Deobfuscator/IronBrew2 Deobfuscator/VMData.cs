using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;


namespace IronBrew2_Deobfuscator
{
    public static class VMData
    {
        public static string FindInterpreter(string script)
        {
            Regex regex = new Regex(Recognizers.VM.Interpreter(), RegexOptions.Singleline);
            var matches = regex.Matches(script);

            if (matches.Count != 1)
            {
                Debug.Log("Unable to find interpreter loop!", ConsoleColor.Red);
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
            interpreter = ReplaceOp(interpreter, "CALL_B2_C1", Recognizers.Opcodes.Call.Call_B2_C1(),ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "GETGLOBAL", Recognizers.Opcodes.GetGlobal(),ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "LOADK", Recognizers.Opcodes.LoadK(),ref opcodeIDs);
            interpreter = ReplaceOp(interpreter, "RETURN", Recognizers.Opcodes.Return(),ref opcodeIDs);

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
            for (int i = 0; i < (6 - text.Length); i++)
            {
                ret = "0" + ret;
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

                string _new = before + (opcodename == "NO_OP" ? " NO_OP_USED=true; " : (" appendOpcode('ENUM:' .. enumval .. ' | OPCODE:" + opcodename + "', "+opcodeIDs.opidx+"); ")) + after;
                opcodeIDs.opidx++;
                diff += _new.Length - interpreter.Length;

                interpreter = _new;
            }
            return interpreter;
        }

    }
}
