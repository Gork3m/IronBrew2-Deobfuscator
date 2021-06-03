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
            interpreter = ReplaceOp(interpreter, "CALL_B2_C1", Recognizers.Opcodes.Call.Call_B2_C1());
            interpreter = ReplaceOp(interpreter, "GETGLOBAL", Recognizers.Opcodes.GetGlobal());
            interpreter = ReplaceOp(interpreter, "LOADK", Recognizers.Opcodes.LoadK());
            interpreter = ReplaceOp(interpreter, "RETURN", Recognizers.Opcodes.Return());


            return interpreter;
        }

        private static string ReplaceOp(string interpreter, string opcodename, string rgx)
        {
            Regex regex = new Regex(rgx, RegexOptions.Singleline);
            var matches = regex.Matches(interpreter);
            int diff = 0;
            foreach (Match m in matches)
            {
                string before = interpreter.Substring(0, diff + m.Index);
                string after = interpreter.Substring(m.Index + m.Length + diff);

                string _new = before + " print('ENUM:' .. enumval .. ' | OPCODE:" + opcodename + "'); " + after;
                diff += _new.Length - interpreter.Length;

                interpreter = _new;
            }
            return interpreter;
        }

    }
}
