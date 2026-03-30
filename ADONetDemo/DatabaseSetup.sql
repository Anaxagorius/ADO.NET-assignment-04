-- =============================================================================
-- DatabaseSetup.sql
-- Creates the ADONetDemo database and seeds it with sample data.
-- Run this script against a SQL Server LocalDB or full SQL Server instance
-- before starting the WPF application.
-- =============================================================================

USE master;
GO

-- Create the database if it does not exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ADONetDemo')
BEGIN
    CREATE DATABASE ADONetDemo;
END
GO

USE ADONetDemo;
GO

-- -------------------------
-- Products table
-- -------------------------
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products (
        ProductId INT           IDENTITY(1,1) PRIMARY KEY,
        Name      NVARCHAR(100) NOT NULL,
        Category  NVARCHAR(50)  NOT NULL,
        Price     DECIMAL(10,2) NOT NULL DEFAULT 0.00,
        Stock     INT           NOT NULL DEFAULT 0
    );
END
GO

-- Seed data (idempotent – only inserts if table is empty)
IF NOT EXISTS (SELECT TOP 1 1 FROM Products)
BEGIN
    INSERT INTO Products (Name, Category, Price, Stock) VALUES
        ('Laptop Pro 15',      'Electronics',   1299.99, 42),
        ('Wireless Mouse',     'Accessories',     29.95, 150),
        ('USB-C Hub 7-Port',   'Accessories',     49.99,  85),
        ('4K Monitor 27"',     'Electronics',    449.00,  30),
        ('Mechanical Keyboard','Accessories',     89.95, 120),
        ('Webcam 1080p',       'Electronics',     79.99,  60),
        ('Office Chair',       'Furniture',      349.00,  15),
        ('Standing Desk',      'Furniture',      599.00,   8),
        ('Desk Lamp LED',      'Accessories',     35.00,  75),
        ('Noise-Cancel Headphones','Electronics', 249.99, 55);
END
GO

PRINT 'ADONetDemo database setup complete.';
GO
