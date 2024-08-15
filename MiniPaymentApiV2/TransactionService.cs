
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MiniPaymentApiV2;

namespace MiniPaymentApi.Services
{
    public class TransactionService
    {
        private readonly PaymentDbContext _context;

        public TransactionService(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction> Pay(Transaction transaction)
        {
            // Transaction ve TransactionDetails için yeni ID'ler oluşturun
            transaction.Id = Guid.NewGuid();
            transaction.TransactionDate = DateTime.UtcNow;
            transaction.Status = "Success"; // Durumu başarılı olarak ayarlıyoruz

            foreach (var detail in transaction.TransactionDetails)
            {
                detail.Id = Guid.NewGuid();
                detail.TransactionId = transaction.Id;
                detail.Status = "Success"; // Her bir detay için durumu başarılı olarak ayarlıyoruz
            }

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return transaction;
        }



        // Cancel method: Cancels a transaction if within the same day
        public async Task<Transaction> Cancel(Guid transactionId)
        {
            var transaction = await _context.Transactions
                .Include(t => t.TransactionDetails)
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null || transaction.TransactionDate.Date != DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("Transaction cannot be canceled.");
            }

            var cancelDetail = new TransactionDetails
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                TransactionType = "Cancel",
                Status = "Success", // or Fail depending on your logic
                Amount = 0 // Or set it to the correct cancel amount
            };

            transaction.TransactionDetails.Add(cancelDetail);
            transaction.NetAmount -= cancelDetail.Amount;

            await _context.SaveChangesAsync();

            return transaction;
        }

        // Refund method: Refunds a transaction if at least one day has passed
        public async Task<Transaction> Refund(Guid transactionId)
        {
            var transaction = await _context.Transactions
                .Include(t => t.TransactionDetails)
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null || transaction.TransactionDate.Date >= DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("Transaction cannot be refunded.");
            }

            var refundDetail = new TransactionDetails
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                TransactionType = "Refund",
                Status = "Success", // or Fail depending on your logic
                Amount = transaction.NetAmount // Or set the correct refund amount
            };

            transaction.TransactionDetails.Add(refundDetail);
            transaction.NetAmount -= refundDetail.Amount;

            await _context.SaveChangesAsync();

            return transaction;
        }

        // Report method: Filters transactions based on various criteria
        public async Task<List<Transaction>> Report(int? bankId, string status, string orderReference, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Transactions.AsQueryable();

            if (bankId.HasValue)
            {
                query = query.Where(t => t.BankId == bankId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }

            if (!string.IsNullOrEmpty(orderReference))
            {
                query = query.Where(t => t.OrderReference == orderReference);
            }

            if (startDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= endDate.Value);
            }

            return await query.Include(t => t.TransactionDetails).ToListAsync();
        }

        // GetTransactionById method: Gets a transaction by its ID
        public async Task<Transaction> GetTransactionById(Guid id)
        {
            return await _context.Transactions
                .Include(t => t.TransactionDetails)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

    }
}
