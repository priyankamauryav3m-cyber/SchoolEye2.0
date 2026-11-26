/* =====================================================================================
   DPDP - Student Sensitive Details
   04. V3M_DPDP_GetStudentSensitiveHistory

   Change history of a student's sensitive fields (written by USP_DPDP_UpdateStudentSensitiveDetails
   through dbo.Usp_SaveTransactionLog). Shown only on the Student Sensitive Details page.

   @FieldKey NULL = every field. The save's reason (ColumnName 'UpdateReason', written last in
   the same save) is returned on every row of that save; the reason row itself is not listed.
   ===================================================================================== */
CREATE OR ALTER PROCEDURE dbo.V3M_DPDP_GetStudentSensitiveHistory
    @GroupCode  VARCHAR(10),
    @BranchCode VARCHAR(10),
    @StudentId  BIGINT,
    @FieldKey   VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @RefId VARCHAR(50) = CAST(@StudentId AS VARCHAR(50));
    SET @FieldKey = NULLIF(LTRIM(RTRIM(@FieldKey)), '');

    SELECT TOP (500)
           L.LogId,
           L.ColumnName      AS FieldKey,
           L.TableName,
           L.OldColumnValue  AS OldValue,
           L.NewColumnValue  AS NewValue,
           L.TransBy         AS ChangedBy,
           L.IP,
           L.TransDatetime   AS ChangedOn,
           R.NewColumnValue  AS Reason
    FROM dbo.Det_TransactionLog L WITH (NOLOCK)
    OUTER APPLY (SELECT TOP (1) X.NewColumnValue
                 FROM dbo.Det_TransactionLog X WITH (NOLOCK)
                 WHERE X.ReferenceKey  = L.ReferenceKey
                   AND X.ReferenceId   = L.ReferenceId
                   AND X.ColumnName    = 'UpdateReason'
                   AND X.LogId         > L.LogId          -- the reason row is written last in the same save
                 ORDER BY X.LogId ASC) R
    WHERE L.ReferenceKey = 'DPDP_StudentSensitive'
      AND L.ReferenceId  = @RefId
      AND L.ColumnName  <> 'UpdateReason'
      AND (@FieldKey IS NULL OR L.ColumnName = @FieldKey)
      -- only a student of the caller's group / branch (empty list otherwise)
      AND EXISTS (SELECT 1 FROM dbo.StudentMaster S WITH (NOLOCK)
                  WHERE S.StudentId = @StudentId AND S.GroupCode = @GroupCode AND S.BranchCode = @BranchCode)
    ORDER BY L.LogId DESC;
END
GO
