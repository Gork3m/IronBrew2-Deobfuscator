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
ffi = nil
string.dump = nil

";
        }
        public static string ApplyDeserializerDump(string script)
        {
            string dump1 = @"

"+GetCondom()+ @"

local json = {}

-- Internal functions.

local function kind_of(obj)
    if type(obj) ~= ""table"" then
        return type(obj)
    end
    local i = 1
    for _ in pairs(obj) do
        if obj[i] ~= nil then
            i = i + 1
        else
            return ""table""
        end
    end
    if i == 1 then
        return ""table""
    else
        return ""array""
    end
end

local function escape_str(s)
   local s2 = """"
   for i=1,#s do
    local toadd = string.byte(s:sub(i,i))
    if toadd >= 32 and toadd <= 126 and toadd ~= 92 then 
        toadd = string.char(toadd)
        else
        toadd = '\\\\' .. toadd
    end
    s2 = s2 .. toadd 
   end
    return s2 
end

-- Returns pos, did_find; there are two cases:
-- 1. Delimiter found: pos = pos after leading space + delim; did_find = true.
-- 2. Delimiter not found: pos = pos after leading space;     did_find = false.
-- This throws an error if err_if_missing is true and the delim is not found.
local function skip_delim(str, pos, delim, err_if_missing)
    pos = pos + #str:match(""^%s*"", pos)
    if str:sub(pos, pos) ~= delim then
        if err_if_missing then
            error(""Expected "" .. delim .. "" near position "" .. pos)
        end
        return pos, false
    end
    return pos + 1, true
end

-- Expects the given pos to be the first character after the opening quote.
-- Returns val, pos; the returned pos is after the closing quote character.
local function parse_str_val(str, pos, val)
    val = val or """"
    local early_end_error = ""End of input found while parsing string.""
    if pos > #str then
        error(early_end_error)
    end
    local c = str:sub(pos, pos)
    if c == '""' then
        return val, pos + 1
    end
    if c ~= ""\\"" then
        return parse_str_val(str, pos + 1, val .. c)
    end
    -- We must have a \ character.
    local esc_map = {b = ""\b"", f = ""\f"", n = ""\n"", r = ""\r"", t = ""\t""}
    local nextc = str:sub(pos + 1, pos + 1)
    if not nextc then
        error(early_end_error)
    end
    return parse_str_val(str, pos + 2, val .. (esc_map[nextc] or nextc))
end

-- Returns val, pos; the returned pos is after the number's final character.
local function parse_num_val(str, pos)
    local num_str = str:match(""^-?%d+%.?%d*[eE]?[+-]?%d*"", pos)
    local val = tonumber(num_str)
    if not val then
        error(""Error parsing number at position "" .. pos .. ""."")
    end
    return val, pos + #num_str
end

-- Public values and functions.

function json.stringify(obj, as_key)
    local s = {} -- We'll build the string as an array of strings to be concatenated.
    local kind = kind_of(obj) -- This is 'array' if it's an array or type(obj) otherwise.
    if kind == ""array"" then
        if as_key then
            error(""Can't encode array as key."")
        end
        s[#s + 1] = ""[""
        for i, val in ipairs(obj) do
            if i > 1 then
                s[#s + 1] = "", ""
            end
            s[#s + 1] = json.stringify(val)
        end
        s[#s + 1] = ""]""
    elseif kind == ""table"" then
        if as_key then
            error(""Can't encode table as key."")
        end
        s[#s + 1] = ""{""
        for k, v in pairs(obj) do
            if #s > 1 then
                s[#s + 1] = "", ""
            end
            s[#s + 1] = json.stringify(k, true)
            s[#s + 1] = "":""
            s[#s + 1] = json.stringify(v)
        end
        s[#s + 1] = ""}""
    elseif kind == ""string"" then
        return '""' .. escape_str(obj) .. '""'
    elseif kind == ""number"" then
        if as_key then
            return '""' .. tostring(obj) .. '""'
        end
        return tostring(obj)
    elseif kind == ""boolean"" then
        return tostring(obj)
    elseif kind == ""nil"" then
        return ""null""
    else
       -- error(""Unjsonifiable type: "" .. kind .. ""."")
       return tostring(kind)
    end
    return table.concat(s)
end

json.null = {} -- This is a one-off table to represent the null value.

function json.parse(str, pos, end_delim)
    pos = pos or 1
    if pos > #str then
        error(""Reached unexpected end of input."")
    end
    local pos = pos + #str:match(""^%s*"", pos) -- Skip whitespace.
    local first = str:sub(pos, pos)
    if first == ""{"" then -- Parse an object.
        local obj, key, delim_found = {}, true, true
        pos = pos + 1
        while true do
            key, pos = json.parse(str, pos, ""}"")
            if key == nil then
                return obj, pos
            end
            if not delim_found then
                error(""Comma missing between object items."")
            end
            pos = skip_delim(str, pos, "":"", true) -- true -> error if missing.
            obj[key], pos = json.parse(str, pos)
            pos, delim_found = skip_delim(str, pos, "","")
        end
    elseif first == ""["" then -- Parse an array.
        local arr, val, delim_found = {}, true, true
        pos = pos + 1
        while true do
            val, pos = json.parse(str, pos, ""]"")
            if val == nil then
                return arr, pos
            end
            if not delim_found then
                error(""Comma missing between array items."")
            end
            arr[#arr + 1] = val
            pos, delim_found = skip_delim(str, pos, "","")
        end
    elseif first == '""' then -- Parse a string.
        return parse_str_val(str, pos + 1)
    elseif first == ""-"" or first:match(""%d"") then -- Parse a number.
        return parse_num_val(str, pos)
    elseif first == end_delim then -- End of an object or array.
        return nil, pos + 1
    else -- Parse true, false, or null.
        local literals = {[""true""] = true, [""false""] = false, [""null""] = json.null}
        for lit_str, lit_val in pairs(literals) do
            local lit_end = pos + #lit_str - 1
            if str:sub(pos, lit_end) == lit_str then
                return lit_val, lit_end + 1
            end
        end
        local pos_info_str = ""position "" .. pos .. "": "" .. str:sub(pos, pos + 10)
        error(""Invalid json syntax starting at "" .. pos_info_str)
    end
end

";
            
            Regex regex = new Regex(Recognizers.VM.DeserializerFunction(), RegexOptions.Singleline);

            
            var names = regex.Match(script);
            string fullReturn = "";
            try
            {
                fullReturn = names.Groups[1].Value;

            }
            catch {
                Debug.Log("Unable to find deserializer", ConsoleColor.Red);
                throw new Exception("DESERIALIZER_NOT_FOUND");
            }
            string deserializerFuncName = names.Groups[2].Value;
            script = script.Substring(0, names.Groups[1].Index);

            script = dump1 + script  + "\n\nprint(json.stringify(" + deserializerFuncName + "()));";

            return script;

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
{vm.Body}
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
