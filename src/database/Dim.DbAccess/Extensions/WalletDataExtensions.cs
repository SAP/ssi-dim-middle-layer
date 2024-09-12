using Dim.DbAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

namespace Dim.DbAccess.Extensions;

public static class WalletDataExtensions
{
    public static (string TokenAddress, string ClientId, byte[] ClientSecret, byte[] InitializationVector, int EncryptionMode) ValidateData(this WalletData walletData)
    {
        var (tokenAddress, clientId, clientSecret, initializationVector, encryptionMode) = walletData;
        if (string.IsNullOrWhiteSpace(tokenAddress))
        {
            throw new ConflictException("TokenAddress must not be null");
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ConflictException("ClientId must not be null");
        }

        if (clientSecret == null)
        {
            throw new ConflictException("Secret must not be null");
        }

        if (initializationVector == null)
        {
            throw new ConflictException("Vector must not be null");
        }

        if (encryptionMode == null)
        {
            throw new ConflictException("EncryptionMode must not be null");
        }

        return (tokenAddress, clientId, clientSecret, initializationVector, encryptionMode.Value);
    }
}
