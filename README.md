# proyecto-dap

Arduino:
- Librerías externas (necesarias para compilar):
  - https://github.com/mrRobot62/Arduino-logging-library
  - https://github.com/luisllamasbinaburo/Arduino-List
  - https://github.com/adafruit/RTClib

- Conexiones:
  - Los pines están definidos en config.h
  
- Testeo desde consola (agregar alarmas, borrar eeprom, mostrar detalle de alarma, etc):
  - Hay un test.h para poder agregar métodos y hacer llamadas a la lógica principal desde la consola.

- WiFi
  - Existe un .ino propio para el sketch principal (en otra carpeta) que corre sobre la placa ESP. Se compila aparte y se sube directo al ESP.
  - Se comunica via serial con la placa Arduino para intercambiar los mensajes.
