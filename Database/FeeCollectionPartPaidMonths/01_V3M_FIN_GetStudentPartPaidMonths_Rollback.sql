/*
    Rollback of 01_V3M_FIN_GetStudentPartPaidMonths.sql
    Drops only the new read-only procedure; no data or other object is touched.
    Deploy the UI/API without the "partly paid months" call first (or together), otherwise the screen just
    shows no month summary (the API returns an empty list on error).
*/
IF OBJECT_ID('dbo.V3M_FIN_GetStudentPartPaidMonths', 'P') IS NOT NULL
    DROP PROCEDURE dbo.V3M_FIN_GetStudentPartPaidMonths;
GO
