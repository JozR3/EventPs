// ============================================
// ESTADO GLOBAL
// ============================================
let currentUser = null;
let selectedSeats = [];
let currentEventId = null;
let sectorPrices = {};

window.addEventListener('DOMContentLoaded', () => {
    const saved = localStorage.getItem('user');
    if (saved) {
        try { currentUser = JSON.parse(saved); } catch { currentUser = null; }
    }
    updateHeader();
    loadEvents();
});

// ============================================
// FUNCIONES DE AUTENTICACIÓN
// ============================================

function openLoginModal() {
    document.getElementById('authModal').classList.add('active');
    document.getElementById('loginForm').style.display = 'block';
    document.getElementById('registerForm').style.display = 'none';
    document.getElementById('modalTitle').textContent = 'Iniciar Sesión';
    clearMessages();
}

function closeAuthModal() {
    document.getElementById('authModal').classList.remove('active');
    clearMessages();
}

function toggleToRegister() {
    document.getElementById('loginForm').style.display = 'none';
    document.getElementById('registerForm').style.display = 'block';
    document.getElementById('modalTitle').textContent = 'Registrarse';
    clearMessages();
}

function toggleToLogin() {
    document.getElementById('registerForm').style.display = 'none';
    document.getElementById('loginForm').style.display = 'block';
    document.getElementById('modalTitle').textContent = 'Iniciar Sesión';
    clearMessages();
}

async function handleLogin(event) {
    event.preventDefault();

    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;

    try {
        const res = await fetch('/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });

        if (!res.ok) {
            const data = await res.json();
            showErrorMessage(data.message || 'Error al iniciar sesión');
            return;
        }

        const user = await res.json();
        currentUser = user;

        localStorage.setItem('user', JSON.stringify(user));

        showSuccessMessage('✅ Sesión iniciada correctamente');
        setTimeout(() => {
            closeAuthModal();
            updateHeader();
            loadEvents();
        }, 1500);

    } catch (error) {
        showErrorMessage('Error de conexión: ' + error.message);
    }
}

async function handleRegister(event) {
    event.preventDefault();

    const name = document.getElementById('registerName').value;
    const email = document.getElementById('registerEmail').value;
    const password = document.getElementById('registerPassword').value;
    const confirmPassword = document.getElementById('registerConfirmPassword').value;

    if (password !== confirmPassword) {
        showErrorMessage('Las contraseñas no coinciden');
        return;
    }

    try {
        const res = await fetch('/api/auth/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name, email, password, confirmPassword })
        });

        if (!res.ok) {
            const data = await res.json();
            showErrorMessage(data.message || 'Error al registrarse');
            return;
        }

        const user = await res.json();
        currentUser = user;

        localStorage.setItem('user', JSON.stringify(user));

        showSuccessMessage('✅ Registro exitoso. Bienvenido ' + user.name);
        setTimeout(() => {
            closeAuthModal();
            updateHeader();
            loadEvents();
        }, 1500);

    } catch (error) {
        showErrorMessage('Error de conexión: ' + error.message);
    }
}

function logout() {
    currentUser = null;
    localStorage.removeItem('user');
    selectedSeats = [];
    updateHeader();
    loadEvents();
}

function updateHeader() {
    const headerRight = document.getElementById('headerRight');

    if (currentUser) {
        headerRight.innerHTML = `
            <div class="user-info">
                👤 ${currentUser.name}
            </div>
            <button class="btn-logout" onclick="logout()">Cerrar Sesión</button>
        `;
    } else {
        headerRight.innerHTML = `
            <button class="btn-logout" onclick="openLoginModal()">Iniciar Sesión</button>
        `;
    }
}

function showErrorMessage(message) {
    document.getElementById('messageContainer').innerHTML = `<div class="error-message">${message}</div>`;
}

function showSuccessMessage(message) {
    document.getElementById('messageContainer').innerHTML = `<div class="success-message">${message}</div>`;
}

function clearMessages() {
    document.getElementById('messageContainer').innerHTML = '';
}

// ============================================
// FUNCIONES DE EVENTOS Y ASIENTOS
// ============================================

async function loadEvents() {
    const contenedor = document.getElementById("contenedor");

    contenedor.innerHTML = `
        <div class="loading">
            <div class="spinner"></div>
            Cargando eventos...
        </div>
    `;

    try {
        const res = await fetch("/api/events");
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        const eventos = await res.json();

        showEvents(eventos);
    } catch (error) {
        contenedor.innerHTML = `
            <div class="error-message">
                ❌ Error al cargar eventos. Intenta de nuevo.
            </div>
        `;
        console.error(error);
    }
}

