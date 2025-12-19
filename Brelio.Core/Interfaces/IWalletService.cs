using Brelio.Core.DTOs;

namespace Brelio.Core.Interfaces;

public interface IWalletService
{
    Task<List<WalletDto>> GetUserWalletsAsync(Guid userId);
    Task<WalletDto> CreateWalletAsync(Guid userId, CreateWalletRequest request);
    Task<WalletDto> UpdateWalletAsync(Guid userId, Guid walletId, UpdateWalletRequest request);
    Task DeleteWalletAsync(Guid userId, Guid walletId);
    Task<bool> ValidateSolanaAddress(string address);
}
