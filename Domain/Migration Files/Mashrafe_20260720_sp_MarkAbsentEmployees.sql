USE [HRM];

-- EXEC dbo.sp_MarkAbsentEmployees @AttendanceDate = '2026-07-20';
GO
CREATE OR ALTER PROCEDURE sp_MarkAbsentEmployees
    @AttendanceDate DATE
    AS
BEGIN
    SET NOCOUNT ON;

INSERT INTO Attendance( CompanyId, EmployeeId, AttendanceDate, [Status], CreatedAt)
SELECT e.CompanyId, e.EmployeeId, @AttendanceDate, 'Absent', GETDATE()
FROM Employee e
WHERE e.IsActive = 1
  AND NOT EXISTS (
    SELECT 1 FROM Attendance a
    WHERE a.EmployeeId = e.EmployeeId AND a.AttendanceDate = @AttendanceDate
)
  AND NOT EXISTS (
    SELECT 1 FROM LeaveRequest l
    WHERE l.EmployeeId = e.EmployeeId AND l.Status = 'Approved'
      AND @AttendanceDate BETWEEN l.FromDate AND l.ToDate
);
END
GO