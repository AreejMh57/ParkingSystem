// Infrastructure/Repositories/BookingRepository.cs
using Domain.Entities;
using Domain.IRepositories;
using Infrastructure.Contexts; // لـAppDbContext
using Microsoft.EntityFrameworkCore; // لـCountAsync()
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions; // لـExpression<Func<Booking, bool>>
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        private readonly AppDbContext _context; // للوصول المباشر إلى DbContext

        public BookingRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        // <--- تطبيق دالة GetCountAsync هنا --->
        public async Task<int> GetCountAsync(Expression<Func<Booking, bool>> predicate)
        {
            // تطبيق الشرط على DbSet<Booking> ثم حساب العدد
            return await _context.Bookings.CountAsync(predicate);
        }

        // ... يمكنك إضافة دوال أخرى خاصة بالحجوزات هنا ...
    }
}