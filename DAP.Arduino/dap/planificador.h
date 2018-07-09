#ifndef _PLANIFICADOR_h
#define _PLANIFICADOR_h

#include "config.h"
#include "RTClib.h"
#include "ListLib.h"
#include <Logging.h>
#include <Servo.h>


struct PlateConfig {
  int plateID;
  DateTime startTime; //fecha y hora de inicio
  int interval; //invervalo de toma
  int quantity; //cantidad a tomar
  int times; //total de veces que se dispenso
};

class Planificador{

   protected:

    int modoOperacion;
    void setInitTime();
    List<PlateConfig> configDataList;
    
    
   
   public: 
    Planificador();
    
    DateTime getTime();
    String getTimeString();
    void setAlarm(DateTime startTime, int period, int times, int plateID);
    PlateConfig getAlarm(int index);
    String getPlateConfigString(PlateConfig config);
    void loadAlarms();
    void saveAlarms();
    bool isDispenseTime();
    void logEvento(String evento, String msg="");

    //MOTOR
    void initServo(Servo servo, int plateID);
    void startPlate(Servo plate);
    void stopPlate(Servo plate);
    bool isButtonPressed();
    bool isDispensed(int plateID);

};

#endif
