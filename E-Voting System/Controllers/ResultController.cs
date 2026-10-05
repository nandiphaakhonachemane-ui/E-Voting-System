using E_Voting_System.Data;
using E_Voting_System.Models;
using E_Voting_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Voting_System.Controllers
{
    [Authorize(Roles = "Admin,ElectionOfficer")]
    public class ResultController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IResultsService _resultsService;

        public ResultController(ApplicationDbContext context, IResultsService resultsService)
        {
            _context = context;
            _resultsService = resultsService;
        }

        public async Task<IActionResult> Index()
        {
            var elections = await _context.Elections
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();
            return View(elections);
        }

        public async Task<IActionResult> Details(int id)
        {
            var results = await _resultsService.GetResultsAsync(id);
            return View(results);
        }

        public async Task<IActionResult> SeatAllocation(int id, int totalSeats = 400)
        {
            // 400 is the number of seats in the National Assembly
            var allocation = await _resultsService.CalculateSeatAllocationAsync(id, totalSeats);
            ViewBag.ElectionId = id;
            ViewBag.TotalSeats = totalSeats;
            return View(allocation);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PublishResults(int id)
        {
            var election = await _context.Elections.FindAsync(id);
            if (election != null)
            {
                election.ResultsPublished = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv(int id)
        {
            var results = await _resultsService.GetResultsAsync(id);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Party,Code,Votes,Percentage");

            foreach (var r in results.NationalResults)
            {
                double percentage = results.TotalNationalVotes > 0
                    ? (double)r.Votes / results.TotalNationalVotes * 100
                    : 0;

                sb.AppendLine($"\"{r.PartyName}\",{r.PartyCode},{r.Votes},{percentage:0.00}%");
            }

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"Election_{id}_Results.csv");
        }
    }
}

