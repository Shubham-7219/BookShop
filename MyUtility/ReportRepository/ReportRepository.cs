using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MyBookUi.Data;
using MyDataAccess.MyConstant;
using MyModel.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyUtility.ReportRepository
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context;
        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TopNSoldBookModel>> GetTopNSellingBooksByDate(DateTime startDate, DateTime endDate)
        {
            var startDateParam = new SqlParameter("@startDate", startDate);
            var endDateParam = new SqlParameter("@endDate", endDate);
            var topFiveSoldBooks = await _context.Database.SqlQueryRaw<TopNSoldBookModel>("exec Usp_GetTopNSellingBookByDate @startDate,@endDate", startDateParam, endDateParam).ToListAsync();
            return topFiveSoldBooks;
        }

    }
}
