// 로그인 화면 (Views/Account/Login.cshtml, Login_screen.md)
(function () {
    const DEFAULT_ERROR_MESSAGE = '오류가 발생했습니다. 잠시 후 다시 시도해 주세요.';

    const loginIdInput = document.getElementById('login-id');
    const passwordInput = document.getElementById('password');
    const loginButton = document.getElementById('btn-login');
    const globalError = document.getElementById('global-error');
    const antiforgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

    let isSubmitting = false;

    function showFieldError(input, message) {
        document.getElementById(input.id + '-error').textContent = message;
        input.classList.add('is-invalid');
    }

    function clearFieldError(input) {
        document.getElementById(input.id + '-error').textContent = '';
        input.classList.remove('is-invalid');
    }

    function showGlobalError(message) {
        globalError.textContent = message;
        globalError.classList.remove('d-none');
    }

    function clearGlobalError() {
        globalError.textContent = '';
        globalError.classList.add('d-none');
    }

    // 빈 칸인지만 검사한다. 형식 규칙은 알려주지 않는다 (Login_screen.md)
    function checkRequired() {
        let isValid = true;
        if (!loginIdInput.value) {
            showFieldError(loginIdInput, '아이디를 입력해 주세요.');
            isValid = false;
        }

        if (!passwordInput.value) {
            showFieldError(passwordInput, '비밀번호를 입력해 주세요.');
            isValid = false;
        }

        return isValid;
    }

    async function postLogin() {
        try {
            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': antiforgeryToken
                },
                body: JSON.stringify({ loginId: loginIdInput.value, password: passwordInput.value })
            });
            const result = await response.json();
            if (response.ok) {
                return { isSuccess: true, data: result.data };
            }

            return { isSuccess: false, message: result.error.message };
        } catch (error) {
            return { isSuccess: false, message: DEFAULT_ERROR_MESSAGE };
        }
    }

    async function login() {
        if (isSubmitting) {
            return;
        }

        clearGlobalError();
        if (!checkRequired()) {
            return;
        }

        isSubmitting = true;
        loginButton.disabled = true;
        loginButton.textContent = '로그인 중...';

        const result = await postLogin();
        if (result.isSuccess) {
            // 권한별 첫 화면으로 이동한다 (D-048)
            window.location.href = result.data.redirectUrl;
            return;
        }

        isSubmitting = false;
        loginButton.disabled = false;
        loginButton.textContent = '로그인';
        showGlobalError(result.message || DEFAULT_ERROR_MESSAGE);
        passwordInput.value = '';
        passwordInput.focus();
    }

    function onInput(event) {
        clearFieldError(event.target);
        clearGlobalError();
    }

    function onKeyDown(event) {
        if (event.key === 'Enter') {
            event.preventDefault();
            login();
        }
    }

    loginIdInput.addEventListener('input', onInput);
    passwordInput.addEventListener('input', onInput);
    loginIdInput.addEventListener('keydown', onKeyDown);
    passwordInput.addEventListener('keydown', onKeyDown);
    loginButton.addEventListener('click', login);
})();
