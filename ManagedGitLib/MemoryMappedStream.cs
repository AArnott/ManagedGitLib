﻿using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace ManagedGitLib
{
    /// <summary>
    /// Provides read-only, seekable access to a <see cref="MemoryMappedFile"/>.
    /// </summary>
    public unsafe class MemoryMappedStream : Stream
    {
        private readonly MemoryMappedViewAccessor accessor;
        private readonly long length;
        private long position;
        private byte* ptr;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryMappedStream"/> class.
        /// </summary>
        /// <param name="accessor">
        /// The accessor to the memory mapped stream.
        /// </param>
        public MemoryMappedStream(MemoryMappedViewAccessor accessor)
        {
            this.accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            this.accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref this.ptr);
            this.length = this.accessor.Capacity;
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => this.length;

        /// <inheritdoc/>
        public override long Position
        {
            get => this.position;
            set
            {
                this.position = (int)value;
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(MemoryMappedStream));
            }

            int read = (int)Math.Min(count, this.length - this.position);

            new Span<byte>(this.ptr + this.position, read)
                .CopyTo(buffer.AsSpan(offset, count));

            this.position += read;
            return read;
        }

#if !NETSTANDARD2_0
        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(MemoryMappedStream));
            }

            int read = (int)Math.Min(buffer.Length, this.length - this.position);
            
            new Span<byte>(this.ptr + this.position, read)
                .CopyTo(buffer);

            this.position += read;
            return read;
        }
#endif

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(MemoryMappedStream));
            }

            long newPosition = this.position;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    newPosition = offset;
                    break;

                case SeekOrigin.Current:
                    newPosition += offset;
                    break;

                case SeekOrigin.End:
                    throw new NotSupportedException();
            }

            if (newPosition > this.length)
            {
                newPosition = this.length;
            }

            if (newPosition < 0)
            {
                throw new IOException("Attempted to seek before the start or beyond the end of the stream.");
            }

            this.position = newPosition;
            return this.position;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.accessor.SafeMemoryMappedViewHandle.ReleasePointer();
                this.disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
