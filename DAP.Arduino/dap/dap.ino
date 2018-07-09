#include "config.h"
#include "planificador.h"

//TEST
#include "test.h"

Servo plate1;
Planificador *planif;

void setup() {
  Serial.begin(9600);  //Monitor Serial
  Log.Init(LOGLEVEL, 9600);

  planif = new Planificador();
  //planif->initServo(plate1, 0);
}

void loop() {

  //TEST - para testear las funciones desde la consola. Ver test.h
  bool received = getTestCommand(CommandLine);      //global CommandLine is defined in CommandLine.h
  if (received) executeTestCommand(CommandLine);

  planif->isDispenseTime();
  delay(MAIN_LOOP_DELAY+1000);
}





