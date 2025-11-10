using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mobilprog.Services
{
    public class LlmService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey; // You will need to provide your Google Gemini API key

        public LlmService(string apiKey)
        {
            _httpClient = new HttpClient();
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey), "API key cannot be null.");
        }

        public async Task<List<string>> GetReadableProductNamesAsync(List<string> originalNames)
        {
            if (originalNames == null || originalNames.Count == 0)
            {
                return new List<string>();
            }

            var readableNames = new List<string>();
            var prompt = $"Make the following product names more readable and descriptive. Provide only the improved names, one per line, in the same order as the input. Do not add any other text or numbering.\n\n{string.Join("\n", originalNames)}";

            try
            {
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonDocument>(responseString);

                // Parse the response to extract the readable names
                // The exact structure might vary slightly based on Gemini API response.
                // This assumes the response contains a "candidates" array, each with "content" and "parts"
                // and the readable names are in the "text" property of the first part.
                if (responseObject != null && responseObject.RootElement.TryGetProperty("candidates", out JsonElement candidates) && candidates.ValueKind == JsonValueKind.Array)
                {
                    foreach (var candidate in candidates.EnumerateArray())
                    {
                        if (candidate.TryGetProperty("content", out JsonElement contentElement) &&
                            contentElement.TryGetProperty("parts", out JsonElement parts) && parts.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var part in parts.EnumerateArray())
                            {
                                if (part.TryGetProperty("text", out JsonElement textElement))
                                {
                                    var rawText = textElement.GetString();
                                    if (!string.IsNullOrWhiteSpace(rawText))
                                    {
                                        // Split by new line and trim each name
                                        foreach (var name in rawText!.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                                        {
                                            readableNames.Add(name?.Trim() ?? string.Empty); // Use null-conditional and null-coalescing
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP Request Error: {httpEx.Message}");
                // Log or handle the error appropriately
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON Deserialization Error: {jsonEx.Message}");
                // Log or handle the error appropriately
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                // Log or handle other exceptions
            }

            // If LLM fails, return original names as a fallback
            if (readableNames.Count != originalNames.Count)
            {
                return originalNames;
            }

            return readableNames;
        }
    }
}
