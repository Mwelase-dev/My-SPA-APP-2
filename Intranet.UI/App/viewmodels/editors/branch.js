define(['services/datacontext','viewmodels/editors/baseeditor', 'viewmodels/editors/staff'],
    function (dataContext, editor, staffEditor) {
        
        
        //#region Division CRUD
        function divisionAdd(rootObject) {

            var divInitialValues = {
              branchId : rootObject.branch.branchId()  
            };

            return editor.objAdd(editor.objModel.entityNames.branchDivisionName, rootObject.branch.branchDivisions,true,divInitialValues);
        };
        function divisionEdit(model) {
            return editor.objEdit(model);
        };
        function divisionDelete (model) {
            return editor.objDelete(model);
        };
        //#endregion
        
        //#region Staff CRUD
   
        function staffAdd(rootObject) {
          //  return editor.objEdit(new staffEditor(dataContext.manufactureEntity(editor.objModel.entityNames.staffMemberName), rootObject.branches()));
            return editor.objEdit(new staffEditor(dataContext.manufactureEntity(editor.objModel.entityNames.staffMemberName), rootObject.branch.branchDivisions));
        };
        function staffEdit(model) {
            return getStaffEditor(model);
        };

        function getStaffEditor(staffmodel, branchModel) {
            var tmpStaff = ko.observable();
            return dataContext.getStaffbyId(tmpStaff, staffmodel.staffId()).then(function () {
                return editor.objEdit(new staffEditor(tmpStaff(), branchModel));
            });
        }

        function staffDelete(model) {
            return editor.objDelete(model);
        };
        //#endregion
        
        var vm = function (objModel,branches) {
            this.branch = objModel;
            this.branches = branches;
            //#region Division CRUD
            this.divisionAdd    = divisionAdd;
            this.divisionEdit   = divisionEdit;
            this.divisionDelete = divisionDelete;
            //#endregion
            
            //#region Staff CRUD
            this.staffAdd    = staffAdd;
            this.staffEdit   = staffEdit;
            this.staffDelete = staffDelete;
            //#endregion
        };
        return vm;
    });