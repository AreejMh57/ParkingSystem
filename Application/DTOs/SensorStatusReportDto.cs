// Application/DTOs/Sensor/SensorStatusReportDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class SensorStatusReportDto
    {
        [Required]
        public Guid SensorId { get; set; } // معرف الحساس الذي يرسل التقرير

        [Required]
        public string SensorKey { get; set; } // مفتاح سري للحساس (للمصادقة الأولية للجهاز)

        [Required]
        public bool IsOccupied { get; set; } // الحالة الجديدة للموقف (صحيح: مشغول، خطأ: فارغ)

        public Guid? BookingId { get; set; } // معرف الحجز المرتبط (إذا كان هذا التقرير عن دخول/خروج حجز معين)

        [Required]
        public DateTime EventTimestamp { get; set; } = DateTime.UtcNow; // وقت وقوع الحدث في الحساس
    }
}
