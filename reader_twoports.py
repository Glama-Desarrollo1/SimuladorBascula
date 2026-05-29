#!/usr/bin/env python3
"""\nLector simple para 2 puertos COM.
Uso:
  python reader_twoports.py --port1 COM5 --port2 COM6

Requiere: pyserial
  pip install pyserial
"""
import argparse
import serial
import threading
import sys

try:
    from serial.serialutil import SerialException
except Exception:
    print("Error: falta la librería 'pyserial'. Instala con: pip install pyserial", file=sys.stderr)
    raise


def read_loop(port, name, stop_event, baud):
    while not stop_event.is_set():
        try:
            ser = serial.Serial(port, baudrate=baud, timeout=1)
            break
        except SerialException as e:
            print(f"[{name}] no se puede abrir {port}: {e}. Reintentando en 1s...")
            stop_event.wait(1)
    else:
        return

    with ser:
        while not stop_event.is_set():
            try:
                line = ser.readline().decode(errors='ignore').strip()
                if line:
                    print(f"[{name}] <- {port}: {line}")
            except SerialException as e:
                print(f"[{name}] fallo lectura en {port}: {e}")
                break


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Lector de dos puertos COM')
    parser.add_argument('--port1', required=True)
    parser.add_argument('--port2', required=True)
    parser.add_argument('--baud', type=int, default=9600)
    args = parser.parse_args()

    stop = threading.Event()
    t1 = threading.Thread(target=read_loop, args=(args.port1, 'SCALE1', stop, args.baud), daemon=True)
    t2 = threading.Thread(target=read_loop, args=(args.port2, 'SCALE2', stop, args.baud), daemon=True)

    t1.start(); t2.start()

    try:
        while True:
            stop.wait(1)
    except KeyboardInterrupt:
        print('\nDeteniendo...')
        stop.set()
        t1.join(); t2.join()

    print('Lector detenido.')
