DECLARE @Email NVARCHAR(256) = 'tu-correo@ejemplo.com';

UPDATE dbo.Users
SET Role = 2
WHERE Email = LOWER(@Email);

SELECT Id, FirstName, LastName, Email, Role, IsActive
FROM dbo.Users
WHERE Email = LOWER(@Email);
