document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("registerForm");
    const name = document.getElementById("FullName");
    const email = document.getElementById("Email");
    const password = document.getElementById("Password");
    const confirmPassword = document.getElementById("ConfirmPassword");

    const nameError = document.getElementById("nameError");
    const emailError = document.getElementById("emailError");
    const passwordError = document.getElementById("passwordError");
    const confirmPasswordError = document.getElementById("confirmPasswordError");

    form.addEventListener("submit", function (e) {
        let valid = true;
        nameError.textContent = "";
        emailError.textContent = "";
        passwordError.textContent = "";
        confirmPasswordError.textContent = "";

        // Họ tên
        if (!name.value.trim()) {
            nameError.textContent = "Vui lòng nhập họ và tên.";
            valid = false;
        }

        // Email
        if (!email.value.trim()) {
            emailError.textContent = "Vui lòng nhập email.";
            valid = false;
        } else if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email.value)) {
            emailError.textContent = "Định dạng email không hợp lệ.";
            valid = false;
        }

        // Mật khẩu
        if (password.value.length < 6) {
            passwordError.textContent = "Mật khẩu phải có ít nhất 6 ký tự.";
            valid = false;
        }

        // Xác nhận mật khẩu
        if (password.value !== confirmPassword.value) {
            confirmPasswordError.textContent = "Mật khẩu xác nhận không khớp.";
            valid = false;
        }

        if (!valid) e.preventDefault();
    });
});