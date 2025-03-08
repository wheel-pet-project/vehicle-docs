namespace Domain.VehicleDocumentsAggregate;

public sealed class Status
{
    private Status()
    {
        IsPtsAdded = false;
        IsStsAdded = false;
        IsOsagoAdded = false;
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
}
