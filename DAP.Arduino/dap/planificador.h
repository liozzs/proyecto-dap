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

struct Alarm {
  byte plateID;
  DateTime startTime; //fecha y hora de inicio
  int interval; //invervalo de toma
  byte quantity; //cantidad a tomar
  byte times; //total de veces que se dispenso
  int criticalStock; // stock critico
  byte periodicity; // enum Periodicidad
  char days[7] = "1111111"; //lun-dom dias a dispensar array con unos o ceros
  byte valid; //data valida
};


class Planificador{

   protected:
    List<Alarm> configDataList;
    void setInitTime(time_t initTime);
    int getIndexForPlateID(int plateID);
    void loadAlarms();
    void saveAlarms();
    time_t getLocalTime(time_t utc);
    void logEvento(String evento, String msg="");
   
   public: 
    Planificador();
    int storedAlarms = 0;
    DateTime getTime();
    String getTimeString(DateTime t);
    void setAlarm(DateTime startTime, int interval, int quantity, int plateID);
    void setAlarm(DateTime startTime, int interval, int quantity, int criticalStock, byte periodicity, char* days, int plateID);
    Alarm getAlarm(int index);
    String getAlarmString(Alarm config);
    void resetAlarms();
    bool isDispenseTime();
    
    
    void procesarAcciones();

    //MOTOR
    void initServo(Servo servo, int plateID);
    void startPlate(Servo plate);
    void stopPlate(Servo plate);
    bool isButtonPressed();
    bool isDispensed(int plateID);

};

#endif
