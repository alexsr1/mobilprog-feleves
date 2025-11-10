using SQLite;

namespace Mobilprog.Models
{
    public class Product
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int ReceiptId { get; set; } // Foreign key to Receipt

        public string? OriginalName { get; set; }
        public string? ReadableName { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; } // Use decimal for quantity to handle non-integer amounts
        public string? Unit { get; set; } // e.g., "kg", "db", "liter"
    }
}
