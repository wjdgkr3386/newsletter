// 뉴스레터 설정 관리 화면 (Views/Admin/Settings.cshtml, Settings_screen.md, D-043, D-079)
(function () {
    const SETTINGS_URL = '/api/newsletter-settings';
    const SUCCESS_ALERT_MILLISECONDS = 5000;
    const DEFAULT_ERROR_MESSAGE = '오류가 발생했습니다. 잠시 후 다시 시도해 주세요.';

    const antiforgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;
    const pageAlert = document.getElementById('page-alert');
    const settingsLoading = document.getElementById('settings-loading');
    const settingsCard = document.getElementById('settings-card');
    const generateHour = document.getElementById('generate-hour');
    const generateMinute = document.getElementById('generate-minute');
    const sendHour = document.getElementById('send-hour');
    const sendMinute = document.getElementById('send-minute');
    const timeSelects = [generateHour, generateMinute, sendHour, sendMinute];
    const timeError = document.getElementById('time-error');
    const saveButton = document.getElementById('btn-save');

    // 마지막으로 저장된(불러온) 값. 이 값과 같으면 저장 버튼을 끈다
    let savedSetting = null;
    let isSaving = false;
    let alertTimerId = null;

    // ---------- API 호출 (api.md 6.3) ----------
    async function requestJson(url, options) {
        try {
            const response = await fetch(url, options);
            const result = await response.json();
            if (response.ok) {
                return { isSuccess: true, data: result.data };
            }

            return { isSuccess: false, errorCode: result.error.code, message: result.error.message };
        } catch (error) {
            return { isSuccess: false, errorCode: 'INTERNAL_ERROR', message: DEFAULT_ERROR_MESSAGE };
        }
    }

    // ---------- 표시 ----------
    function showPageAlert(isSuccess, message) {
        if (alertTimerId !== null) {
            clearTimeout(alertTimerId);
            alertTimerId = null;
        }

        pageAlert.textContent = message;
        pageAlert.className = 'alert ' + (isSuccess ? 'alert-success' : 'alert-danger');
        if (isSuccess) {
            alertTimerId = setTimeout(function () {
                pageAlert.classList.add('d-none');
                alertTimerId = null;
            }, SUCCESS_ALERT_MILLISECONDS);
        }
    }

    function hidePageAlert() {
        pageAlert.classList.add('d-none');
    }

    function showTimeError(message) {
        timeError.textContent = message;
        timeError.classList.remove('d-none');
        timeSelects.forEach(function (select) { select.classList.add('is-invalid'); });
    }

    function clearTimeError() {
        timeError.classList.add('d-none');
        timeSelects.forEach(function (select) { select.classList.remove('is-invalid'); });
    }

    // ---------- 값 다루기 ----------
    function getSelectedSetting() {
        return {
            generateTime: generateHour.value + ':' + generateMinute.value,
            sendTime: sendHour.value + ':' + sendMinute.value
        };
    }

    function applySetting(setting) {
        const generateParts = setting.generateTime.split(':');
        const sendParts = setting.sendTime.split(':');
        generateHour.value = generateParts[0];
        generateMinute.value = generateParts[1];
        sendHour.value = sendParts[0];
        sendMinute.value = sendParts[1];
    }

    // 값을 바꾸면 바로 검사한다. "HH:mm" 문자열은 같은 길이라 문자열 비교로 시각 순서를 비교할 수 있다
    function checkSelection() {
        const selected = getSelectedSetting();
        const isTimeValid = selected.generateTime < selected.sendTime;
        if (isTimeValid) {
            clearTimeError();
        } else {
            showTimeError('생성 시간은 발송 시간보다 앞서야 합니다.');
        }

        const isChanged = savedSetting === null
            || selected.generateTime !== savedSetting.generateTime
            || selected.sendTime !== savedSetting.sendTime;
        saveButton.disabled = isSaving || !isTimeValid || !isChanged;
    }

    // ---------- 불러오기, 저장 ----------
    async function loadSetting() {
        const result = await requestJson(SETTINGS_URL, { method: 'GET' });
        settingsLoading.classList.add('d-none');
        if (!result.isSuccess) {
            showPageAlert(false, result.message || DEFAULT_ERROR_MESSAGE);
            return;
        }

        savedSetting = result.data;
        applySetting(savedSetting);
        settingsCard.classList.remove('d-none');
        checkSelection();
    }

    async function saveSetting() {
        if (isSaving) {
            return;
        }

        hidePageAlert();
        isSaving = true;
        saveButton.disabled = true;
        saveButton.textContent = '저장 중...';

        const result = await requestJson(SETTINGS_URL, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': antiforgeryToken
            },
            body: JSON.stringify(getSelectedSetting())
        });

        isSaving = false;
        saveButton.textContent = '저장';
        if (!result.isSuccess) {
            if (result.errorCode === 'INVALID_SETTING_TIME') {
                showTimeError(result.message);
            } else {
                showPageAlert(false, result.message || DEFAULT_ERROR_MESSAGE);
                checkSelection();
            }

            return;
        }

        savedSetting = result.data;
        applySetting(savedSetting);
        checkSelection();
        showPageAlert(true, '설정을 저장했습니다.');
    }

    // ---------- 이벤트 연결 ----------
    timeSelects.forEach(function (select) { select.addEventListener('change', checkSelection); });
    saveButton.addEventListener('click', saveSetting);

    loadSetting();
})();
