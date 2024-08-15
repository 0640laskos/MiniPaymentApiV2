using Microsoft.AspNetCore.Mvc;
using MiniPaymentApi.Services;
using System;
using System.Threading.Tasks;


    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
    private readonly TransactionService _transactionService;

    public TransactionController(TransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost("pay")]
    public async Task<IActionResult> Pay([FromBody] Transaction transaction)
    {
        if (transaction == null || transaction.TransactionDetails == null || !transaction.TransactionDetails.Any())
        {
            return BadRequest("Invalid transaction data.");
        }

        try
        {
            var createdTransaction = await _transactionService.Pay(transaction);
            return CreatedAtAction(nameof(GetTransactionById), new { id = createdTransaction.Id }, createdTransaction);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during transaction creation: {ex.Message}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionById(Guid id)
    {
        var transaction = await _transactionService.GetTransactionById(id);
        if (transaction == null)
        {
            return NotFound();
        }

        return Ok(transaction);
    }


    [HttpPost("cancel/{id}")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                var canceledTransaction = await _transactionService.Cancel(id);
                return Ok(canceledTransaction);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("refund/{id}")]
        public async Task<IActionResult> Refund(Guid id)
        {
            try
            {
                var refundedTransaction = await _transactionService.Refund(id);
                return Ok(refundedTransaction);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("report")]
        public async Task<IActionResult> Report(int? bankId, string status, string orderReference, DateTime? startDate, DateTime? endDate)
        {
            var transactions = await _transactionService.Report(bankId, status, orderReference, startDate, endDate);
            return Ok(transactions);
        }

       
    }

