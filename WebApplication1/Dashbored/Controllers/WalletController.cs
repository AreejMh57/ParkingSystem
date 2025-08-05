using Application.DTOs;
using Application.IServices;
using Domain.Entities;
using Domain.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dashboard.Controllers
{
    [Authorize] // تأمين المتحكم
    public class WalletController : Controller
    {
        private readonly IWalletService _walletService;
        private readonly IRepository<Wallet> _walletRepo; // للحصول على جميع المحافظ
        private readonly ILogger<WalletController> _logger;

        public WalletController(
            IWalletService walletService,
            IRepository<Wallet> walletRepo,
            ILogger<WalletController> logger)
        {
            _walletService = walletService;
            _walletRepo = walletRepo;
            _logger = logger;
        }

        // GET: /Wallet
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Admin accessed the Wallet management page.");

            // بما أن IWalletService لا تحتوي على GetAllWalletsAsync()
            // سنستخدم الـ Repository مباشرة هنا (وهذا ليس أفضل ممارسة للـ Clean Arch. لكنه حل عملي مؤقت)
            // الأفضل هو إضافة GetAllWalletsAsync() إلى IWalletService
            var wallets = await _walletRepo.GetAllAsync();

            // يجب أن نستخدم AutoMapper لتحويل الكيانات إلى DTOs
            // سنفترض وجود تابع Map في الـ controller أو نمرر الـ mapper
            // لكن لتسهيل الأمر، سننشئ قائمة من DTOs يدوياً أو نستخدم الـ mapper
            var walletDtos = new List<WalletDto>();
            foreach (var wallet in wallets)
            {
                walletDtos.Add(new WalletDto
                {
                    WalletId = wallet.WalletId,
                   // UserId = wallet.UserId,
                    Balance = wallet.Balance,
                  //  CreatedAt = wallet.CreatedAt,
                  //  UpdatedAt = wallet.UpdatedAt
                });
            }

            // ملاحظة: لتحسين هذا، أضف GetAllWalletsAsync() إلى IWalletService واستخدم الـ mapper هناك.

            return View(walletDtos);
        }

        // GET: /Wallet/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            _logger.LogInformation("Admin accessed details for wallet ID: {WalletId}.", id);

            // بما أن IWalletService لا تحتوي على GetWalletByIdAsync()، سنستخدم الـ repo
            var wallet = await _walletRepo.GetByIdAsync(id);
            if (wallet == null)
            {
                _logger.LogWarning("Wallet details for ID {WalletId} not found.", id);
                return NotFound();
            }

            var walletDto = new WalletDto
            {
                WalletId = wallet.WalletId,
              //  UserId = wallet.UserId,
                Balance = wallet.Balance,
               // CreatedAt = wallet.CreatedAt,
               // UpdatedAt = wallet.UpdatedAt
            };

            return View(walletDto);
        }

        // POST: /Wallet/Deposit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(Guid walletId, decimal amount)
        {
            if (amount <= 0)
            {
                TempData["ErrorMessage"] = "Amount must be a positive number.";
                return RedirectToAction(nameof(Details), new { id = walletId });
            }

            try
            {
                var result = await _walletService.DepositAsync(walletId, amount);
                _logger.LogInformation("Admin deposited {Amount} to wallet {WalletId}. New balance: {Balance}", amount, walletId, result.Balance);
                TempData["SuccessMessage"] = $"Successfully deposited {amount.ToString("C")} to the wallet. New balance is {result.Balance.ToString("C")}.";
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Failed to deposit to wallet {WalletId}: Wallet not found.", walletId);
                TempData["ErrorMessage"] = "Wallet not found.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during deposit to wallet {WalletId}: {Message}", walletId, ex.Message);
                TempData["ErrorMessage"] = "An unexpected error occurred during the deposit.";
            }

            return RedirectToAction(nameof(Details), new { id = walletId });
        }

        // POST: /Wallet/Deduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deduct(Guid walletId, decimal amount)
        {
            if (amount <= 0)
            {
                TempData["ErrorMessage"] = "Amount must be a positive number.";
                return RedirectToAction(nameof(Details), new { id = walletId });
            }

            try
            {
                var result = await _walletService.DeductAsync(walletId, amount);
                _logger.LogInformation("Admin deducted {Amount} from wallet {WalletId}. New balance: {Balance}", amount, walletId, result.Balance);
                TempData["SuccessMessage"] = $"Successfully deducted {amount.ToString("C")} from the wallet. New balance is {result.Balance.ToString("C")}.";
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Failed to deduct from wallet {WalletId}: Wallet not found.", walletId);
                TempData["ErrorMessage"] = "Wallet not found.";
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to deduct from wallet {WalletId}: {Message}", walletId, ex.Message);
                TempData["ErrorMessage"] = ex.Message; // "Insufficient balance."
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during deduction from wallet {WalletId}: {Message}", walletId, ex.Message);
                TempData["ErrorMessage"] = "An unexpected error occurred during the deduction.";
            }

            return RedirectToAction(nameof(Details), new { id = walletId });
        }
    }
}