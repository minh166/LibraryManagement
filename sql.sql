-- =========================
-- CREATE DATABASE
-- =========================
CREATE DATABASE Librarydb;
GO

USE Librarydb;
GO

-- =========================
-- USERS
-- =========================
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    Role INT NOT NULL, -- 1=Admin, 2=Librarian, 3=Borrower
    FullName NVARCHAR(150) NULL,
    Email NVARCHAR(150) NULL,
    Phone NVARCHAR(20) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- =========================
-- CATEGORIES
-- =========================
CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL
);

-- =========================
-- BOOKS
-- =========================
CREATE TABLE Books (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(255) NOT NULL,
    Author NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    CategoryId INT NOT NULL,
    TotalQuantity INT NOT NULL DEFAULT 0,
    AvailableQuantity INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Books_Categories 
        FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- =========================
-- BORROW RECORDS
-- =========================
CREATE TABLE BorrowRecords (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    BookId INT NOT NULL,
    BorrowDate DATETIME NOT NULL DEFAULT GETDATE(),
    DueDate DATETIME NOT NULL,
    ReturnDate DATETIME NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',

    CONSTRAINT FK_Borrow_User 
        FOREIGN KEY (UserId) REFERENCES Users(Id),

    CONSTRAINT FK_Borrow_Book 
        FOREIGN KEY (BookId) REFERENCES Books(Id),

    CONSTRAINT CHK_Borrow_Status 
        CHECK (Status IN ('Pending', 'Borrowing', 'Returned', 'Overdue'))
);

-- =========================
-- FINES
-- =========================
CREATE TABLE Fines (
    Id INT PRIMARY KEY IDENTITY(1,1),
    BorrowRecordId INT NOT NULL UNIQUE,
    Amount DECIMAL(10,2) NOT NULL DEFAULT 0,
    IsPaid BIT NOT NULL DEFAULT 0,
    PaidDate DATETIME NULL,

    CONSTRAINT FK_Fines_Borrow 
        FOREIGN KEY (BorrowRecordId) REFERENCES BorrowRecords(Id)
);

-- =========================
-- SAMPLE DATA
-- =========================

-- Users
INSERT INTO Users (Username, Password, Role, FullName)
VALUES 
('admin', '123', 1, N'Admin User'),
('librarian', '123', 2, N'Thủ thư'),
('user1', '123', 3, N'Người dùng 1');

-- Categories
INSERT INTO Categories (Name) VALUES
(N'Văn học'),
(N'Công nghệ'),
(N'Lịch sử');

-- Books
INSERT INTO Books (Title, Author, CategoryId, TotalQuantity, AvailableQuantity)
VALUES
(N'Clean Code', N'Robert C. Martin', 2, 5, 5),
(N'Lập trình C#', N'Nguyễn Văn A', 2, 3, 3),
(N'Sapiens', N'Yuval Noah Harari', 3, 4, 4);