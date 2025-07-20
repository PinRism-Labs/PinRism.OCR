using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinRism.Lib.Models.DTOs
{
    public class OcrResultDto
    {
        public bool Success { get; set; }
        public Data Data { get; set; } = new();
        public Meta Meta { get; set; } = new();
        public string? Error { get; set; }
    }

    public class Data
    {
        public string? Name { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public string? RawText {  get; set; }

    }

    public class Meta
    {
        public string? FileName {  get; set; }
        public string ? MimeType { get; set; }
        public long Length { get; set; }
    }
}
