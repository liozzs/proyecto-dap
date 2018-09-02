#ifndef _PLANIFICADOR_h
#define _PLANIFICADOR_h

#include "config.h"
#include "RTClib.h"
#include "ListLib.h"
#include <Logging.h>
#include <EEPROM.h>
#include <Servo.h>
#include <Timezone.h> 
#include "conectividad.h"
#include <ArduinoJson.h>


#define PERIODICIDAD_DIARIA 0
#define PERIODICIDAD_SEMANAL 1
#define PERIODICIDAD_PERSONALIZADA 2

#define BUTTON_THRESHOLD 10 //segundos a esperar para que se presione el boton que inicia el dispendio

//ALARMA
const int UMBRAL_ALARMA_SEG = 2; //umbral para determinar si la hora actual coincide con alguna alarma


struct Alarm {
  byte plateID;
  DateTime startTime; //fecha y hora de inicio
  int interval; //invervalo de toma
  byte quantity; //cantidad a tomar
  byte times; //total de veces que se ejecuto la alarma, para calcular el proximo dispendio
  byte dispensedTimes; //total de veces que se dispenso (usuario apreto el boton)
  int stock; // stock
  int criticalStock; // stock critico
  byte periodicity; // enum Periodicidad
  char days[7] = "1111111"; //lun-dom dias a dispensar array con unos o ceros
  bool block; // si block es true, se bloquea si no se reprograma
  bool blocked; // indica que esta alarma/plato dispensador esta bloqueado
  bool waitingForButton; // si tiene que dispensar y esta esperando el boton
  bool movePlate; // si tiene que mover el plato
  byte valid; //data valida


};


class Planificador{

   protected:
    char * DAP_UNIQUE_ID = "DAP_001"; // ver si esto se puede hacer flexible para soportar mas de 1 DAP en el server.
    List<Alarm> configDataList;
    void setInitTime(time_t initTime);
    
    void loadAlarms();
    void saveAlarms();
    time_t getLocalTime(time_t utc);
    void logEvento(String evento, String msg="");
    int plateIDToIndex(int plateID);
    long nextDispense(Alarm* config);
    bool alarmDispensed(Alarm* config);
    void activarBuzzer();
    void desactivarBuzzer();
    
    //MOTOR
    Servo plate1;
    Servo plate2;
    Servo plates[MAX_SUPPORTED_ALARMS] = {plate1, plate2};
    unsigned long previousMillisMotor[MAX_SUPPORTED_ALARMS] = {0,0};  
    unsigned long previousMillisBuzzer = 0;
   
   public: 
    Planificador();
    int storedAlarms = 0;
    
    bool execute();   
    DateTime getTime();
    String getTimeString(DateTime t);
    void setAlarm(DateTime startTime, int interval, int quantity, int plateID);
    void setAlarm(DateTime startTime, int interval, int quantity, int stock, int criticalStock, byte periodicity, char* days, bool block, int plateID);
    Alarm getAlarm(int index);
    String getAlarmString(Alarm config);
    void resetAlarms();
    void processPlates();
    int getIndexForPlateID(int plateID);
    void checkCriticalStock();
    void processCommandsWIFI();

    //MOTOR
    void initServo();
    void startPlate(Servo plate, int index);
    void stopPlate(Servo plate);


};

#endif
