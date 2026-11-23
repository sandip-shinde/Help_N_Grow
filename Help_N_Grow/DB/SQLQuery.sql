
-- Switch to the new database
USE GrowUp_Help;
GO

CREATE TABLE dbo.Registration (
    Reg_Id INT IDENTITY(1,1) PRIMARY KEY,
    Full_Name NVARCHAR(200) NOT NULL,
    Mobile_No DECIMAL(18,0) NOT NULL,
    Bank_AC DECIMAL(18,0) NULL,
    IFSC_Code NVARCHAR(50) NULL,
    UPI_ID NVARCHAR(100) NULL,
    Password NVARCHAR(200) NOT NULL,
    Term_Condition_Accepted BIT NOT NULL,
    Role NVARCHAR(50) NULL,
    UserName NVARCHAR(100) NOT NULL,
    Reg_Date DATE NOT NULL,
    Level_Id INT NULL,
    Parent_Id INT NULL,
    Is_Active BIT NOT NULL DEFAULT 1,
    Package_ID INT NULL,
    Package_Name NVARCHAR(100) NULL
);
CREATE TABLE dbo.Level (
    Level_Id INT IDENTITY(1,1) PRIMARY KEY,
    Level_Name NVARCHAR(100) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Company_Percentage DECIMAL(18,2) NOT NULL,
    Self_Amount DECIMAL(18,2) NOT NULL,
    Package_ID INT NOT NULL,
    Package_Name NVARCHAR(100) NOT NULL,
    MemberCount INT NOT NULL DEFAULT 0,
    Branchfee INT NOT NULL DEFAULT 0
);

CREATE TABLE dbo.TblTransaction (
    Transaction_ID INT IDENTITY(1,1) PRIMARY KEY,
    Upload_Path NVARCHAR(500) NOT NULL,
    Transaction_No NVARCHAR(100) NOT NULL,
    Is_Approved BIT NOT NULL DEFAULT 0,
    Reg_Id INT NULL,
    Level_Id INT NULL,
    Parent_Id INT NULL,
    Transaction_NoSelf NVARCHAR(100) NOT NULL,
    Upload_PathSelf NVARCHAR(500) NOT NULL,
    Self_Amount DECIMAL(18,2) NULL,
    Company_Percentage DECIMAL(18,2) NULL,
    T_Date DATE NULL,
    MemberTransactionPhoto VARBINARY(MAX) NULL,
    CompanyTransactionPhoto VARBINARY(MAX) NULL,
    CompanyTransactionPhotoContentType NVARCHAR(100) NULL,
    MemberTransactionPhotoContentType NVARCHAR(100) NULL,
    Note NVARCHAR(MAX) NULL,
    Package_ID INT NULL,
    Package_Name NVARCHAR(200) NULL
);


-- Insert into Registration
INSERT INTO dbo.Registration (Full_Name, Mobile_No, Bank_AC, IFSC_Code, UPI_ID, Password, Term_Condition_Accepted, Role, UserName, Reg_Date, Level_Id, Parent_Id, Is_Active, Package_ID, Package_Name)
VALUES ('Super_Admin', 9872424210, 1234567890, 'HDFC0001234', 'Super_Admin@upi', 'Super_Admin', 1, 'Super_Admin', 'Super_Admin', GETDATE(), 1, 0, 1, 1, 'Basic Package');

-- Insert into Registration
INSERT INTO dbo.Registration (Full_Name, Mobile_No, Bank_AC, IFSC_Code, UPI_ID, Password, Term_Condition_Accepted, Role, UserName, Reg_Date, Level_Id, Parent_Id, Is_Active, Package_ID, Package_Name)
VALUES ('Admin', 9872424210, 1234567890, 'HDFC0001234', 'Admin@upi', 'Admin', 1, 'Admin', 'Admin', GETDATE(), 1, 0, 1, 1, 'Basic Package');

-- Insert into Registration
INSERT INTO dbo.Registration (Full_Name, Mobile_No, Bank_AC, IFSC_Code, UPI_ID, Password, Term_Condition_Accepted, Role, UserName, Reg_Date, Level_Id, Parent_Id, Is_Active, Package_ID, Package_Name)
VALUES ('member Kumar', 9872424210, 1234567890, 'HDFC0001234', 'member@upi', 'member', 1, 'member', 'member', GETDATE(), 1, 1, 1, 1, 'Basic Package');
-- Insert into Level
INSERT INTO dbo.Level (Level_Name, Amount, Company_Percentage, Self_Amount, Package_ID, Package_Name, MemberCount, Branchfee)
VALUES ('Starter Level', 1000.00, 500, 500, 1, 'Basic Package', 3, 100);
-- Insert into Level
INSERT INTO dbo.Level (Level_Name, Amount, Company_Percentage, Self_Amount, Package_ID, Package_Name, MemberCount, Branchfee)
VALUES ('Medium Level', 1500.00, 750, 750, 1, 'Basic Package', 3, 0);
-- Insert into Level
INSERT INTO dbo.Level (Level_Name, Amount, Company_Percentage, Self_Amount, Package_ID, Package_Name, MemberCount, Branchfee)
VALUES ('Advanced Level', 2250.00, 1125, 1125, 1, 'Basic Package', 3, 0);