(function () {
    var shell = document.getElementById('authShell');
    var toRegister = document.querySelector('[data-to-register]');
    var toLogin = document.querySelector('[data-to-login]');
    var loginForm = document.getElementById('loginForm');
    var registerForm = document.getElementById('registerForm');
    var loginMsg = document.getElementById('loginMsg');
    var registerMsg = document.getElementById('registerMsg');
    var loginError = document.getElementById('loginError');
    var loginSubmitBtn = document.getElementById('loginSubmitBtn');
    var passwordToggles = document.querySelectorAll('[data-toggle-password]');
    var registerPassword = document.getElementById('registerPassword');
    var registerConfirmPassword = document.getElementById('registerConfirmPassword');
    var confirmPasswordWrap = document.getElementById('confirmPasswordWrap');
    var confirmPasswordError = document.getElementById('confirmPasswordError');
    var confirmTouched = false;

    function showRegister() {
        shell.classList.add('register-mode');
    }

    function showLogin() {
        shell.classList.remove('register-mode');
    }

    function showLoginError(message) {
        if (!loginError) return;
        loginError.textContent = message;
        loginError.classList.add('show');
    }

    function clearLoginError() {
        if (!loginError) return;
        loginError.textContent = '';
        loginError.classList.remove('show');
    }

    function showLoginSuccess(message) {
        if (!loginMsg) return;
        loginMsg.textContent = message;
        loginMsg.classList.add('show');
    }

    function clearLoginSuccess() {
        if (!loginMsg) return;
        loginMsg.textContent = '';
        loginMsg.classList.remove('show');
    }

    if (toRegister) {
        toRegister.addEventListener('click', showRegister);
    }

    if (toLogin) {
        toLogin.addEventListener('click', showLogin);
    }

    if (loginForm) {
        loginForm.addEventListener('submit', async function (e) {
            e.preventDefault();

            clearLoginError();
            clearLoginSuccess();

            var accountInput = document.getElementById('loginAccount');
            var passwordInput = document.getElementById('loginPassword');
            var rememberLogin = document.getElementById('rememberLogin');

            var account = accountInput ? accountInput.value.trim() : '';
            var matKhau = passwordInput ? passwordInput.value : '';

            if (!account || !matKhau) {
                showLoginError('Vui lòng nhập username/email và mật khẩu.');
                return;
            }

            var payload = {
                username: '',
                email: '',
                matKhau: matKhau
            };

            if (account.includes('@')) {
                payload.email = account;
            } else {
                payload.username = account;
            }

            try {
                if (loginSubmitBtn) {
                    loginSubmitBtn.disabled = true;
                    loginSubmitBtn.textContent = 'Đang đăng nhập...';
                }

                var response = await fetch('api/auth-login-proxy.php', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Accept': 'application/json'
                    },
                    body: JSON.stringify(payload)
                });

                var responseText = await response.text();
                var data = {};

                try {
                    data = responseText ? JSON.parse(responseText) : {};
                } catch (parseError) {
                    throw new Error('Phản hồi API không đúng định dạng JSON.');
                }

                if (!response.ok || !data.success) {
                    throw new Error(data.message || 'Đăng nhập thất bại.');
                }

                var authData = {
                    idTaiKhoan: data.idTaiKhoan,
                    username: data.username,
                    email: data.email,
                    loaiTaiKhoan: data.loaiTaiKhoan,
                    hoTen: data.hoTen,
                    idAdmin: data.idAdmin,
                    idChuQuanLy: data.idChuQuanLy,
                    isLoggedIn: true,
                    loginAt: new Date().toISOString()
                };

                if (rememberLogin && rememberLogin.checked) {
                    localStorage.setItem('admin_auth', JSON.stringify(authData));
                } else {
                    sessionStorage.setItem('admin_auth', JSON.stringify(authData));
                }

                showLoginSuccess(data.message || 'Đăng nhập thành công.');

                setTimeout(function () {
                    var target = 'index1st.php?usecase=dashboard';
                    if (data.loaiTaiKhoan === 'chu_quan_ly') {
                        target = 'index1st.php?usecase=store';
                    }
                    window.location.href = target;
                }, 700);
            } catch (error) {
                showLoginError(error.message || 'Không thể kết nối tới máy chủ.');
            } finally {
                if (loginSubmitBtn) {
                    loginSubmitBtn.disabled = false;
                    loginSubmitBtn.textContent = 'Đăng nhập';
                }
            }
        });
    }

    if (registerForm) {
        registerForm.addEventListener('submit', function (e) {
            e.preventDefault();

            if (!validateConfirmPassword(true)) {
                return;
            }

            registerMsg.classList.add('show');
            setTimeout(showLogin, 700);
        });
    }

    function validateConfirmPassword(forceShow) {
        if (!registerPassword || !registerConfirmPassword || !confirmPasswordWrap || !confirmPasswordError) {
            return true;
        }

        var needShow = forceShow || confirmTouched;
        var hasConfirmValue = registerConfirmPassword.value.length > 0;
        var isMatched = registerPassword.value === registerConfirmPassword.value;
        var isValid = hasConfirmValue && isMatched;

        if (needShow && !isValid) {
            confirmPasswordWrap.classList.add('error');
            confirmPasswordError.classList.add('show');
            registerMsg.classList.remove('show');
            return false;
        }

        confirmPasswordWrap.classList.remove('error');
        confirmPasswordError.classList.remove('show');
        return true;
    }

    if (registerConfirmPassword) {
        registerConfirmPassword.addEventListener('blur', function () {
            confirmTouched = true;
            validateConfirmPassword(true);
        });

        registerConfirmPassword.addEventListener('input', function () {
            if (confirmTouched) {
                validateConfirmPassword(false);
            }
        });
    }

    if (registerPassword) {
        registerPassword.addEventListener('input', function () {
            if (confirmTouched && registerConfirmPassword && registerConfirmPassword.value.length > 0) {
                validateConfirmPassword(false);
            }
        });
    }

    if (passwordToggles.length) {
        passwordToggles.forEach(function (toggleButton) {
            toggleButton.addEventListener('click', function () {
                var targetId = toggleButton.getAttribute('data-toggle-password');
                var targetInput = document.getElementById(targetId);
                var icon = toggleButton.querySelector('i');

                if (!targetInput || !icon) {
                    return;
                }

                var isHidden = targetInput.type === 'password';
                targetInput.type = isHidden ? 'text' : 'password';
                toggleButton.setAttribute('aria-pressed', isHidden ? 'true' : 'false');
                toggleButton.setAttribute('aria-label', isHidden ? 'Ẩn mật khẩu' : 'Hiện mật khẩu');

                icon.classList.remove('fa-eye', 'fa-eye-slash');
                icon.classList.add(isHidden ? 'fa-eye-slash' : 'fa-eye');
            });
        });
    }
})();
