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
                Date = DateTime.Now,
                StoreName = "Unknown Store",
                TotalPrice = 0,
                PdfPath = filePath,
                PaymentMethod = "Unknown",
                ReceiptNumber = "N/A"
            };

            // Extract Store Name
            var storeNameMatch = Regex.Match(allText, @"SPAR MARKET [^\n]+");
            if (storeNameMatch.Success)
            {
                receipt.StoreName = storeNameMatch.Value.Trim();
            }

            // Extract Date and Time
            var dateTimeMatch = Regex.Match(allText, @"(\d{4}\.\d{2}\.\d{2}\.)\s*(\d{2}:\d{2})");
            if (dateTimeMatch.Success)
            {
                if (DateTime.TryParse(dateTimeMatch.Groups[1].Value.Replace(".", "-").TrimEnd('-'), out DateTime parsedDate))
                {
                    receipt.Date = parsedDate;
                }
                // Optionally, you can also store the time separately if needed
                // string time = dateTimeMatch.Groups[2].Value;
            }
            else
            {
                // Fallback for date if not found with time
                var dateMatch = Regex.Match(allText, @"\d{4}\.\d{2}\.\d{2}\.");
                if (dateMatch.Success && DateTime.TryParse(dateMatch.Value.Replace(".", "-").TrimEnd('-'), out DateTime parsedDate))
                {
                    receipt.Date = parsedDate;
                }
            }


            // Extract Receipt Number
            var receiptNumberMatch = Regex.Match(allText, @"NYUGTASZÁM:(\d{4}/\d{5})");
            if (receiptNumberMatch.Success)
            {
                receipt.ReceiptNumber = receiptNumberMatch.Groups[1].Value;
            }

            // Extract Total Price
            var totalMatch = Regex.Match(allText, @"Összeg:\s*([\d\s]+)\s*FT");
            if (totalMatch.Success && decimal.TryParse(totalMatch.Groups[1].Value.Replace(" ", "").Replace(",", "."), out decimal totalPrice))
            {
                receipt.TotalPrice = totalPrice;
            }

            // Extract Payment Method
            var paymentMethodMatch = Regex.Match(allText, @"(Bankkártya|Készpénz)\s*[\d\s]+FT");
            if (paymentMethodMatch.Success)
            {
                receipt.PaymentMethod = paymentMethodMatch.Groups[1].Value;
            }

            var products = new List<Product>();

            // Extract products
            // Products are between "---- NEM ADÓÜGYI BIZONYLAT ---" and "Összeg:"
            var productSectionMatch = Regex.Match(allText, @"---- NEM ADÓÜGYI BIZONYLAT ---(.*?)(?=Összeg:)", RegexOptions.Singleline);
            if (productSectionMatch.Success)
            {
                string productSection = productSectionMatch.Groups[1].Value;
                var productLineRegex = new Regex(@"^[A-Z]\d{2}\s(.+?)\s(\d+)$", RegexOptions.Multiline);

                foreach (Match match in productLineRegex.Matches(productSection))
                {
                    if (decimal.TryParse(match.Groups[2].Value.Replace(" ", "").Replace(",", "."), out decimal price))
                    {
                        products.Add(new Product
                        {
                            OriginalName = match.Groups[1].Value.Trim(),
                            Price = price,
                            Quantity = 1, // Assuming quantity is 1 if not explicitly stated
                            Unit = "db" // Default unit to 'db' (piece)
                        });
                    }
                }
            }

            return (receipt, products);
        }
    }
}
