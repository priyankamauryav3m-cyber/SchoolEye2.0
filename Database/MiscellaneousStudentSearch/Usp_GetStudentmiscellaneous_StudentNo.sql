-- Usp_GetStudentmiscellaneous: search by Student No. without class / section
-- Only caller: StudentMiscellaneous/GetStudentMiscellaneousData (/miscellaneousinvoice)
-- - @ControlNo given  -> class / section optional (used as filter only when passed)
-- - @ControlNo empty  -> class and section mandatory (same as before)
-- Rollback: Usp_GetStudentmiscellaneous_Rollback.sql
--
-- EXEC dbo.Usp_GetStudentmiscellaneous '01', '01', 1043, '', '', '', '0101CON0363,0101CON0387', '', '', '-1';
-- EXEC dbo.Usp_GetStudentmiscellaneous '01', '01', 1043, 'Class1', '18', '', '', '', '', '-1';
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE [dbo].[Usp_GetStudentmiscellaneous]
(
    @GroupCode   VARCHAR(20),
    @BranchCode  VARCHAR(20),
    @SessionId   BIGINT,
    @ClassCode   VARCHAR(MAX),
    @SectionCode VARCHAR(MAX),
    @Gender      VARCHAR(20) = '',
    @ControlNo   VARCHAR(MAX) = '',
    @StudentName NVARCHAR(200) = '',
    @IsEWS       VARCHAR(10) = '',
    @IsValid     VARCHAR(10) = '-1'
)
AS
BEGIN
    SET NOCOUNT ON;

    SET @ClassCode   = LTRIM(RTRIM(ISNULL(@ClassCode, '')));
    SET @SectionCode = LTRIM(RTRIM(ISNULL(@SectionCode, '')));
    SET @ControlNo   = LTRIM(RTRIM(ISNULL(@ControlNo, '')));

    -- class + section are mandatory only when no student no. is given
    IF @ControlNo = ''
    BEGIN
        IF @ClassCode = ''
            THROW 50001, 'ClassCode is mandatory.', 1;

        IF @SectionCode = ''
            THROW 50002, 'SectionCode is mandatory.', 1;
    END;

    SELECT
        vsd.StudentNo,
        Vsd.StudentId,
        vsd.ControlNo,
        vsd.AdmissionNo,
        vsd.StudentName,
        vsd.Gender,
        vsd.DateOfBirth,
        vsd.ClassCode,
        vsd.ClassName,
        vsd.ClassSection,
        vsd.SectionId,
        vsd.SectionName,
        vsd.RollNo,
        vsd.SMSMobileNo,
        vsd.FatherName,
        vsd.FatherContactNo,
        vsd.MotherName,
        vsd.MotherContactNo,
        vsd.IsReservedSeat,
        vsd.ImagePath,
        vsd.MotherImagePath,
        vsd.FatherImagePath,
        vsd.AdmissionDate,
        ISNULL(vsd.AdmClass, '') AS AdmClass,
        ISNULL(vsd.AdmSession, '') AS AdmSession,
        vsd.BoardRollNo,
        ISNULL(vsd.StudentCategoryName, '') AS StudentCategoryName,
        vsd.SocietyId,
        ISNULL(vsd.AadhaarNo, '') AS AadhaarNo,
        vsd.ReligionName,
        svd.Visitor1ImagePath,
        svd.Visitor2ImagePath,
        svd.Visitor3ImagePath,
        svd.Visitor4ImagePath
    FROM dbo.vwStudentDetails AS vsd
    LEFT JOIN dbo.StudentVisitorsDetails AS svd
        ON vsd.StudentId = svd.StudentId
    WHERE
        vsd.GroupCode COLLATE DATABASE_DEFAULT =
            @GroupCode COLLATE DATABASE_DEFAULT
        AND vsd.BranchCode COLLATE DATABASE_DEFAULT =
            @BranchCode COLLATE DATABASE_DEFAULT
        AND vsd.SessionId = @SessionId
        AND
        (
            @ClassCode = ''
            OR vsd.ClassCode COLLATE DATABASE_DEFAULT IN
            (
                SELECT LTRIM(RTRIM(data)) COLLATE DATABASE_DEFAULT
                FROM dbo.fn_SplitString(@ClassCode, ',')
                WHERE NULLIF(LTRIM(RTRIM(data)), '') IS NOT NULL
            )
        )
        AND
        (
            @SectionCode = ''
            OR CAST(vsd.SectionId AS VARCHAR(50)) COLLATE DATABASE_DEFAULT IN
            (
                SELECT LTRIM(RTRIM(data)) COLLATE DATABASE_DEFAULT
                FROM dbo.fn_SplitString(@SectionCode, ',')
                WHERE NULLIF(LTRIM(RTRIM(data)), '') IS NOT NULL
            )
        )
        AND
        (
            NULLIF(LTRIM(RTRIM(@Gender)), '') IS NULL
            OR vsd.Gender COLLATE DATABASE_DEFAULT =
               @Gender COLLATE DATABASE_DEFAULT
        )
        AND
        (
            NULLIF(LTRIM(RTRIM(@IsEWS)), '') IS NULL
            OR CAST(vsd.IsReservedSeat AS VARCHAR(10)) COLLATE DATABASE_DEFAULT =
               @IsEWS COLLATE DATABASE_DEFAULT
        )
        AND
        (
            @ControlNo = ''
            OR vsd.ControlNo COLLATE DATABASE_DEFAULT IN
            (
                SELECT LTRIM(RTRIM(data)) COLLATE DATABASE_DEFAULT
                FROM dbo.fn_SplitString(@ControlNo, ',')
                WHERE NULLIF(LTRIM(RTRIM(data)), '') IS NOT NULL
            )
        )
        AND
        (
            NULLIF(LTRIM(RTRIM(@StudentName)), '') IS NULL
            OR vsd.StudentName COLLATE DATABASE_DEFAULT LIKE
               LTRIM(RTRIM(@StudentName)) COLLATE DATABASE_DEFAULT + '%'
        )
        AND
        (
            ISNULL(@IsValid, '') = ''
            OR @IsValid = '-1'
            OR vsd.IsValid = TRY_CONVERT(INT, @IsValid)
        )
    ORDER BY
        vsd.ClassOrder,
        vsd.SectionName,
        TRY_CONVERT(INT, vsd.RollNo),
        vsd.StudentName;
END;
GO
