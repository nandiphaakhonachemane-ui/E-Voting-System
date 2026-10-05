using E_Voting_System.Data;
using E_Voting_System.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Voting_System.Controllers
{
    public class PublicResultsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IResultsService _resultsService;

        public PublicResultsController(ApplicationDbContext context, IResultsService resultsService)
        {
            _context = context;
            _resultsService = resultsService;
        }

        public async Task<IActionResult> Index()
        {
            var publishedElections = await _context.Elections
                .Where(e => e.ResultsPublished)
                .OrderByDescending(e => e.EndDate)
                .ToListAsync();

            return View(publishedElections);
        }

        public async Task<IActionResult> Details(int id)
        {
            var election = await _context.Elections.FindAsync(id);
            if (election == null || !election.ResultsPublished)
            {
                return NotFound(); // or show "Results not yet published"
            }

            var results = await _resultsService.GetResultsAsync(id);
            return View(results);
        }
    }
}