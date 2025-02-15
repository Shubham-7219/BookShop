﻿using Microsoft.AspNetCore.Mvc;
using MyModel.DTOs;
using MyUtility.ReportRepository;

namespace MyBookUi.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportRepository _reportRepository;
        public ReportController(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }
        // GET: ReportsController
        public async Task<ActionResult> TopFiveSellingBooks(DateTime? sDate = null, DateTime? eDate = null)
        {
            try
            {
                // by default, get last 7 days record
                DateTime startDate = sDate ?? DateTime.UtcNow.AddDays(-7);
                DateTime endDate = eDate ?? DateTime.UtcNow;
                var topFiveSellingBooks = await _reportRepository.GetTopNSellingBooksByDate(startDate, endDate);
                var vm = new TopNSoldBooksVm(startDate, endDate, topFiveSellingBooks);
                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = "Something went wrong";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
