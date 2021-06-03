using System;

namespace IronBrew2_Deobfuscator
{
    public static class Debug
    {

        public static void Log(string msg, ConsoleColor color = ConsoleColor.Gray)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ForegroundColor = old;

        }


    }
}
