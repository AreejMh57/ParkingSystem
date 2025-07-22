// Presentation/Controllers/WalletController.cs
using Application.DTOs; // لـWalletDto, CreateWalletDto
using Application.IServices; // لـIWalletService
using Microsoft.AspNetCore.Authorization; // لـ[Authorize] attribute
using Microsoft.AspNetCore.Mvc; // لـControllerBase, IActionResult, إلخ
using System; // لـGuid
using System.Collections.Generic; // لـIEnumerable
using System.Security.Claims; // لـClaimTypes.NameIdentifier
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [ApiController] 
    [Route("api/[controller]")] 
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        /// <summary>
        /// Retrieves the wallet details for the authenticated user.
        /// Requires 'wallet_browse' permission.
        /// </summary>
        /// <returns>A WalletDto of the user's wallet.</returns>
        [HttpGet("my-wallet")]
        [Authorize(Policy = "WALLET_BROWSE")]
        public async Task<IActionResult> GetMyWallet()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token. Please log in.");
            }

            var wallet = await _walletService.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                return NotFound(new { Message = "Wallet not found for this user. It might need to be created." });
            }
            return Ok(wallet);
        }

        /// <summary>
        /// Creates a new wallet for a user. (Typically called internally or by admin)
        /// Requires 'wallet_create' permission.
        /// </summary>
        /// <param name="dto">Wallet creation details.</param>
        /// <returns>The created WalletDto on success.</returns>
        [HttpPost("create")]
        [Authorize(Policy = "WALLET_CREATE")]
        public async Task<IActionResult> CreateWallet([FromBody] CreateWalletDto dto)
        {
            // Optional: Get UserId from authenticated user if not provided in DTO
            // For this DTO, UserId is required, so we'll trust the DTO for now.
            // var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // if (string.IsNullOrEmpty(dto.UserId) || dto.UserId != authenticatedUserId)
            // {
            //     return Forbid("You can only create a wallet for yourself.");
            // }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdWallet = await _walletService.CreateWalletAsync(dto);
                return StatusCode(201, createdWallet); // HTTP 201 Created
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message }); // User already has a wallet
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the wallet.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Deposits funds into a user's wallet.
        /// Requires 'wallet_update' permission.
        /// </summary>
        /// <param name="walletId">The ID of the wallet to deposit into.</param>
        /// <param name="amount">The amount to deposit.</param>
        /// <returns>The updated WalletDto.</returns>
        [HttpPut("{walletId}/deposit")]
        [Authorize(Policy = "WALLET_UPDATE")]
        public async Task<IActionResult> Deposit(Guid walletId, [FromBody] decimal amount)
        {
            if (amount <= 0)
            {
                return BadRequest(new { Message = "Deposit amount must be positive." });
            }
            // Optional: Authorization check - ensure user owns this walletId
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // var userWallet = await _walletService.GetWalletByUserIdAsync(userId);
            // if (userWallet == null || userWallet.WalletId != walletId) { return Forbid("Not authorized to deposit to this wallet."); }

            try
            {
                var updatedWallet = await _walletService.DepositAsync(walletId, amount);
                return Ok(updatedWallet);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred during deposit.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Deducts funds from a user's wallet.
        /// Requires 'wallet_update' permission.
        /// </summary>
        /// <param name="walletId">The ID of the wallet to deduct from.</param>
        /// <param name="amount">The amount to deduct.</param>
        /// <returns>The updated WalletDto.</returns>
        [HttpPut("{walletId}/deduct")]
        [Authorize(Policy = "WALLET_UPDATE")]
        public async Task<IActionResult> Deduct(Guid walletId, [FromBody] decimal amount)
        {
            if (amount <= 0)
            {
                return BadRequest(new { Message = "Deduction amount must be positive." });
            }
            // Optional: Authorization check - ensure user owns this walletId
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // var userWallet = await _walletService.GetWalletByUserIdAsync(userId);
            // if (userWallet == null || userWallet.WalletId != walletId) { return Forbid("Not authorized to deduct from this wallet."); }

            try
            {
                var updatedWallet = await _walletService.DeductAsync(walletId, amount);
                return Ok(updatedWallet);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex) // Insufficient balance
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred during deduction.", Details = ex.Message });
            }
        }
    }
}