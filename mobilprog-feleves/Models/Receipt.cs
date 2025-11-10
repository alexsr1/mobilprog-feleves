using SQLite;
using System;

namespace Mobilprog.Models
{
    public class Receipt
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string? StoreName { get; set; }
        public decimal TotalPrice { get; set; }
        public string? PdfPath { get; set; }
    }
}
