namespace DBC.CareCommerce.Contracts.Enums
{
    public enum BillingAction
    {
        NoBilling = 0,
        CreatePendingCharge = 1,
        CreatePostingImmediately = 2,
        InformationalOnly = 3,
        ExternalPayment = 4
    }
}