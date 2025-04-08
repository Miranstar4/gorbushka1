using System;
using System.Collections.Generic;

namespace backend6.Models
{
    public partial class ErrorLog
    {
        public Guid Id { get; set; }
        public string? FuncName { get; set; }
        public string? InnerException { get; set; }
        public string? ErrorMessage { get; set; }
        public string? StackTrace { get; set; }
        public string? Createtime { get; set; }
    }
}
