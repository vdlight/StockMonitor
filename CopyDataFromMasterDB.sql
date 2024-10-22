-- Ensure you are in the context of the target database
USE Simulations;

-- Step 1: Clear existing data from the tables
DELETE FROM dbo.history;
DELETE FROM dbo.stockData;

-- Step 2: Enable IDENTITY_INSERT for stockData
SET IDENTITY_INSERT dbo.stockData ON;

-- Step 3: Insert into stockData including the ID from master
INSERT INTO dbo.stockData (ID, Name, MA200, Price, OwnedCnt, PurPrice, List)
SELECT ID, Name, MA200, Price, OwnedCnt, PurPrice, List
FROM [master].[dbo].[stockData];

-- Step 4: Disable IDENTITY_INSERT for stockData
SET IDENTITY_INSERT dbo.stockData OFF;

-- Step 5: Now insert into history table
INSERT INTO dbo.history (Date, Price, MA200, StockId, OwnedCnt)
SELECT h.Date, h.Price, h.MA200, mapping.ID AS StockId, h.OwnedCnt
FROM [master].[dbo].[history] h
JOIN dbo.stockData mapping ON h.StockId = mapping.ID;  -- Assuming StockId is the ID in stockData