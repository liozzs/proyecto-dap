#include "config.h"
#include "planificador.h"
#include "seg_display.h"

//TEST
#include "test.h"

Planificador *planif;

//Manejo de la frecuencia de chequeo del tiempo de dispendio. Esto permite no interferir con la frecuencia de los motores.
unsigned long previousMillis = 0;  
unsigned long previousMillisWIFI = 0;  
 
void setup() {
  Serial.begin(115200);  //Monitor Serial
  Serial1.begin(115200); //Comunicacion con ESP8266
  init_display();
  
  Log.Init(LOG_LEVEL_DEBUG, 115200); // LOG_LEVEL_NOOUTPUT //LOG_LEVEL_ERRORS / LOG_LEVEL_DEBUG

  planif = new Planificador();
 
}

void loop() {
  unsigned long currentMillis = millis();
  
  //TEST - para testear las funciones desde la consola. Ver test.h
  bool received = getTestCommand(CommandLine);      //global CommandLine is defined in CommandLine.h
  if (received) executeTestCommand(CommandLine);
  // Fin TEST
  
  if (currentMillis - previousMillisWIFI >= LOOP_DELAY_WIFI) {
    //leer mensajes de control desde WiFi
    planif->processCommandsWIFI();
    previousMillisWIFI = currentMillis;
  }

  //Ejecutar solo cada "MAIN_LOOP_DELAY" millis para no perjudicar el loop principal
  if (currentMillis - previousMillis >= MAIN_LOOP_DELAY) {
    Serial.println(planif->getTimeString(planif->getTime())); 
    previousMillis = currentMillis;
    planif->execute();
    planif->checkCriticalStock();
  }

  //Procesa las ordenes de movimiento y parada de los platos.
  planif->processPlates();

  //LED
  planif->processLED();

  //Refrescar display
  refresh_display();
}





