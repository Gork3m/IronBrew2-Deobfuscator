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
            if (args.Length == 1) {
                string raw = File.ReadAllText(args[0]);
                File.WriteAllText("Deobfuscated_" + DateTime.Now.ToString().Replace("/", "-").Replace(":", "-") + ".txt", Deobfuscator.DeobfuscateScript(args[0]));
                Console.ReadKey();
            } else {
                string p = Path.GetFullPath(args[0]);
                Console.WriteLine(p);
                Deobfuscator.DeobfuscateScript(p + "/" + args[1]);

            }
          
        }
    }
}
