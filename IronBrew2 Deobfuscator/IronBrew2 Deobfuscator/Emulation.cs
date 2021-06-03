using System;
using System.Collections.Generic;
using System.Text;

namespace IronBrew2_Deobfuscator
{
    public static class Emulation
    {
        public static string GetInterpreterEmulatorFunction(Deobfuscator.IronBrewVM vm)
        {
            return $@"local function ITERATE(enumval) 
    local {vm.EnumName} = enumval;
{vm.Interpreter}
    end

print('DUMP_BEGIN');
for i=0, 128 do
    ITERATE(i);
end
print('DUMP_END');

";
        }
    }
}
