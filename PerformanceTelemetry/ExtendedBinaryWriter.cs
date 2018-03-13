using System;
using System.Text;
using System.IO;

namespace PerformanceTelemetry
{
    //
    // Summary:
    //     Writes primitive types in binary to a stream and supports writing strings in
    //     a specific encoding.
    public class ExtendedBinaryWriter : IDisposable
    {
        private readonly BinaryWriter _writer;

        //
        // Summary:
        //     Initializes a new instance of the System.IO.ExtendedBinaryWriter class based on the specified
        //     stream and using UTF-8 encoding.
        //
        // Parameters:
        //   output:
        //     The output stream.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     The stream does not support writing or is already closed.
        //
        //   T:System.ArgumentNullException:
        //     output is null.
        public ExtendedBinaryWriter(Stream output)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            _writer = new BinaryWriter(output);
        }

        //
        // Summary:
        //     Initializes a new instance of the System.IO.ExtendedBinaryWriter class based on the specified
        //     stream and character encoding.
        //
        // Parameters:
        //   output:
        //     The output stream.
        //
        //   encoding:
        //     The character encoding to use.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     The stream does not support writing or is already closed.
        //
        //   T:System.ArgumentNullException:
        //     output or encoding is null.
        public ExtendedBinaryWriter(Stream output, Encoding encoding)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            _writer = new BinaryWriter(output, encoding);
        }

        //
        // Summary:
        //     Initializes a new instance of the System.IO.ExtendedBinaryWriter class based on the specified
        //     stream and character encoding, and optionally leaves the stream open.
        //
        // Parameters:
        //   output:
        //     The output stream.
        //
        //   encoding:
        //     The character encoding to use.
        //
        //   leaveOpen:
        //     true to leave the stream open after the System.IO.ExtendedBinaryWriter object is disposed;
        //     otherwise, false.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     The stream does not support writing or is already closed.
        //
        //   T:System.ArgumentNullException:
        //     output or encoding is null.
        public ExtendedBinaryWriter(Stream output, Encoding encoding, bool leaveOpen)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            _writer = new BinaryWriter(output, encoding, leaveOpen);
        }

        //
        // Summary:
        //     Gets the underlying stream of the System.IO.ExtendedBinaryWriter.
        //
        // Returns:
        //     The underlying stream associated with the BinaryWriter.
        public virtual Stream BaseStream
        {
            get
            {
                return _writer.BaseStream;
            }
        }
        

        //
        // Summary:
        //     Closes the current System.IO.ExtendedBinaryWriter and the underlying stream.
        public virtual void Close()
        {
            _writer.Close();
        }

        //
        // Summary:
        //     Releases all resources used by the current instance of the System.IO.ExtendedBinaryWriter
        //     class.
        public void Dispose()
        {
            _writer.Dispose();
        }

        //
        // Summary:
        //     Clears all buffers for the current writer and causes any buffered data to be
        //     written to the underlying device.
        public virtual void Flush()
        {
            _writer.Flush();
        }

        //
        // Summary:
        //     Sets the position within the current stream.
        //
        // Parameters:
        //   offset:
        //     A byte offset relative to origin.
        //
        //   origin:
        //     A field of System.IO.SeekOrigin indicating the reference point from which the
        //     new position is to be obtained.
        //
        // Returns:
        //     The position with the current stream.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     The file pointer was moved to an invalid location.
        //
        //   T:System.ArgumentException:
        //     The System.IO.SeekOrigin value is invalid.
        public virtual long Seek(int offset, SeekOrigin origin)
        {
            return
                _writer.Seek(offset, origin);
        }

        //
        // Summary:
        //     Writes a section of a character array to the current stream, and advances the
        //     current position of the stream in accordance with the Encoding used and perhaps
        //     the specific characters being written to the stream.
        //
        // Parameters:
        //   chars:
        //     A character array containing the data to write.
        //
        //   index:
        //     The starting point in chars from which to begin writing.
        //
        //   count:
        //     The number of characters to write.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     The buffer length minus index is less than count.
        //
        //   T:System.ArgumentNullException:
        //     chars is null.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     index or count is negative.
        //
        //   T:System.IO.IOException:
        //     An I/O error occurs.
        //
        //   T:System.ObjectDisposedException:
        //     The stream is closed.
        public virtual long Write(char[] chars, int index, int count)
        {
            var before = _writer.BaseStream.Position;

            _writer.Write(chars, index, count);

            var after = _writer.BaseStream.Position;

            return
                after - before;
        }
        //
        // Summary:
        //     Writes a length-prefixed string to this stream in the current encoding of the
        //     System.IO.ExtendedBinaryWriter, and advances the current position of the stream in accordance
        //     with the encoding used and the specific characters being written to the stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurs.
        //
        //   T:System.ArgumentNullException:
        //     value is null.
        //
        //   T:System.ObjectDisposedException:
        //     The stream is closed.
        public virtual long Write(string value)
        {
            var before = _writer.BaseStream.Position;

            _writer.Write(value);

            var after = _writer.BaseStream.Position;

            return
                after - before;
        }

        //
        // Summary:
        //     Writes a four-byte floating-point value to the current stream and advances the
        //     stream position by four bytes.
        //
        // Parameters:
        //   value:
        //     The four-byte floating-point value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurs.
        //
        //   T:System.ObjectDisposedException:
        //     The stream is closed.
        public virtual long Write(float value)
        {
            var before = _writer.BaseStream.Position;

            _writer.Write(value);

            var after = _writer.BaseStream.Position;

            return
                after - before;
        }

        //
        // Summary:
        //     Writes an eight-byte unsigned integer to the current stream and advances the
        //     stream position by eight bytes.
        //
        // Parameters:
        //   value:
        //     The eight-byte unsigned integer to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurs.
        //
        //   T:System.ObjectDisposedException:
        //     The stream is closed.
        [CLSCompliant(false)]
        public virtual long Write(ulong value)
        {
            var before = _writer.BaseStream.Position;

            _writer.Write(value);

            var after = _writer.BaseStream.Position;

            return
                after - before;
        }

        //
        // Summary:
        //     Writes an eight-byte signed integer to the current stream and advances the stream
        //     position by eight bytes.
        //
        // Parameters:
        //   value:
        //     The eight-byte signed integer to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurs.
        //
        //   T:System.ObjectDisposedException:
        //     The stream is closed.
        public virtual long Write(long value)
        {
            var before = _writer.BaseStream.Position;

            _writer.Write(value);

            var after = _writer.BaseStream.Position;

            return
                after - before;
        }

        //
        // Summary:
        //     Writes a four-byte unsigned integer to the current stream and advances the stream
        //     position by four bytes.
        //
        // Parameters:
        //   value:
        //     The four-byte unsigned integer to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurs.
        //
        //   T:System.ObjectDisposedException:
        //     The stream is closed.
        [CLSCompliant(false)]
        public virtual long Write(uint value)
        {
            var before = _writer.BaseStream.Position;

            _writer.Write(value);

            var after = _writer.BaseStream.Position;

            return
                after - before;
        }

        //
        // Summary:
        //     Writes a four-byte signed integer to the current stream and advances the stream
        //     position by four bytes.
        //
        // Parameters:
        //   value:
        //     The four-byte signed integer to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurs.
        //
        //   T:System.ObjectDisposedException:
        //     The stream is closed.
        public virtual long Write(int value)
        {
            var before = _writer.BaseStream.Position;

            _writer.Write(value);

            var after = _writer.BaseStream.Position;

            return
                after - before;
        }

        //
        // Summary:
        //     Writes a two-byte unsigned integer to the current stream and advances the stream
        //     position by two bytes.
        //
        // Parameters:
        //   value:
        //     The two-byte unsigned integer to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurs.
        //
        //   T:System.ObjectDisposedException:
        //     The stream is closed.
        [CLSCompliant(false)]
        public virtual long Write(ushort value)
        {
            var before = _writer.BaseStream.Position;

            _writer.Write(value);

            var after = _writer.BaseStream.Position;

            return
                after - before;
        }

        //
        // Summary:
        //     Writes a two-byte signed integer to the current stream and advances the stream
        //     position by two bytes.
        //
        // Parameters:
        //   value:
        //     The two-byte signed integer to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurs.
        //
        //   T:System.ObjectDisposedException:
        //     The stream is closed.
        public virtual long Write(short value)
        {
            var before = _writer.BaseStream.Position;

            _writer.Write(value);

            var after = _writer.BaseStream.Position;

            return
                after - before;
        }

        //
        // Summary:
        //     Writes a region of a byte array to the current stream.
        //
        // Parameters:
        //   buffer:
        //     A byte array containing the data to write.
        //
        //   index:
        //     The starting point in buffer at which to begin writing.
        //
        //   count:
        //     The number of bytes to write.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     The buffer length minus index is less than count.
        //
        //   T:System.ArgumentNullException:
        //     buffer is null.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     index or count is negative.
        //
        //   T:System.IO.IOException:
        //     An I/O error occurs.
        //
        //   T:System.ObjectDisposedException:
        //     The stream is closed.
        public virtual long Write(byte[] buffer, int index, int count)
        {
            var before = _writer.BaseStream.Position;

            _writer.Write(buffer, index, count);

            var after = _writer.BaseStream.Position;

            return
                after - before;
        }

        //
        // Summary:
        //     Writes an eight-byte floating-point value to the current stream and advances
        //     the stream position by eight bytes.
        //
        // Parameters:
        //   value:
        //     The eight-byte floating-point value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurs.
        //
        //   T:System.ObjectDisposedException:
        //     The stream is closed.
        public virtual long Write(double value)
        {
            var before = _writer.BaseStream.Position;

            _writer.Write(value);

            var after = _writer.BaseStream.Position;

            return
                after - before;
        }

        //
        // Summary:
        //     Writes a character array to the current stream and advances the current position
        //     of the stream in accordance with the Encoding used and the specific characters
        //     being written to the stream.
        //
        // Parameters:
        //   chars:
        //     A character array containing the data to write.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     chars is null.
        //
        //   T:System.ObjectDisposedException:
        //     The stream is closed.
        //
        //   T:System.IO.IOException:
        //     An I/O error occurs.
        public virtual long Write(char[] chars)
        {
            var before = _writer.BaseStream.Position;

            _writer.Write(chars);

            var after = _writer.BaseStream.Position;

            return
                after - before;
        }

        //
        // Summary:
        //     Writes a Unicode character to the current stream and advances the current position
        //     of the stream in accordance with the Encoding used and the specific characters
        //     being written to the stream.
        //
        // Parameters:
        //   ch:
        //     The non-surrogate, Unicode character to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurs.
        //
        //   T:System.ObjectDisposedException:
        //     The stream is closed.
        //
        //   T:System.ArgumentException:
        //     ch is a single surrogate character.
        public virtual long Write(char ch)
        {
            var before = _writer.BaseStream.Position;

            _writer.Write(ch);

            var after = _writer.BaseStream.Position;

            return
                after - before;
        }
        //
        // Summary:
        //     Writes a byte array to the underlying stream.
        //
        // Parameters:
        //   buffer:
        //     A byte array containing the data to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurs.
        //
        //   T:System.ObjectDisposedException:
        //     The stream is closed.
        //
        //   T:System.ArgumentNullException:
        //     buffer is null.
        public virtual long Write(byte[] buffer)
        {
            var before = _writer.BaseStream.Position;

            _writer.Write(buffer);

            var after = _writer.BaseStream.Position;

            return
                after - before;
        }

        //
        // Summary:
        //     Writes a signed byte to the current stream and advances the stream position by
        //     one byte.
        //
        // Parameters:
        //   value:
        //     The signed byte to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurs.
        //
        //   T:System.ObjectDisposedException:
        //     The stream is closed.
        [CLSCompliant(false)]
        public virtual long Write(sbyte value)
        {
            var before = _writer.BaseStream.Position;

            _writer.Write(value);

            var after = _writer.BaseStream.Position;

            return
                after - before;
        }

        //
        // Summary:
        //     Writes an unsigned byte to the current stream and advances the stream position
        //     by one byte.
        //
        // Parameters:
        //   value:
        //     The unsigned byte to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurs.
        //
        //   T:System.ObjectDisposedException:
        //     The stream is closed.
        public virtual long Write(byte value)
        {
            var before = _writer.BaseStream.Position;

            _writer.Write(value);

            var after = _writer.BaseStream.Position;

            return
                after - before;
        }

        //
        // Summary:
        //     Writes a one-byte Boolean value to the current stream, with 0 representing false
        //     and 1 representing true.
        //
        // Parameters:
        //   value:
        //     The Boolean value to write (0 or 1).
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurs.
        //
        //   T:System.ObjectDisposedException:
        //     The stream is closed.
        public virtual long Write(bool value)
        {
            var before = _writer.BaseStream.Position;

            _writer.Write(value);

            var after = _writer.BaseStream.Position;

            return
                after - before;
        }

        //
        // Summary:
        //     Writes a decimal value to the current stream and advances the stream position
        //     by sixteen bytes.
        //
        // Parameters:
        //   value:
        //     The decimal value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurs.
        //
        //   T:System.ObjectDisposedException:
        //     The stream is closed.
        public virtual long Write(decimal value)
        {
            var before = _writer.BaseStream.Position;

            _writer.Write(value);

            var after = _writer.BaseStream.Position;

            return
                after - before;
        }
    }

}
