namespace DBC.CareCommerce.Contracts.Enums
{
    public enum InventoryAction
    {
        None = 0,
        Reserve = 1,
        DecrementOnPost = 2,
        DecrementImmediately = 3,
        FullscriptOnly = 4
    }
}