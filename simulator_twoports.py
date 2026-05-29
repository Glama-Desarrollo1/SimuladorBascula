#!/usr/bin/env python3
"""
Simulador de 2 puertos COM que envía peso periódicamente a ambos puertos.
Uso:
  python simulator_twoports.py --port1 COM6 --port2 COM7 --interval 1.0

Requiere: pyserial
  pip install pyserial
"""
import argparse
import random
import time
import threading
import sys

try:
    import serial
    from serial.serialutil import SerialException
except Exception:
    print("Error: falta la librería 'pyserial'. Instala con: pip install pyserial", file=sys.stderr)
    raise


def send_loop(port, name, interval, stop_event, baud):
    # intenta abrir puerto, reintentando si falla
    while not stop_event.is_set():
        try:
            ser = serial.Serial(port, baudrate=baud, timeout=1)
            break
        except SerialException as e:
            print(f"[{name}] no se puede abrir {port}: {e}. Reintentando en 1s...")
            time.sleep(1)
    else:
        return

    with ser:
        while not stop_event.is_set():
            peso = round(random.uniform(0.0, 250.0), 2)
            # Formato simple: W:123.45\r\n
            msg = f"W:{peso:.2f}\r\n"
            try:
                ser.write(msg.encode())
                print(f"[{name}] -> {port}: {msg.strip()}")
            except SerialException as e:
                print(f"[{name}] fallo escritura en {port}: {e}")
                break
            # espera con interrupción controlada
            stop_event.wait(interval)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Simulador de dos puertos COM que emiten peso simultáneamente')
    parser.add_argument('--port1', required=True, help='Puerto COM para báscula 1 (ej: COM6 o /dev/ttyV1)')
    parser.add_argument('--port2', required=True, help='Puerto COM para báscula 2')
    parser.add_argument('--baud', type=int, default=9600, help='Baud rate (por defecto 9600)')
    parser.add_argument('--interval', type=float, default=1.0, help='Intervalo entre envíos en segundos')
    args = parser.parse_args()

    stop = threading.Event()

    t1 = threading.Thread(target=send_loop, args=(args.port1, 'SCALE1', args.interval, stop, args.baud), daemon=True)
    t2 = threading.Thread(target=send_loop, args=(args.port2, 'SCALE2', args.interval, stop, args.baud), daemon=True)

    t1.start()
    t2.start()

    try:
        while t1.is_alive() or t2.is_alive():
            time.sleep(0.1)
    except KeyboardInterrupt:
        print('\nInterrupción recibida. Deteniendo...')
        stop.set()
        t1.join()
        t2.join()

    print('Simulador detenido.')
