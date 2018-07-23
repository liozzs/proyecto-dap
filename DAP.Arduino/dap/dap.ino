#include "config.h"
#include "planificador.h"

//TEST
#include "test.h"

Planificador *planif;

//Manejo de la frecuencia de chequeo del tiempo de dispendio. Esto permite no interferir con la frecuencia de los motores.
unsigned long previousMillis = 0;  

void setup() {
  Serial.begin(115200);  //Monitor Serial
  Serial1.begin(115200); //Comunicacion con ESP8266
  
  Log.Init(LOGLEVEL, 115200);

  planif = new Planificador();

}

void loop() {
  unsigned long currentMillis = millis();
  
  //TEST - para testear las funciones desde la consola. Ver test.h
  bool received = getTestCommand(CommandLine);      //global CommandLine is defined in CommandLine.h
  if (received) executeTestCommand(CommandLine);
  // Fin TEST
  
  //leer mensajes de control desde WiFi
  planif->processCommandsWIFI();

  //Ejecutar solo cada "MAIN_LOOP_DELAY" millis para no perjudicar el loop principal
  if (currentMillis - previousMillis >= MAIN_LOOP_DELAY) {
    Serial.println(planif->getTimeString(planif->getTime()));
    previousMillis = currentMillis;
    planif->execute();
  }

  //Procesa las ordenes de movimiento y parada de los platos.
  planif->processPlates();
}





