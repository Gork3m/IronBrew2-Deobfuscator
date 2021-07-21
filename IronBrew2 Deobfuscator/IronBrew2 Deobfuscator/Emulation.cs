using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace IronBrew2_Deobfuscator
{
    public static class Emulation
    {
        public static class Lua {
            public class Value {
                public enum Type {
                    String,
                    Number,
                    Boolean,
                    Nil
                }
                public dynamic value { get; set; }
                public Type type { get; set; }
                public Value(dynamic val, Type _type) {
                    value = val;
                    type = _type;
                    
                }
            }

        }
        public static string GetCondom() {
            return @"
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
dofile = nil
collectgarbage = nil
setmetatable = nil

";
        }
        public static string ApplyDeserializerDump(string script)
        {
            string dump1 = @"

"+GetCondom()+@"
local datatypes = {['nil'] = 'L', ['number'] = 'N', ['boolean'] = 'B', ['string'] = 'S', ['table'] = 'T'}
local DUMPTABLE = {}
local RecursiveDump
RecursiveDump = function(a, b)
    if type(a) == 'number' then return end
    for iX, aX in pairs(a) do
        local tn = #aX .. '|'
        for i = 1, #aX do
            tn = tn .. datatypes[type(aX[i])]..''.. aX[i]
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

            
            var names = regex.Match(script);
            string argName = "";
            try
            {
                argName = names.Groups[1].Value;

            }
            catch {
                Debug.Log("Unable to find deserialized tables", ConsoleColor.Red);
                throw new Exception("DESERIALIZED_TABLES_NOT_FOUND");
            }
            bool shouldSkipArgParser = false;
            string argparser = names.Groups[2].Value;
            if (!argparser.Contains("true")) { //aztupbrew's bullshit
                shouldSkipArgParser = true;
            }
            string before = script.Substring(0, shouldSkipArgParser ? names.Groups[2].Index : names.Groups[3].Index);
            string after = script.Substring(names.Groups[3].Index);
            string newscript = dump1 + before + @" table.insert(DUMPTABLE, '>INSTR\n')
    RecursiveDump("+argName+@"[1])
    table.insert(DUMPTABLE, '>PROTO\n')    
    RecursiveDump("+argName+@"[2]) table.insert(DUMPTABLE, '>END'); " + dump2 + "  " + after;

            return newscript;
        }
        public static string GetInterpreterEmulatorFunction(Deobfuscator.IronBrewVM vm)
        {
            return $@"
{GetCondom()}

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
