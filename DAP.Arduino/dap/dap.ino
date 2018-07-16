#include "config.h"
#include "planificador.h"

//TEST
#include "test.h"

Servo plate1;
Planificador *planif;

void setup() {
  Serial.begin(115200);  //Monitor Serial
  Serial1.begin(115200); //Comunicacion con ESP8266
  
  Log.Init(LOGLEVEL, 115200);

  planif = new Planificador();
  //planif->initServo(plate1, 0);
}

void loop() {
  //TEST - para testear las funciones desde la consola. Ver test.h
  bool received = getTestCommand(CommandLine);      //global CommandLine is defined in CommandLine.h
  if (received) executeTestCommand(CommandLine);

  //leer mensajes de control desde WiFi
  planif->procesarAcciones();

  Serial.println(planif->getTimeString());
  //planif->isDispenseTime();
  delay(MAIN_LOOP_DELAY+1000);
}





