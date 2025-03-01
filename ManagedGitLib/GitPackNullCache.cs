﻿#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace ManagedGitLib
{
    /// <summary>
    /// A no-op implementation of the <see cref="GitPackCache"/> class.
    /// </summary>
    public class GitPackNullCache : GitPackCache
    {
        /// <summary>
        /// Gets the default instance of the <see cref="GitPackCache"/> class.
        /// </summary>
        public static GitPackNullCache Instance { get; } = new GitPackNullCache();

        /// <inheritdoc/>
        public override Stream Add(long offset, Stream stream)
        {
            return stream;
        }

        /// <inheritdoc/>
        public override bool TryOpen(long offset, [NotNullWhen(true)] out Stream? stream)
        {
            stream = null;
            return false;
        }

        /// <inheritdoc/>
        public override void GetCacheStatistics(StringBuilder builder)
        {
        }
    }
}
