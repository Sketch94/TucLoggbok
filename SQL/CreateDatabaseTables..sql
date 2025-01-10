CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),   -- Auto-incrementerande primärnyckel
    Username VARCHAR(50),                     -- Kolumn för användarnamn
    Password VARCHAR(255),                    -- Kolumn för lösenord (kan hantera hashade lösenord)
    UserType VARCHAR(20),                     -- Kolumn för användartyp (t.ex. 'User' eller 'Admin')
    Email VARCHAR(100),                       -- Kolumn för användarens e-postadress
    Phone VARCHAR(20)                         -- Kolumn för användarens telefonnummer
);

CREATE TABLE Books (
    BookID INT PRIMARY KEY IDENTITY(1,1),    -- Auto-incrementerande primärnyckel
    Title VARCHAR(100),                       -- Kolumn för bokens titel
    Author VARCHAR(50),                       -- Kolumn för bokens författare
    ISBN VARCHAR(50),                         -- Kolumn för ISBN
    Status VARCHAR(20)                        -- Kolumn för bokens status (t.ex. 'Tillgänglig' eller 'Utlånad')
);

CREATE TABLE Borrowing (
    BorrowingID INT PRIMARY KEY IDENTITY(1,1), -- Auto-incrementerande primärnyckel
    UserID INT,                         -- Kolumn för användar-ID (referens till Users)
    BookID INT,                         -- Kolumn för bok-ID (referens till Books)
    BorrowDate DATETIME,                    -- Datum när boken lånades
    ReturnDate DATETIME,                        -- Datum när boken återlämnades (kan vara NULL)
);
