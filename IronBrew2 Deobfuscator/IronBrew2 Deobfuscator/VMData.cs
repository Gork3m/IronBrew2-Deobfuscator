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


    }
}
