using System;
using System.Collections.Generic;
using System.Text;

namespace IronBrew2_Deobfuscator
{
    public static class Emulation
    {
        public static string GetInterpreterEmulatorFunction(Deobfuscator.IronBrewVM vm)
        {
            return $@"
os = nil
io = nil
_G.os = nil
_G.io = nil
require = nil
_G.require = nil
load = nil
loadstring = nil
_G.loadstring = nil
_G.load = nil


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
