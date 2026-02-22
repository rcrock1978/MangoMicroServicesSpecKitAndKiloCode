namespace Mango.Common.Enums;

/// <summary>
/// Application-wide roles
/// </summary>
public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Customer = "Customer";
    public const string Guest = "Guest";

    public static readonly string[] All = { SuperAdmin, Admin, Manager, Customer, Guest };
}

/// <summary>
/// Order status enumeration
/// </summary>
public enum OrderStatus
{
    Pending = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    Refunded = 6
}

/// <summary>
/// Payment status enumeration
/// </summary>
public enum PaymentStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Refunded = 5
}

/// <summary>
/// Payment method enumeration
/// </summary>
public enum PaymentMethod
{
    CreditCard = 1,
    DebitCard = 2,
    PayPal = 3,
    BankTransfer = 4,
    CashOnDelivery = 5
}

/// <summary>
/// Product availability status
/// </summary>
public enum ProductAvailability
{
    InStock = 1,
    OutOfStock = 2,
    PreOrder = 3,
    Discontinued = 4
}

/// <summary>
/// Coupon type enumeration
/// </summary>
public enum CouponType
{
    Percentage = 1,
    FixedAmount = 2,
    FreeShipping = 3
}

/// <summary>
/// Reward point transaction type
/// </summary>
public enum RewardTransactionType
{
    Earned = 1,
    Redeemed = 2,
    Expired = 3,
    Adjusted = 4
}

/// <summary>
/// Email template type
/// </summary>
public enum EmailTemplateType
{
    Welcome = 1,
    OrderConfirmation = 2,
    OrderShipped = 3,
    OrderDelivered = 4,
    OrderCancelled = 5,
    PasswordReset = 6,
    EmailVerification = 7,
    RewardEarned = 8,
    RewardExpired = 9
}

/// <summary>
/// Email priority
/// </summary>
public enum EmailPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}
