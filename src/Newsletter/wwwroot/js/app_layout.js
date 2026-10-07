// 로그인 후 공통 레이아웃 (Views/Shared/_AppLayout.cshtml, _AppLayout_screen.md)
(function () {
    const LOGIN_PAGE_URL = '/Account/Login';

    const logoutButton = document.getElementById('btn-logout');
    const antiforgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

    async function logout() {
        logoutButton.disabled = true;
        try {
            await fetch('/api/auth/logout', {
                method: 'POST',
                headers: { 'RequestVerificationToken': antiforgeryToken }
            });
        } catch (error) {
            // 호출이 실패해도 로그인 화면으로 이동한다 (_AppLayout_screen.md)
        }

        window.location.href = LOGIN_PAGE_URL;
    }

    logoutButton.addEventListener('click', logout);
})();
