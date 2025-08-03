// Application/IServices/IBookingRepository.cs
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions; // لـExpression<Func<Booking, bool>>
using System.Threading.Tasks;

namespace Domain.IRepositories
{
    public interface IBookingRepository : IRepository<Booking> // يرث من IRepository العامة
    {
        // <--- إضافة هذه الدالة (إذا لم تكن موجودة) --->
        Task<int> GetCountAsync(Expression<Func<Booking, bool>> predicate);
    }
}