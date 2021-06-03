using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;


namespace IronBrew2_Deobfuscator
{
    public static class Deobfuscator
    {
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
        public static string DeobfuscateScript(string script)
        {
            string raw = script;
            if (!Directory.Exists("./tasks"))
            {
                Directory.CreateDirectory("./tasks");
            }
            Random rnd = new Random();
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



            return "";
        }
    }
}
