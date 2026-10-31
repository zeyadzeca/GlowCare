namespace GlowCare.API.Enums;

/// <summary>
/// Per-seller fulfillment state of one order item (approved schema addition).
/// In a multi-seller marketplace one seller can ship while another has not,
/// so fulfillment is tracked per item, not per order.
/// </summary>
public enum FulfillmentStatus
{
    NotFulfilled = 0,
    Fulfilled = 1
}
