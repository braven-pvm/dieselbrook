var inactivityTimeout;
var loginTimeOutMinutes;
var logoutUrl;

function resetInactivityTimer() {
    clearTimeout(inactivityTimeout);
    inactivityTimeout = setTimeout(logoutUser, loginTimeOutMinutes * 60 * 1000); // minutes in milliseconds
}

function logoutUser() {
    // Redirect the user to the logout URL
    window.location.href = logoutUrl;
}

// Add event listeners to reset the timer to 0 when the window loses focus
window.addEventListener('blur', function () {
    clearTimeout(inactivityTimeout);
});
