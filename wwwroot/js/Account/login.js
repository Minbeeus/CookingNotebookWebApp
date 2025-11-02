document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("loginForm");
    const email = document.getElementById("Email");
    const password = document.getElementById("Password");
    const emailError = document.getElementById("emailError");
    const passwordError = document.getElementById("passwordError");

    function validateEmail(emailValue) {
        if (!emailValue.trim()) {
            if (emailError) emailError.textContent = "Vui lòng nhập email.";
            return false;
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(emailValue)) {
            if (emailError) emailError.textContent = "Email không hợp lệ.";
            return false;
        }
        if (emailError) emailError.textContent = "";
        return true;
    }

    function validatePassword(passwordValue) {
        if (!passwordValue.trim()) {
            if (passwordError) passwordError.textContent = "Vui lòng nhập mật khẩu.";
            return false;
        } else if (passwordValue.length < 6) {
            if (passwordError) passwordError.textContent = "Mật khẩu phải có ít nhất 6 ký tự.";
            return false;
        }
        if (passwordError) passwordError.textContent = "";
        return true;
    }

    function clearErrors() {
        if (emailError) emailError.textContent = "";
        if (passwordError) passwordError.textContent = "";
    }

    if (email) {
        email.addEventListener("input", function () {
            validateEmail(email.value);
        });
    }

    if (password) {
        password.addEventListener("input", function () {
            validatePassword(password.value);
        });
    }

    if (form) {
    form.addEventListener("submit", function (e) {
    e.preventDefault(); // Prevent default submit
            clearErrors();

    const isEmailValid = validateEmail(email.value);
            const isPasswordValid = validatePassword(password.value);

    if (!isEmailValid || !isPasswordValid) {
        return; // Don't submit if client validation fails
    }

        // AJAX submit
            const formData = new FormData(form);
            fetch('/Account/Login', {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                }
            })
            .then(response => {
                if (response.redirected) {
                    // Success, redirect
                    window.location.href = response.url;
                } else {
                    // Error, get error message
                    return response.text();
                }
            })
            .then(html => {
                if (html) {
                    // Parse HTML to get error message
                    const parser = new DOMParser();
                    const doc = parser.parseFromString(html, 'text/html');
                    const errorDiv = doc.querySelector('.alert.alert-danger');
                    if (errorDiv) {
                        alert(errorDiv.textContent.trim());
                    } else {
                        alert('Đăng nhập thất bại.');
                    }
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Có lỗi xảy ra. Vui lòng thử lại.');
            });
        });
    }
});