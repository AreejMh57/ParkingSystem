using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.IServices
{
    public interface IWalletService
    {
        //Creates a new wallet for a user
       // Task<WalletDto> CreateWalletAsync(CreateWalletDto dto);
        //Deposits an amount into a specified wallet
        Task<WalletDto> DepositAsync(Guid walletId, decimal amount);

        //Deducts an amount from a specified walle
        Task<WalletDto> DeductAsync(Guid walletId, decimal amount);

        // Retrieves the details of a specific user's wallet
        Task<WalletDto> GetWalletByUserIdAsync(string userId);
        Task<IEnumerable<WalletDto>> GetAllWalletsAsync();
    }
}
