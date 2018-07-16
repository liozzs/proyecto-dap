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

struct Alarm {
  byte plateID;
  DateTime startTime; //fecha y hora de inicio
  int interval; //invervalo de toma
  byte quantity; //cantidad a tomar
  byte times; //total de veces que se dispenso
  byte valid; //data valida
};


class Planificador{

   protected:

    void setInitTime(time_t initTime);
    
    int getIndexForPlateID(int plateID);
    
   
   public: 
    Planificador();
    int storedAlarms = 0;
     List<Alarm> configDataList;
    DateTime getTime();
    String getTimeString();
    void setAlarm(DateTime startTime, int period, int times, int plateID);
    Alarm getAlarm(int index);
    String getAlarmString(Alarm config);
    void loadAlarms();
    void saveAlarms();
    bool isDispenseTime();
    void logEvento(String evento, String msg="");
    time_t getLocalTime(time_t utc);
    void procesarAcciones();

    //MOTOR
    void initServo(Servo servo, int plateID);
    void startPlate(Servo plate);
    void stopPlate(Servo plate);
    bool isButtonPressed();
    bool isDispensed(int plateID);

};

#endif
