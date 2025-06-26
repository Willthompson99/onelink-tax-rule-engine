UPDATE Claims
SET ClaimTypeID = 3  -- Health
WHERE ClaimID = 1;

UPDATE Claims
SET ClaimTypeID = 1  -- Auto
WHERE ClaimID = 2;

UPDATE Claims
SET ClaimTypeID = 4  -- Travel
WHERE ClaimID = 3;
