$(document).on('hidden.bs.modal', '#shippingAddressModal', function () {
    CheckoutShipping.resetShippingForm();
});

$(document).ready(function () {

    // Open modal on "Add New Address"
    $('.enter-address-button').on('click', function () {

        $('#shippingAddressModalLabel').text("Add a new Address");

        $('#shippingAddressModal').modal('show');

        // Reset after modal shown to ensure form exists
        setTimeout(function () {
            CheckoutShipping.resetShippingForm();

            const $input = $('#small-provincessearchterms');
            $input.attr('required', 'required');
            if ($input.siblings('span.required').length === 0) {
                $input.after('<span class="required">*</span>');
            }
        }, 100); // wait a bit for modal + form DOM to render
    });

    //search functionality
    $('#address-search-button').on('click', function () {
        var query = $('#address-search-input').val().trim().toLowerCase();
        var $rows = $('#shipping-addresses-form .data-table tbody tr');
        var matched = $rows.filter(function () {
            return $(this).find('.name').text().trim().toLowerCase().includes(query);
        });

        if (matched.length > 0) {
            $rows.hide();
            matched.show();
        } else {
            displayPopupNotification('No results found', 'Warning', true);
            $rows.show();
        }
    });
});

var CheckoutShipping = {
    form: false,
    selectedStateId: 0,

    init: function (form) {
        this.form = form;
    },

    editAddress: function (url, addressId, titleText) {
        CheckoutShipping.resetShippingForm();

        var prefix = 'ShippingNewAddress_';
        $.ajax({
            cache: false,
            type: "GET",
            url: url,
            data: {
                "addressId": addressId
            },
            success: function (data, textStatus, jqXHR) {
                $.each(data,
                    function (id, value) {
                        if (id.indexOf("CustomAddressAttributes") >= 0 && Array.isArray(value)) {
                            $.each(value, function (i, customAttribute) {
                                if (customAttribute.DefaultValue) {
                                    $(`#${customAttribute.ControlId}`).val(
                                        customAttribute.DefaultValue
                                    );
                                } else {
                                    $.each(customAttribute.Values, function (j, attributeValue) {
                                        if (attributeValue.IsPreSelected) {
                                            $(`#${customAttribute.ControlId}`).val(attributeValue.Id);
                                            $(
                                                `#${customAttribute.ControlId}_${attributeValue.Id}`
                                            ).prop("checked", attributeValue.Id);
                                        }
                                    });
                                }
                            });

                            return;
                        }

                        if (value !== null) {
                            var field = $(`#${prefix}${id}`);
                            field.val(value);

                            // Handle CountryId separately to load states before setting StateProvinceId
                            if (id.indexOf('CountryId') >= 0) {
                                field.trigger('change'); // This triggers state dropdown AJAX call

                                // Wait for states to load before setting StateProvinceId
                                const stateFieldId = id.replace('CountryId', 'StateProvinceId');
                                const stateValue = data[stateFieldId];

                                // Used ajaxStop to wait for AJAX to finish
                                $(document).one('ajaxStop', function () {
                                    $(`#${prefix}${stateFieldId}`).val(stateValue);
                                });
                            }
                        }
                    });
            },
            complete: function (jqXHR, textStatus) {
                $('#shippingAddressModalLabel').text(titleText); // update modal title
                $('#shippingAddressModal').modal('show'); // Show Bootstrap modal

                // Remove 'required' from suburb input during edit
                $('#small-provincessearchterms')
                    .removeAttr('required'); // Remove HTML5 required attribute

                $('#small-provincessearchterms').siblings('span.required').remove();//Remove required astric sign from Edit form 
            },
            error: function (err) {
                alert(err);
            }
        });
    },

    deleteEditAddress: function (url, addressId) {
        $.ajax({
            cache: false,
            type: "GET",
            url: url,
            data: {
                "addressId": addressId
            },
            success: function (response) {
                location.href = response.redirect;
                return true;
            },
            error: function (err) {
                alert(err);
            }
        });
    },
    setSelectedStateId: function (id) {
        this.selectedStateId = id;
    },
    resetShippingForm: function () {
        const $form = $('#shipping-address-form');

        if (!$form.length) return;

        // 1. Clear all fields (except antiforgery)
        $form.find(':input').each(function () {
            const type = this.type;
            const tag = this.tagName.toLowerCase();
            const name = this.name;

            if (name === '__RequestVerificationToken') return;

            if (type === 'text' || type === 'password' || type === 'email' || tag === 'textarea') {
                $(this).val('');
            } else if (type === 'checkbox' || type === 'radio') {
                $(this).prop('checked', false);
            } else if (tag === 'select') {
                $(this).prop('selectedIndex', 0);
            } else {
                $(this).val('');
            }
        });

        // 2. Clear validation errors
        $form.find('.field-validation-error').empty();
        $form.find('.input-validation-error').removeClass('input-validation-error');
        if ($form.data('validator')) {
            $form.validate().resetForm();
        }
    },
}