using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlqStudio.Application.SQL.ResponseModels
{
    public class SqlValidateResponse
    {
        public bool IsValid { get; set; }
        public string? Error { get; set; }
    }
}
