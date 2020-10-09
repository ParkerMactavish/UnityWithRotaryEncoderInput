import socket
import serial
import serial.tools.list_ports
import time

client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
client.connect(("127.0.0.1", 8000))
serialObj = serial.Serial(str(serial.tools.list_ports.comports()[0].device), 9600)
serialObj.setDTR()

recv  = ""

while True:
    try:
        recv += serialObj.read().decode()
        if recv[-1] == '\r':
            client.send(recv[0:-1].encode())
        elif recv[-1] == '\n':
            recv = ""
    except KeyboardInterrupt:
        raise
