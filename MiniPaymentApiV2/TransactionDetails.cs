using System.Text.Json.Serialization;

public class TransactionDetails
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }  // Foreign Key
    public string TransactionType { get; set; }
    public string Status { get; set; }
    public decimal Amount { get; set; }

    [JsonIgnore]  // JSON serileştirmesini engeller
    public Transaction Transaction { get; set; }  // Navigation Property
}
