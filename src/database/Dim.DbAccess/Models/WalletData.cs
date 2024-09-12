namespace Dim.DbAccess.Models;

public record WalletData(
    string? TokenAddress,
    string? ClientId,
    byte[]? ClientSecret,
    byte[]? InitializationVector,
    int? EncryptionMode
);
