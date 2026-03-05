$(document).ready(function () {

    // Show/Hide password
    $(".password-eye").on("click", function () {
        const input = $(this).siblings('input');
        const type = input.attr("type") === "password" ? "text" : "password";
        input.attr("type", type);
        $(this).toggleClass("password-eye-open");
    });

    // Reset modal on close
    $('#GuestModal').on('hidden.bs.modal', function () {
        const $modal = $(this);
        $modal.find('form').each(function () {
            this.reset();
        });
        $modal.find('[id$="-error"]').text('').hide();
        $modal.find('#loginError').hide().text('');
        $modal.find('#RegisterError').hide().text('');
        var backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) {
            backdrop.remove();
        }
        document.body.style.overflowX = 'hidden';
        document.body.style.overflowY = 'auto';
    });

    // Serialize form to JSON
    function getFormDataAsJson($form) {
        const unindexedArray = $form.serializeArray();
        const indexedArray = {};
        $.map(unindexedArray, function (n) {
            indexedArray[n['name']] = n['value'];
        });
        return indexedArray;
    }

    // Login button
    $('#loginButton').click(function () {
        const $form = $('#loginForm');
        const $button = $(this);
        if (!$form.valid()) return;

        $button.prop('disabled', true);
        const antiForgeryToken = $('input[name="__RequestVerificationToken"]').val();
        const formDataJson = getFormDataAsJson($form);
        displayAjaxLoading(true);

        $.ajax({
            type: 'POST',
            url: '/checkout-login',
            contentType: 'application/json',
            data: JSON.stringify(formDataJson),
            headers: { 'RequestVerificationToken': antiForgeryToken },
            success: function (response) {
                displayAjaxLoading(false);
                $button.prop('disabled', false);
                if (response.success) {
                    $('#GuestModal').modal('hide');

                    setTimeout(() => {
                        if (typeof returnUrl !== "undefined" && returnUrl) {
                            console.log(returnUrl);
                            window.location.href = returnUrl;
                        } else {
                            window.location.reload(); // default behavior if no ReturnUrl
                        }
                    }, 300);
                } else {
                    $('#loginError').text(response.message).show();
                }
            },
            error: function () {
                displayAjaxLoading(false);
                $button.prop('disabled', false);
                $('#loginError').text('Login failed. Please try again.').show();
            }
        });
    });

    // Register button
    $('#signUpButton').click(function () {
        const $form = $('#signUp-form');
        const $button = $(this);
        if (!$form.valid()) return;

        $button.prop('disabled', true);
        const antiForgeryToken = $('input[name="__RequestVerificationToken"]').val();
        const formDataJson = getFormDataAsJson($form);

        $.ajax({
            type: 'POST',
            url: '/checkout-register',
            contentType: 'application/json',
            data: JSON.stringify(formDataJson),
            headers: { 'RequestVerificationToken': antiForgeryToken },
            success: function (response) {
                $button.prop('disabled', false);
                if (response.success) {
                    $('#GuestModal').modal('hide');

                    setTimeout(() => {
                        if (typeof returnUrl !== "undefined" && returnUrl) {
                            window.location.href = returnUrl;
                        } else {
                            window.location.reload(); // default behavior if no ReturnUrl
                        }
                    }, 300);
                } else {
                    $('#RegisterError').text(response.message).show();
                }
            },
            error: function () {
                $button.prop('disabled', false);
                $('#RegisterError').text('Registration failed. Please try again.').show();
            }
        });
    });

});
