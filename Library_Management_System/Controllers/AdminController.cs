using Library_Management_System.Models;
using Library_Management_System.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IRentRepository _rentRepository;
        private readonly IPenaltyRepository _penaltyRepository;

        public AdminController(
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
            var students = await _userRepository.GetAllAsync();
            var books = await _bookRepository.GetAllAsync();
            var rents = await _rentRepository.GetAllIssuedAsync();

            ViewBag.TotalStudents = students.Count();
            ViewBag.TotalBooks = books.Count();
            ViewBag.TotalIssued = rents.Count();

            return View();
        }


        public async Task<IActionResult> Students(string search, int page = 1)
        {
            int pageSize = 10; 

            var students = await _userRepository.GetPaginatedStudentsAsync(search, page, pageSize);

            ViewBag.SearchTerm = search ?? "";

            return View(students); 
        }

        public IActionResult CreateStudent() => View();

        [HttpPost]
        public async Task<IActionResult> CreateStudent(User student)
        {
            if (ModelState.IsValid)
            {
                student.Role = "Student"; 
                await _userRepository.CreateAsync(student);
                return RedirectToAction(nameof(Students));
            }
            return View(student);
        }

        public async Task<IActionResult> EditStudent(int id)
        {
            var student = await _userRepository.GetByIdAsync(id);
            if (student == null) return NotFound();
            return View(student);
        }
        ///--------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> EditStudent(User student)
        {
            if (ModelState.IsValid)
            {
              
                var existingStudent = await _userRepository.GetByIdAsync(student.Id);
                if (existingStudent == null) return NotFound();

                if (!string.IsNullOrEmpty(student.PasswordHash))
                {
                    student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(student.PasswordHash);
                }
                else
                {
                    
                    student.PasswordHash = existingStudent.PasswordHash;
                }

                await _userRepository.UpdateAsync(student);
                return RedirectToAction(nameof(Students));
            }

            return View(student);
        }
//----------------------------------------------------------

        public async Task<IActionResult> DeleteStudent(int id)
        {
            await _userRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Students));
        }

      
        public async Task<IActionResult> Books(string search, int page = 1)
        {
            int pageSize = 10;

            var paginatedBooks = await _bookRepository.GetPaginatedBooksAsync(search, page, pageSize);

            return View(paginatedBooks);
        }
        public IActionResult CreateBook() => View();
        //------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> CreateBook(Book book, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/books");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                    var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    book.Image = "/images/books/" + fileName; 
                }

                await _bookRepository.InsertAsync(book);
                return RedirectToAction(nameof(Books));
            }
            return View(book);
        }


        public async Task<IActionResult> EditBook(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }
//---------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> EditBook(Book book, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                var existingBook = await _bookRepository.GetByIdAsync(book.BookID);
                if (existingBook == null) return NotFound();

                if (ImageFile != null && ImageFile.Length > 0)
                {
                  
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/books");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                    var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    book.Image = "/images/books/" + fileName;
                }
                else
                {
                  
                    book.Image = existingBook.Image;
                }

                await _bookRepository.UpdateAsync(book);
                return RedirectToAction(nameof(Books));
            }
            return View(book);
        }


        public async Task<IActionResult> DeleteBook(int id)
        {
            await _bookRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Books));
        }


        public async Task<IActionResult> EditIssuedBook(int id)
        {
            var rent = await _rentRepository.GetByIdAsync(id);
            if (rent == null) return NotFound();
            return View(rent);
        }
