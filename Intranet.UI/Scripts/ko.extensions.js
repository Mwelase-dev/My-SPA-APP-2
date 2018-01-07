//#region jQuery UI Accordion (Working)
ko.bindingHandlers.bind_Accordion = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        $(element).accordion({
            heightStyle: "content",
            collapsible: true,
            active: false
        });
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        $(element).accordion('destroy').accordion({
            heightStyle: "content",
            collapsible: true,
            active: false
        });
    }
};
//#endregion

//#region TinyMCE HTML Editor (Working)
ko.bindingHandlers.bind_tinymce = {
    init: function (element, valueAccessor, allBindingsAccessor, context) {
        var options = allBindingsAccessor().tinymceOptions || {};
        var modelValue = valueAccessor();
        var value = ko.utils.unwrapObservable(valueAccessor());
        var el = $(element);

        // Handle edits made in the editor. Updates after an undo point is reached.
        options.setup = function (ed) {
            ed.onChange.add(function (ed, l) {
                if (ko.isWriteableObservable(modelValue)) {
                    modelValue(l.content);
                }
            });
        };

        // Handle destroying an editor 
        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
            setTimeout(function () {
                $(element).tinymce().remove();
            }, 0);
        });

        // $(element).tinymce(options);
        setTimeout(function () {
            $(element).tinymce(options);
        }, 0);
        el.html(value);
    },
    update: function (element, valueAccessor, allBindingsAccessor, context) {
        var el = $(element);
        var value = ko.utils.unwrapObservable(valueAccessor());
        var id = el.attr('id');

        //handle programmatic updates to the observable
        // also makes sure it doesn't update it if it's the same. 
        // otherwise, it will reload the instance, causing the cursor to jump.
        if (id !== undefined) {
            var content = tinymce.getInstanceById(id).getContent({ format: 'raw' });
            if (content !== value) {
                el.html(value);
            }
        }
    }
};
//#endregion

//#region jQuery UI Sorting Helper (Working)
ko.bindingHandlers.sortData = {
    update: function (element, valueAccessor, allBindingsAccessor) {
        var sortOptions = allBindingsAccessor();
        if (sortOptions) {
            var arrayToSort = sortOptions.sortData;
            var fieldName   = sortOptions.sortfield;
            var direction   = sortOptions.direction;
            if ((arrayToSort != undefined) && (fieldName != undefined)) {
                arrayToSort.sort(function(left, right) {
                    var objLeft = typeof left[fieldName] == 'function' ? left[fieldName]() : left[fieldName];
                    var objRight = typeof right[fieldName] == 'function' ? right[fieldName]() : right[fieldName];
                    return objLeft == objRight ? 0 : (objLeft < objRight ? direction == "asc" ? -1 : 1 : direction == "asc" ? 1 : -1);
                });
            }
        }
    }
};
//#endregion

//#region jQuery CalleryView onto the element (Working)
ko.bindingHandlers.makeGalleryUI = {
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        $(element).galleryView({
            panel_width: 630,
            panel_height: 472
        });
    }
};
//#endregion

//#region jQuery UI Tabs (Working)
ko.bindingHandlers.bind_Tabs = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        $(element).tabs();
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        $(element).tabs('destroy').tabs();
    }
};
//#endregion

//#region Format datetimepicker
//This is only meant for the datetimepicker not the datepicker

ko.bindingHandlers.bind_datetimepicker = {

    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        $(element).datetimepicker();
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewmodel, bindingContext) {
        //$(element).datetimepicker('destroy').datetimepicker();
        
        //var value = valueAccessor();
        //var date = new Date(ko.unwrap(value));
        //value(date);
        //$(element).val(date);
        
        //$(element).html(date);
    }

};

//#endregion

//#region - Format datepicker 
//Use this extension to bind a date type input


//#endregion


