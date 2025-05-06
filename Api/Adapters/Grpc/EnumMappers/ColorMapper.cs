using Domain.SharedKernel.Exceptions.PublicExceptions;
using DomainColor = Domain.SharedKernel.ValueObjects.Color;

namespace Api.Adapters.Grpc.EnumMappers;

public class ColorMapper
{
    public DomainColor ProtoToDomain(Color protoColor)
    {
        return protoColor switch
        {
            Color.WhiteUnspecified => DomainColor.White,
            Color.Grey => DomainColor.Grey,
            Color.Black => DomainColor.Black,
            Color.Blue => DomainColor.Blue,
            Color.Red => DomainColor.Red,
            Color.Yellow => DomainColor.Yellow,
            Color.Orange => DomainColor.Orange,
            Color.Green => DomainColor.Green,
            Color.Beige => DomainColor.Beige,
            _ => throw new ValueIsUnsupportedException($"{nameof(protoColor)} is unknown color")
        };
    }

    public Color DomainToProto(DomainColor domainColor)
    {
        return domainColor switch
        {
            _ when domainColor == DomainColor.White => Color.WhiteUnspecified,
            _ when domainColor == DomainColor.Grey => Color.Grey,
            _ when domainColor == DomainColor.Black => Color.Black,
            _ when domainColor == DomainColor.Blue => Color.Blue,
            _ when domainColor == DomainColor.Red => Color.Red,
            _ when domainColor == DomainColor.Yellow => Color.Yellow,
            _ when domainColor == DomainColor.Orange => Color.Orange,
            _ when domainColor == DomainColor.Green => Color.Green,
            _ when domainColor == DomainColor.Beige => Color.Beige,
            _ => throw new ValueIsUnsupportedException($"{nameof(domainColor)} is unknown color")
        };
    }
}