-- READ-ONLY: Fee module SP definitions, referenced tables, existing indexes, row counts, missing-index hints
SET NOCOUNT ON; DECLARE @n TABLE(n sysname PRIMARY KEY);
INSERT @n VALUES('ADM_GetSiblingDetailList');
INSERT @n VALUES('ADM_HandoverStudentDocument');
INSERT @n VALUES('ADM_UspGetSearchedStudent');
INSERT @n VALUES('FEE_UspAddAdjustmentReceipt');
INSERT @n VALUES('FEE_UspAddJournalVoucherDetail');
INSERT @n VALUES('FEE_UspAddJournalVoucherMaster');
INSERT @n VALUES('FEE_UspAddMiscReceipt');
INSERT @n VALUES('FEE_UspAddUpdateJournalAccount');
INSERT @n VALUES('FEE_UspAddUpdateJournalAccountGroup');
INSERT @n VALUES('FEE_UspAddUpdateMiscChallan');
INSERT @n VALUES('FEE_UspAddUpdateOptionalFeeGroup');
INSERT @n VALUES('FEE_UspApproveAdjustmentReceipt');
INSERT @n VALUES('FEE_UspGetAdjustmentReceiptList');
INSERT @n VALUES('FEE_UspGetFeeHeadLedger');
INSERT @n VALUES('FEE_UspGetJournalAccountGroupList');
INSERT @n VALUES('FEE_UspGetJournalAccountList');
INSERT @n VALUES('FEE_UspGetJournalVoucherDetail');
INSERT @n VALUES('FEE_UspGetJournalVoucherList');
INSERT @n VALUES('FEE_UspGetMiscChallanList');
INSERT @n VALUES('FEE_UspGetMiscReceiptList');
INSERT @n VALUES('FEE_UspGetOptionalFeeGroupList');
INSERT @n VALUES('FEE_UspGetStudentLedger');
INSERT @n VALUES('FEE_UspMapOptionalFeeGroupClass');
INSERT @n VALUES('FEE_UspMapOptionalFeeGroupHead');
INSERT @n VALUES('FEE_UspToggleOptionalFeeGroup');
INSERT @n VALUES('FEE_UspValidateJournalVoucherBalance');
INSERT @n VALUES('FIN_GetWithModeRecieptHeadWiseDetail');
INSERT @n VALUES('FIN_Usp_AddFeeHeadToStudentChallan');
INSERT @n VALUES('FIN_Usp_RemoveFeeHeadFromChallan');
INSERT @n VALUES('FIN_Usp_UpdateChallanDueDate');
INSERT @n VALUES('FNGT_InsertUpdate_session');
INSERT @n VALUES('Fin_GetOnlyChqCombineReport');
INSERT @n VALUES('Fin_V3M_USPMapUnmapDefaultFeeTemplateWithCourse');
INSERT @n VALUES('Get_FeeHeadSession');
INSERT @n VALUES('MNGT_InsertUpdate_AccountNo');
INSERT @n VALUES('MNGT_InsertUpdate_Bank');
INSERT @n VALUES('MNGT_InsertUpdate_BankAccount');
INSERT @n VALUES('MNGT_InsertUpdate_ChequeBook');
INSERT @n VALUES('MNGT_InsertUpdate_ChequeType');
INSERT @n VALUES('MNGT_InsertUpdate_Concession');
INSERT @n VALUES('MNGT_InsertUpdate_Distance');
INSERT @n VALUES('MNGT_InsertUpdate_FeeCollection');
INSERT @n VALUES('MNGT_InsertUpdate_FeeCollectionPeriod');
INSERT @n VALUES('MNGT_InsertUpdate_FeeHead');
INSERT @n VALUES('MNGT_InsertUpdate_FeeHeadMappedToClass');
INSERT @n VALUES('MNGT_InsertUpdate_FeeTemplate');
INSERT @n VALUES('MNGT_InsertUpdate_Month');
INSERT @n VALUES('MNGT_InsertUpdate_PaymentMode');
INSERT @n VALUES('MNGT_InsertUpdate_Society');
INSERT @n VALUES('MNGT_InsertUpdate_TransportFee');
INSERT @n VALUES('Sp_GetSiblingDetail');
INSERT @n VALUES('TPT_GetStudentTranportDetail');
INSERT @n VALUES('TVP_AdmissionReceiptFeeHead');
INSERT @n VALUES('TVP_MonthNo');
INSERT @n VALUES('USP_FINGetStudentBalanceList');
INSERT @n VALUES('USP_FIN_GetReceiptDetail');
INSERT @n VALUES('USP_GetAllStudentByFeeTemplate');
INSERT @n VALUES('USP_GetClassesList');
INSERT @n VALUES('USP_GetFeeCollectionConfig');
INSERT @n VALUES('USP_GetLateFeeConfigList');
INSERT @n VALUES('USP_GetPromotionConcessionStudent');
INSERT @n VALUES('USP_GetQuarterlyMonthMapping');
INSERT @n VALUES('USP_GetSearchedStudent');
INSERT @n VALUES('USP_GetStudentAddress');
INSERT @n VALUES('USP_GetStudentForFeeChallan');
INSERT @n VALUES('USP_GetStudentInvoiceDues');
INSERT @n VALUES('USP_GetStudentList');
INSERT @n VALUES('USP_GetStudentMappedApprovedConcession');
INSERT @n VALUES('USP_GetStudentMappedConcession');
INSERT @n VALUES('USP_GetStudentParentDetails');
INSERT @n VALUES('USP_GetStudentPassportDetail');
INSERT @n VALUES('USP_GetStudentViewDetails');
INSERT @n VALUES('USP_GetStudentVisitorsDetails');
INSERT @n VALUES('USP_GetStudentWithConcession');
INSERT @n VALUES('USP_GetStudentsByClass');
INSERT @n VALUES('USP_ManageConcessionApproval');
INSERT @n VALUES('USP_MapConcessionWithFeeHead');
INSERT @n VALUES('USP_UnMapConcessionWithStudent');
INSERT @n VALUES('USP_VerifyAndTakeStudentDocument');
INSERT @n VALUES('Usp_AddUpdateSibling');
INSERT @n VALUES('Usp_CancelGeneratedStudentChallan');
INSERT @n VALUES('Usp_EditInvoiceFeeheadMonthNoUpdate');
INSERT @n VALUES('Usp_Fin_CopyFeeHeadMappedToStudent');
INSERT @n VALUES('Usp_Fin_GetStudentForFeePeriodTypeMapping');
INSERT @n VALUES('Usp_GenerateStudentChallanForAllClassWithTemplate');
INSERT @n VALUES('Usp_GetClassSection');
INSERT @n VALUES('Usp_GetRegistrationDetails');
INSERT @n VALUES('Usp_GetStudentmiscellaneous');
INSERT @n VALUES('Usp_MapFeePeriodWithStudent');
INSERT @n VALUES('Usp_MapFeeTemplateWithStudent');
INSERT @n VALUES('Usp_RemoveProfileImage');
INSERT @n VALUES('Usp_StudentAddressDetails');
INSERT @n VALUES('Usp_StudentParentDetails');
INSERT @n VALUES('Usp_StudentPassportDetails');
INSERT @n VALUES('Usp_StudentPersonalDetails');
INSERT @n VALUES('Usp_StudentVisitorsDetails');
INSERT @n VALUES('Usp_UpdateSiblingId');
INSERT @n VALUES('V3M_ADM_UspDeactivateStudentDocument');
INSERT @n VALUES('V3M_ADM_UspGetClassStudentDocuments');
INSERT @n VALUES('V3M_FIN_CancelStudentReceipt');
INSERT @n VALUES('V3M_FIN_FeeHeadMonthType');
INSERT @n VALUES('V3M_FIN_GenerateMiscellaneousChallan');
INSERT @n VALUES('V3M_FIN_GenerateMiscellaneousReceipt');
INSERT @n VALUES('V3M_FIN_GenerateReceiptChqBounceChallan');
INSERT @n VALUES('V3M_FIN_GeneratedStudentReceiptNo');
INSERT @n VALUES('V3M_FIN_GetClassGroup');
INSERT @n VALUES('V3M_FIN_GetClassMapTempateFeeHead');
INSERT @n VALUES('V3M_FIN_GetDefaultFeeTemplateWithClass');
INSERT @n VALUES('V3M_FIN_GetFeeHeadForCombo');
INSERT @n VALUES('V3M_FIN_GetFeeHeadTemplatesList');
INSERT @n VALUES('V3M_FIN_GetFeeHeadsMappedWithTemplateList');
INSERT @n VALUES('V3M_FIN_GetInvoiceMasterDetails');
INSERT @n VALUES('V3M_FIN_GetStudentChallan');
INSERT @n VALUES('V3M_FIN_GetStudentDetailsForFee');
INSERT @n VALUES('V3M_FIN_GetStudentFeeReceiptList');
INSERT @n VALUES('V3M_FIN_GetStudentMappedWithFeeHead');
INSERT @n VALUES('V3M_FIN_GetStudentReceiptById');
INSERT @n VALUES('V3M_FIN_InsertAdmissionReceiptDetails');
INSERT @n VALUES('V3M_FIN_InsertCounterReceiptDetails');
INSERT @n VALUES('V3M_FIN_MiscellaniousChallanWithMonths');
INSERT @n VALUES('V3M_FIN_PrintViewStudentReceipts');
INSERT @n VALUES('V3M_FIN_SaveFeeTemplateCollectionConfig_Bulk');
INSERT @n VALUES('V3M_FIN_SaveStudentConcession');
INSERT @n VALUES('V3M_FIN_TransportMapMonthConfig');
INSERT @n VALUES('V3M_FIN_UDT_MiscChallanFeeHead');
INSERT @n VALUES('V3M_FIN_UDT_MiscChallanStudent');
INSERT @n VALUES('V3M_FIN_UnMapFeeHeadWithStudent');
INSERT @n VALUES('V3M_FIN_UspAdmitChild');
INSERT @n VALUES('V3M_FIN_UspApplyStudentConcessionOnChallan');
INSERT @n VALUES('V3M_FIN_UspGetFeeHeadLedger');
INSERT @n VALUES('V3M_FIN_UspGetStudentFeeHeadDuesForAdjustmentDemo');
INSERT @n VALUES('V3M_FIN_UspGetStudentLedger');
INSERT @n VALUES('V3M_FIN_UspGetStudentReceipts');
INSERT @n VALUES('V3M_FIN_UspInsertLateFeeConfigration');
INSERT @n VALUES('V3M_FIN_UspMapStudentConcessionWithAdmission');
INSERT @n VALUES('V3M_FIN_UspPromoteStudentConcession');
INSERT @n VALUES('V3M_FIN_UspRemoveStudentConcessionFromInvoice');
INSERT @n VALUES('V3M_FIN_UspUpdateLateFee');
INSERT @n VALUES('V3M_FIN_UspUpdateStudentConcessionRemarks');
INSERT @n VALUES('V3M_Fee_GetTransportStudentData');
INSERT @n VALUES('V3M_GetFeeCollectionPeriodConfig');
INSERT @n VALUES('V3M_GetStudentLedgerChallanMiniDetails');
INSERT @n VALUES('V3M_STU_GetSearchedStudent');
INSERT @n VALUES('V3M_UspActivateDeactivateLateFeeConfig');
INSERT @n VALUES('V3M_UspAddEditFeeTemplateFeeHead');
DECLARE @p TABLE(object_id int PRIMARY KEY);
INSERT @p SELECT o.object_id FROM @n x JOIN sys.procedures o ON o.name = x.n;

