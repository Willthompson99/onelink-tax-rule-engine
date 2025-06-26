USE OneLink;

CREATE TABLE Claims (
    ClaimID INT PRIMARY KEY IDENTITY(1,1),
    ClaimantName NVARCHAR(100),
    ClaimAmount DECIMAL(10, 2),
    ClaimDate DATE,
    Status NVARCHAR(50)
);
