SELECT 
    c.ClaimID,
    c.ClaimantName,
    c.ClaimAmount,
    c.ClaimDate,
    ct.TypeName AS ClaimType,
    c.Status
FROM Claims c
JOIN ClaimTypes ct ON c.ClaimTypeID = ct.ClaimTypeID;
