// 회원가입 화면 (Views/Account/Register.cshtml, Register_screen.md)
(function () {
    const LOGIN_PAGE_URL = '/Account/Login';
    const ALLOWED_SPECIAL_CHARACTERS = '!"#$%&\'()*+,-./:;<=>?@[\\]^_`{|}~';
    const LOGIN_ID_PATTERN = /^[a-z0-9]{6,20}$/;
    const EMAIL_PATTERN = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
    const CODE_PATTERN = /^[0-9]{6}$/;
    const EMAIL_MAX_LENGTH = 50;
    const CODE_LIFETIME_MILLISECONDS = 10 * 60 * 1000;
    const DEFAULT_ERROR_MESSAGE = '오류가 발생했습니다. 잠시 후 다시 시도해 주세요.';

    const loginIdInput = document.getElementById('login-id');
    const passwordInput = document.getElementById('password');
    const passwordConfirmInput = document.getElementById('password-confirm');
    const emailInput = document.getElementById('email');
    const codeInput = document.getElementById('code');
    const nameInput = document.getElementById('name');
    const sendCodeButton = document.getElementById('btn-send-code');
    const confirmCodeButton = document.getElementById('btn-confirm-code');
    const registerButton = document.getElementById('btn-register');
    const codeTimer = document.getElementById('code-timer');
    const verifiedText = document.getElementById('verified-text');
    const globalError = document.getElementById('global-error');
    const antiforgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

    let timerId = null;
    let isEmailVerified = false;
    let isCodeUnusable = false;
    let currentExpiresAt = null;

    // ---------- 입력값 검사 (D-052, D-065). 통과하면 null, 실패하면 메시지 ----------
    function validateLoginId(value) {
        return LOGIN_ID_PATTERN.test(value) ? null : '아이디는 영문 소문자와 숫자 6~20자로 입력해 주세요.';
    }

    function validatePassword(value) {
        const message = '비밀번호는 8~20자이며 영문, 숫자, 특수문자를 각각 1자 이상 포함해야 합니다.';
        if (value.length < 8 || value.length > 20) {
            return message;
        }

        let hasLetter = false;
        let hasDigit = false;
        let hasSpecial = false;
        for (const character of value) {
            if (/[A-Za-z]/.test(character)) {
                hasLetter = true;
            } else if (/[0-9]/.test(character)) {
                hasDigit = true;
            } else if (ALLOWED_SPECIAL_CHARACTERS.includes(character)) {
                hasSpecial = true;
            } else {
                return message;
            }
        }

        return hasLetter && hasDigit && hasSpecial ? null : message;
    }

    function validatePasswordConfirm(value) {
        return value === passwordInput.value ? null : '비밀번호가 일치하지 않습니다.';
    }

    function validateEmail(value) {
        return value.length <= EMAIL_MAX_LENGTH && EMAIL_PATTERN.test(value) ? null : '이메일 형식이 올바르지 않습니다. (최대 50자)';
    }

    function validateCode(value) {
        return CODE_PATTERN.test(value) ? null : '인증 코드는 6자리 숫자로 입력해 주세요.';
    }

    function validateName(value) {
        const trimmed = value.trim();
        return trimmed.length >= 2 && trimmed.length <= 30 ? null : '이름은 2~30자로 입력해 주세요.';
    }

    // ---------- 검사 결과 표시 (D-067: 틀리면 빨간 테두리+메시지, 맞으면 초록 테두리+체크) ----------
    function showFieldError(input, message) {
        const errorElement = document.getElementById(input.id + '-error');
        errorElement.textContent = message;
        input.classList.remove('is-valid');
        input.classList.add('is-invalid');
    }

    function showFieldValid(input) {
        const errorElement = document.getElementById(input.id + '-error');
        errorElement.textContent = '';
        input.classList.remove('is-invalid');
        input.classList.add('is-valid');
    }

    function clearFieldState(input) {
        const errorElement = document.getElementById(input.id + '-error');
        errorElement.textContent = '';
        input.classList.remove('is-invalid', 'is-valid');
    }

    function checkField(input, validate) {
        const message = validate(input.value);
        if (message) {
            showFieldError(input, message);
            return false;
        }

        showFieldValid(input);
        return true;
    }

    function showGlobalError(message) {
        globalError.textContent = message;
        globalError.classList.remove('d-none');
    }

    function clearGlobalError() {
        globalError.textContent = '';
        globalError.classList.add('d-none');
    }

    // 오류 코드별 표시 위치 (Register_screen.md "API 오류 표시 위치")
    function showApiError(errorCode, message) {
        const fieldByCode = {
            LOGIN_ID_DUPLICATED: loginIdInput,
            EMAIL_DUPLICATED: emailInput,
            EMAIL_NOT_VERIFIED: emailInput,
            VERIFICATION_CODE_INVALID: codeInput,
            VERIFICATION_CODE_EXPIRED: codeInput,
            VERIFICATION_CODE_LOCKED: codeInput,
            NOT_FOUND: codeInput
        };
        const field = fieldByCode[errorCode];
        if (field) {
            showFieldError(field, message);
            return;
        }

        showGlobalError(message || DEFAULT_ERROR_MESSAGE);
    }

    // ---------- API 호출 (api.md 6.1) ----------
    async function postJson(url, body) {
        try {
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': antiforgeryToken
                },
                body: JSON.stringify(body)
            });
            const result = await response.json();
            if (response.ok) {
                return { isSuccess: true, data: result.data };
            }

            return { isSuccess: false, errorCode: result.error.code, message: result.error.message };
        } catch (error) {
            return { isSuccess: false, errorCode: 'INTERNAL_ERROR', message: DEFAULT_ERROR_MESSAGE };
        }
    }

    // ---------- 인증 코드 남은 시간 ----------
    function formatRemaining(milliseconds) {
        const totalSeconds = Math.max(0, Math.floor(milliseconds / 1000));
        const minutes = String(Math.floor(totalSeconds / 60)).padStart(2, '0');
        const seconds = String(totalSeconds % 60).padStart(2, '0');
        return '남은 시간 ' + minutes + ':' + seconds;
    }

    function stopTimer() {
        if (timerId !== null) {
            clearInterval(timerId);
            timerId = null;
        }
    }

    function startTimer(expiresAt) {
        stopTimer();
        const expiresAtTime = new Date(expiresAt).getTime();
        codeTimer.classList.remove('d-none', 'text-danger');

        function tick() {
            const remaining = expiresAtTime - Date.now();
            codeTimer.textContent = formatRemaining(remaining);
            if (remaining <= 0) {
                stopTimer();
                isCodeUnusable = true;
                codeTimer.classList.add('text-danger');
                confirmCodeButton.disabled = true;
                showFieldError(codeInput, '인증 코드가 만료되었습니다. 재발송해 주세요.');
            }
        }

        tick();
        timerId = setInterval(tick, 1000);
    }

    // ---------- 버튼 동작 ----------
    async function sendCode() {
        clearGlobalError();
        if (!checkField(emailInput, validateEmail)) {
            return;
        }

        // 응답을 기다리지 않고 바로 "코드 발송 후" 화면으로 바꾼다 (D-069).
        // 발송에 실패하면 누르기 전 상태로 되돌리고 오류를 표시한다
        const previousExpiresAt = currentExpiresAt;
        const wasCodeUnusable = isCodeUnusable;
        showCodeSentState(new Date(Date.now() + CODE_LIFETIME_MILLISECONDS).toISOString());
        sendCodeButton.disabled = true; // 응답 전에 다시 눌러 중복 발송되지 않게 한다

        const result = await postJson('/api/email-verifications', { email: emailInput.value });

        sendCodeButton.disabled = false;
        if (!result.isSuccess) {
            restoreCodeState(previousExpiresAt, wasCodeUnusable);
            showApiError(result.errorCode, result.message);
            return;
        }

        // 남은 시간은 서버가 정한 만료 시각에 맞춘다
        currentExpiresAt = result.data.expiresAt;
        startTimer(currentExpiresAt);
    }

    function showCodeSentState(expiresAt) {
        sendCodeButton.textContent = '재발송';
        codeInput.disabled = false;
        codeInput.value = '';
        clearFieldState(codeInput);
        isCodeUnusable = false;
        confirmCodeButton.disabled = false;
        startTimer(expiresAt);
        codeInput.focus();
    }

    function restoreCodeState(previousExpiresAt, wasCodeUnusable) {
        if (previousExpiresAt === null) {
            // 처음 발송에 실패하면 처음 열었을 때 상태로 되돌린다
            stopTimer();
            sendCodeButton.textContent = '인증 코드 발송';
            codeInput.disabled = true;
            codeInput.value = '';
            clearFieldState(codeInput);
            confirmCodeButton.disabled = true;
            codeTimer.classList.add('d-none');
            isCodeUnusable = false;
            return;
        }

        // 재발송에 실패하면 이전 코드의 남은 시간으로 되돌린다 (이전 코드는 그대로 쓸 수 있다)
        startTimer(previousExpiresAt);
        if (wasCodeUnusable) {
            isCodeUnusable = true;
            confirmCodeButton.disabled = true;
            codeTimer.classList.add('text-danger');
        }
    }

    async function confirmCode() {
        clearGlobalError();
        if (!checkField(codeInput, validateCode)) {
            return;
        }

        confirmCodeButton.disabled = true;
        const result = await postJson('/api/email-verifications/confirm', { email: emailInput.value, code: codeInput.value });
        if (!result.isSuccess) {
            // 만료되었거나 5번 틀려 무효가 된 코드는 재발송 전까지 확인 버튼을 끈다 (D-068)
            const isUnusable = result.errorCode === 'VERIFICATION_CODE_EXPIRED' || result.errorCode === 'VERIFICATION_CODE_LOCKED';
            confirmCodeButton.disabled = isUnusable;
            if (isUnusable) {
                stopTimer();
                isCodeUnusable = true;
                codeTimer.classList.add('text-danger');
                if (result.errorCode === 'VERIFICATION_CODE_EXPIRED') {
                    codeTimer.textContent = formatRemaining(0);
                }
            }

            showApiError(result.errorCode, result.message);
            return;
        }

        isEmailVerified = true;
        stopTimer();
        codeTimer.classList.add('d-none');
        showFieldValid(codeInput);
        showFieldValid(emailInput);
        emailInput.readOnly = true;
        codeInput.readOnly = true;
        sendCodeButton.disabled = true;
        confirmCodeButton.disabled = true;
        verifiedText.classList.remove('d-none');
        registerButton.disabled = false;
    }

    async function register() {
        clearGlobalError();
        const checks = [
            checkField(loginIdInput, validateLoginId),
            checkField(passwordInput, validatePassword),
            checkField(passwordConfirmInput, validatePasswordConfirm),
            checkField(emailInput, validateEmail),
            checkField(nameInput, validateName)
        ];
        if (checks.includes(false)) {
            return;
        }

        if (!isEmailVerified) {
            showFieldError(emailInput, '이메일 인증을 완료해 주세요.');
            return;
        }

        registerButton.disabled = true;
        registerButton.textContent = '처리 중...';

        // 비밀번호 확인 값은 서버로 보내지 않는다 (D-065)
        const result = await postJson('/api/users', {
            loginId: loginIdInput.value,
            password: passwordInput.value,
            email: emailInput.value,
            name: nameInput.value.trim()
        });

        if (!result.isSuccess) {
            registerButton.disabled = false;
            registerButton.textContent = '가입하기';
            showApiError(result.errorCode, result.message);
            return;
        }

        alert('가입이 완료되었습니다.');
        window.location.href = LOGIN_PAGE_URL;
    }

    // ---------- 이벤트 연결 ----------
    // 입력할 때마다(첫 글자부터) 바로 검사하고, 아무것도 입력하지 않고 칸을 벗어날 때도 검사한다 (D-067)
    function watchField(input, validate, afterCheck) {
        function onChange() {
            if (input.readOnly || input.disabled) {
                return;
            }

            checkField(input, validate);
            if (afterCheck) {
                afterCheck();
            }
        }

        input.addEventListener('input', onChange);
        input.addEventListener('blur', onChange);
    }

    watchField(loginIdInput, validateLoginId);
    watchField(passwordInput, validatePassword, function () {
        // 비밀번호를 바꾸면 이미 입력한 비밀번호 확인도 다시 비교한다
        if (passwordConfirmInput.value) {
            checkField(passwordConfirmInput, validatePasswordConfirm);
        }
    });
    watchField(passwordConfirmInput, validatePasswordConfirm);
    watchField(emailInput, validateEmail);
    watchField(nameInput, validateName);
    codeInput.addEventListener('input', function () {
        // 만료되었거나 5번 틀려 무효가 된 코드는 형식이 맞아도 메시지를 그대로 둔다
        if (!codeInput.readOnly && !isCodeUnusable) {
            checkField(codeInput, validateCode);
        }
    });
    sendCodeButton.addEventListener('click', sendCode);
    confirmCodeButton.addEventListener('click', confirmCode);
    registerButton.addEventListener('click', register);
})();
