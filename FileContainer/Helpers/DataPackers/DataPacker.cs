using System;

namespace FileContainer
{
    interface IDataHandler
    {
        Span<byte> Pack(Span<byte>   data);
        Span<byte> Unpack(Span<byte> data);
    }
}