function showEvents(eventos) {
    const contenedor = document.getElementById("contenedor");

    let html = '<div class="events-grid">';

    eventos.forEach(e => {
        const fecha = new Date(e.eventDate).toLocaleDateString('es-ES', {
            day: '2-digit',
            month: 'short',
            year: 'numeric'
        });

        const hora = new Date(e.eventDate).toLocaleTimeString('es-ES', {
            hour: '2-digit',
            minute: '2-digit'
        });

        const totalAsientos = e.sectors.reduce((sum, s) => sum + s.totalSeats, 0);
        const disponibles = e.sectors.reduce((sum, s) => sum + s.availableSeats, 0);

        html += `
            <div class="event-card" onclick="verEvento(${e.id})">
                <div class="event-poster">
                    🎬 ${e.name}
                </div>
                <div class="event-info">
                    <div class="event-title">${e.name}</div>
                    <div class="event-date">📅 ${fecha} • 🕐 ${hora}</div>
                    <div class="event-venue">📍 ${e.venue}</div>
                    <div class="event-rating">⭐⭐⭐⭐⭐ (5/5)</div>
                    <div class="event-sectors">
                        ${disponibles}/${totalAsientos} asientos disponibles
                    </div>
                    <button class="event-button">Ver y Reservar</button>
                </div>
            </div>
        `;
    });

    html += '</div>';
    contenedor.innerHTML = html;
}

async function seeEvento(eventId) {
    const contenedor = document.getElementById("contenedor");

    contenedor.innerHTML = `
        <div class="loading">
            <div class="spinner"></div>
            Cargando asientos...
        </div>
    `;

    try {
        const res = await fetch(`/api/events/${eventId}/seats`);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        const sectores = await res.json();

        currentEventId = eventId;
        selectedSeats = [];
        showSeats(sectores, eventId);
    } catch (error) {
        contenedor.innerHTML = `
            <div class="error-message">
                ❌ Error al cargar asientos. Intenta de nuevo.
            </div>
        `;
        console.error(error);
    }
}

function showSeats(sectores, eventId) {
    const contenedor = document.getElementById("contenedor");

    sectorPrices = {};
    sectores.forEach(s => {
        sectorPrices[s.sectorId] = s.price;
    });

    let html = `
        <div class="seats-section">
            <button class="back-button" onclick="loadEvents()">← Volver a eventos</button>
            <div id="selectedInfo" class="selected-info">
                <span id="selectedCount">Asientos seleccionados: 0</span>
                <span id="selectedTotal">Total: $0</span>
            </div>
    `;

    sectores.forEach(sector => {
        const filas = agruparPorFilas(sector.seats);
        const asientosDisponibles = sector.seats.filter(s => s.status && s.status.toLowerCase() === "available").length;

        html += `
            <div style="margin-bottom: 40px;">
                <div class="sector-title">${sector.sectorName}</div>
                <div class="sector-info">
                    <span class="sector-price">💵 $${sector.price ?? 'N/A'}</span>
                    <span class="sector-available">${asientosDisponibles} asientos disponibles</span>
                </div>

                <div class="seats-grid">
                    ${filas.map(fila =>
            fila.asientos.map(seat => `
                            <div
                                class="seat ${getSeatClass(seat.status)}"
                                onclick="toggleSeatSelection('${seat.id}', '${seat.rowIdentifier}', ${seat.seatNumber}, ${sector.sectorId}, ${sector.price ?? 0}, '${(sector.sectorName || '').replace(/'/g, "\\'")}')"
                                title="${seat.rowIdentifier}-${seat.seatNumber}"
                                data-seat-id="${seat.id}"
                            >
                                ${seat.seatNumber}
                            </div>
                        `).join('')
        ).join('')}
                </div>
            </div>
        `;
    });

    html += `
        <div class="seat-legend">
            <div class="legend-item"><div class="legend-box" style="background: #95a5a6;"></div>Disponible</div>
            <div class="legend-item"><div class="legend-box" style="background: #f39c12;"></div>Reservado</div>
            <div class="legend-item"><div class="legend-box" style="background: #e74c3c;"></div>Vendido</div>
            <div class="legend-item"><div class="legend-box" style="background: #27ae60;"></div>Seleccionado</div>
        </div>

        <button id="btn-reserve" class="btn-primary" onclick="reservedSelectect(${eventId})" disabled>
            Reservar Seleccionados (${selectedSeats.length})
        </button>
        </div>
    `;

    contenedor.innerHTML = html;
    updateSelectedInfo();
}

