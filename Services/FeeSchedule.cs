namespace TicketResell.Services;

public static class FeeSchedule
{
    public const decimal BuyerFeeRate = 0.10m;
    public const decimal SellerCommissionRate = 0.12m;

    public static decimal BuyerFee(decimal subtotal)
    {
        return Round(subtotal * BuyerFeeRate);
    }

    public static decimal SellerCommission(decimal subtotal)
    {
        return Round(subtotal * SellerCommissionRate);
    }

    private static decimal Round(decimal amount)
    {
        return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    }
}
