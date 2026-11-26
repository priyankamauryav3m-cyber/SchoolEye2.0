using DomainModel.SchoolAdmin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolAdmin
{
    public interface IDynamicFormController
    {
        // USP_AddUpdateDynamicFormController2:
        //  1 inserted, 2 updated, -1 not found, -2 invalid field name, -3 field name required,
        //  -4 field already exists, -99 error;
        //  API checks: -5 activity not of this branch / session, -6 reserved (base) column name
        Task<int> AddUpdateFieldAsync(DynamicFormControllerModel model);

        // Usp_GetFieldNameList
        Task<IEnumerable<DynamicFormControllerModel>> GetFieldsAsync(DynamicFormControllerSearchModel search);

        // Usp_ActiveDeactiveDynamicFormController: 1 changed, -1 not found, -4 same field already active
        Task<int> ActiveDeactiveFieldAsync(DynamicFormControllerStatusModel model);
    }
}
