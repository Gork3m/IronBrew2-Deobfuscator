using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using MoonSharp;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


namespace IronBrew2_Deobfuscator
{
    public static class Deobfuscator
    {
        public class IronBrewVM
        {
            public string EnumName { get; set; }
            public string StackName { get; set; }
            public string EnvName { get; set; }
            public string InstructionName { get; set; }
            public string Body { get; set; }
        }

        public class Instruction {
            public int Op { get; set; }
            public dynamic RA { get; set; }
            public dynamic RB { get; set; }
            public dynamic RC { get; set; }

            public string Opcode { get; set; }

            public Serializer.Enums.Opcode enumOpcode { get; set; }
            public Serializer.Enums.InstructionType instructionType { get; set; }

        }

        public class Constant {

            public dynamic Data { get; set; }
            public Serializer.Enums.ConstantType constantType { get; set; }
        }

        public class Chunk {
            public Instruction[] Instructions { get; set; }
            public Chunk[] chunks { get; set; }
            public int Flag { get; set; }

            public int ParamCount { get; set; }
            public Constant[] constants { get; set; }
            // should have a line info as well but fuck it
            
        }
        public static string MinifyScript(string script, string path)
        {
            Debug.Log("[!] Minifying...");
            System.Diagnostics.Process prc = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "./util/luajit.exe",
                    WorkingDirectory = "./util/lua/minifier",
                    Arguments = $" luasrcdiet.lua ../../../{path}/raw.txt",
                    RedirectStandardOutput = false

                }
            };
            prc.Start();
            prc.WaitForExit();

            return File.ReadAllText($"{path}/raw_.txt");
        }
        public static Random rnd = new Random();
        public static string DeobfuscateScript(string scriptPath)
        {
            IronBrewVM vm = new IronBrewVM();
            string raw = File.ReadAllText(scriptPath);
            if (!Directory.Exists("./tasks"))
            {
                Directory.CreateDirectory("./tasks");
            }
            
            string L = "023456789qwertyuiopasdfghjklzxcvbnm";
            string deobfuscationToken = "";
            for (int i = 0; i < 20; i++)
            {
                deobfuscationToken += L[rnd.Next(0, L.Length)].ToString();
            }
            deobfuscationToken = Path.GetFileNameWithoutExtension(scriptPath);
            string path = "./tasks/DEOBF_" + deobfuscationToken;
            // try { Directory.Delete(path, true); } catch { }
            Debug.Log(deobfuscationToken, ConsoleColor.Red);
            Directory.CreateDirectory(path);
// ##################################################################################################
// ##################################################################################################

            Debug.Log("[*] Loaded script");
            File.WriteAllText($"{path}/raw.txt", raw);
            raw = MinifyScript(raw, path);

// ##################################################################################################
// ##################################################################################################

            Debug.Log("[1] Fetching sections..", ConsoleColor.White);
            vm = VMData.FetchVMSections(raw);
            File.WriteAllText($"{path}/interpreter.txt", vm.Body);
            Debug.Log("----[A] VMBodyLength: " + vm.Body.Length + "  VMStackName: " + vm.StackName + "  VMEnvName: " + vm.EnvName + "  VMInstName: " + vm.InstructionName);


// ##################################################################################################
// ##################################################################################################

            Debug.Log("[2] Finding opcodes..");
            vm.Body = VMData.ReplaceOpcodes(vm);
            File.WriteAllText($"{path}/opcodes_ip.txt", vm.Body);

// ##################################################################################################
// ##################################################################################################

            Debug.Log("[3] Preparing the interpreter for enum // opcode matching", ConsoleColor.Green);
            Debug.Log("----[A] Finding enum variable");
            Regex r = new Regex(Recognizers.VM.InterpreterEnumerator(), RegexOptions.Singleline);
            string enumvar = (r.Match(vm.Body)).Groups[1].Value;
            Debug.Log("----[B] Enum name: " + enumvar);
            vm.EnumName = enumvar;
            
            string emulatorCode = Emulation.GetInterpreterEmulatorFunction(vm);
            File.WriteAllText($"{path}/ip_emulator.txt", emulatorCode);

// ##################################################################################################
// ##################################################################################################

            Debug.Log("[4] Dumping instructions..");
            string dumperscript = Emulation.ApplyDeserializerDump(raw);
            File.WriteAllText($"{path}/instr_dump.txt", dumperscript);

            System.Diagnostics.Process prc = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = "/c \"util\\luajit.exe\" " + path.Substring(2).Replace("/","\\") + "\\instr_dump.txt > " + path.Substring(2).Replace("/","\\") + "\\instr_dump_out.txt"

                }
            };
            prc.Start();
            prc.WaitForExit();
            string instrs = "";
            try
            {
                instrs = File.ReadAllText(path + "/instr_dump_out.txt");
                if (!instrs.Contains("{"))
                {
                    int a = 0;
                    a = a / 0;
                }
            }
            catch { Debug.Log("Unable to dump instructions!", ConsoleColor.Red); throw new Exception("INSTR_DUMP_FAIL"); }

            Debug.Log(instrs, ConsoleColor.Cyan);
           

