using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using MoonSharp;
using MoonSharp.Interpreter;




namespace IronBrew2_Deobfuscator
{
    public static class Deobfuscator
    {
        public class IronBrewVM
        {
            public string Interpreter { get; set; }
            public string EnumName { get; set; }
        }
        public static string MinifyScript(string script, string path)
        {
            Debug.Log("[!] Minifying...");
            System.Diagnostics.Process prc = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "util/luajit.exe",
                    WorkingDirectory = "util/lua/minifier",
                    Arguments = $" luasrcdiet.lua ../../../{path}/raw.txt",
                    RedirectStandardOutput = false

                }
            };
            prc.Start();
            prc.WaitForExit();

            return File.ReadAllText($"{path}/raw_.txt");
        }
        public static Random rnd = new Random();
        public static string DeobfuscateScript(string script)
        {
            IronBrewVM vm = new IronBrewVM();
            string raw = script;
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
            string path = "./tasks/DEOBF_" + deobfuscationToken;
            Directory.CreateDirectory(path);
// ##################################################################################################
// ##################################################################################################

            Debug.Log("[*] Loaded script");
            File.WriteAllText($"{path}/raw.txt", raw);
            raw = MinifyScript(raw, path);

// ##################################################################################################
// ##################################################################################################

            Debug.Log("[1] Finding interpreter..");
            string _interpreter = VMData.FindInterpreter(raw);
            File.WriteAllText($"{path}/interpreter.txt", _interpreter);
            vm.Interpreter = _interpreter;

// ##################################################################################################
// ##################################################################################################

            Debug.Log("[2] Finding opcodes..");
            _interpreter = VMData.ReplaceOpcodes(_interpreter);
            File.WriteAllText($"{path}/opcodes_ip.txt", _interpreter);

// ##################################################################################################
// ##################################################################################################

            Debug.Log("[3] Preparing the interpreter for enum // opcode matching", ConsoleColor.Green);
            Debug.Log("----[A] Finding enum variable");
            Regex r = new Regex(Recognizers.VM.InterpreterEnumerator(), RegexOptions.Singleline);
            string enumvar = (r.Match(_interpreter)).Groups[1].Value;
            Debug.Log("----[B] Enum name: " + enumvar);
            vm.EnumName = enumvar;
            vm.Interpreter = _interpreter;
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
                if (!instrs.Contains(">"))
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
            List<Emulation.Lua.Value[]> _Instructions = new List<Emulation.Lua.Value[]>();

            StreamReader sr = new StreamReader(path + "/instr_dump_out.txt");
            int stage = 0;
            dynamic getLuaData(string s) {
                string meaning = s[0].ToString();
                string rest = s.Substring(1);
                switch(meaning) {
                    case "N":
                        return Double.Parse(rest);
                        
                    case "B":
                        return rest == "true";

                    case "S":
                        return rest;

                    case "L":
                        return null;
                }
                return null;
            }
            string[] dtypes = { "S", "N", "B", "L" };
            while (!sr.EndOfStream) {
                string l = sr.ReadLine();
                if (l.Length < 2) continue;
                if (l == ">END") break;
                if (l == ">INSTR") stage = 1;
                if (l == ">PROTO") stage = 2;
                
                if (l[0] != '>') {
                    if (stage == 1) {
                        List<Emulation.Lua.Value> instdata = new List<Emulation.Lua.Value>();
                        int sizeof_data = Int32.Parse(l.Substring(0, l.IndexOf("|")));
                        int p = 1;
                        Debug.Log(sizeof_data + " = SIZE");
                        l = l.Substring(l.IndexOf("|") + 1);
                        while(p <= sizeof_data) {
                            string data = "";
                            
                            if (p != sizeof_data) {
                                // not last item
                                data = l.Substring(0, l.IndexOf("|"));
                                l = l.Substring(l.IndexOf("|") + 1);

                            } else {
                                data = l;
                            }
                            Debug.Log(l + " / " + data, ConsoleColor.Yellow);

                            instdata.Add(new Emulation.Lua.Value(getLuaData(data), (Emulation.Lua.Value.Type)Array.IndexOf(dtypes, data[0].ToString())));

                            p++;
                        }
                        _Instructions.Add(instdata.ToArray());

                    }
                }


            }
            sr.Close();
            Debug.Log("---[+] Deserialized instructions!");
            sr = new StreamReader(path + "/enum_dump_out.txt");
            List<VMData.IB2Opcode> mainOps = new List<VMData.IB2Opcode>();
            while (!sr.EndOfStream) {
                string l = sr.ReadLine();
                if (l.Length < 2) continue;

                string _e = l.Substring(0, l.IndexOf("|"));
                string _o = l.Substring(l.IndexOf("|") + 1);
                mainOps.Add(new VMData.IB2Opcode(Int32.Parse(_e), _o));

            }
            Debug.Log("---[+] Deserialized opcodes!");
            for (int i = 0; i < _Instructions.Count; i++) {
                Debug.Log("ENUM:" + _Instructions[i][0].value + " / " + _Instructions[i][_Instructions[i].Length - 1].value, ConsoleColor.White);
            }
            int lastIdx = -1;
            for (int i = 0; i < mainOps.Count; i++) {
                if (lastIdx == mainOps[i].Enum) {
                    mainOps[i - 1].isSuperOperatorOpcode = true;
                    mainOps[i].isSuperOperatorOpcode = true;
                }
                Debug.Log("OPCODE:" + mainOps[i].Enum + " / " + mainOps[i].Opcode);
                lastIdx = mainOps[i].Enum;
            }

            

            Debug.Log("[7] Matching opcodes with instructions..");
            List<Emulation.Lua.Value[]> matches = new List<Emulation.Lua.Value[]>();
            int superOpCombo = 0;
            int lastSuperOpIdx = 0;
            int superOpAmount = 0;
            int superOpValue = 0;
            for (int i = 0; i < _Instructions.Count; i++) {
                Emulation.Lua.Value[] currentInstruction = _Instructions[i];
                Debug.Log("ENUM:" + currentInstruction[0].value + " / " + _Instructions[i][_Instructions[i].Length - 1].value, ConsoleColor.White);

                for (int b = 0; b < mainOps.Count; b++) {
                    
                    VMData.IB2Opcode currentOpcode = mainOps[b];
                    if (superOpAmount > 0) {
                        currentOpcode = mainOps[lastSuperOpIdx];
                        b = lastSuperOpIdx;
                    }
                    List<Emulation.Lua.Value> currentMatch = new List<Emulation.Lua.Value>();
                    if (currentInstruction[0].value == currentOpcode.Enum || superOpAmount > 0) {
                        
                        if (currentOpcode.isSuperOperatorOpcode && superOpAmount == 0) {
                            superOpValue = currentOpcode.Enum;
                            for (int j = b; j < mainOps.Count; j++) {
                                if (mainOps[j].Enum == superOpValue) {
                                    superOpAmount++;
                                } else {
                                    Debug.Log("[*] Got " + superOpAmount + " super operators in a row", ConsoleColor.Red);
                                    break;
                                }
                            }
                        }
                        if (superOpAmount > 0) { superOpAmount--; lastSuperOpIdx = b + 1; }

                        currentMatch.Add(new Emulation.Lua.Value(currentOpcode.Enum, Emulation.Lua.Value.Type.Number));
                        currentMatch.Add(new Emulation.Lua.Value(VMData.Pad(currentOpcode.Opcode) + "|", Emulation.Lua.Value.Type.String));
                        string serializedLine = "";
                        for (int k = 1; k < currentInstruction.Length; k++) {
                            
                            serializedLine = currentInstruction[k].value + " ";
                            currentMatch.Add(new Emulation.Lua.Value(currentInstruction[k].value, Emulation.Lua.Value.Type.Nil));
                        }
                        Debug.Log("Match: " + VMData.Pad(currentOpcode.Opcode) + " | " + serializedLine);
                        matches.Add(currentMatch.ToArray());
                        break;
                    }
                    
                }

            }
            //Console.Clear();
            Debug.Log("[+] Done with matching", ConsoleColor.Green);
            for (int i = 0; i < matches.Count; i++) {
                string serializedline = "";
                for (int k = 0; k < matches[i].Length; k++) {
                    serializedline += matches[i][k].value + " ";
                }
                if (serializedline != "")
                Debug.Log(i + " --> " + serializedline, ConsoleColor.Cyan);
            }
            

            return "";
        }
    }
}
