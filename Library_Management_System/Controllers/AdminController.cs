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

     
        private IActionResult HandleNotFound(string entityName, string redirectAction)
        {
            TempData["Error"] = $"{entityName} not found.";
            return RedirectToAction(redirectAction);
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var students = await _userRepository.GetAllAsync();
                var books = await _bookRepository.GetAllAsync();
                var rents = await _rentRepository.GetAllIssuedAsync();

                ViewBag.TotalStudents = students.Count();
                ViewBag.TotalBooks = books.Count();
                ViewBag.TotalIssued = rents.Count();

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load dashboard: " + ex.Message;
                return View();
            }
        }

        public async Task<IActionResult> Students(string search, int page = 1)
        {
            try
            {
                int pageSize = 10;
                var students = await _userRepository.GetPaginatedStudentsAsync(search, page, pageSize);
                ViewBag.SearchTerm = search ?? "";
                return View(students);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load students: " + ex.Message;
                return View(Enumerable.Empty<User>());
            }
        }

        public IActionResult CreateStudent() => View();

        [HttpPost]
        public async Task<IActionResult> CreateStudent(User student)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    student.Role = "Student";
                    await _userRepository.CreateAsync(student);
                    TempData["Success"] = "Student created successfully.";
                    return RedirectToAction(nameof(Students));
                }
                return View(student);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to create student: " + ex.Message;
                return View(student);
            }
        }

        public async Task<IActionResult> EditStudent(int id)
        {
            try
            {
                var student = await _userRepository.GetByIdAsync(id);
                if (student == null)
                    return HandleNotFound($"Student with ID {id}", nameof(Students));

                return View(student);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load student: " + ex.Message;
                return RedirectToAction(nameof(Students));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditStudent(User student)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingStudent = await _userRepository.GetByIdAsync(student.Id);
                    if (existingStudent == null)
                        return HandleNotFound($"Student with ID {student.Id}", nameof(Students));
                    var userWithSameEmail = await _userRepository.GetByEmailAsync(student.Email);
                    if (userWithSameEmail != null && userWithSameEmail.Id != student.Id)
                    {
                        ModelState.AddModelError("Email", "This email is already in use by another student.");
                        return View(student);
                    }
                
                    if (!string.IsNullOrEmpty(student.PasswordHash))
                        student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(student.PasswordHash);
                    else
                        student.PasswordHash = existingStudent.PasswordHash;

                    await _userRepository.UpdateAsync(student);
                    TempData["Success"] = "Student updated successfully.";
                    return RedirectToAction(nameof(Students));
                }
                return View(student);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to update student: " + ex.Message;
                return View(student);
            }
        }

        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                var student = await _userRepository.GetByIdAsync(id);
                if (student == null)
                    return HandleNotFound($"Student with ID {id}", nameof(Students));

                await _userRepository.DeleteAsync(id);
                TempData["Success"] = "Student deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to delete student: " + ex.Message;
            }
            return RedirectToAction(nameof(Students));
        }

        public async Task<IActionResult> Books(string search, int page = 1)
        {
            try
            {
                int pageSize = 10;
                var paginatedBooks = await _bookRepository.GetPaginatedBooksAsync(search, page, pageSize);
                return View(paginatedBooks);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load books: " + ex.Message;
                return View(Enumerable.Empty<Book>());
            }
        }

        public IActionResult CreateBook() => View();

        [HttpPost]
        public async Task<IActionResult> CreateBook(Book book, IFormFile? ImageFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/books");
                        if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                        var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                        var filePath = Path.Combine(uploads, fileName);

                        using var stream = new FileStream(filePath, FileMode.Create);
                        await ImageFile.CopyToAsync(stream);
                        book.Image = "/images/books/" + fileName;
                    }

                    await _bookRepository.InsertAsync(book);
                    TempData["Success"] = "Book created successfully.";
                    return RedirectToAction(nameof(Books));
                }
                return View(book);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to create book: " + ex.Message;
                return View(book);
            }
        }

        public async Task<IActionResult> EditBook(int id)
        {
            try
            {
                var book = await _bookRepository.GetByIdAsync(id);
                if (book == null)
                    return HandleNotFound($"Book with ID {id}", nameof(Books));

                return View(book);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load book: " + ex.Message;
                return RedirectToAction(nameof(Books));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditBook(Book book, IFormFile? ImageFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingBook = await _bookRepository.GetByIdAsync(book.BookID);
                    if (existingBook == null)
                        return HandleNotFound($"Book with ID {book.BookID}", nameof(Books));

                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/books");
                        if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                        var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                        var filePath = Path.Combine(uploads, fileName);

                        using var stream = new FileStream(filePath, FileMode.Create);
                        await ImageFile.CopyToAsync(stream);

                        book.Image = "/images/books/" + fileName;
                    }
                    else
                    {
                        book.Image = existingBook.Image;
                    }

                    await _bookRepository.UpdateAsync(book);
                    TempData["Success"] = "Book updated successfully.";
                    return RedirectToAction(nameof(Books));
                }
                return View(book);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to update book: " + ex.Message;
                return View(book);
            }
        }

        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                var book = await _bookRepository.GetByIdAsync(id);
                if (book == null)
                    return HandleNotFound($"Book with ID {id}", nameof(Books));

                await _bookRepository.DeleteAsync(id);
                TempData["Success"] = "Book deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to delete book: " + ex.Message;
            }
            return RedirectToAction(nameof(Books));
        }

        public async Task<IActionResult> EditIssuedBook(int id)
        {
            try
            {
                var rent = await _rentRepository.GetByIdAsync(id);
                if (rent == null)
                    return HandleNotFound($"Issued book with ID {id}", nameof(IssuedBooks));

                return View(rent);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load issued book: " + ex.Message;
                return RedirectToAction(nameof(IssuedBooks));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditIssuedBook(Rent rent)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingRent = await _rentRepository.GetByIdAsync(rent.RentID);
                    if (existingRent == null)
                        return HandleNotFound($"Issued book with ID {rent.RentID}", nameof(IssuedBooks));

                    rent.ReturnDate = rent.IssueDate.AddDays(rent.Days);
                    await _rentRepository.UpdateAsync(rent);
                    TempData["Success"] = "Issued book updated successfully.";
                    return RedirectToAction(nameof(IssuedBooks));
                }
                return View(rent);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to update issued book: " + ex.Message;
                return View(rent);
            }
        }

        public async Task<IActionResult> DeleteIssuedBook(int id)
        {
            try
            {
                var rent = await _rentRepository.GetByIdAsync(id);
                if (rent == null)
                    return HandleNotFound($"Issued book with ID {id}", nameof(IssuedBooks));

                await _rentRepository.DeleteAsync(id);
                TempData["Success"] = "Issued book deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to delete issued book: " + ex.Message;
            }
            return RedirectToAction(nameof(IssuedBooks));
        }

        public async Task<IActionResult> IssueBook()
        {
            try
            {
                ViewBag.Students = await _userRepository.GetAllStudentsAsync();
                ViewBag.Books = await _bookRepository.GetAllAvailableAsync();
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load issue book page: " + ex.Message;
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> IssueBook(int studentId, int bookId, int days)
        {
            try
            {
                var student = await _userRepository.GetByIdAsync(studentId);
                var book = await _bookRepository.GetByIdAsync(bookId);

                if (student == null || book == null || book.AvailableQnt <= 0)
                {
                    TempData["Error"] = "Invalid student or book selection, or book not available.";
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

                TempData["Success"] = "Book issued successfully.";
                return RedirectToAction(nameof(IssuedBooks));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to issue book: " + ex.Message;
                return RedirectToAction(nameof(IssueBook));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReturnBook(int rentId)
        {
            try
            {
                var rent = await _rentRepository.GetByIdAsync(rentId);
                if (rent == null)
                    return HandleNotFound($"Issued book with ID {rentId}", nameof(IssuedBooks));

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

                TempData["Success"] = "Book returned successfully.";
                return RedirectToAction(nameof(IssuedBooks));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to return book: " + ex.Message;
                return RedirectToAction(nameof(IssuedBooks));
            }
        }

        public async Task<IActionResult> Penalties(string? status = "all", string? search = "", int page = 1)
        {
            try
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
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load penalties: " + ex.Message;
                return View(Enumerable.Empty<Penalty>());
            }
        }

        public async Task<IActionResult> EditPenalty(int id)
        {
            try
            {
                var penalty = await _penaltyRepository.GetByIdAsync(id);
                if (penalty == null)
                    return HandleNotFound($"Penalty with ID {id}", nameof(Penalties));

                return View(penalty);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load penalty: " + ex.Message;
                return RedirectToAction(nameof(Penalties));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditPenalty(Penalty penalty)
        {
            try
            {
                if (!ModelState.IsValid) return View(penalty);

                var existingPenalty = await _penaltyRepository.GetByIdAsync(penalty.PenaltyID);
                if (existingPenalty == null)
                    return HandleNotFound($"Penalty with ID {penalty.PenaltyID}", nameof(Penalties));

                await _penaltyRepository.UpdateAsync(penalty);
                TempData["Success"] = "Penalty updated successfully.";
                return RedirectToAction(nameof(Penalties));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to update penalty: " + ex.Message;
                return View(penalty);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeletePenalty(int id)
        {
            try
            {
                var penalty = await _penaltyRepository.GetByIdAsync(id);
                if (penalty == null)
                    return HandleNotFound($"Penalty with ID {id}", nameof(Penalties));

                await _penaltyRepository.DeleteAsync(id);
                TempData["Success"] = "Penalty deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to delete penalty: " + ex.Message;
            }
            return RedirectToAction(nameof(Penalties));
        }

        [HttpPost]
        public async Task<IActionResult> MarkPenaltyAsPaid(int id)
        {
            try
            {
                var penalty = await _penaltyRepository.GetByIdAsync(id);
                if (penalty == null)
                    return HandleNotFound($"Penalty with ID {id}", nameof(Penalties));

                await _penaltyRepository.MarkAsPaidAsync(id);
                TempData["Success"] = "Penalty marked as paid.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to mark penalty as paid: " + ex.Message;
            }
            return RedirectToAction(nameof(Penalties));
        }

        public async Task<IActionResult> IssuedBooks(string search, int page = 1)
        {
            try
            {
                int pageSize = 10;
                var paginatedRents = await _rentRepository.GetPaginatedIssuedBooksAsync(search, page, pageSize);
                return View(paginatedRents);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load issued books: " + ex.Message;
                return View(Enumerable.Empty<Rent>());
            }
        }
    }
}