function toggleSeatSelection(seatId, rowIdentifier, seatNumber, sectorId, price, sectorName) {
    const seatEl = document.querySelector(`[data-seat-id="${seatId}"]`);
    if (!seatEl) return;

    if (!seatEl.classList.contains('available')) {
        showToast("❌ Este asiento no está disponible");
        return;
    }

    if (!currentUser) {
        showToast("Debes iniciar sesión para seleccionar asientos.");
        openLoginModal();
        return;
    }

    const index = selectedSeats.findIndex(s => s.id === seatId);

    if (index > -1) {
        selectedSeats.splice(index, 1);
        seatEl.classList.remove('selected');
    } else {
        selectedSeats.push({
            id: seatId,
            rowIdentifier,
            seatNumber,
            price,
            sectorId,
            sectorName
        });
        seatEl.classList.add('selected');
    }

    updateSelectedInfo();
}

function updateSelectedInfo() {
    const countEl = document.getElementById('selectedCount');
    const totalEl = document.getElementById('selectedTotal');
    const btnReserve = document.getElementById('btn-reserve');

    if (countEl) countEl.textContent = `Asientos seleccionados: ${selectedSeats.length}`;

    const total = selectedSeats.reduce((sum, s) => sum + s.price, 0);
    if (totalEl) totalEl.textContent = `Total: $${total.toFixed(2)}`;

    if (btnReserve) {
        btnReserve.disabled = selectedSeats.length === 0;
        btnReserve.textContent = `Reservar Seleccionados (${selectedSeats.length})`;
    }
}

async function reservedSelected(eventId) {
    if (selectedSeats.length === 0) {
        showToast("Selecciona al menos un asiento");
        return;
    }

    if (!currentUser) {
        showToast("Debes iniciar sesión.");
        return;
    }

    const btnReserve = document.getElementById('btn-reserve');
    if (btnReserve) {
        btnReserve.disabled = true;
        btnReserve.textContent = 'Reservando...';
    }

    try {
        const seatsToReserve = [...selectedSeats];
        const results = await Promise.allSettled(seatsToReserve.map(async seat => {
            const response = await fetch("/api/reservation", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ userId: currentUser.id, seatId: seat.id })
            });
            return { seat: seat, ok: response.ok, status: response.status };
        }));
        let succeededIds = [];
        let conflictOccurred = false;

        results.forEach(r => {
            if (r.status === 'fulfilled') {
                if (r.value.ok) {
                    succeededIds.push(r.value.seat.id);
                } else if (r.value.status === 409) {
                    conflictOccurred = true;
                }
            }
        });

        selectedSeats = selectedSeats.filter(s => succeededIds.includes(s.id));

        if (conflictOccurred) {
            showToast("Asiento ya no disponible");
            seeEvento(eventId);
        }

        if (succeededIds.length > 0) {
            openPaymentModal();
        } else if (!conflictOccurred) {
            seeEvento(eventId);
        }

    } catch (error) {
        console.error(error);
        showToast('❌ Error al reservar: ' + error.message);
    } finally {
        if (btnReserve) {
            btnReserve.disabled = false;
            btnReserve.textContent = `Reservar Seleccionados (${selectedSeats.length})`;
        }
    }
}

function getSeatClass(status) {
    switch (status?.toLowerCase()) {
        case "available":
            return "available";
        case "reserved":
            return "reserved";
        case "sold":
            return "sold";
        default:
            return "available";
    }
}

function agruparPorFilas(seats) {
    const filas = {};

    seats.forEach(seat => {
        if (!filas[seat.rowIdentifier]) {
            filas[seat.rowIdentifier] = [];
        }
        filas[seat.rowIdentifier].push(seat);
    });

    return Object.keys(filas)
        .sort()
        .map(fila => ({
            fila,
            asientos: filas[fila].sort((a, b) => a.seatNumber - b.seatNumber)
        }));
}

function isAvailable(status) {
    return status && status.toLowerCase() === "available";
}

// ============================================
// MODAL DE PAGO
// ============================================

