using System;

namespace FileContainer;

interface IDataPacker
{
    Span<byte> Pack(Span<byte>   data);
    Span<byte> Unpack(Span<byte> data);
}