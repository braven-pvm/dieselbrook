function getProductIdFromUrl(url) {
    var thirdValue = 0;

    var parts = url.split('/');
    if (parts.length >= 4) {
        thirdValue = parts[3];
    }
    return thirdValue;
}

//Custom ajax cart
var AjaxCart = {
    loadWaiting: false,
    usepopupnotifications: false,
    topcartselector: '',
    topwishlistselector: '',
    flyoutcartselector: '',
    localized_data: false,
    addToCartLink: '',

    init: function (usepopupnotifications, topcartselector, topwishlistselector, flyoutcartselector, localized_data, addToCartLink) {
        this.loadWaiting = false;
        this.usepopupnotifications = usepopupnotifications;
        this.topcartselector = topcartselector;
        this.topwishlistselector = topwishlistselector;
        this.flyoutcartselector = flyoutcartselector;
        this.localized_data = localized_data;
        this.addToCartLink = addToCartLink;
    },

    setLoadWaiting: function (display) {
        displayAjaxLoading(display);
        this.loadWaiting = display;
    },

    //add a product to the cart/wishlist from the catalog pages
    addproducttocart_catalog: function (urladd) {
        if (this.loadWaiting !== false) {
            return;
        }
        this.setLoadWaiting(true);

        this.addToCartLink = urladd;

        var postData = {};
        addAntiForgeryToken(postData);

        $.ajax({
            cache: false,
            url: urladd,
            type: "POST",
            data: postData,
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },

    //add a product to the cart/wishlist from the product details page
    addproducttocart_details: function (urladd, formselector) {
        if (this.loadWaiting !== false) {
            return;
        }
        this.setLoadWaiting(true);
        this.addToCartLink = urladd;

        $.ajax({
            cache: false,
            url: urladd,
            data: $(formselector).serialize(),
            type: "POST",
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },

    //add a product to compare list
    addproducttocomparelist: function (urladd) {
        if (this.loadWaiting !== false) {
            return;
        }
        this.setLoadWaiting(true);

        var postData = {};
        addAntiForgeryToken(postData);

        $.ajax({
            cache: false,
            url: urladd,
            type: "POST",
            data: postData,
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },
    success_process: function (response) {
        if (response.updatetopcartsectionhtml) {
            $(AjaxCart.topcartselector).html(response.updatetopcartsectionhtml);
        }
        if (response.updatetopwishlistsectionhtml) {
            $(AjaxCart.topwishlistselector).html(response.updatetopwishlistsectionhtml);
        }
        if (response.updateflyoutcartsectionhtml) {
            $(AjaxCart.flyoutcartselector).replaceWith(response.updateflyoutcartsectionhtml);
        }
        if (response.message) {
            //display notification
            if (response.success === true) {

                //success
                if (AjaxCart.usepopupnotifications === true) {
                    displayPopupNotification(response.message, 'success', true);

                    //get product id from url
                    var productId = getProductIdFromUrl(AjaxCart.addToCartLink);
                    if (productId > 0) {
                        //update cart quantity for product in product box wrapper
                        updateProductCartItemQuantity(productId);
                    }

                    //Update cart total value in header top
                    updateCartTotalValue();
                }
                else {
                    //specify timeout for success messages
                    displayBarNotification(response.message, 'success', 3500);
                    //get product id from url
                    var productId = getProductIdFromUrl(AjaxCart.addToCartLink);
                    if (productId > 0) {
                        //update cart quantity for product in product box wrapper
                        updateProductCartItemQuantity(productId);
                    }

                    //Update cart Total value in header top
                    updateCartTotalValue();
                }
            }
            else {
                //error
                if (AjaxCart.usepopupnotifications === true) {
                    displayPopupNotification(response.message, 'error', true);
                }
                else {
                    //no timeout for errors
                    displayBarNotification(response.message, 'error', 0);
                }
            }
            return false;
        }
        if (response.redirect) {
            location.href = response.redirect;
            return true;
        }
        return false;
    },

    resetLoadWaiting: function () {
        AjaxCart.setLoadWaiting(false);
    },

    ajaxFailure: function () {
        /*alert(this.localized_data.AjaxCartFailure);*/
        const msg = AjaxCart.localized_data?.AjaxCartFailure || 'An unexpected error occurred.';
        alert(msg);
    }
};

function displayBarNotificationInPopup(message, messagetype, timeout) {
    var notificationTimeout;

    var messages = typeof message === 'string' ? [message] : message;
    if (messages.length === 0)
        return;

    // types: success, error, warning
    var cssclass = ['success', 'error', 'warning'].indexOf(messagetype) !== -1 ? messagetype : 'success';

    // Create the notification element
    var htmlcode = document.createElement('div');
    htmlcode.classList.add('bar-notification', cssclass);
    htmlcode.classList.add(cssclass);

    var content = document.createElement('p');
    content.classList.add('content');
    content.innerHTML = message;
    htmlcode.appendChild(content);

    // Create a close button for the notification
    var close = document.createElement('span');
    close.classList.add('close');
    close.setAttribute('title', document.getElementById('otp-popup').dataset.close);
    htmlcode.appendChild(close);

    // Find the OTP pop-up and append the notification to it
    var otpPopup = document.getElementById('otp-popup');
    if (otpPopup) {
        otpPopup.querySelector('.modal-body').appendChild(htmlcode);
    }

    // Fade in the notification
    $(htmlcode).fadeIn('slow').on('mouseenter', function () {
        clearTimeout(notificationTimeout);
    });

    // Callback for notification removal
    var removeNoteItem = function () {
        $(htmlcode).remove();
    };

    $(close).on('click', function () {
        $(htmlcode).fadeOut('slow', removeNoteItem);
    });

    // Timeout (if set)
    if (timeout > 0) {
        notificationTimeout = setTimeout(function () {
            $(htmlcode).fadeOut('slow', removeNoteItem);
        }, timeout);
    }
}

function displayCustomPopupNotification(message, messagetype, modal) {
    //types: success, error, warning
    var container;
    if (messagetype == 'success') {
        //success
        container = $('#dialog-notifications-success');
    }
    else if (messagetype == 'error') {
        //error
        container = $('#dialog-notifications-error');
    }
    else if (messagetype == 'warning') {
        //warning
        container = $('#dialog-notifications-warning');
    }
    else {
        //other
        container = $('#dialog-notifications-success');
    }

    //we do not encode displayed message
    var htmlcode = '';
    if ((typeof message) == 'string') {
        htmlcode = '<p>' + message + '</p>';
    } else {
        for (var i = 0; i < message.length; i++) {
            htmlcode = htmlcode + '<p>' + message[i] + '</p>';
        }
    }

    container.html(htmlcode);

    var isModal = (modal ? true : false);
    container.dialog({
        modal: isModal,
        width: 350,
        dialogClass: 'private-message-popup-wrapper'
    });
}

function displayBarNotificationInSpecialOfferPopup(message, messagetype, timeout) {
    var notificationTimeout;

    var messages = typeof message === 'string' ? [message] : message;
    if (messages.length === 0)
        return;

    // types: success, error, warning
    var cssclass = ['success', 'error', 'warning'].indexOf(messagetype) !== -1 ? messagetype : 'success';

    // Create the notification element
    var htmlcode = document.createElement('div');
    htmlcode.classList.add('bar-notification', cssclass);
    htmlcode.classList.add(cssclass);

    var content = document.createElement('p');
    content.classList.add('content');
    content.innerHTML = message;
    htmlcode.appendChild(content);

    // Create a close button for the notification
    var close = document.createElement('span');
    close.classList.add('close');
    close.setAttribute('title', document.getElementById('specialOfferModal').dataset.close);
    htmlcode.appendChild(close);

    // Find the OTP pop-up and append the notification to it
    var otpPopup = document.getElementById('specialOfferModal');
    if (otpPopup) {
        otpPopup.querySelector('.modal-body').appendChild(htmlcode);
    }

    // Fade in the notification
    $(htmlcode).fadeIn('slow').on('mouseenter', function () {
        clearTimeout(notificationTimeout);
    });

    // Callback for notification removal
    var removeNoteItem = function () {
        $(htmlcode).remove();
    };

    $(close).on('click', function () {
        $(htmlcode).fadeOut('slow', removeNoteItem);
    });

    // Timeout (if set)
    if (timeout > 0) {
        notificationTimeout = setTimeout(function () {
            $(htmlcode).fadeOut('slow', removeNoteItem);
        }, timeout);
    }
}

var marquee = document.querySelector('.active-offers-marquee');
if (marquee) {
    if (window.innerWidth < 1200) {
        var marqueeHeight = marquee.offsetHeight;
        var sliderWrapper = document.querySelector('.slider-wrapper');
        if (sliderWrapper) {
            sliderWrapper.style.paddingTop = marqueeHeight + 'px';
        }
    }
}