function openPaymentModal() {
    const modal = document.getElementById('paymentModal');
    if (!modal) return;

    const total = selectedSeats.reduce((sum, s) => sum + s.price, 0);
    const seatsHtml = selectedSeats.map(s => `
        <div class="payment-seat-item">
            <span>${s.sectorName} - Fila ${s.rowIdentifier} Asiento ${s.seatNumber}</span>
            <span>$${s.price.toFixed(2)}</span>
        </div>
    `).join('');

    const paymentContent = document.getElementById('paymentContent');
    if (paymentContent) {
        paymentContent.innerHTML = `
            <div class="payment-header">
                <h2>Resumen de Pago</h2>
            </div>

            <div class="payment-body">
                <div class="payment-section">
                    <h3>Asientos Reservados</h3>
                    <div class="payment-seats-list">
                        ${seatsHtml}
                    </div>
                </div>

                <div class="payment-section">
                    <h3>Detalles</h3>
                    <div class="payment-detail-row">
                        <span>Subtotal</span>
                        <span>$${total.toFixed(2)}</span>
                    </div>
                    <div class="payment-detail-row">
                        <span>Comisión (5%)</span>
                        <span>$${(total * 0.05).toFixed(2)}</span>
                    </div>
                    <div class="payment-detail-row payment-total">
                        <span><strong>Total</strong></span>
                        <span><strong>$${(total * 1.05).toFixed(2)}</strong></span>
                    </div>
                </div>

                <div class="payment-section">
                    <h3>Tiempo de Reserva</h3>
                    <div id="paymentTimer" class="payment-timer">
                        Expira en: <strong id="timerDisplay">02:00</strong>
                    </div>
                </div>

                <div class="payment-section">
                    <h3>Usuario</h3>
                    <div class="payment-user">
                        <strong>${currentUser.name}</strong>
                        <small>${currentUser.email}</small>
                    </div>
                </div>

                <button class="btn-primary" onclick="procesarPago(${currentEventId})">
                    Procesar Pago - $${(total * 1.05).toFixed(2)}
                </button>

                <button class="btn-secondary" onclick="confirmCancelPayment()">
                    Cancelar
                </button>
            </div>
        `;
    }

    modal.classList.add('active');
    startPaymentTimer();
}
function showToast(mensaje) {
    const toast = document.getElementById("toastNotification");
    if (toast) {
        toast.textContent = mensaje;
        toast.className = "toast-container show";
        setTimeout(function () {
            toast.className = toast.className.replace("show", "").trim();
        }, 5000);
    }
}
async function confirmCancelPayment() {
    if (!selectedSeats || selectedSeats.length === 0) {
        closePaymentModal();
        return;
    }

    const confirmar = confirm("¿Cancelar el pago y liberar los asientos seleccionados?");
    if (!confirmar) return;

    try {
        for (let seat of selectedSeats) {
            await fetch('/api/reservation/cancel', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ userId: currentUser.id, seatId: seat.id })
            });
        }

        showToast("Pago cancelado. Los asientos han sido liberados.");

    } catch (err) {
        console.error(err);
        showToast('Error al liberar asientos: ' + err.message);
    } finally {
        selectedSeats = [];

        closePaymentModal();

        seeEvento(currentEventId);
    }
}

function closePaymentModal() {
    const modal = document.getElementById('paymentModal');
    if (modal) modal.classList.remove('active');
    stopPaymentTimer();
}

let paymentTimerInterval = null;

function startPaymentTimer() {
    stopPaymentTimer();

    let timeLeft = 300; 

    function updateTimer() {
        const minutes = Math.floor(timeLeft / 60);
        const seconds = timeLeft % 60;
        const display = `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;

        const timerEl = document.getElementById('timerDisplay');
        if (timerEl) timerEl.textContent = display;

        if (timeLeft <= 0) {
            stopPaymentTimer();
            showSeats('⏰ Tu reserva ha expirado. Los asientos vuelven a estar disponibles.');
            closePaymentModal();
        }

        timeLeft--;
    }

    updateTimer();
    paymentTimerInterval = setInterval(updateTimer, 1000);
}

function stopPaymentTimer() {
    if (paymentTimerInterval) {
        clearInterval(paymentTimerInterval);
        paymentTimerInterval = null;
    }
}

async function procesarPago(eventId) {
    if (selectedSeats.length === 0) {
        showToast("No hay asientos seleccionados");
        return;
    }

    if (!currentUser) {
        showToast("Debes iniciar sesión.");
        return;
    }

    const btnPay = document.querySelector('#paymentContent .btn-primary');
    if (btnPay) {
        btnPay.disabled = true;
        btnPay.textContent = 'Procesando...';
    }

    try {
        const results = await Promise.allSettled(selectedSeats.map(seat =>
            fetch('/api/pagos', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    userId: currentUser.id,
                    seatId: seat.id
                })
            })
        ));

        let success = 0, failed = 0;
        for (const r of results) {
            if (r.status === 'fulfilled' && r.value.ok) success++;
            else failed++;
        }

        const message = `Pagos procesados: ${success} exitosos, ${failed} fallidos`;
        showToast('✅ ' + message);

        if (success > 0) {
            selectedSeats = [];
            closePaymentModal();
            setTimeout(() => seeEvento(eventId), 1000);
        }

    } catch (error) {
        console.error(error);
        showToast('❌ Error al procesar pago: ' + error.message);
    } finally {
        if (btnPay) {
            btnPay.disabled = false;
            btnPay.textContent = `Procesar Pago`;
        }
    }
}