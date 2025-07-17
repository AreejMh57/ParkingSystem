using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.IServices;
using AutoMapper;
using Domain.Entities;
using Domain.IRepositories;


namespace Infrastructure.Services
{
    public class WalletService : IWalletService
    
    {

        private readonly IRepository<Wallet> _walletRepo;
        private readonly ILogService _logService;
        private readonly IMapper _mapper; // Inject IMapper

        public WalletService(IRepository<Wallet> walletRepo, ILogService logService, IMapper mapper) // Add IMapper to constructor
        {
            _walletRepo = walletRepo;
            _logService = logService;
            _mapper = mapper; // Assign mapper
        }

        public async Task<WalletDto> CreateWalletAsync(CreateWalletDto dto) // Changed return type
        {
            var existingWallets = await _walletRepo.FilterByAsync(new Dictionary<string, object> { { "UserId", dto.UserId } });
            if (existingWallets.Any())
            {
                await _logService.LogWarningAsync($"Attempted to create wallet for existing user {dto.UserId}.");
                throw new InvalidOperationException("User already has a wallet."); // Throw exception instead of string
            }

            var wallet = new Wallet
            {
                WalletId = Guid.NewGuid(),
                Balance = dto.Balance,
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            await _walletRepo.AddAsync(wallet);
            await _walletRepo.SaveChangesAsync();
            await _logService.LogInfoAsync($"Wallet {wallet.WalletId} created for user {dto.UserId}. Initial balance: {dto.Balance}");

            return _mapper.Map<WalletDto>(wallet); // Return WalletDto
        }

        public async Task<WalletDto> DepositAsync(Guid walletId, decimal amount) // Changed return type
        {
            if (amount <= 0)
            {
                await _logService.LogWarningAsync($"Invalid deposit attempt for wallet {walletId}: amount {amount} <= 0.");
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive."); // Throw exception
            }

            var wallet = await _walletRepo.GetByIdAsync(walletId);
            if (wallet == null)
            {
                await _logService.LogWarningAsync($"Deposit failed: Wallet {walletId} not found.");
                throw new KeyNotFoundException($"Wallet with ID {walletId} not found."); // Throw exception
            }

            wallet.Balance += amount;
            wallet.LastUpdated = DateTime.UtcNow;
            wallet.UpdatedAt = DateTime.UtcNow;

            _walletRepo.Update(wallet);
            await _walletRepo.SaveChangesAsync();
            await _logService.LogInfoAsync($"Wallet {walletId}: Deposited {amount}. New balance: {wallet.Balance}");

            return _mapper.Map<WalletDto>(wallet); // Return WalletDto
        }

        public async Task<WalletDto> DeductAsync(Guid walletId, decimal amount) // Changed return type
        {
            if (amount <= 0)
            {
                await _logService.LogWarningAsync($"Invalid deduction attempt for wallet {walletId}: amount {amount} <= 0.");
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive."); // Throw exception
            }

            var wallet = await _walletRepo.GetByIdAsync(walletId);
            if (wallet == null)
            {
                await _logService.LogWarningAsync($"Deduction failed: Wallet {walletId} not found.");
                throw new KeyNotFoundException($"Wallet with ID {walletId} not found."); // Throw exception
            }
            if (wallet.Balance < amount)
            {
                await _logService.LogWarningAsync($"Deduction failed for wallet {walletId}: insufficient balance ({wallet.Balance} < {amount}).");
                throw new InvalidOperationException("Insufficient balance."); // Throw exception
            }

            wallet.Balance -= amount;
            wallet.LastUpdated = DateTime.UtcNow;
            wallet.UpdatedAt = DateTime.UtcNow;

            _walletRepo.Update(wallet);
            await _walletRepo.SaveChangesAsync();
            await _logService.LogInfoAsync($"Wallet {walletId}: Deducted {amount}. New balance: {wallet.Balance}");

            return _mapper.Map<WalletDto>(wallet); // Return WalletDto
        }

        public async Task<WalletDto> GetWalletByUserIdAsync(string userId)
        {
            var wallets = await _walletRepo.FilterByAsync(new Dictionary<string, object>
            {
                { "UserId", userId }
            });

            var wallet = wallets.FirstOrDefault();
            if (wallet == null)
            {
                return null;
            }


            return _mapper.Map<WalletDto>(wallet); // Return WalletDto
        }
    }
}

