using Application.DTOs;
using Application.IServices; // افترض وجود IBookingService و IPaymentService
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.IRepositories;

using Domain.Entities;

// يحدد المسار الرئيسي للـ Controller، مما يجعله متاحًا على "api/Payment"
[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    // حقن التبعية (Dependency Injection) لخدمة الدفع
    private readonly IPaymentTransactionService _paymentService;
    private readonly IRepository<PaymentTransaction> _paymentRepo;
    private readonly IMapper _mapper;


    // تابع البناء (Constructor) لحقن خدمة الدفع
    public PaymentController(IPaymentTransactionService paymentService, IRepository<PaymentTransaction> paymentRepo, IMapper mapper)
    {
        _paymentService = paymentService;
        _paymentRepo = paymentRepo;
        _mapper = mapper;           

    }

    /// <summary>
    /// يؤكد حجزًا مؤقتًا، ويخصم المبلغ من المحفظة، وينشئ توكنًا للحجز.
    /// </summary>
    /// <remarks>
    /// يرسل تطبيق الموبايل طلبًا من نوع POST إلى هذا المسار: /api/Payment/confirm
    /// مع كائن CreatePaymentTransactionDto في جسم الطلب.
    /// </remarks>
    /// <param name="dto">كائن نقل البيانات (DTO) الذي يحتوي على تفاصيل المعاملة.</param>
    /// <returns>ActionResult يمثل نتيجة العملية.</returns>
    [HttpPost("confirm")]
    public async Task<ActionResult<ConfirmedBookingDto>> ConfirmPayment([FromBody] CreatePaymentTransactionDto dto)
    {
        try
        {
            // استدعاء خدمة الدفع لتأكيد الحجز ومعالجة الدفع وإنشاء التوكن
            var resultDto = await _paymentService.ConfirmPaymentAndCreateTokenAsync(dto);

            // عند النجاح، يعود برمز حالة 200 OK مع الكائن الذي تم إنشاؤه.
            return Ok(resultDto);
        }
        catch (InvalidOperationException ex)
        {
            // في حالة وجود بيانات غير صالحة (مثل رصيد غير كافٍ أو حجز مؤكد بالفعل)، يعود برمز حالة 400 Bad Request.
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            // في حالة عدم العثور على الحجز أو المستخدم، يعود برمز حالة 404 Not Found.
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            // في حالة محاولة مستخدم تأكيد حجز لا يخصه، يعود برمز حالة 403 Forbidden.
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            // في حالة وجود أي خطأ غير متوقع، يعود برمز حالة 500 Internal Server Error.
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("history/{userId}")] // تحديد مسار HTTP GET مع متغير في المسار
    [ProducesResponseType(typeof(IEnumerable<PaymentTransactionDto>), 200)] // تحديد نوع الاستجابة المتوقعة (للـ Swagger)
    [ProducesResponseType(404)] // في حال عدم وجود المستخدم أو دفعات (اختياري)
    public async Task<ActionResult<IEnumerable<PaymentTransactionDto>>> GetUserPaymentHistory(string userId)
    {
        // التحقق الأساسي من صحة المدخلات
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("User ID cannot be empty.");
        }

        var payments = await _paymentRepo.FilterByAsync(new Dictionary<string, object>
            {
                { "UserId", userId }
            });

        // يمكن إضافة منطق للتعامل مع حالة عدم وجود دفعات (إذا كانت القائمة فارغة)
        if (payments == null || !payments.Any())
        {
            // يمكنك إعادة قائمة فارغة أو NotFound، حسب منطق عملك
            return NotFound($"No payment history found for user ID: {userId}");
        }

        return Ok(payments.Select(p => _mapper.Map<PaymentTransactionDto>(p)));
    }

}
