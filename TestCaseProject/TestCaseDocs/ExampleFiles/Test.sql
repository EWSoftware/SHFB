-- SQL Example
Select  *
From    tblFirstTable
Where   Field1 = 'A'

-- #region SQL Snippet With a Longer Name
-- This snippet should not be matched for the one below
Select  *
From    tblSecondTable
Where   Field2 = 'B'
-- #endregion

-- #region SQL Snippet
Select  *
From    tblTest
Where   Field = 'X'
-- #endregion