// ##################################################################################################
// ##################################################################################################

            Debug.Log("[5] Dumping opcodes..");
            prc.StartInfo.Arguments = "/c \"util\\luajit.exe\" " + path.Substring(2).Replace("/", "\\") + "\\ip_emulator.txt > " + path.Substring(2).Replace("/", "\\") + "\\enum_dump_out.txt";
            prc.Start();
            prc.WaitForExit();
            string opcodes = "";
            try
            {
                opcodes = File.ReadAllText(path + "/enum_dump_out.txt");
            }
            catch { Debug.Log("Unable to dump opcodes!", ConsoleColor.Red); throw new Exception("OPCODE_DUMP_FAIL"); }
            Debug.Log(opcodes, ConsoleColor.Cyan);




            Debug.Log("[6] Deserializing lua data..");

            JObject main = JObject.Parse("{\"MAIN\":" + instrs + "}");

            Debug.Log("::::CRITICAL ZONE:::: ", ConsoleColor.Red);
            


            

            StreamReader sr;
            
            sr = new StreamReader(path + "/enum_dump_out.txt");
            List<VMData.IB2Opcode> mainOps = new List<VMData.IB2Opcode>();
            while (!sr.EndOfStream) {
                string l = sr.ReadLine();
                if (l.Length < 2) continue;

                string _e = l.Substring(0, l.IndexOf("|"));
                string _o = l.Substring(l.IndexOf("|") + 1);
                mainOps.Add(new VMData.IB2Opcode(Int32.Parse(_e), _o));

            }

            int lastIdx = -1;
            for (int i = 0; i < mainOps.Count; i++) {
                if (lastIdx == mainOps[i].Enum) {
                    mainOps[i - 1].isSuperOperatorOpcode = true;
                    mainOps[i].isSuperOperatorOpcode = true;
                }
                Debug.Log("OPCODE:" + mainOps[i].Enum + " / " + mainOps[i].Opcode + "," + mainOps[i].isSuperOperatorOpcode);
                lastIdx = mainOps[i].Enum;
            }

            sr.Close();

            Debug.Log("---[+] Deserialized opcodes!");

            int getOpIdx(int e) {
               // Debug.Log("Searched for " + e);
                for (int i = 0; i < mainOps.Count; i++) {
                    if (e == mainOps[i].Enum) {
                        return i;
                    }
                }
                return -1;
            }


            Chunk testchunk = new Chunk() {
                Flag = 2,
                ParamCount = 1,
                chunks = new Chunk[] { },
                constants = new Constant[] { new Constant() { constantType = Serializer.Enums.ConstantType.String, Data = "print" },
            new Constant() { constantType = Serializer.Enums.ConstantType.String, Data = "World" },
            new Constant() { constantType = Serializer.Enums.ConstantType.Number, Data = 5 }
            },
                Instructions = new Instruction[] {
               new Instruction() {
                enumOpcode = Serializer.Enums.Opcode.Len,
                RA = 1,
                RB = 0,
                RC = 0,
                instructionType = Serializer.Enums.InstructionMappings[Serializer.Enums.Opcode.Len]

            },
               new Instruction() {
                enumOpcode = Serializer.Enums.Opcode.LoadConst,
                RA = 2,
                RB = 2,
                RC = 0,
                instructionType = Serializer.Enums.InstructionMappings[Serializer.Enums.Opcode.LoadConst]

            },
               new Instruction() {
                enumOpcode = Serializer.Enums.Opcode.Eq,
                RA = 0,
                RB = 1,
                RC = 2,
                instructionType = Serializer.Enums.InstructionMappings[Serializer.Enums.Opcode.Eq]

            },
             new Instruction() {
                enumOpcode = Serializer.Enums.Opcode.Jmp,
                RA = 4,
                RB = 4,
                RC = 4,
                instructionType = Serializer.Enums.InstructionMappings[Serializer.Enums.Opcode.Jmp]

            },
            new Instruction() {
                enumOpcode = Serializer.Enums.Opcode.GetGlobal,
                RA = 1,
                RB = 0,
                RC = 0,
                instructionType = Serializer.Enums.InstructionMappings[Serializer.Enums.Opcode.GetGlobal]

            },
            new Instruction() {
                enumOpcode = Serializer.Enums.Opcode.Move,
                RA = 2,
                RB = 0,
                RC = 0,
                instructionType = Serializer.Enums.InstructionMappings[Serializer.Enums.Opcode.Move]

            },
            new Instruction() {
                enumOpcode = Serializer.Enums.Opcode.LoadConst,
                RA = 3,
                RB = 1,
                RC = 0,
                instructionType = Serializer.Enums.InstructionMappings[Serializer.Enums.Opcode.LoadConst]

            },
                new Instruction() {
                enumOpcode = Serializer.Enums.Opcode.Call,
                RA = 1,
                RB = 3,
                RC = 1,
                instructionType = Serializer.Enums.InstructionMappings[Serializer.Enums.Opcode.Call]

            },
                new Instruction() {
                enumOpcode = Serializer.Enums.Opcode.Return,
                RA = 0,
                RB = 1,
                RC = 0,
                instructionType = Serializer.Enums.InstructionMappings[Serializer.Enums.Opcode.Return]

            }

            }
            };


            Chunk testchunkmain = new Chunk() {
                Flag = 2,
                ParamCount = 0,
                chunks = new Chunk[] { testchunk },
                constants = new Constant[] { new Constant() { constantType = Serializer.Enums.ConstantType.String, Data = "error" },
            new Constant() { constantType = Serializer.Enums.ConstantType.String, Data = "Hello" }
            },
                Instructions = new Instruction[] {
            new Instruction() {
                enumOpcode = Serializer.Enums.Opcode.Closure,
                RA = 0,
                RB = 0,
                RC = 0,
                instructionType = Serializer.Enums.InstructionMappings[Serializer.Enums.Opcode.Closure]

            },

            new Instruction() {
                enumOpcode = Serializer.Enums.Opcode.LoadConst,
                RA = 1,
                RB = 1,
                RC = 0,
                instructionType = Serializer.Enums.InstructionMappings[Serializer.Enums.Opcode.LoadConst]

            },

           

              
                new Instruction() {
                enumOpcode = Serializer.Enums.Opcode.Call,
                RA = 0,
                RB = 2,
                RC = 1,
                instructionType = Serializer.Enums.InstructionMappings[Serializer.Enums.Opcode.Call]

            },
                new Instruction() {
                enumOpcode = Serializer.Enums.Opcode.Return,
                RA = 0,
                RB = 1,
                RC = 0,
                instructionType = Serializer.Enums.InstructionMappings[Serializer.Enums.Opcode.Return]

            }

            }
            };

            // File.WriteAllBytes("nuts.out", Serializer.Serialize(testchunkmain));

            int GetConstantIndex(Constant c,ref List<Constant> pool) {
                int idx = 0;
                foreach(Constant cs in pool) {
                    if (cs.constantType == c.constantType && cs.Data == c.Data) {
                        return idx;
                    }
                    idx++;
                }
                pool.Add(c);
                return pool.Count - 1;
            }

            string logs = "";
            void Log(string msg) {
                logs += msg + "\n";
            }
            Chunk GetChunk(JArray array) {
                Chunk c = new Chunk();
                List<Constant> constpool = new List<Constant>() { };
                c.Flag = 2;
                
                List<Instruction> instructions = new List<Instruction>();
                JArray ins = (JArray)array[0];
                int JMPReference = 0;
                int offset = 0;
                int lastSuperOpIdx = -1;
                for (int i = 0; i < ins.Count; i++) {
                    string inst2print = "";
                    Instruction inst = new Instruction();
                    inst.Op = (int)((ins[i])[0]); 
                    try { inst.RA = 0; inst.RA = (ins[i])[1]; inst2print += VMData.Pad("| A: " + inst.RA, 9); } catch { }
                    try { inst.RB = 0; inst.RB = (ins[i])[2]; inst2print += VMData.Pad("| B: " + inst.RB, 15); } catch { }
                    try { inst.RC = 0; inst.RC = (ins[i])[3]; inst2print += VMData.Pad("| C: " + inst.RC, 25); } catch { }

                    int opcodeIndexToGet = inst.Op;
                    if (lastSuperOpIdx != -1) {
                        opcodeIndexToGet = lastSuperOpIdx;
                    }
                    int oi = getOpIdx(opcodeIndexToGet) + offset;
                   // Debug.Log("Length " + mainOps.Count);
                   // Debug.Log("Got " + oi + " / " + offset);
                    if (oi == -1) continue;
                    VMData.IB2Opcode OP = mainOps[oi];
                    VMData.IB2Opcode NextOP = new VMData.IB2Opcode(-1, "") { isSuperOperatorOpcode = false };
                    try {
                        NextOP = mainOps[oi + 1];
                    } catch {
                        
                    }
                    

                    if (OP.isSuperOperatorOpcode) {
                       
                        if (lastSuperOpIdx == -1) {
                            Debug.Log("SUPER OPERATOR START", ConsoleColor.Red);
                            Log("SUPER OPERATOR START");
                            lastSuperOpIdx = OP.Enum;                            
                        }
                        offset++;
                        inst.Opcode = OP.Opcode;
                    } else {

                        offset = 0;
                        lastSuperOpIdx = -1;
                        inst.Opcode = OP.Opcode;
                    }

                    if (OP.isSuperOperatorOpcode && (NextOP.Enum != lastSuperOpIdx || OP.Opcode == "RETURN")) {
                        offset = 0;
                        lastSuperOpIdx = -1;
                        Log("SUPER OPERATOR END");
                        Debug.Log("SUPER OPERATOR END", ConsoleColor.DarkRed);
                    }

                    inst2print = "[" + i + "] " + VMData.Pad(inst.Opcode + (inst.Opcode == "CLOSURENU" || inst.Opcode == "CLOSURE" ? ("(" + inst.RB + ")") : ""), 12) + inst2print;

                    switch(inst.Opcode) {

                        case "CALL":
                            inst.enumOpcode = Serializer.Enums.Opcode.Call;
                            inst.instructionType = Serializer.Enums.InstructionMappings[inst.enumOpcode];

                            break;

                        case "JMP":
                            inst.enumOpcode = Serializer.Enums.Opcode.Jmp;
                            inst.instructionType = Serializer.Enums.InstructionMappings[inst.enumOpcode];
                            
                            break;
                        case "EQ":
                            inst.enumOpcode = Serializer.Enums.Opcode.Eq;
                            inst.instructionType = Serializer.Enums.InstructionMappings[inst.enumOpcode];
                            JMPReference = inst.RB - i;
                            int rb2 = inst.RB;
                            inst.RB = inst.RA;
                            inst.RA = 0;
                            break;
                        case "NEQ":
                            inst.enumOpcode = Serializer.Enums.Opcode.Eq;
                            inst.instructionType = Serializer.Enums.InstructionMappings[inst.enumOpcode];
                            JMPReference = inst.RB - i;
                            int rb = inst.RB;
                            inst.RB = inst.RA;
                            inst.RA = 1;

                            break;

                        case "LEN":
                            inst.enumOpcode = Serializer.Enums.Opcode.Len;
                            inst.instructionType = Serializer.Enums.InstructionMappings[inst.enumOpcode];

                            break;
                        case "CONCAT":
                            inst.enumOpcode = Serializer.Enums.Opcode.Concat;
                            inst.instructionType = Serializer.Enums.InstructionMappings[inst.enumOpcode];

                            break;


                        case "CLOSURE":
                            inst.enumOpcode = Serializer.Enums.Opcode.Closure;
                            inst.instructionType = Serializer.Enums.InstructionMappings[inst.enumOpcode];

                            break;
                        case "RETURN":
                            inst.enumOpcode = Serializer.Enums.Opcode.Return;
                            inst.instructionType = Serializer.Enums.InstructionMappings[inst.enumOpcode];

                            break;
                        case "LOADK":
                            inst.enumOpcode = Serializer.Enums.Opcode.LoadConst;
                            inst.instructionType = Serializer.Enums.InstructionMappings[inst.enumOpcode];
                            Constant _c = new Constant() { Data = inst.RB };
                            //Debug.Log("CONSTANT TYPE: " + _c.Data.Type.ToString(), ConsoleColor.Cyan);
                            if (_c.Data.Type.ToString() == "String") {
                                _c.constantType = Serializer.Enums.ConstantType.String;
                            }
                            if (_c.Data.Type.ToString() == "Float" || _c.Data.Type.ToString() == "Integer") {
                                _c.constantType = Serializer.Enums.ConstantType.Number;
                            }
                            inst.RB = GetConstantIndex(_c, ref constpool);
                            break;
                        case "GETGLOBAL":
                            inst.enumOpcode = Serializer.Enums.Opcode.GetGlobal;
                            inst.instructionType = Serializer.Enums.InstructionMappings[inst.enumOpcode];
                            Constant _c2 = new Constant() { Data = inst.RB };
                            if (_c2.Data.Type.ToString() == "String") {
                                _c2.constantType = Serializer.Enums.ConstantType.String;
                            }
                            if (_c2.Data.Type.ToString() == "Integer" || _c2.Data.Type.ToString() == "Float") {
                                _c2.constantType = Serializer.Enums.ConstantType.Number;
                            }
                            inst.RB = GetConstantIndex(_c2, ref constpool);
                            break;
                    }

                    instructions.Add(inst);

                    Log(inst2print);
                    Debug.Log(inst2print, ConsoleColor.Green);
                }
                c.Instructions = instructions.ToArray();
                c.constants = constpool.ToArray();
                Debug.Log(constpool.Count+"");
                List<Chunk> subchunks = new List<Chunk>();
                JObject chks = (JObject)array[1];
                for (int i = 0; i < chks.Count; i++) {
                    Debug.Log("Function: " + i + " Start");
                    Log("Function: " + i + " Start");
                    subchunks.Add(GetChunk((JArray)chks[i.ToString()]));
                    Debug.Log("Function: " + i + " End");
                    Log("Function: " + i + " End");
                }

                c.chunks = subchunks.ToArray();
                c.Flag = (int)array[2];

                return c;

            }

            Debug.Log("Function Top Start");
            Log("Function Top Start");
            Chunk MainChunk = GetChunk((JArray)main["MAIN"]);
            Debug.Log("Function Top End");
            Log("Function Top End");

            Debug.Log("---[+] Deserialized instructions!");


            Debug.Log("Serializing to bytecode.luac", ConsoleColor.Yellow);
            Directory.CreateDirectory(path + "/Deobfuscated");
            byte[] bc = Serializer.Serialize(MainChunk);
            File.WriteAllBytes(path + "/Deobfuscated/bytecode.luac", bc);
            File.WriteAllText(path + "/Deobfuscated/ast.json", (JsonConvert.SerializeObject(MainChunk)));
            File.WriteAllText(path + "/Deobfuscated/disassembly.txt", logs);
            File.WriteAllText(path + "/Deobfuscated/original.lua", raw);
            File.WriteAllText(path + "/Deobfuscated/deobfuscated.lua", VMData.DecompileBytecode(bc));
           

            //Console.Clear();
            Debug.Log("[+] Done with matching", ConsoleColor.Green);

            

            return logs;
        }
    }
}
