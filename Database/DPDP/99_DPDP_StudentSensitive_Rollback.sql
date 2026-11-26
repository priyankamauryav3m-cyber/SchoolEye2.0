/* =====================================================================================
   DPDP - Student Sensitive Details : ROLLBACK

   Removes only the new objects of this feature. No table / column / data is dropped:
   - Det_TransactionLog rows (ReferenceKey = 'DPDP_StudentSensitive') are kept as audit evidence.
   - Student data changed through the page stays as saved (old values are in Det_TransactionLog
     and the *History tables if a value must be restored by hand).
   ===================================================================================== */
DROP PROCEDURE IF EXISTS dbo.USP_DPDP_UpdateStudentSensitiveDetails;
DROP PROCEDURE IF EXISTS dbo.V3M_DPDP_GetStudentSensitiveDetails;
DROP PROCEDURE IF EXISTS dbo.V3M_DPDP_GetStudentSensitiveHistory;
DROP PROCEDURE IF EXISTS dbo.V3M_DPDP_CheckPiiAccess;
GO

-- optional: the index helps every module that reads Det_TransactionLog by reference; drop only if required
-- DROP INDEX IF EXISTS IX_Det_TransactionLog_Reference ON dbo.Det_TransactionLog;
