using System;
using System.Collections.Generic;
using System.Text;

namespace SecureStorage.Core.Models
{
    /// <summary>
    /// Projection model used by repository to return a file record together with its owner.
    /// </summary>
    public class FileRecordWithOwner
    {
        public FileRecord FileRecord { get; set; } = null!;
        public User Owner { get; set; } = null!;
    }
}
