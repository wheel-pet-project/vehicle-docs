namespace Domain.VehicleDocumentsAggregate;

public sealed class Status
{
    private Status(bool isPtsAdded = false, bool isStsAdded = false, bool isOsagoAdded = false)
    {
        IsPtsAdded = isPtsAdded;
        IsStsAdded = isStsAdded;
        IsOsagoAdded = isOsagoAdded;
    }

    public bool IsPtsAdded { get; private set; }
    public bool IsStsAdded { get; private set; }
    public bool IsOsagoAdded { get; private set; }

    public bool AddingCompleted => IsPtsAdded && IsStsAdded && IsOsagoAdded;

    public void MarkAsPtsAdded()
    {
        IsPtsAdded = true;
    }

    public void MarkAsStsAdded()
    {
        IsStsAdded = true;
    }

    public void MarkAsOsagoAdded()
    {
        IsOsagoAdded = true;
    }

    public static Status Create()
    {
        return new Status();
    }

    public static Status FromValues(bool isPtsAdded, bool isStsAdded, bool isOsagoAdded)
    {
        return new Status(isPtsAdded, isStsAdded, isOsagoAdded);
    }
}