using Mobilprog.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Threading.Tasks;

namespace Mobilprog.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;

        public DatabaseService()
        {
        }

        private async Task Init()
        {
            if (_database != null)
                return;

            _database = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "Receipts.db3"), SQLite.SQLiteOpenFlags.ReadWrite | SQLite.SQLiteOpenFlags.Create | SQLite.SQLiteOpenFlags.SharedCache);
            await _database.CreateTableAsync<Receipt>();
            await _database.CreateTableAsync<Product>();
        }

        public async Task<List<Receipt>> GetReceiptsAsync()
        {
            await Init();
            return await _database!.Table<Receipt>().OrderByDescending(r => r.Date).ToListAsync();
        }

        public async Task<Receipt> GetReceiptAsync(int id)
        {
            await Init();
            return await _database!.Table<Receipt>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Product>> GetProductsForReceiptAsync(int receiptId)
        {
            await Init();
            return await _database!.Table<Product>().Where(p => p.ReceiptId == receiptId).ToListAsync();
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            await Init();
            return await _database!.Table<Product>().ToListAsync();
        }

        public async Task SaveReceiptAsync(Receipt receipt, List<Product> products)
        {
            await Init();
            if (receipt.Id != 0)
            {
                await _database!.UpdateAsync(receipt);
            }
            else
            {
                await _database!.InsertAsync(receipt);
            }

            // Clear existing products for this receipt if it's an update
            if (receipt.Id != 0)
            {
                await _database!.Table<Product>().Where(p => p.ReceiptId == receipt.Id).DeleteAsync();
            }

            foreach (var product in products)
            {
                product.ReceiptId = receipt.Id;
                await _database!.InsertAsync(product);
            }
        }

        public async Task DeleteReceiptAsync(Receipt receipt)
        {
            await Init();
            await _database!.DeleteAsync(receipt);
            await _database!.Table<Product>().Where(p => p.ReceiptId == receipt.Id).DeleteAsync();
        }
    }
}
