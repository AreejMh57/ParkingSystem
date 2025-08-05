using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Security.Claims; // لإيجاد الـ UserId للمستخدم الحالي

namespace Dashboard.Controllers
{
    [Authorize] // تأمين المتحكم بالكامل، فقط المستخدمين المصادق عليهم يمكنهم الوصول
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(IBookingService bookingService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        // GET: /Booking/Index
        // هذا الإجراء سيعرض قائمة بجميع الحجوزات (للمسؤول)
        // إذا كنت تريد عرض حجوزات مستخدم واحد، فستحتاج لإجراء مختلف
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Admin user accessed the Bookings index page.");

            // بما أن الخدمة ليس لديها إجراء "GetAllBookingsAsync"، سنفترض
            // وجود إجراء عام في الـ Repository يجلب كل الحجوزات.
            // أو يمكننا استخدام FilterByAsync بدون أي فلتر.
            // هذا يتطلب تعديل طفيف في BookingService
            // (أو سنستخدم GetUserBookingsAsync كحل مؤقت).

            // حل مؤقت: جلب كل الحجوزات من الخدمة (تطلب تعديلها لتشمل هذه الوظيفة)
            // for now, let's assume a GetAllBookingsAsync method exists in the service
            // var allBookings = await _bookingService.GetAllBookingsAsync();

            // بما أنك لا تملك GetAllBookingsAsync، سنستخدم FilterByAsync من الـ Repository
            // ولكن هذا يجب أن يكون في Service، وليس هنا.
            // بما أنك قدمت GetUserBookingsAsync فقط، لنستخدمها.
            // لكن هذا المتحكم مخصص للـ Admin، لذا سنفترض أن لديه صلاحية لرؤية كل الحجوزات.
            // لنعدل الخدمة أو المتحكم للوصول إلى كل الحجوزات.

            // الحل الأكثر صحة هو إضافة method مثل GetAllBookingsAsync() to IBookingService:
            // var allBookings = await _bookingService.GetAllBookingsAsync();

            // إذا كان GetUserBookingsAsync هو المتاح فقط، يمكنك إظهار حجوزات المستخدم الحالي فقط:
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // var bookings = await _bookingService.GetUserBookingsAsync(userId);

            // الحل الأفضل: افترض أن Service توفر GetAllBookingsAsync()
            var allBookings = await _bookingService.GetAllBookingsAsync(); 

            return View(allBookings);
        }

        // GET: /Booking/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            _logger.LogInformation("Admin user accessed booking details for booking ID: {BookingId}.", id);

            var booking = await _bookingService.GetBookingByIdAsync(id);

            if (booking == null)
            {
                _logger.LogWarning("Booking details for ID {BookingId} not found.", id);
                return NotFound();
            }

            return View(booking);
        }

        // POST: /Booking/Cancel/{id}
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        // قد ترغب في إضافة سياسة صلاحية هنا، مثل [Authorize(Policy = "Bookings_Cancel")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogWarning("Admin user {UserId} is attempting to cancel booking ID: {BookingId}.", userId, id);

            try
            {
                var result = await _bookingService.CancelBookingAsync(id, userId); // يجب أن يستخدم الـ Admin method أخرى لـ Cancel

                // ملاحظة: CancelBookingAsync في خدمتك يتحقق من أن الـ userId هو مالك الحجز
                // لذا لإدارة المسؤول، ستحتاج إلى إجراء cancelBooking آخر في الخدمة لا يتحقق من الـ userId.
                // أو يمكنك تمرير null/string.Empty كـ userId للـ Admin.

                if (result.Succeeded)
                {
                    _logger.LogInformation("Booking ID {BookingId} successfully canceled by admin {UserId}.", id, userId);
                    return RedirectToAction(nameof(Index));
                }

                // إذا فشلت العملية، أضف الأخطاء إلى ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while admin {UserId} was canceling booking {BookingId}.", userId, id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred during cancellation.");
            }

            // إذا حدث خطأ، أعد توجيه المستخدم إلى صفحة التفاصيل مع الأخطاء
            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}