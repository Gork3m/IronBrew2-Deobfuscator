using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using IronBrew2_Deobfuscator;

namespace CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Debug.Log("[*] Starting..", ConsoleColor.Green);
            if(!Directory.Exists("./tasks"))
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
            string raw = File.ReadAllText(args[0]);
            Debug.Log("[*] Loaded script");
            File.WriteAllText($"{path}/raw.txt", raw);
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
            

            Console.ReadKey();
        }
    }
}
