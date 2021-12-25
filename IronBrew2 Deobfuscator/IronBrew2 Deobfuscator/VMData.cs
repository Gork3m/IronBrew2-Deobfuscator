using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using IronBrew2_Deobfuscator;
using System.IO;
using System.Diagnostics;
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
        public static Random rnd = new Random();
        public static string DecompileBytecode(byte[] bytecode) {
            string L = "023456789qwertyuiopasdfghjklzxcvbnm";
            string decompToken = "";
            for (int i = 0; i < 20; i++) {
                decompToken += L[rnd.Next(0, L.Length)].ToString();
            }
            Directory.CreateDirectory("util/decomp/tasks/" + decompToken);
            File.WriteAllBytes("util/decomp/tasks/" + decompToken + "/bc.luac", bytecode);
            Process prc = new Process() {
                StartInfo = new ProcessStartInfo() {
                    FileName = "util/decomp/unluac.exe",
                    WorkingDirectory = "util/decomp",
                    Arguments = "tasks/" + decompToken + "/bc.luac " + "tasks/" + decompToken + "/deobfuscated.lua"
                }
            };
            prc.Start();
            prc.WaitForExit();
            
            if (File.Exists("util/decomp/tasks/" + decompToken + "/deobfuscated.lua")) {
                string s = File.ReadAllText("util/decomp/tasks/" + decompToken + "/deobfuscated.lua");
                Directory.Delete("util/decomp/tasks/" + decompToken, true);
                return s;
            } else {
                Directory.Delete("util/decomp/tasks/" + decompToken, true);
                return "-- Error occured while decompiling.. Use unluac.exe";
            }

        }
        public static Deobfuscator.IronBrewVM FetchVMSections(string script)
        {
            Regex regex = new Regex(Recognizers.VM.Interpreter(), RegexOptions.Singleline);
            var matches = regex.Matches(script);

            if (matches.Count != 1)
            {
                Debug.Log("Unable to find interpreter loop!", ConsoleColor.Blue);
                throw new Exception("INTERPRETER_NOT_FOUND");
            }
            
            string IP = matches[0].Groups[1].Value;


            return new Deobfuscator.IronBrewVM() { 
                Body = IP, 
                EnvName = (new Regex(Recognizers.VM.EnvName(), RegexOptions.Singleline)).Match(script).Groups[1].Value,
                StackName = (new Regex(Recognizers.VM.StackName(), RegexOptions.Singleline)).Match(script).Groups[1].Value,
                InstructionName = (new Regex(Recognizers.VM.InstName(), RegexOptions.Singleline)).Match(script).Groups[1].Value                
            };
        }

        public static string ReplaceOpcodes(Deobfuscator.IronBrewVM vm)
        {
            OpcodeIDs opcodeIDs = new OpcodeIDs()
            {
                opidx = 0
            };

            string interpreter = vm.Body;


            interpreter = ReplaceOp(interpreter, "CLOSURE", Recognizers.Opcodes.Closure.ClosureActual(vm), opcodeIDs);
            interpreter = ReplaceOp(interpreter, "LEN", Recognizers.Opcodes.Len(vm), opcodeIDs);


            interpreter = ReplaceOp(interpreter, "EQ", Recognizers.Opcodes.Eq.EqA(vm), opcodeIDs);
            interpreter = ReplaceOp(interpreter, "EQ", Recognizers.Opcodes.Eq.EqB(vm), opcodeIDs);
            interpreter = ReplaceOp(interpreter, "EQ", Recognizers.Opcodes.Eq.EqC(vm), opcodeIDs);
            interpreter = ReplaceOp(interpreter, "EQ", Recognizers.Opcodes.Eq.EqD(vm), opcodeIDs);

            interpreter = ReplaceOp(interpreter, "NEQ", Recognizers.Opcodes.Neq.NeqA(vm), opcodeIDs);
            interpreter = ReplaceOp(interpreter, "NEQ", Recognizers.Opcodes.Neq.NeqB(vm), opcodeIDs);
            interpreter = ReplaceOp(interpreter, "NEQ", Recognizers.Opcodes.Neq.NeqC(vm), opcodeIDs);
            interpreter = ReplaceOp(interpreter, "NEQ", Recognizers.Opcodes.Neq.NeqD(vm), opcodeIDs);

            interpreter = ReplaceOp(interpreter, "CONCAT", Recognizers.Opcodes.Concat(vm), opcodeIDs);





            interpreter = ReplaceOp(interpreter, "GETGLOBAL", Recognizers.Opcodes.GetGlobal(vm), opcodeIDs);
            interpreter = ReplaceOp(interpreter, "LOADK", Recognizers.Opcodes.LoadK(vm), opcodeIDs);

            interpreter = ReplaceOp(interpreter, "CALL", Recognizers.Opcodes.Call.CallC2B2(vm), opcodeIDs);
            interpreter = ReplaceOp(interpreter, "CALL", Recognizers.Opcodes.Call.CallC1B2(vm), opcodeIDs);
            interpreter = ReplaceOp(interpreter, "CLOSURE", Recognizers.Opcodes.Closure.ClosureNU(vm), opcodeIDs);
            interpreter = ReplaceOp(interpreter, "CALL", Recognizers.Opcodes.Call.CallC1B1(vm), opcodeIDs);

            interpreter = ReplaceOp(interpreter, "JMP", Recognizers.Opcodes.Jmp(vm), opcodeIDs);
            interpreter = ReplaceOp(interpreter, "MOVE", Recognizers.Opcodes.Move(vm), opcodeIDs);
            interpreter = ReplaceOp(interpreter, "GETUPVAL", Recognizers.Opcodes.GetUpVal(vm), opcodeIDs);
            interpreter = ReplaceOp(interpreter, "RETURN", Recognizers.Opcodes.Return(), opcodeIDs);

            
            
            // allah yok dinin yalan //

            interpreter = ReplaceOp(interpreter, "NO_OP", Recognizers.Opcodes.OP_NextInstruction(vm), opcodeIDs);

            return interpreter;
        }

        public class OpcodeIDs
        {
            public int opidx { get; set; }
        } 
        
        public static string Pad(string text, int optlen = 14)
        {
            string ret = text;
            for (int i = 0; i < (optlen - text.Length); i++)
            {
                ret += " ";
            }
            return ret;
        }
        private static string ReplaceOp(string interpreter, string opcodename, string rgx, OpcodeIDs opcodeIDs)
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
