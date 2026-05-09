-- =============================================
-- Employee Task & Attendance Management (MVP)
-- Fresh SQL Server Create Script
-- Includes:
-- 5 tables exactly as requested
-- relationships
-- performance indexes
-- 5 enum-style CHECK constraints
-- =============================================

-- Optional: create/use database
-- IF DB_ID(N'EmployeeTaskAttendanceDB') IS NULL
--     CREATE DATABASE EmployeeTaskAttendanceDB;
-- GO
-- USE EmployeeTaskAttendanceDB;
-- GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Drop in dependency order (for reruns)
IF OBJECT_ID(N'dbo.Leaves', N'U') IS NOT NULL DROP TABLE dbo.Leaves;
IF OBJECT_ID(N'dbo.Tasks', N'U') IS NOT NULL DROP TABLE dbo.Tasks;
IF OBJECT_ID(N'dbo.Attendance', N'U') IS NOT NULL DROP TABLE dbo.Attendance;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID(N'dbo.Departments', N'U') IS NOT NULL DROP TABLE dbo.Departments;
GO

-- 1) Departments
CREATE TABLE dbo.Departments
(
    Id    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name  NVARCHAR(100) NOT NULL
);
GO

CREATE UNIQUE INDEX UX_Departments_Name
ON dbo.Departments(Name);
GO

-- 2) Users
CREATE TABLE dbo.Users
(
    Id            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name          NVARCHAR(150) NOT NULL,
    Email         NVARCHAR(256) NOT NULL,
    PasswordHash  NVARCHAR(500) NOT NULL,
    Role          NVARCHAR(20) NOT NULL,      -- Enum #1
    DepartmentId  INT NULL,

    CONSTRAINT FK_Users_Departments
        FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(Id)
        ON DELETE SET NULL
        ON UPDATE NO ACTION,

    CONSTRAINT CK_Users_Role
        CHECK (Role IN (N'Admin', N'Manager', N'Employee'))  -- Enum #1
);
GO

CREATE UNIQUE INDEX UX_Users_Email
ON dbo.Users(Email);
GO

CREATE INDEX IX_Users_DepartmentId
ON dbo.Users(DepartmentId);
GO

CREATE INDEX IX_Users_Role
ON dbo.Users(Role);
GO

-- 3) Attendance
CREATE TABLE dbo.Attendance
(
    Id           BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    EmployeeId   INT NOT NULL,
    CheckIn      DATETIME2(0) NOT NULL,
    CheckOut     DATETIME2(0) NULL,
    WorkingHours DECIMAL(5,2) NULL,
    Status       NVARCHAR(20) NOT NULL,       -- Enum #2
    [Date]       DATE NOT NULL,

    CONSTRAINT FK_Attendance_Users
        FOREIGN KEY (EmployeeId) REFERENCES dbo.Users(Id)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,

    CONSTRAINT CK_Attendance_Status
        CHECK (Status IN (N'Present', N'Absent', N'Late', N'HalfDay', N'OnLeave')) -- Enum #2
);
GO

CREATE UNIQUE INDEX UX_Attendance_Employee_Date
ON dbo.Attendance(EmployeeId, [Date]);
GO

CREATE INDEX IX_Attendance_Date_Status
ON dbo.Attendance([Date], Status);
GO

-- 4) Tasks
CREATE TABLE dbo.Tasks
(
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Title       NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    AssignedTo  INT NULL,
    AssignedBy  INT NOT NULL,
    Priority    NVARCHAR(20) NOT NULL,        -- Enum #3
    Status      NVARCHAR(20) NOT NULL,        -- Enum #4
    Deadline    DATETIME2(0) NULL,
    CreatedAt   DATETIME2(0) NOT NULL CONSTRAINT DF_Tasks_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT FK_Tasks_AssignedTo_Users
        FOREIGN KEY (AssignedTo) REFERENCES dbo.Users(Id)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,

    CONSTRAINT FK_Tasks_AssignedBy_Users
        FOREIGN KEY (AssignedBy) REFERENCES dbo.Users(Id)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,

    CONSTRAINT CK_Tasks_Priority
        CHECK (Priority IN (N'Low', N'Medium', N'High')),    -- Enum #3

    CONSTRAINT CK_Tasks_Status
        CHECK (Status IN (N'Pending', N'InProgress', N'Completed', N'Blocked')) -- Enum #4
);
GO

CREATE INDEX IX_Tasks_AssignedTo_Status
ON dbo.Tasks(AssignedTo, Status);
GO

CREATE INDEX IX_Tasks_Deadline
ON dbo.Tasks(Deadline);
GO

CREATE INDEX IX_Tasks_AssignedBy
ON dbo.Tasks(AssignedBy);
GO

-- 5) Leaves
CREATE TABLE dbo.Leaves
(
    Id         BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    EmployeeId INT NOT NULL,
    LeaveType  NVARCHAR(30) NOT NULL,
    StartDate  DATE NOT NULL,
    EndDate    DATE NOT NULL,
    Reason     NVARCHAR(500) NULL,
    Status     NVARCHAR(20) NOT NULL,         -- Enum #5

    CONSTRAINT FK_Leaves_Users
        FOREIGN KEY (EmployeeId) REFERENCES dbo.Users(Id)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,

    CONSTRAINT CK_Leaves_Status
        CHECK (Status IN (N'Pending', N'Approved', N'Rejected')) -- Enum #5
);
GO

CREATE INDEX IX_Leaves_EmployeeId_StartDate
ON dbo.Leaves(EmployeeId, StartDate);
GO

CREATE INDEX IX_Leaves_Status
ON dbo.Leaves(Status);
GO