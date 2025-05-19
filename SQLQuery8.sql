SELECT MenuItemID, ItemName, Course, CategoryID FROM MenuItems;

SELECT * FROM MenuCategories;

SELECT DISTINCT Course FROM MenuItems;

SELECT m.ItemName, m.Course, c.CategoryName
FROM MenuItems m
JOIN MenuCategories c ON m.CategoryID = c.CategoryID;

SELECT 
    MI.MenuItemID,
    MI.ItemName,
    MI.Course,
    MC.CategoryName,
    MI.IsActive
FROM MenuItems MI
JOIN MenuCategories MC ON MI.CategoryID = MC.CategoryID
WHERE LOWER(MI.Course) = 'dessert'
  AND LOWER(MC.CategoryName) = 'dinner'
  AND MI.IsActive = 1;

  ALTER TABLE MenuItems
ADD IsActive BIT NOT NULL DEFAULT 1;
