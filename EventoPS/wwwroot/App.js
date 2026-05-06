async function cargarEventos(soloReservados) {
    const contenedor = document.getElementById("contenedor");

    contenedor.innerHTML = "<h2>Cargando eventos...</h2>";

    try {
        const res = await fetch("/api/events");
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        const eventos = await res.json();

        mostrarEventos(eventos, soloReservados);
    } catch (error) {
        contenedor.innerHTML = "<p>Error al cargar eventos</p>";
        console.error(error);
    }
}
function mostrarEventos(eventos, soloReservados) {
    const contenedor = document.getElementById("contenedor");

    contenedor.innerHTML = `
    <div style="display:grid; grid-template-columns: repeat(auto-fill, 250px); gap:20px;">
        ${eventos.map(e => `
            <div onclick="verEvento(${e.id}, ${soloReservados})"
                style="background:#222; padding:15px; border-radius:10px; cursor:pointer;">
                <h3>${e.name}</h3>
                <p>${new Date(e.eventDate).toLocaleDateString()}</p>
            </div>
        `).join("")}
    </div>
    `;
}

async function verEvento(eventId, soloReservados) {
    const contenedor = document.getElementById("contenedor");
    contenedor.innerHTML = "<h2>Cargando asientos...</h2>";

    try {
        const url = soloReservados
            ? `/api/events/${eventId}/reservedseats`   // backend filtrado
            : `/api/events/${eventId}/seats`;   // todos

        const res = await fetch(url);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);

        const sectores = await res.json();

        mostrarAsientos(sectores, eventId, soloReservados);

    } catch (error) {
        contenedor.innerHTML = "<p>Error al cargar asientos</p>";
        console.error(error);
    }
}
function mostrarAsientos(sectores, eventId, modo) {
    const contenedor = document.getElementById("contenedor");

    contenedor.innerHTML = sectores.map(sector => {
        const filas = agruparPorFilas(sector.seats);

        return `
        <div style="margin-bottom:40px;">
            <h2>${sector.sectorName}</h2>

            ${filas.map(f => `
                <div style="display:flex; align-items:center; margin-bottom:5px;">
                    
                    <div style="width:20px; margin-right:10px;">
                        ${f.fila}
                    </div>

                    <div style="display:flex; gap:5px;">
                        ${f.asientos.map(seat => `
                            <div
                                onclick='handleSeatClick(${JSON.stringify(seat)}, ${eventId}, ${modo})'
                                style="
                                    width:40px;
                                    height:40px;
                                    display:flex;
                                    align-items:center;
                                    justify-content:center;
                                    border-radius:6px;
                                    font-size:12px;
                                    color:white;
                                    cursor:${isAvailable(seat.status) ? "pointer" : "not-allowed"};
                                    opacity:${isAvailable(seat.status) ? "1" : "0.5"};
                                    background:${getColor(seat.status)};
                                ">
                                ${seat.seatNumber}
                            </div>
                        `).join("")}
                    </div>

                </div>
            `).join("")}
        </div>
        `;
    }).join("");
}
async function handleSeatClick(seat, eventId, modo) {
    if (modo === true) {
        // BUY
        if (seat.status !== "Reserved") {
            return alert("No está disponible para comprar");
        }

        try {
            const confirmar = confirm(
                `¿Comprar asiento ${seat.rowIdentifier}-${seat.seatNumber}?`
            );
            if (!confirmar) return;

            const res = await fetch("/api/checkouts", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    userId: 1,
                    seatId: seat.id
                })
            });

            if (!res.ok) {
                const msg = await res.text();
                console.error(msg);
                alert("Error: " + msg);
                return;
            }

            alert("Compra exitosa 🎉");

        } catch (error) {
            console.error(error);
            alert("Error de conexión");
        }
    }
    else if (modo === false){
        try {
            const confirmar = confirm(
                `¿Reservar asiento ${seat.rowIdentifier}-${seat.seatNumber}?`
            );
            if (!confirmar) return;

            const res = await fetch("/api/reservation", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    userId: 1,
                    seatId: seat.id
                })
            });

            if (!res.ok) {
                const msg = await res.text();
                console.error(msg);
                alert("Error: " + msg);
                return;
            }

            alert("Reserva exitosa 🎉");

        } catch (error) {
            console.error(error);
            alert("Error de conexión");
        }
    }
    // 🔄 recargar asientos
    verEvento(eventId, modo);
}
function agruparPorFilas(seats) {
    const filas = {};

    seats.forEach(seat => {
        if (!filas[seat.rowIdentifier]) {
            filas[seat.rowIdentifier] = [];
        }
        filas[seat.rowIdentifier].push(seat);
    });

    // Ordenar filas (A, B, C...)
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
function getColor(status) {
    switch (status.toLowerCase()) {
        case "available":
            return "#4CAF50"; // verde
        case "reserved":
            return "#FFC107"; // amarillo
        case "occupied":
            return "#F44336"; // rojo
        default:
            return "#555";
    }
}
