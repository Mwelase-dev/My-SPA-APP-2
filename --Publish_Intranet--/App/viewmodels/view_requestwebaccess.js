define(['durandal/plugins/router', 'durandal/app', 'services/logger', 'services/datacontext'],
    function (router, app, logger, dataContext) {

        var validationErrors = ko.observable();
        var allValid = ko.observable(true);

        function activate() {
            logger.log("Request web access loaded");
        }

        function requestWebAccess() {

            var requestModel = {
                NameSurname: $("#txtNameSurname").val(),
                EmailAddress: $("#txtEmailAddress").val(),
                WebsiteAddress: $("#txtWebAddress").val(),
                Motivation: $("#txtMotivation").val()
            };

            var isValid = validateFields($("#txtNameSurname").val(), $("#txtEmailAddress").val(), $("#txtWebAddress").val(), $("#txtMotivation").val());

            if (isValid) {
                dataContext.requestWebAccess(requestModel);

                Q.resolve(app.showMessage("An email has been sent to \n Ant,\n Brendan and \n Quentin.", "Web request successfull")).then(function() {
                    router.navigateTo('#/view_staffmenu');
                });
            }

        }

        function validateFields(nameSurnam, email, webAddress, motivation) {

            $("#validation").children().remove();

            var errors = "";

            if (nameSurnam == "")
                errors += "<li>Name is required</li>";

            if (email == "")
                errors += "<li>Please enter your email address</li>";
            else {
                var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;

                if (!re.test(email)) {
                    errors += "<li>Please enter a valid email</li>";
                }
            }

            if (webAddress == "")
                errors += "<li> Please enter the web address you need to access </li>";

            if (motivation == "")
                errors += "<li> Please enter the reason why you need to access the website </li>";

           validationErrors(errors);
           allValid(errors == "");

           $("#validation").append(errors);

            return errors == "";

        }

        function cancel() {
            router.navigateTo('#/view_staffmenu');
        }

        var vm = {
            active: activate,

            requestWebAccess: requestWebAccess,
            cancel: cancel,

        };

        return vm;
    });