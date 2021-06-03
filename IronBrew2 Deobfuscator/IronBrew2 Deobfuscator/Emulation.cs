using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace IronBrew2_Deobfuscator
{
    public static class Emulation
    {
        public static string ApplyDeserializerDump(string script)
        {
            string dump1 = @"

io = nil
arg = nil
gcinfo = nil
module = nil
jit = nil
package = nil
debug = nil
load = nil
loadfile = nil
require = nil
newproxy = nil
loadstring = nil
os = nil

local DUMPTABLE = {}
local RecursiveDump
RecursiveDump = function(a, b)
    for iX, aX in pairs(a) do
        local tn = #aX .. '|'
        for i = 1, #aX do
            tn = tn .. aX[i]
            if (i ~= #aX) then
                tn = tn .. '|'
            end
        end
       -- print(tn)
        table.insert(DUMPTABLE, tn .. '\n')
    end
end
";
            string dump2 = @"
local sex_string = table.concat(DUMPTABLE);
print(sex_string)
    do
        return sex_string; 
    end;
";
            Regex regex = new Regex(Recognizers.VM.DeserializedTables(), RegexOptions.Singleline);

            string INSTR_NAME = "";
            string PROTO_NAME = "";
            string PARAM_NAME = "";
            var names = regex.Match(script);
            try
            {
                INSTR_NAME = names.Groups[1].Value;
                PROTO_NAME = names.Groups[2].Value;
                PARAM_NAME = names.Groups[3].Value;

            }
            catch {
                Debug.Log("Unable to find deserialized tables", ConsoleColor.Red);
                throw new Exception("DESERIALIZED_TABLES_NOT_FOUND");
            }

            string before = script.Substring(0, names.Index + names.Length);
            string after = script.Substring(names.Index + names.Length);
            string newscript = dump1 + before + @" table.insert(DUMPTABLE, '::INSTR::\n')
    RecursiveDump("+INSTR_NAME+@")
    table.insert(DUMPTABLE, '::PROTO::\n')    
    RecursiveDump("+PROTO_NAME+@") " + dump2 + " " + after;

            return newscript;
        }
        public static string GetInterpreterEmulatorFunction(Deobfuscator.IronBrewVM vm)
        {
            return $@"
io = nil
arg = nil
gcinfo = nil
module = nil
jit = nil
package = nil
debug = nil
load = nil
loadfile = nil
require = nil
newproxy = nil
loadstring = nil
os = nil


local opcodeStream = {{}};
local reachedTop = false;
local USED_OPCODES = {{}};
appendOpcode = function(op, idx)
    if USED_OPCODES[idx] then
        reachedTop = true;
    else
        USED_OPCODES[idx] = true;
        table.insert(opcodeStream, op .. '\n');
    end
end

local function ITERATE(enumval) 
    local {vm.EnumName} = enumval;
{vm.Interpreter}
    end


for i=0, 65536 do -- bunu da gecersen ohamk
    if reachedTop then break end
    ITERATE(i);
end

print( table.concat(opcodeStream) );

";
        }
    }
}
