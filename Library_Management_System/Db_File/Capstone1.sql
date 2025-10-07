
CREATE TABLE Users4 (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,

    [Name] NVARCHAR(100) NULL,

    [Email] NVARCHAR(256) NOT NULL,
    [PasswordHash] NVARCHAR(256) NOT NULL, 

   [Role] NVARCHAR(50) NOT NULL 
        CONSTRAINT CK_Users_Role CHECK ([Role] IN ('Admin', 'Student'))
        DEFAULT 'Student',

    [Image] NVARCHAR(500) NULL,

    [CreatedAt] DATETIME NOT NULL DEFAULT(GETDATE()),


    CONSTRAINT UQ_Users_Email UNIQUE ([Email]),

   

    CONSTRAINT CK_Users_Email CHECK ([Email] LIKE '_%@_%._%')
);

select *from Users4

CREATE TABLE Branch4 (
    BranchID INT IDENTITY(1,1) PRIMARY KEY,
    BranchName NVARCHAR(50) NOT NULL
);


CREATE TABLE Publication4 (
    PublicationID INT IDENTITY(1,1) PRIMARY KEY,
    PublicationName NVARCHAR(100) NOT NULL,
    EntryDate DATETIME DEFAULT GETDATE()
);


CREATE TABLE Book4 (
    BookID INT IDENTITY(1,1) PRIMARY KEY,
    BookName NVARCHAR(50) NOT NULL,
    Author NVARCHAR(50),
    Detail NVARCHAR(500),
    Price FLOAT,
    PublicationID INT NULL, 
    BranchID INT NULL,     
    Quantities INT,
    AvailableQnt INT,
    RentQnt INT,
    Image NVARCHAR(1000),
    BookPDF NVARCHAR(1000),
    EntryDate DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Book_Publication FOREIGN KEY (PublicationID) REFERENCES Publication4(PublicationID),
    CONSTRAINT FK_Book_Branch FOREIGN KEY (BranchID) REFERENCES Branch4(BranchID)
);
alter table Book4
drop constraint FK_Book_Branch

CREATE TABLE Rent4 (
    RentID INT IDENTITY(1,1) PRIMARY KEY,
    BookID INT NOT NULL,     
    UserID INT NOT NULL,     
    Days INT,
    IssueDate DATETIME DEFAULT GETDATE(),
    ReturnDate DATETIME NULL,
    Status INT DEFAULT 1,    

    CONSTRAINT FK_Rent_User FOREIGN KEY (UserID) REFERENCES Users4(Id),
    CONSTRAINT FK_Rent_Book FOREIGN KEY (BookID) REFERENCES Book4(BookID)
);


CREATE TABLE Penalty4 (
    PenaltyID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,     
    BookID INT NOT NULL,     
    Price FLOAT,
    PenaltyAmount FLOAT,
    Detail NVARCHAR(500),
    EntryDate DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Penalty_User FOREIGN KEY (UserID) REFERENCES Users4(Id),
    CONSTRAINT FK_Penalty_Book FOREIGN KEY (BookID) REFERENCES Book4(BookID)
);
select *from Users4

select *from Book4
select *from Rent4
select *from Penalty4

ALTER TABLE Book4 ADD Publication NVARCHAR(255);
ALTER TABLE Book4 ADD Branch NVARCHAR(255);

ALTER TABLE Penalty4
ADD BookName NVARCHAR(200) NULL;

ALTER TABLE Penalty4 ADD IsPaid BIT NOT NULL DEFAULT 0;


ALTER TABLE Book4
ADD AvailableCopies INT NOT NULL DEFAULT 1;
sp_help Book4

drop table Branch4 
drop table Publication4

alter table book4 
drop column PublicationID,BranchID,BookPDF


-----------------------------------------------
select *from Users4
select *from Book4
select *from Rent4
select *from Penalty4
-----------------------------------------------

INSERT INTO Book4 
( BookName, Author, Detail, Price, Quantities, AvailableQnt, RentQnt, Image, EntryDate, Publication, Branch, AvailableCopies)
VALUES
( 'C# Basics', 'John Smith', 'Introduction to C# programming', 500, 10, 8, 2, 'csharp1.jpg', GETDATE(), 'TechPress', 'Computer Science', 8),
 ('ASP.NET Core', 'Jane Doe', 'Building web apps with ASP.NET Core', 600, 12, 10, 2, 'aspnet.jpg', GETDATE(), 'WebBooks', 'Computer Science', 10),
( 'SQL Fundamentals', 'Mike Johnson', 'Learn SQL from scratch', 400, 15, 15, 0, 'sql.jpg', GETDATE(), 'DB Press', 'Database', 15),
( 'Python for Beginners', 'Anna Lee', 'Python programming for beginners', 450, 20, 18, 2, 'python.jpg', GETDATE(), 'CodeBooks', 'Programming', 18),
( 'Java in Depth', 'Robert Brown', 'Advanced Java programming', 550, 8, 6, 2, 'java.jpg', GETDATE(), 'TechPress', 'Programming', 6),
('Data Structures', 'Emily Davis', 'Learn data structures and algorithms', 500, 10, 9, 1, 'datastruct.jpg', GETDATE(), 'AlgoBooks', 'Computer Science', 9),
( 'Machine Learning', 'Daniel Wilson', 'Introduction to machine learning concepts', 700, 5, 5, 0, 'ml.jpg', GETDATE(), 'AI Press', 'AI & ML', 5),
( 'JavaScript Essentials', 'Sophia Taylor', 'Modern JavaScript for web development', 450, 12, 12, 0, 'js.jpg', GETDATE(), 'WebBooks', 'Web Development', 12),
('React.js Guide', 'Liam Martinez', 'Build dynamic UI with React', 500, 10, 10, 0, 'react.jpg', GETDATE(), 'WebBooks', 'Web Development', 10),
('Algorithms', 'Olivia Anderson', 'Algorithm design and problem solving', 600, 7, 7, 0, 'algo.jpg', GETDATE(), 'AlgoBooks', 'Computer Science', 7);
