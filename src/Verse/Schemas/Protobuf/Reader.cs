﻿using System;
using System.IO;
using System.Text;
using Verse.DecoderDescriptors.Tree;
using Verse.Formats.Protobuf;
using Verse.Schemas.Protobuf.Definition;

namespace Verse.Schemas.Protobuf;

internal static class ReaderHelper
{
    public static uint ReadInt32(ReaderState state)
    {
        var u32 = (uint)state.Stream.ReadByte();
        u32 += (uint)state.Stream.ReadByte() << 8;
        u32 += (uint)state.Stream.ReadByte() << 16;
        u32 += (uint)state.Stream.ReadByte() << 24;

        return u32;
    }

    public static ulong ReadInt64(ReaderState state)
    {
        var u64 = (ulong)state.Stream.ReadByte();
        u64 += (ulong)state.Stream.ReadByte() << 8;
        u64 += (ulong)state.Stream.ReadByte() << 16;
        u64 += (ulong)state.Stream.ReadByte() << 24;
        u64 += (ulong)state.Stream.ReadByte() << 32;
        u64 += (ulong)state.Stream.ReadByte() << 40;
        u64 += (ulong)state.Stream.ReadByte() << 48;
        u64 += (ulong)state.Stream.ReadByte() << 56;

        return u64;
    }

    public static ulong ReadVarInt(ReaderState state)
    {
        byte current;
        var shift = 0;
        var u64 = 0UL;

        do
        {
            current = (byte)state.Stream.ReadByte();
            u64 += (current & 127u) << shift;
            shift += 7;
        }
        while ((current & 128u) != 0);

        return u64;
    }
}

internal class Reader : IReader<ReaderState, ProtobufValue, int>
{
    public ReaderStatus ReadToArray<TElement>(ReaderState state,
        ReaderCallback<ReaderState, ProtobufValue, int, TElement> callback, out ArrayReader<TElement> arrayReader)
    {
        throw new NotImplementedException();
    }

    public ReaderStatus ReadToObject<TObject>(ReaderState state,
        ILookupNode<int, ReaderCallback<ReaderState, ProtobufValue, int, TObject>> root, ref TObject target)
    {
        var current = state.Stream.ReadByte();

        if (current < 0)
            return ReaderStatus.Succeeded;

        // Read field number and wire type
        var index = current >> 3 & 15;
        var wire = (WireType)(current & 7);

        while ((current & 128) != 0)
        {
            current = state.Stream.ReadByte();
            index = (index << 8) + current;
        }

        // Decode value
        var field = ProtoBinding.Empty;
        /*
                    if (index >= 0 && index < this.bindings.Length && this.bindings[index].Type != ProtoType.Undefined)
                        field = this.bindings[index];
                    else if (!this.rejectUnknown)
                        field = ProtoBinding.Empty;
                    else
                    {
                        state.RaiseError("field {0} with wire type {1} is unknown", index, wire);

                        return false;
                    }
        */

        switch (wire)
        {
            case WireType.Fixed32:
                var u32 = ReaderHelper.ReadInt32(state);

                switch (field.Type)
                {
                    case ProtoType.Fixed32:
                        state.Value = new ProtobufValue(u32);

                        break;

                    case ProtoType.Float:
                        unsafe
                        {
                            state.Value = new ProtobufValue(*(float*)&u32);
                        }

                        break;

                    case ProtoType.SFixed32:
                        state.Value = new ProtobufValue((int)u32);

                        break;

                    case ProtoType.Undefined:
                        break;

                    default:
                        state.RaiseError("field {0} is incompatible with wire type {1}", field.Name, wire);

                        return ReaderStatus.Failed;
                }

                break;

            case WireType.Fixed64:
                var u64 = ReaderHelper.ReadInt64(state);

                switch (field.Type)
                {
                    case ProtoType.Double:
                        unsafe
                        {
                            state.Value = new ProtobufValue(*(double*)&u64);
                        }

                        break;

                    case ProtoType.Fixed64:
                        state.Value = new ProtobufValue(u64);

                        break;

                    case ProtoType.SFixed64:
                        state.Value = new ProtobufValue((long)u64);

                        break;

                    case ProtoType.Undefined:
                        break;

                    default:
                        state.RaiseError("field {0} is incompatible with wire type {1}", field.Name, wire);

                        return ReaderStatus.Failed;
                }

                break;

            case WireType.GroupBegin:
                if (field.Type != ProtoType.Undefined)
                {
                    state.RaiseError("groups are not supported");

                    return ReaderStatus.Failed;
                }

                break;

            case WireType.GroupEnd:
                if (field.Type != ProtoType.Undefined)
                {
                    state.RaiseError("groups are not supported");

                    return ReaderStatus.Failed;
                }

                break;

            case WireType.LengthDelimited:
                switch (field.Type)
                {
                    case ProtoType.Custom:
                        throw new NotImplementedException();

                    case ProtoType.String:
                        var length = ReaderHelper.ReadVarInt(state);
                        /*
                                                    if (length > this.maximumLength)
                                                    {
                                                        state.RaiseError("number of bytes in field {0} ({1}) exceeds allowed maximum ({2})", field.Name, length, this.maximumLength);

                                                        return false;
                                                    }
                        */
                        var buffer = new byte[length];

                        if (state.Stream.Read(buffer, 0, (int)length) != (int)length)
                            return ReaderStatus.Failed;

                        state.Value = new ProtobufValue(Encoding.UTF8.GetString(buffer));

                        break;

                    case ProtoType.Undefined:
                        break;

                    default:
                        state.RaiseError("field {0} is incompatible with wire type {1}", field.Name, wire);

                        return ReaderStatus.Failed;
                }

                return ReaderStatus.Failed;

            case WireType.VarInt:
                var varint = ReaderHelper.ReadVarInt(state);

                switch (field.Type)
                {
                    case ProtoType.Boolean:
                        state.Value = new ProtobufValue(varint != 0);

                        break;

                    case ProtoType.Int32:
                        state.Value = new ProtobufValue((int)varint);

                        break;

                    case ProtoType.Int64:
                        state.Value = new ProtobufValue((long)varint);

                        break;

                    case ProtoType.SInt32:
                    case ProtoType.SInt64:
                        state.Value = new ProtobufValue(-(long)(varint & 1) ^ (long)(varint >> 1));

                        break;

                    case ProtoType.UInt32:
                    case ProtoType.UInt64:
                        state.Value = new ProtobufValue(varint);

                        break;

                    case ProtoType.Undefined:
                        break;

                    default:
                        state.RaiseError("field {0} is incompatible with wire type {1}", field.Name, wire);

                        return ReaderStatus.Failed;
                }

                break;

            default:
                state.RaiseError("field {0} has unsupported wire type {1}", field.Name, wire);

                return ReaderStatus.Failed;
        }

        /*
                    return this.fields[index] != null
                        ? this.fields[index](state, ref entity)
                        : Reader<TObject>.Ignore(state);
        */
        return ReaderStatus.Failed;
    }

    public ReaderStatus ReadToValue(ReaderState state, out ProtobufValue value)
    {
        throw new NotImplementedException();
    }

    public ReaderState Start(Stream stream, ErrorEvent error)
    {
        return new ReaderState(stream, error);
    }

    public void Stop(ReaderState state)
    {
    }
}