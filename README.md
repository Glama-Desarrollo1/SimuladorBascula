# Simulador de Báscula para Trailers

Aplicación de escritorio (WinForms) en Visual Basic .NET que simula una báscula para trailers enviando valores de peso por un puerto serie. Útil para pruebas, desarrollo y demostraciones cuando no se dispone del hardware real.

## Características

- Interfaz gráfica para controlar la simulación.
- Envío periódico de datos por puerto serie (`SerialPort`).
- Modos de simulación: `Aleatorio`, `Estable`, `Realista`, `Incremental`.
- Acciones rápidas: simular carga/descarga, establecer peso manual, aplicar tara.
- Registro de eventos (log) con límite de 100 líneas.
- Configuración de rango de pesos y intervalo de envío.

## Requisitos

- Windows 10/11.
- .NET Framework o .NET compatible con WinForms (ver el archivo de proyecto para la versión objetivo).
- Visual Studio 2022/2026 para desarrollo y depuración.

## Instalación

1. Clona el repositorio:
2. Abre la solución en Visual Studio: abre `SimuladorBascula.sln`.
3. Restaura paquetes NuGet si los hubiera (__Herramientas > Administrador de paquetes NuGet > Restaurar paquetes__).
4. Compila y ejecuta la solución (__Depurar > Iniciar sin depuración__ o presiona __F5__ para depurar).

## Uso

1. Selecciona el puerto serie en el combo `Puerto`. Si no se detecta ninguno, el simulador sugiere `COM7` por defecto.
2. Ajusta `Intervalo (ms)`, `Peso Mín` y `Peso Máx`.
3. Selecciona el `Modo de Simulación`:
   - `Aleatorio`: variaciones aleatorias constantes.
   - `Estable`: peso fijo.
   - `Realista`: estabilización gradual (por defecto).
   - `Incremental`: simulaciones de carga/descarga progresiva.
4. Pulsa `▶ INICIAR` para comenzar. Se habilitarán `Pausar`, `Detener`, `Tara` y acciones rápidas.
5. Usa `📦 Simular Carga Trailer` o `📤 Simular Descarga Trailer` para secuencias predefinidas, o `Establecer Peso` para introducir un valor manual.
6. Consulta el `Registro de Eventos` para mensajes y errores. El log mantiene las últimas 100 líneas.

### Visualización y eventos

- El panel principal muestra `Peso Actual`, `Estado` y `Último envío`.
- Se registran eventos como inicio, parada, errores, cambios de modo y estado de conexión del puerto.

## Estructura del proyecto

- Interfaz: `SimuladorBascula\FormSimuladorBascula.vb`
- Lógica del simulador: `SimuladorBascula\ModulePeso.vb` (u otros módulos relacionados)

Sigue las reglas de estilo definidas en `.editorconfig` y las guías en `CONTRIBUTING.md` si existen. Asegúrate de que los cambios compilables respeten el formateo y las convenciones del proyecto.

## Contribuir

1. Crea un fork y una rama descriptiva: `feature/descripcion-corta` o `fix/descripcion-corta`.
2. Sigue el estándar de commits del repositorio (mensajes claros y concisos).
3. Abre un pull request describiendo los cambios y las pruebas realizadas.
