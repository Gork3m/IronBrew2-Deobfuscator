using System;
using System.Collections.Generic;
using System.Text;

namespace IronBrew2_Deobfuscator {
    public static class Serializer {



		public static Encoding luaEnc = Encoding.GetEncoding(28591);

		public static byte[] Serialize(Deobfuscator.Chunk _chunk) {
			List<byte> res = new List<byte>();

			void WriteByte(byte b) =>
				res.Add(b);

			void WriteBytes(byte[] bs) =>
				res.AddRange(bs);

			void WriteInt(int i) =>
				WriteBytes(BitConverter.GetBytes(i));

			void WriteUInt(uint i) =>
				WriteBytes(BitConverter.GetBytes(i));

			void WriteNum(double d) =>
				WriteBytes(BitConverter.GetBytes(d));

			void WriteString(string str) {
				byte[] bytes = luaEnc.GetBytes(str);

				WriteInt(bytes.Length + 1);
				WriteBytes(bytes);
				WriteByte(0);
			}

			void WriteChunk(Deobfuscator.Chunk chunk) {

				WriteInt(0); // chunk name

				WriteInt(0); // chunk line
				WriteInt(0); // last line
				WriteByte(0); // upvalue count - FIXME
				WriteByte((byte)chunk.ParamCount); // param count - FIXME
				WriteByte(2); // vararg flag - FIXME
				WriteByte(0xff); // stack size - FIXME


				WriteInt(chunk.Instructions.Length);
				foreach (var i in chunk.Instructions) {

					int a = i.RA;
					int b = i.RB;
					int c = i.RC;

					uint result = 0;

					result |= (uint)i.enumOpcode;
					result |= ((uint)a << 6);

					switch (i.instructionType) {
						case Enums.InstructionType.ABx:
							result |= ((uint)b << (6 + 8));
							break;

						case Enums.InstructionType.AsBx:
							b += 131071;
							result |= ((uint)b << (6 + 8));
							break;

						case Enums.InstructionType.ABC:
							result |= ((uint)c << (6 + 8));
							result |= ((uint)b << (6 + 8 + 9));
							break;
					}

					WriteUInt(result);
				}

				WriteInt(chunk.constants.Length);
				foreach (var constant in chunk.constants) {
					switch (constant.constantType) {
						case Enums.ConstantType.Nil:
							WriteByte(0);
							break;

						case Enums.ConstantType.Boolean:
							WriteByte(1);
							WriteByte((byte)((bool)constant.Data ? 1 : 0));
							break;

						case Enums.ConstantType.Number:
							WriteByte(3);
							WriteNum(constant.Data);
							break;

						case Enums.ConstantType.String:
							WriteByte(4);
							WriteString(constant.Data);
							break;
					}
				}

				WriteInt(chunk.chunks.Length);
				foreach (var sChunk in chunk.chunks)
					WriteChunk(sChunk);

				WriteInt(0);
				WriteInt(0);
				WriteInt(0);

				//WriteInt(chunk.Upvalues.Count);
				//foreach (var str in chunk.Upvalues)
				//	WriteString(str);
			}

			WriteByte(27);
			WriteBytes(luaEnc.GetBytes("Lua"));
			WriteByte(0x51);
			WriteByte(0);
			WriteByte(1);
			WriteByte(4);
			WriteByte(4);
			WriteByte(4);
			WriteByte(8);
			WriteByte(0);

			WriteChunk(_chunk);

			return res.ToArray();
		}

		public static class Enums {
			public enum ConstantType {
				Nil,
				Boolean,
				Number,
				String
			}

			public enum InstructionType {
				ABC,
				ABx,
				AsBx,
				AsBxC,
				Data
			}
			public enum Opcode {
				Move,
				LoadConst,
				LoadBool,
				LoadNil,
				GetUpval,
				GetGlobal,
				GetTable,
				SetGlobal,
				SetUpval,
				SetTable,
				NewTable,
				Self,
				Add,
				Sub,
				Mul,
				Div,
				Mod,
				Pow,
				Unm,
				Not,
				Len,
				Concat,
				Jmp,
				Eq,
				Lt,
				Le,
				Test,
				TestSet,
				Call,
				TailCall,
				Return,
				ForLoop,
				ForPrep,
				TForLoop,
				SetList,
				Close,
				Closure,
				VarArg
			}
			public static Dictionary<Opcode, InstructionType> InstructionMappings = new Dictionary<Opcode, InstructionType>()
			{
			{ Opcode.Move, InstructionType.ABC },
			{ Opcode.LoadConst, InstructionType.ABx },
			{ Opcode.LoadBool, InstructionType.ABC },
			{ Opcode.LoadNil, InstructionType.ABC },
			{ Opcode.GetUpval, InstructionType.ABC },
			{ Opcode.GetGlobal, InstructionType.ABx },
			{ Opcode.GetTable, InstructionType.ABC },
			{ Opcode.SetGlobal, InstructionType.ABx },
			{ Opcode.SetUpval, InstructionType.ABC },
			{ Opcode.SetTable, InstructionType.ABC },
			{ Opcode.NewTable, InstructionType.ABC },
			{ Opcode.Self, InstructionType.ABC },
			{ Opcode.Add, InstructionType.ABC },
			{ Opcode.Sub, InstructionType.ABC },
			{ Opcode.Mul, InstructionType.ABC },
			{ Opcode.Div, InstructionType.ABC },
			{ Opcode.Mod, InstructionType.ABC },
			{ Opcode.Pow, InstructionType.ABC },
			{ Opcode.Unm, InstructionType.ABC },
			{ Opcode.Not, InstructionType.ABC },
			{ Opcode.Len, InstructionType.ABC },
			{ Opcode.Concat, InstructionType.ABC },
			{ Opcode.Jmp, InstructionType.AsBx },
			{ Opcode.Eq, InstructionType.ABC },
			{ Opcode.Lt, InstructionType.ABC },
			{ Opcode.Le, InstructionType.ABC },
			{ Opcode.Test, InstructionType.ABC },
			{ Opcode.TestSet, InstructionType.ABC },
			{ Opcode.Call, InstructionType.ABC },
			{ Opcode.TailCall, InstructionType.ABC },
			{ Opcode.Return, InstructionType.ABC },
			{ Opcode.ForLoop, InstructionType.AsBx },
			{ Opcode.ForPrep, InstructionType.AsBx },
			{ Opcode.TForLoop, InstructionType.ABC },
			{ Opcode.SetList, InstructionType.ABC },
			{ Opcode.Close, InstructionType.ABC },
			{ Opcode.Closure, InstructionType.ABx },
			{ Opcode.VarArg, InstructionType.ABC }
		};
		}
	}
}