//-----------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> EditIssuedBook(Rent rent)
        {
            if (ModelState.IsValid)
            {
                rent.ReturnDate = rent.IssueDate.AddDays(rent.Days);

                await _rentRepository.UpdateAsync(rent);
                return RedirectToAction(nameof(IssuedBooks));
            }
            return View(rent);
        }

        public async Task<IActionResult> DeleteIssuedBook(int id)
        {
            await _rentRepository.DeleteAsync(id);
            return RedirectToAction(nameof(IssuedBooks));
        }

     
        ///////////DAY2/////////////////




   
        public async Task<IActionResult> IssueBook()
        {
            var students = await _userRepository.GetAllStudentsAsync();
            var books = await _bookRepository.GetAllAvailableAsync(); 

            ViewBag.Students = students;
            ViewBag.Books = books;

            return View();
        }
        //-----------------------------------------
        [HttpPost]
        public async Task<IActionResult> IssueBook(int studentId, int bookId, int days)
        {
            var student = await _userRepository.GetByIdAsync(studentId);
            var book = await _bookRepository.GetByIdAsync(bookId);

            if (student == null || book == null || book.AvailableQnt <= 0)
            {
                ModelState.AddModelError("", "Invalid student or book selection, or book not available.");

                ViewBag.Students = await _userRepository.GetAllStudentsAsync();
                ViewBag.Books = await _bookRepository.GetAllAvailableAsync();
                return View();
            }

            var rent = new Rent
            {
                BookID = book.BookID,   
                UserID = student.Id,
                Days = days,
                IssueDate = DateTime.Now,
                Status = 1
            };

            await _rentRepository.InsertAsync(rent);

            book.AvailableQnt -= 1;
            book.RentQnt += 1;
            await _bookRepository.UpdateAsync(book);

            return RedirectToAction(nameof(IssuedBooks));
        }

    
//---------------------------------------------------

        [HttpPost]
        public async Task<IActionResult> ReturnBook(int rentId)
        {
            var rent = await _rentRepository.GetByIdAsync(rentId);
            if (rent == null) return NotFound();

            rent.Status = 0;
            rent.ReturnDate = DateTime.Now;
            await _rentRepository.UpdateAsync(rent);

            var book = await _bookRepository.GetByIdAsync(rent.BookID);

            double penaltyAmount = 0;
            if (rent.ReturnDate > rent.IssueDate.AddDays(rent.Days))
            {
                int lateDays = (rent.ReturnDate.Value - rent.IssueDate.AddDays(rent.Days)).Days;
                penaltyAmount = lateDays * 10; 
            }

            if (penaltyAmount > 0)
            {
                var penalty = new Penalty
                {
                    UserID = rent.UserID,
                    BookID = book.BookID,           
                    BookName = book.BookName,
                    Price = book.Price,             
                    PenaltyAmount = penaltyAmount,
                    Detail = $"Late return, {penaltyAmount} penalty applied.",
                    EntryDate = DateTime.Now
                };

                await _penaltyRepository.InsertAsync(penalty);
            }

           
            book.AvailableQnt += 1;
            book.RentQnt -= 1;
            await _bookRepository.UpdateAsync(book);

            return RedirectToAction(nameof(IssuedBooks));
        }

        public async Task<IActionResult> Penalties(string? status = "all", string? search = "", int page = 1)
        {
            int pageSize = 10;

            var (penalties, totalCount) = await _penaltyRepository.GetPaginatedAsync(status, search, page, pageSize);

            ViewBag.StatusFilter = status;
            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(penalties);
        }




        public async Task<IActionResult> EditPenalty(int id)
        {
            var penalty = await _penaltyRepository.GetByIdAsync(id);
            if (penalty == null) return NotFound();
            return View(penalty);
        }

        [HttpPost]
        public async Task<IActionResult> EditPenalty(Penalty penalty)
        {
            if (!ModelState.IsValid) return View(penalty);

            await _penaltyRepository.UpdateAsync(penalty);
            return RedirectToAction(nameof(Penalties));
        }

        [HttpPost]
        public async Task<IActionResult> DeletePenalty(int id)
        {
            await _penaltyRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Penalties));
        }

        [HttpPost]
        public async Task<IActionResult> MarkPenaltyAsPaid(int id)
        {
            await _penaltyRepository.MarkAsPaidAsync(id);
            return RedirectToAction(nameof(Penalties));
        }
        ///////////////
        ///

        public async Task<IActionResult> IssuedBooks(string search, int page = 1)
        {
            int pageSize = 10;

            var paginatedRents = await _rentRepository.GetPaginatedIssuedBooksAsync(search, page, pageSize);

            return View(paginatedRents);
        }

   



    }
}
