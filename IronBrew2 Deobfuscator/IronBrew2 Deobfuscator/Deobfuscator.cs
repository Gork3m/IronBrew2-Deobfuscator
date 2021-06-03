using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;


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
            
            Debug.Log("[*] Loaded script");
            File.WriteAllText($"{path}/raw.txt", raw);
            raw = MinifyScript(raw, path);

            Debug.Log("[1] Finding interpreter..");
            string _interpreter = VMData.FindInterpreter(raw);
            File.WriteAllText($"{path}/interpreter.txt", _interpreter);
            vm.Interpreter = _interpreter;

            Debug.Log("[2] Finding opcodes..");
            _interpreter = VMData.ReplaceOpcodes(_interpreter);
            File.WriteAllText($"{path}/opcodes_ip.txt", _interpreter);

            Debug.Log("[3] Preparing the interpreter for enum // opcode matching", ConsoleColor.Green);
            Debug.Log("----[A] Finding enum variable");

            Regex r = new Regex(Recognizers.VM.InterpreterEnumerator(), RegexOptions.Singleline);
            string enumvar = (r.Match(_interpreter)).Groups[1].Value;
            Debug.Log("----[B] Enum name: " + enumvar);
            vm.EnumName = enumvar;
            vm.Interpreter = _interpreter;

            string emulatorCode = Emulation.GetInterpreterEmulatorFunction(vm);
            File.WriteAllText($"{path}/ip_emulator.txt", emulatorCode);

            

            

            return "";
        }
    }
}
