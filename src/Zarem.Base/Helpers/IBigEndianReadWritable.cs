// Avishai Dernis 2024

using System.IO;

namespace Zarem.Helpers;

/// <summary>
/// An interface for structs that can be read/written to a stream in big endian.
/// </summary>
public interface IBigEndianReadWritable<T>
{
    /// <summary>
    /// Read the object from a stream in big endian.
    /// </summary>
    /// <param name="stream"></param>
    public T Read(Stream stream);

    /// <summary>
    /// Write the object from a stream in big endian.
    /// </summary>
    /// <param name="stream"></param>
    public void Write(Stream stream);
}
