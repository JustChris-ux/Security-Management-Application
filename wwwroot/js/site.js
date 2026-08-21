// ─── FAB ────────────────────────────────────────────────────────────────────
function toggleFab() {
    const main = document.getElementById('fab-main');
    const menu = document.getElementById('fab-menu');
    const overlay = document.getElementById('fab-overlay');
    const icon = document.getElementById('fab-icon');
    if (!main) return;

    const isOpen = menu.classList.contains('open');
    menu.classList.toggle('open');
    overlay.classList.toggle('open');
    main.classList.toggle('open');

    if (!isOpen) {
        icon.classList.replace('fa-plus', 'fa-xmark');
        document.body.style.overflow = 'hidden';
    } else {
        icon.classList.replace('fa-xmark', 'fa-plus');
        document.body.style.overflow = '';
    }
}

// ─── Sidebar (mobile) ────────────────────────────────────────────────────────
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebar-overlay');
    sidebar?.classList.toggle('open');
    overlay?.classList.toggle('open');
}

// ─── Modal helpers ───────────────────────────────────────────────────────────
function openModal(id) {
    const el = document.getElementById(id);
    if (el) {
        el.classList.add('open');
        document.body.style.overflow = 'hidden';
    }
}

function closeModal(id) {
    const el = document.getElementById(id);
    if (el) {
        el.classList.remove('open');
        document.body.style.overflow = '';
    }
}

// Close modal on overlay click
document.addEventListener('click', function (e) {
    if (e.target.classList.contains('modal-overlay')) {
        e.target.classList.remove('open');
        document.body.style.overflow = '';
    }
});

// Close modal on ESC
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        document.querySelectorAll('.modal-overlay.open').forEach(m => {
            m.classList.remove('open');
        });
        document.body.style.overflow = '';
    }
});

// ─── Confirm dialog ──────────────────────────────────────────────────────────
function confirmAction(message, formId) {
    if (confirm(message)) {
        document.getElementById(formId)?.submit();
    }
}

// ─── Camera rows (in location create/edit forms) ──────────────────────────────
let cameraCount = 0;
function addCameraRow() {
    cameraCount++;
    const container = document.getElementById('cameras-container');
    if (!container) return;
    const row = document.createElement('div');
    row.className = 'camera-row d-flex gap-8 align-center mt-8';
    row.id = `cam-row-${cameraCount}`;
    row.innerHTML = `
        <input type="text" name="newCameraNames" placeholder="Наименование на камерата"
               class="form-control" style="flex:2" />
        <input type="text" name="newCameraPositions" placeholder="Местоположение (незадължително)"
               class="form-control" style="flex:3" />
        <button type="button" class="btn btn-ghost btn-icon-sm" onclick="removeCameraRow('cam-row-${cameraCount}')">
            <i class="fas fa-trash" style="color:var(--danger)"></i>
        </button>
    `;
    container.appendChild(row);
}

function removeCameraRow(rowId) {
    document.getElementById(rowId)?.remove();
}

// ─── Accordion ───────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.docs-accordion-header').forEach(header => {
        header.addEventListener('click', function () {
            const body = this.nextElementSibling;
            const icon = this.querySelector('.accordion-icon');
            body.classList.toggle('open');
            if (icon) icon.style.transform = body.classList.contains('open') ? 'rotate(180deg)' : '';
        });
    });
});

// ─── Schedule cell editor ────────────────────────────────────────────────────
let currentCellData = {};

function openScheduleModal(staffId, staffName, locationId, day, month, year, currentHours, currentShift, currentNotes) {
    currentCellData = { staffId, locationId, day, month, year };

    document.getElementById('modal-staff-name').textContent = staffName;
    document.getElementById('modal-day-label').textContent = `${day} ${monthNameBg(month)} ${year}`;
    document.getElementById('schedule-hours').value = currentHours || '';
    document.getElementById('schedule-notes').value = currentNotes || '';

    // Set shift radio
    const shift = currentShift || 'D';
    document.getElementById('shift-day').checked = shift === 'D';
    document.getElementById('shift-night').checked = shift === 'N';

    openModal('schedule-modal');
}

function monthNameBg(m) {
    const months = ['Яну', 'Фев', 'Мар', 'Апр', 'Май', 'Юни',
                    'Юли', 'Авг', 'Сеп', 'Окт', 'Ное', 'Дек'];
    return months[m - 1] || '';
}

async function saveScheduleCell() {
    const hours = parseFloat(document.getElementById('schedule-hours').value) || 0;
    const shift = document.querySelector('input[name="shift"]:checked')?.value || 'D';
    const notes = document.getElementById('schedule-notes').value;

    const payload = {
        ...currentCellData,
        hours,
        shiftType: shift,
        notes
    };

    try {
        const resp = await fetch('/Locations/SaveScheduleEntry', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (resp.ok) {
            const data = await resp.json();
            updateScheduleCell(currentCellData.staffId, currentCellData.day, hours, shift, data);
            closeModal('schedule-modal');
        }
    } catch (e) {
        alert('Грешка при запазване. Моля опитайте отново.');
    }
}

function updateScheduleCell(staffId, day, hours, shift, serverData) {
    const cellId = `cell-${staffId}-${day}`;
    const cell = document.getElementById(cellId);
    if (!cell) return;

    if (hours <= 0) {
        cell.className = 'schedule-cell-btn';
        cell.innerHTML = '<span style="color:var(--text-muted);font-size:10px">-</span>';
        cell.dataset.shift = '';
        cell.dataset.hours = '0';
    } else {
        const shiftLabel = shift === 'N' ? 'Н' : 'Д';
        const shiftClass = shift === 'N' ? 'night' : 'day';
        cell.className = `schedule-cell-btn has-entry ${shiftClass}`;
        cell.dataset.shift = shift;
        cell.dataset.hours = hours;
        cell.innerHTML = `
            <span class="cell-shift ${shiftClass}">${shiftLabel}</span>
            <span class="cell-hours">${hours}ч.</span>
        `;
    }

    // Update total row
    recalcStaffTotal(staffId);
}

function recalcStaffTotal(staffId) {
    const cells = document.querySelectorAll(`[id^="cell-${staffId}-"]`);
    let total = 0;
    cells.forEach(c => { total += parseFloat(c.dataset.hours || '0'); });
    const totalEl = document.getElementById(`total-${staffId}`);
    if (totalEl) totalEl.textContent = total > 0 ? total.toFixed(1) : '-';
}

// ─── Print schedule ──────────────────────────────────────────────────────────
function printSchedule() {
    window.print();
}

// ─── Month navigator ─────────────────────────────────────────────────────────
function navigateMonth(direction) {
    const params = new URLSearchParams(window.location.search);
    let month = parseInt(params.get('month') || new Date().getMonth() + 1);
    let year  = parseInt(params.get('year')  || new Date().getFullYear());
    const id = params.get('id') || window.location.pathname.split('/').pop();

    month += direction;
    if (month > 12) { month = 1; year++; }
    if (month < 1)  { month = 12; year--; }

    window.location.href = `?month=${month}&year=${year}`;
}

// ─── Toast auto-close ────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    setTimeout(() => {
        document.querySelectorAll('.notification').forEach(n => {
            n.style.transition = 'opacity .4s';
            n.style.opacity = '0';
            setTimeout(() => n.remove(), 400);
        });
    }, 4500);
});
