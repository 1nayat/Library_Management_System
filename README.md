Library Management System

A web-based Library Management System built using ASP.NET Core that supports role-based access control and a penalty-based book return mechanism.
This system enables efficient management of books, users, borrowing records, and fine calculation while maintaining clear separation of responsibilities through a layered structure.

Features
Role-Based Access Control (Admin / User)
Book Management (Add, Update, Delete, View)
Borrow and Return Book Workflow
Penalty Calculation for Late Returns
Pagination for large datasets
Authentication and Authorization
Repository Pattern Implementation
Clean Project Structure
Role-Based Access Control (RBAC) 

Admin
Manage books (Create, Update, Delete)
View all users
View borrowing history
Manage penalties
System-level control

User
Browse available books
Borrow books
Return books
View personal borrowing history
View penalty status
Authorization is enforced using ASP.NET Core Identity roles.
Penalty-Based System
Each borrowed book has a due date.
If the return date exceeds the due date:
A penalty is calculated based on overdue days.
Penalty is stored in the database.
Users can view outstanding penalties.

Example logic:
Overdue Days = ReturnDate − DueDate
Fine = OverdueDays × FixedDailyRate

Tech Stack
ASP.NET Core MVC
C#
Entity Framework Core
SQL Server
ASP.NET Core Identity
Razor Views
Ajax