PRINT '===== 1. PROCEDURE DEFINITIONS =====';
DECLARE @id int, @name sysname, @def nvarchar(max), @i int;
DECLARE c CURSOR LOCAL FAST_FORWARD FOR
  SELECT p.object_id, OBJECT_NAME(p.object_id), m.definition FROM @p p JOIN sys.sql_modules m ON m.object_id = p.object_id ORDER BY 2;
OPEN c; FETCH NEXT FROM c INTO @id, @name, @def;
WHILE @@FETCH_STATUS = 0
BEGIN
  PRINT '##### PROC ' + @name;
  SET @i = 1;
  WHILE @i <= LEN(@def) BEGIN PRINT SUBSTRING(@def, @i, 3500); SET @i += 3500; END
  FETCH NEXT FROM c INTO @id, @name, @def;
END
CLOSE c; DEALLOCATE c;

PRINT '===== 2. REFERENCED TABLES =====';
SELECT DISTINCT d.referenced_entity_name AS TableName
INTO #t
FROM @p p JOIN sys.sql_expression_dependencies d ON d.referencing_id = p.object_id
JOIN sys.tables t ON t.name = d.referenced_entity_name;
SELECT t.TableName, SUM(ps.row_count) AS RowCnt
FROM #t t JOIN sys.dm_db_partition_stats ps ON ps.object_id = OBJECT_ID('dbo.' + t.TableName) AND ps.index_id IN (0,1)
GROUP BY t.TableName ORDER BY RowCnt DESC;

