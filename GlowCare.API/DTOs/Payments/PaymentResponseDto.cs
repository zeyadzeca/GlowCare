using GlowCare.API.Enums;

namespace GlowCare.API.DTOs.Payments;

/// <summary>One payment record of an order. Never contains card details.</summary>
public class PaymentResponseDto
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? TransactionId { get; set; }
}
