
namespace Verse.Schemas.Protobuf
{
	enum WireType : byte
	{
		VarInt = 0,
		Fixed64 = 1,
		LengthDelimited = 2,
		GroupBegin = 3,
		GroupEnd = 4,
		Fixed32 = 5
	}
}
