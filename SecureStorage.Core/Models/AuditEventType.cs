using System;
using System.Collections.Generic;
using System.Text;

namespace SecureStorage.Core.Models
{
    public enum AuditEventType
    {
        Upload,
        Download,
        Delete,
        Promote,
        Other
    }
}
