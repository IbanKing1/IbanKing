document.addEventListener('DOMContentLoaded', function () {
    const togglePassword = document.getElementById("togglePassword");
    const toggleCurrentPassword = document.getElementById("toggleCurrentPassword");

    if (togglePassword) {
        togglePassword.addEventListener("click", function () {
            const input = document.getElementById("passwordInput");
            const icon = document.getElementById("toggleIcon");
            input.type = input.type === "password" ? "text" : "password";
            icon.classList.toggle("fa-eye");
            icon.classList.toggle("fa-eye-slash");
        });
    }

    if (toggleCurrentPassword) {
        toggleCurrentPassword.addEventListener("click", function () {
            const input = document.getElementById("currentPasswordInput");
            const icon = document.getElementById("toggleCurrentIcon");
            input.type = input.type === "password" ? "text" : "password";
            icon.classList.toggle("fa-eye");
            icon.classList.toggle("fa-eye-slash");
        });
    }
});

function previewImage(input) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function (e) {
            document.getElementById('profileImage').src = e.target.result;
        }
        reader.readAsDataURL(input.files[0]);
    }
}