using CRISP.Server.Data.ValueObjects;

namespace CRISP.Server.Data.Entities;

public interface IHasQrCode
{
    StoredFileObject? QRCode { get; set; }
}
