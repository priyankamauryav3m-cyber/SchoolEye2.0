-- =============================================================================================
-- Fee Collection receipt: V3M_FIN_GetStudentDetailsForFee also returns RegistrationNo
-- Change : one new column at the end of the result set (ISNULL(reg.RegistrationNo,'') AS RegistrationNo)
--          + LEFT JOIN Adm_Registration. Existing columns, filters and order are unchanged.
-- Callers: Infrastructure/FinanceMNGT/FeeMNGT/StudentFeeService.GetStudentDetailsForFeeAsync (Dapper, by name)
--          V3M_FIN_GetStudentStudentLedger only mentions it in a comment.
-- Rollback: V3M_FIN_GetStudentDetailsForFee_RegistrationNo_Rollback.sql
-- =============================================================================================
GO
-- EXEC V3M_FIN_GetStudentDetailsForFee 'CON0051','',1043,'01','01','2026-09-10',null    
    CREATE OR ALTER PROCEDURE dbo.V3M_FIN_GetStudentDetailsForFee    
    (    
     @ControlNo   VARCHAR(50) = '', @StudentName VARCHAR(200) = '', @SessionId VARCHAR(100), @GroupCode   VARCHAR(20),  
     @BranchCode  VARCHAR(20), @ReceiptDate Date = '', @UpToDate Date = null  
    )    
    AS    
    BEGIN    
        SET NOCOUNT ON;    
       SELECT ISNULL(vsd.SiblingId, '') AS SiblingId, vsd.StudentNo, vsd.StudentId, vsd.SessionId, vsd.SessionName, vsd.ControlNo, vsd.StudentName, vsd.ClassCode,  
            vsd.ClassName,vsd.SectionId,vsd.SectionName,vsd.ClassSection,vsd.FatherContactNo,vsd.FatherName,  
           vsd.MotherName, vsd.ClassOrder, vsd.PeriodType, vsd.SocietyId, vsd.SMSMobileNo, ISNULL(vfsb.Balance, 0) AS PreviousBalance,   
            dbo.Fn_GetStudentSmallAddress( vsd.GroupCode, vsd.BranchCode, 'C', vsd.StudentId ) AS StudentAddress,    
            dbo.V3M_FIN_CalculateLateFee( vsd.GroupCode, vsd.BranchCode, vsd.SessionId, vsd.StudentId, @ReceiptDate, @UpToDate ) AS LateFee,    
            dbo.Fn_GetStudentBalance( vsd.GroupCode, vsd.BranchCode, vsd.SessionId, vsd.StudentId ) AS StudentBalance,    
            ( SELECT COUNT(*) FROM StudentMaster sm WHERE sm.SiblingID = vsd.SiblingID AND sm.IsValid = 1 AND sm.StudentId <> vsd.StudentId ) AS SiblingCount,     
            vsd.HostalRequired,    
            vsd.IsValid,    
            (    
                SELECT TOP 1 mst.Concession    
                FROM V3M_FIN_DetConcessionMapToStudent dt    
                INNER JOIN V3M_FIN_MstConcession mst    
                    ON mst.ConcessionId = dt.ConcessionId    
                   AND dt.GroupCode COLLATE DATABASE_DEFAULT = mst.GroupCode COLLATE DATABASE_DEFAULT    
                   AND dt.BranchCode  COLLATE DATABASE_DEFAULT= mst.BranchCode COLLATE DATABASE_DEFAULT    
                WHERE dt.GroupCode COLLATE DATABASE_DEFAULT = vsd.GroupCode COLLATE DATABASE_DEFAULT    
                  AND dt.BranchCode  COLLATE DATABASE_DEFAULT= vsd.BranchCode COLLATE DATABASE_DEFAULT    
                  AND dt.SessionId = vsd.SessionId    
                  AND dt.Status = '1'    
                  AND dt.StudentId = vsd.StudentId    
            ) AS ConcessionName,    
            ISNULL(vsd.IsTransportRequired, 0) AS IsTransportRequired,    
           vsd.DistanceName,vsd.NextToKin,vsd.NextToKinLabel,vsd.NextToKinName,vsd.NextToKinEmail,vsd.NextToKinContactNo,
           ISNULL(reg.RegistrationNo, '') AS RegistrationNo   -- fee receipt: Registration No.  
        FROM vwStudentMiniDetails vsd
        LEFT JOIN Adm_Registration reg
            ON reg.RegistrationId = TRY_CAST(vsd.RegistrationId AS BIGINT)    
        LEFT JOIN V3M_Fin_StudentBalance vfsb    
            ON vfsb.GroupCode = vsd.GroupCode  AND vfsb.BranchCode = vsd.BranchCode    
           AND vfsb.StudentId = vsd.StudentId AND vfsb.SessionId = vsd.SessionId  AND vfsb.IsValid = 1    
        WHERE vsd.GroupCode = @GroupCode    
          AND vsd.BranchCode = @BranchCode AND vsd.SessionId = @SessionId    
          AND    
          (    
              (    
                  ISNULL(LTRIM(RTRIM(@ControlNo)), '') <> '' AND vsd.ControlNo = LTRIM(RTRIM(@ControlNo))    
              )    
              OR    
              (    
                  ISNULL(LTRIM(RTRIM(@ControlNo)), '') = '' AND vsd.StudentName LIKE '%' + LTRIM(RTRIM(@StudentName)) + '%'    
              )    
          )    
        ORDER BY    
            vsd.ClassOrder,vsd.StudentName;    
    END
GO