PRINT '===== 3. EXISTING INDEXES =====';
SELECT OBJECT_NAME(i.object_id) AS TableName, i.name AS IndexName, i.type_desc, i.is_unique, i.is_primary_key,
  STUFF((SELECT ',' + c.name FROM sys.index_columns ic JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
         WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 0 ORDER BY ic.key_ordinal FOR XML PATH('')),1,1,'') AS KeyCols,
  STUFF((SELECT ',' + c.name FROM sys.index_columns ic JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
         WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 1 FOR XML PATH('')),1,1,'') AS IncludeCols,
  i.filter_definition
FROM sys.indexes i JOIN #t t ON i.object_id = OBJECT_ID('dbo.' + t.TableName)
WHERE i.type > 0 ORDER BY 1, 2;

PRINT '===== 4. COLUMNS (key tables) =====';
SELECT t.TableName, c.name AS ColName, ty.name AS DataType, c.max_length, c.is_nullable
FROM #t t JOIN sys.columns c ON c.object_id = OBJECT_ID('dbo.' + t.TableName) JOIN sys.types ty ON ty.user_type_id = c.user_type_id
ORDER BY t.TableName, c.column_id;

PRINT '===== 5. MISSING INDEX HINTS (SQL Server DMV) =====';
SELECT TOP 60 OBJECT_NAME(d.object_id) AS TableName, d.equality_columns, d.inequality_columns, d.included_columns,
  s.user_seeks, s.user_scans, CAST(s.avg_total_user_cost * s.avg_user_impact * (s.user_seeks + s.user_scans) AS bigint) AS Score
FROM sys.dm_db_missing_index_details d
JOIN sys.dm_db_missing_index_groups g ON g.index_handle = d.index_handle
JOIN sys.dm_db_missing_index_group_stats s ON s.group_handle = g.index_group_handle
JOIN #t t ON d.object_id = OBJECT_ID('dbo.' + t.TableName)
WHERE d.database_id = DB_ID() ORDER BY Score DESC;
DROP TABLE #t;
