using Library_Management_System.Models;
using Library_Management_System.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Library_Management_System.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IRentRepository _rentRepository;
        private readonly IPenaltyRepository _penaltyRepository;

        public StudentController(
            IUserRepository userRepository,
            IBookRepository bookRepository,
            IRentRepository rentRepository,
            IPenaltyRepository penaltyRepository)
        {
            _userRepository = userRepository;
            _bookRepository = bookRepository;
            _rentRepository = rentRepository;
            _penaltyRepository = penaltyRepository;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                return RedirectToAction("Login", "Auth");

            var student = await _userRepository.GetByIdAsync(userId);

            var allBooks = await _bookRepository.GetAllAsync();
            var myIssuedBooks = (await _rentRepository.GetByUserIdAsync(userId))?.Count() ?? 0;
            var myPenalties = (await _penaltyRepository.GetByUserIdAsync(userId))?.Count(p => !p.IsPaid) ?? 0;

            ViewBag.StudentName = student?.Name ?? "Student";
            ViewBag.TotalBooks = allBooks?.Count() ?? 0;
            ViewBag.MyIssued = myIssuedBooks;
            ViewBag.MyPenalties = myPenalties;

            return View();
        }
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> AvailableBooks()
        {
            var allBooks = await _bookRepository.GetAllAsync();
            return View(allBooks); 
        }
        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> IssueBook(int bookId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId)) 
                return RedirectToAction("Login", "Auth");

            await _rentRepository.IssueBookAsync(userId, bookId);

            return RedirectToAction("AvailableBooks");
        }

        [Authorize(Roles = "Student")]
        
        public async Task<IActionResult> MyBooks()
        {

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                return RedirectToAction("Login", "Auth");

            var issuedBooks = await _rentRepository.GetIssuedBooksByUserIdAsync(userId);

            return View(issuedBooks);
        }
        public async Task<IActionResult> MyPenalties()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                return RedirectToAction("Login", "Auth");

            var penalties = await _penaltyRepository.GetByUserIdAsync(userId);

            return View(penalties);
        }


    }
}
