namespace Promotions.Domain.Models;

/// <summary>
/// The kind of discount a promotion applies. Drives how the Pricing Engine
/// evaluates the rule at checkout.
/// </summary>
public enum PromotionType
{
    PercentageDiscount = 0,
    FlatAmountOff = 1,
    BuyOneGetOne = 2,
    FreeShipping = 3,
    LoyaltyPoints = 4
}
