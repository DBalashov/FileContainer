using JetBrains.Annotations;

namespace FileContainer
{
    interface IDataHandler
    {
        [NotNull]
        byte[] Pack([NotNull] byte[] data);

        [NotNull]
        byte[] Unpack([NotNull] byte[] data);
    }
}