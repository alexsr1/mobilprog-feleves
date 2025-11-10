using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using Mobilprog.Models;

namespace Mobilprog.Services
{
    public class PdfParsingService
    {
        public async Task<(Receipt Receipt, List<Product> Products)> ParseReceiptAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"PDF file not found at {filePath}");
            }

            string allText = await Task.Run(() =>
            {
                string text = string.Empty;
                using (var document = PdfDocument.Open(filePath))
                {
                    foreach (var page in document.GetPages())
                    {
                        text += ContentOrderTextExtractor.GetText(page);
                    }
                }
                return text;
            });

            // Placeholder for receipt data extraction
            // You will need to adjust these regex patterns based on the actual Spar receipt format.
            var receipt = new Receipt
            {
                Date = DateTime.Now, // Default to now, try to extract from PDF
                StoreName = "Unknown Spar Store", // Default, try to extract from PDF
                TotalPrice = 0, // Default, try to extract from PDF
                PdfPath = filePath
            };

            // Example regex for date (e.g., "2025.10.26.")
            var dateMatch = Regex.Match(allText, @"\d{4}\.\d{2}\.\d{2}\.");
            if (dateMatch.Success && DateTime.TryParse(dateMatch.Value.Replace(".", "-").TrimEnd('-'), out DateTime parsedDate))
            {
                receipt.Date = parsedDate;
            }

            // Example regex for store name (e.g., "SPAR Budapest, Kossuth Lajos u. 1.")
            var storeMatch = Regex.Match(allText, @"SPAR\s+[^,\n]+(?:,\s*[^,\n]+)*");
            if (storeMatch.Success)
            {
                receipt.StoreName = storeMatch.Value.Trim();
            }

            // Example regex for total price (e.g., "ÖSSZESEN: 1234.56 Ft")
            var totalMatch = Regex.Match(allText, @"ÖSSZESEN:\s*(\d{1,3}(?:\.\d{3})*(?:,\d{2})?)\s*Ft");
            if (totalMatch.Success && decimal.TryParse(totalMatch.Groups[1].Value.Replace(".", "").Replace(",", "."), out decimal totalPrice))
            {
                receipt.TotalPrice = totalPrice;
            }


            var products = new List<Product>();

            // Placeholder for product extraction
            // This is a very basic example. Real-world receipts will require more robust parsing.
            // Look for lines that might represent products, e.g., "S-BUDGET CSIRKE INS.T.90 1 db 1234 Ft"
            var productLineRegex = new Regex(@"^(?<name>.+?)\s+(?<quantity>\d+(?:,\d+)?)\s*(?<unit>kg|db|liter)?\s+(?<price>\d{1,3}(?:\.\d{3})*(?:,\d{2})?)\s*Ft$", RegexOptions.Multiline);

            foreach (Match match in productLineRegex.Matches(allText))
            {
                if (decimal.TryParse(match.Groups["price"].Value.Replace(".", "").Replace(",", "."), out decimal price) &&
                    decimal.TryParse(match.Groups["quantity"].Value.Replace(",", "."), out decimal quantity))
                {
                    products.Add(new Product
                    {
                        OriginalName = match.Groups["name"].Value.Trim(),
                        Price = price,
                        Quantity = quantity,
                        Unit = match.Groups["unit"].Success ? match.Groups["unit"].Value.Trim() : "db" // Default unit if not specified
                    });
                }
            }

            return (receipt, products);
        }
    }
}
