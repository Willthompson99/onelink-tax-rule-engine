USE OneLink;

CREATE TABLE ClaimTypes (
    ClaimTypeID INT PRIMARY KEY IDENTITY(1,1),
    TypeName NVARCHAR(50) NOT NULL
);

INSERT INTO ClaimTypes (TypeName)
VALUES 
    ('Auto'),
    ('Home'),
    ('Health'),
    ('Travel'),
    ('Life');
