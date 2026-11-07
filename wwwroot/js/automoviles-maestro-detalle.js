"use strict";

document.addEventListener("DOMContentLoaded", () => {
    const contenedor = document.getElementById("maestroDetalle");
    if (!contenedor) return;

    const api = contenedor.dataset.api;
    const combo = document.getElementById("tipoAutomovil");
    const boton = document.getElementById("btnConsultar");
    const tablaAutos = document.getElementById("tablaAutomoviles");
    const tablaIngresos = document.getElementById("tablaIngresos");
    const mensaje = document.getElementById("mensajeConsulta");
    const seleccion = document.getElementById("automovilSeleccionado");

    let solicitudActual = 0;

    function mensajeEstado(texto, tipo = "info") {
        mensaje.className = `alert alert-${tipo}`;
        mensaje.textContent = texto;
    }

    function limpiarTabla(tabla, columnas, texto) {
        tabla.replaceChildren();

        const fila = document.createElement("tr");
        const celda = document.createElement("td");

        celda.colSpan = columnas;
        celda.textContent = texto;
        celda.className = "text-muted";

        fila.appendChild(celda);
        tabla.appendChild(fila);
    }

    function agregarCelda(fila, contenido) {
        const celda = document.createElement("td");
        celda.textContent = contenido === null ||
            contenido === undefined ? "Pendiente" : String(contenido);
        fila.appendChild(celda);
        return celda;
    }

    function mostrarFecha(valor) {
        if (!valor) return "Pendiente";

        const fecha = new Date(valor);
        return Number.isNaN(fecha.getTime())
            ? String(valor)
            : fecha.toLocaleString("es-CR");
    }

    function mostrarMonto(valor) {
        if (valor === null || valor === undefined) return "Pendiente";

        const numero = Number(valor);
        if (!Number.isFinite(numero)) return String(valor);

        return "₡" + numero.toLocaleString("es-CR", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function normalizar(valor) {
        return String(valor ?? "")
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .toLowerCase()
            .trim();
    }

    async function consultarApi(ruta) {
        const respuesta = await fetch(ruta, {
            headers: { Accept: "application/json" }
        });

        if (!respuesta.ok) {
            throw new Error(`Error HTTP ${respuesta.status}`);
        }

        return await respuesta.json();
    }

    async function cargarTipos() {
        try {
            const autos = await consultarApi(api);

            if (!Array.isArray(autos)) {
                throw new Error("La respuesta no es una lista.");
            }

            const tipos = [...new Set(
                autos.map(auto => auto.tipo)
                    .filter(tipo => tipo && String(tipo).trim())
            )].sort((a, b) =>
                String(a).localeCompare(String(b), "es"));

            combo.replaceChildren();

            const inicial = document.createElement("option");
            inicial.value = "";
            inicial.textContent = "Seleccione un tipo";
            combo.appendChild(inicial);

            for (const tipo of tipos) {
                const opcion = document.createElement("option");
                opcion.value = tipo;
                opcion.textContent = tipo;
                combo.appendChild(opcion);
            }

            if (tipos.length === 0) {
                mensajeEstado("No hay tipos registrados.", "warning");
            }
        } catch (error) {
            combo.replaceChildren();
            mensajeEstado(
                "No se pudieron cargar los tipos: " + error.message,
                "danger");
        }
    }

    function mostrarAutos(autos) {
        tablaAutos.replaceChildren();

        if (autos.length === 0) {
            limpiarTabla(tablaAutos, 6, "No se encontraron automóviles.");
            return;
        }

        for (const auto of autos) {
            const fila = document.createElement("tr");

            agregarCelda(fila, auto.id);
            agregarCelda(fila, auto.fabricante);
            agregarCelda(fila, auto.tipo);
            agregarCelda(fila, auto.color);
            agregarCelda(fila, auto.anio);

            const celda = document.createElement("td");
            const btnDetalle = document.createElement("button");

            btnDetalle.type = "button";
            btnDetalle.className = "btn btn-sm btn-outline-primary";
            btnDetalle.textContent = "Ver ingresos";
            btnDetalle.addEventListener("click", () =>
                seleccionarAutomovil(auto));

            celda.appendChild(btnDetalle);
            fila.appendChild(celda);
            tablaAutos.appendChild(fila);
        }
    }

    async function consultarAutomoviles() {
        const tipo = combo.value;

        if (!tipo) {
            mensajeEstado("Seleccione un tipo de automóvil.", "warning");
            return;
        }

        solicitudActual++;
        boton.disabled = true;
        seleccion.textContent = "Seleccione un automóvil del listado superior.";
        limpiarTabla(tablaIngresos, 6, "No hay un automóvil seleccionado.");
        mensajeEstado("Consultando automóviles...");

        try {
            const autos = await consultarApi(
                api + "?tipo=" + encodeURIComponent(tipo));

            if (!Array.isArray(autos)) {
                throw new Error("La respuesta no es una lista.");
            }

            const encontrados = autos.filter(auto =>
                normalizar(auto.tipo) === normalizar(tipo));

            mostrarAutos(encontrados);

            mensajeEstado(
                `Se encontraron ${encontrados.length} automóvil(es).`,
                "success");
        } catch (error) {
            mostrarAutos([]);
            mensajeEstado(
                "No se pudo consultar: " + error.message,
                "danger");
        } finally {
            boton.disabled = false;
        }
    }

    async function seleccionarAutomovil(auto) {
        const solicitud = ++solicitudActual;

        seleccion.textContent =
            `Automóvil ${auto.id} - ${auto.fabricante} - ${auto.tipo}`;

        limpiarTabla(tablaIngresos, 6, "Consultando ingresos...");

        try {
            const respuesta = await consultarApi(
                `${api}/${encodeURIComponent(auto.id)}/ingresos`);

            if (solicitud !== solicitudActual) return;

            const ingresos = Array.isArray(respuesta)
                ? respuesta
                : respuesta.ingresos ?? respuesta.Ingresos ?? [];

            if (!Array.isArray(ingresos)) {
                throw new Error("La respuesta no contiene ingresos válidos.");
            }

            tablaIngresos.replaceChildren();

            if (ingresos.length === 0) {
                limpiarTabla(
                    tablaIngresos, 6,
                    "Este automóvil no tiene ingresos registrados.");
                return;
            }

            for (const ingreso of ingresos) {
                const fila = document.createElement("tr");

                agregarCelda(fila, ingreso.idParqueo);
                agregarCelda(fila, mostrarFecha(ingreso.fechaHoraEntrada));
                agregarCelda(fila, mostrarFecha(ingreso.fechaHoraSalida));
                agregarCelda(fila, ingreso.duracionMinutos);
                agregarCelda(fila, ingreso.duracionHoras);
                agregarCelda(fila, mostrarMonto(ingreso.montoTotalPagar));

                tablaIngresos.appendChild(fila);
            }
        } catch (error) {
            if (solicitud !== solicitudActual) return;

            limpiarTabla(
                tablaIngresos, 6,
                "No fue posible consultar los ingresos.");

            mensajeEstado(
                "Error al consultar los ingresos: " + error.message,
                "danger");
        }
    }

    boton.addEventListener("click", consultarAutomoviles);
    cargarTipos();
});
