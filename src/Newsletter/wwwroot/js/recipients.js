// 수신자 관리 화면 (Views/Admin/Recipients.cshtml, Recipients_screen.md, D-076, D-077)
(function () {
    const PAGE_SIZE = 10;
    const SUCCESS_ALERT_MILLISECONDS = 5000;
    const DEFAULT_ERROR_MESSAGE = '오류가 발생했습니다. 잠시 후 다시 시도해 주세요.';

    // 팝업 종류별 문구와 API
    const PICKER_MODES = {
        add: {
            title: '수신자 추가',
            submitText: '추가',
            submittingText: '추가 중...',
            submitClass: 'btn-primary',
            emptyText: '추가할 수 있는 회원이 없습니다.',
            listUrl: '/api/users?isRecipient=false',
            submitUrl: '/api/recipients',
            successText: '명을 수신자로 추가했습니다.'
        },
        delete: {
            title: '수신자 삭제',
            submitText: '삭제',
            submittingText: '삭제 중...',
            submitClass: 'btn-danger',
            emptyText: '삭제할 수신자가 없습니다.',
            listUrl: '/api/recipients',
            submitUrl: '/api/recipients/delete',
            successText: '명을 수신자에서 삭제했습니다.'
        }
    };

    const antiforgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;
    const pageAlert = document.getElementById('page-alert');
    const recipientCount = document.getElementById('recipient-count');
    const recipientLoading = document.getElementById('recipient-loading');
    const recipientTable = document.getElementById('recipient-table');
    const recipientRows = document.getElementById('recipient-rows');
    const recipientPaging = document.getElementById('recipient-paging');
    const recipientEmpty = document.getElementById('recipient-empty');
    const pickerModalElement = document.getElementById('picker-modal');
    const pickerModal = new bootstrap.Modal(pickerModalElement);
    const pickerTitle = document.getElementById('picker-title');
    const pickerError = document.getElementById('picker-error');
    const pickerSearch = document.getElementById('picker-search');
    const pickerLoading = document.getElementById('picker-loading');
    const pickerTable = document.getElementById('picker-table');
    const pickerRows = document.getElementById('picker-rows');
    const pickerCheckAll = document.getElementById('picker-check-all');
    const pickerEmpty = document.getElementById('picker-empty');
    const pickerPaging = document.getElementById('picker-paging');
    const pickerSelected = document.getElementById('picker-selected');
    const pickerSubmit = document.getElementById('picker-submit');

    let recipients = [];
    let mainPage = 1;
    let alertTimerId = null;

    let pickerMode = PICKER_MODES.add;
    let pickerItems = [];
    let pickerFilteredItems = [];
    let pickerPage = 1;
    let selectedUserIds = new Set();
    let isSubmitting = false;

    // ---------- API 호출 (api.md 6.2) ----------
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

    function getJson(url) {
        return requestJson(url, { method: 'GET' });
    }

    function postJson(url, body) {
        return requestJson(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': antiforgeryToken
            },
            body: JSON.stringify(body)
        });
    }

    // ---------- 공통 표시 ----------
    function createCell(text) {
        const cell = document.createElement('td');
        cell.textContent = text;
        return cell;
    }

    function getPageCount(totalCount) {
        return Math.max(1, Math.ceil(totalCount / PAGE_SIZE));
    }

    function getPageItems(items, page) {
        return items.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
    }

    // 페이지 번호: "이전 1 2 3 다음". 1페이지뿐이면 숨긴다 (D-077)
    function renderPaging(nav, totalCount, currentPage, isSmall, onChange) {
        nav.textContent = '';
        const pageCount = getPageCount(totalCount);
        nav.classList.toggle('d-none', pageCount <= 1);
        if (pageCount <= 1) {
            return;
        }

        const list = document.createElement('ul');
        list.className = 'pagination justify-content-center mb-0' + (isSmall ? ' pagination-sm' : '');

        function addItem(label, targetPage, isDisabled, isActive) {
            const item = document.createElement('li');
            item.className = 'page-item' + (isDisabled ? ' disabled' : '') + (isActive ? ' active' : '');
            const link = document.createElement('a');
            link.className = 'page-link';
            link.href = '#';
            link.textContent = label;
            link.addEventListener('click', function (event) {
                event.preventDefault();
                if (!isDisabled && !isActive) {
                    onChange(targetPage);
                }
            });
            item.appendChild(link);
            list.appendChild(item);
        }

        addItem('이전', currentPage - 1, currentPage === 1, false);
        for (let page = 1; page <= pageCount; page++) {
            addItem(String(page), page, false, page === currentPage);
        }
        addItem('다음', currentPage + 1, currentPage === pageCount, false);
        nav.appendChild(list);
    }

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

    // ---------- 본문 수신자 목록 ----------
    function renderMain() {
        recipientLoading.classList.add('d-none');
        recipientCount.textContent = '수신자 ' + recipients.length + '명';
        recipientTable.classList.toggle('d-none', recipients.length === 0);
        recipientEmpty.classList.toggle('d-none', recipients.length !== 0);

        // 추가/삭제로 페이지 수가 줄면 마지막 페이지로 옮긴다
        mainPage = Math.min(mainPage, getPageCount(recipients.length));
        recipientRows.textContent = '';
        getPageItems(recipients, mainPage).forEach(function (recipient) {
            const row = document.createElement('tr');
            row.appendChild(createCell(recipient.name));
            row.appendChild(createCell(recipient.email));
            recipientRows.appendChild(row);
        });

        renderPaging(recipientPaging, recipients.length, mainPage, false, function (page) {
            mainPage = page;
            renderMain();
        });
    }

    async function loadRecipients() {
        const result = await getJson('/api/recipients');
        if (!result.isSuccess) {
            recipientLoading.classList.add('d-none');
            showPageAlert(false, result.message || DEFAULT_ERROR_MESSAGE);
            return;
        }

        recipients = result.data;
        renderMain();
    }

    // ---------- 추가/삭제 팝업 ----------
    function getItemUserId(item) {
        // 회원 목록은 id, 수신자 목록은 userId로 온다
        return pickerMode === PICKER_MODES.add ? item.id : item.userId;
    }

    function updateSelectionState() {
        pickerSelected.textContent = selectedUserIds.size + '명 선택';
        pickerSubmit.disabled = isSubmitting || selectedUserIds.size === 0;

        const pageItems = getPageItems(pickerFilteredItems, pickerPage);
        const selectedOnPage = pageItems.filter(function (item) { return selectedUserIds.has(getItemUserId(item)); }).length;
        pickerCheckAll.checked = pageItems.length > 0 && selectedOnPage === pageItems.length;
        pickerCheckAll.indeterminate = selectedOnPage > 0 && selectedOnPage < pageItems.length;
    }

    function toggleSelection(userId) {
        if (selectedUserIds.has(userId)) {
            selectedUserIds.delete(userId);
        } else {
            selectedUserIds.add(userId);
        }

        renderPickerRows();
    }

    function renderPickerRows() {
        pickerRows.textContent = '';
        getPageItems(pickerFilteredItems, pickerPage).forEach(function (item) {
            const userId = getItemUserId(item);
            const isSelected = selectedUserIds.has(userId);
            const row = document.createElement('tr');
            if (isSelected) {
                row.classList.add('table-active');
            }

            const checkCell = document.createElement('td');
            const checkBox = document.createElement('input');
            checkBox.type = 'checkbox';
            checkBox.className = 'form-check-input';
            checkBox.checked = isSelected;
            checkBox.setAttribute('aria-label', item.name + ' 선택');
            checkCell.appendChild(checkBox);
            row.appendChild(checkCell);
            row.appendChild(createCell(item.name));
            row.appendChild(createCell(item.email));

            // 체크박스나 행 아무 곳을 눌러도 선택/해제한다
            row.addEventListener('click', function () { toggleSelection(userId); });
            pickerRows.appendChild(row);
        });

        updateSelectionState();
    }

    function renderPicker() {
        pickerLoading.classList.add('d-none');
        const hasItems = pickerItems.length > 0;
        const hasFilteredItems = pickerFilteredItems.length > 0;
        pickerTable.classList.toggle('d-none', !hasFilteredItems);
        pickerEmpty.classList.toggle('d-none', hasFilteredItems);
        pickerEmpty.textContent = hasItems ? '검색 결과가 없습니다.' : pickerMode.emptyText;

        pickerPage = Math.min(pickerPage, getPageCount(pickerFilteredItems.length));
        renderPickerRows();
        renderPaging(pickerPaging, pickerFilteredItems.length, pickerPage, true, function (page) {
            pickerPage = page;
            renderPicker();
        });
    }

    // 검색: 이름·이메일에 포함되면 보인다 (대소문자 구분 없음). 검색하면 1페이지로 돌아가고 선택은 유지한다
    function applySearch() {
        const keyword = pickerSearch.value.trim().toLowerCase();
        pickerFilteredItems = pickerItems.filter(function (item) {
            return keyword === ''
                || item.name.toLowerCase().includes(keyword)
                || item.email.toLowerCase().includes(keyword);
        });
        pickerPage = 1;
        renderPicker();
    }

    async function loadPickerItems() {
        pickerLoading.classList.remove('d-none');
        pickerTable.classList.add('d-none');
        pickerEmpty.classList.add('d-none');
        pickerPaging.classList.add('d-none');

        const result = await getJson(pickerMode.listUrl);
        if (!result.isSuccess) {
            pickerLoading.classList.add('d-none');
            showPickerError(result.message || DEFAULT_ERROR_MESSAGE);
            return;
        }

        pickerItems = result.data;
        // 목록에서 사라진 회원은 선택에서도 뺀다
        const loadedIds = new Set(pickerItems.map(getItemUserId));
        selectedUserIds = new Set(Array.from(selectedUserIds).filter(function (userId) { return loadedIds.has(userId); }));
        applySearch();
    }

    function showPickerError(message) {
        pickerError.textContent = message;
        pickerError.classList.remove('d-none');
    }

    function openPicker(mode) {
        pickerMode = mode;
        pickerItems = [];
        pickerFilteredItems = [];
        pickerPage = 1;
        selectedUserIds = new Set();
        isSubmitting = false;

        pickerTitle.textContent = mode.title;
        pickerSubmit.textContent = mode.submitText;
        pickerSubmit.className = 'btn ms-1 ' + mode.submitClass;
        pickerError.classList.add('d-none');
        pickerSearch.value = '';
        pickerRows.textContent = '';
        updateSelectionState();

        pickerModal.show();
        loadPickerItems();
    }

    async function submitPicker() {
        if (isSubmitting || selectedUserIds.size === 0) {
            return;
        }

        isSubmitting = true;
        pickerSubmit.disabled = true;
        pickerSubmit.textContent = pickerMode.submittingText;
        pickerError.classList.add('d-none');

        const result = await postJson(pickerMode.submitUrl, { userIds: Array.from(selectedUserIds) });

        isSubmitting = false;
        pickerSubmit.textContent = pickerMode.submitText;
        if (!result.isSuccess) {
            // 실패하면 팝업을 닫지 않고, 다른 관리자가 먼저 바꾼 경우에 대비해 목록을 다시 불러온다
            showPickerError(result.message || DEFAULT_ERROR_MESSAGE);
            updateSelectionState();
            await Promise.all([loadPickerItems(), loadRecipients()]);
            return;
        }

        const successText = result.data.count + pickerMode.successText;
        pickerModal.hide();
        await loadRecipients();
        showPageAlert(true, successText);
    }

    // ---------- 이벤트 연결 ----------
    document.getElementById('btn-open-add').addEventListener('click', function () { openPicker(PICKER_MODES.add); });
    document.getElementById('btn-open-delete').addEventListener('click', function () { openPicker(PICKER_MODES.delete); });
    pickerSearch.addEventListener('input', applySearch);
    pickerSubmit.addEventListener('click', submitPicker);
    pickerCheckAll.addEventListener('click', function () {
        // 지금 페이지에 보이는 행만 모두 선택/해제한다 (D-077)
        const pageIds = getPageItems(pickerFilteredItems, pickerPage).map(getItemUserId);
        const isAllSelected = pageIds.every(function (userId) { return selectedUserIds.has(userId); });
        pageIds.forEach(function (userId) {
            if (isAllSelected) {
                selectedUserIds.delete(userId);
            } else {
                selectedUserIds.add(userId);
            }
        });
        renderPickerRows();
    });
    pickerModalElement.addEventListener('shown.bs.modal', function () { pickerSearch.focus(); });

    loadRecipients();
})();
