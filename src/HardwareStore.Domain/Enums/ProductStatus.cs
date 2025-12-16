namespace HardwareStore.Domain.Enums
{
    public enum ProductStatus
    {
        Active = 0,      // Producto activo y visible
        Inactive = 1,    // Producto pausado por el admin
        Deleted = 2      // Producto eliminado (soft delete)
    }
}