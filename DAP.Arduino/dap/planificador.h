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
#include <NewTone.h>

#define PERIODICIDAD_DIARIA 0
#define PERIODICIDAD_SEMANAL 1
#define PERIODICIDAD_PERSONALIZADA 2

//codigos notificacion
#define NOTIF_EXPENDIO_NO_REALIZADO_NO_PASTILLAS    1
#define NOTIF_EXPENDIO_NO_REALIZADO_LIMITE_TIEMPO   2
#define NOTIF_STOCK_CRITICO                         3
#define NOTIF_BOTON_NO_PRESIONADO                   4
#define NOTIF_VASO_NO_RETIRADO                      5
#define NOTIF_VASO_NO_DEVUELTO                      6
#define NOTIF_BLOQUEO                               7


//ALARMA
const int UMBRAL_ALARMA_SEG = 1; //umbral para determinar si la hora actual coincide con alguna alarma

const int MAX_NOTIF_ARRAY = 6;

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
  bool block = false;; // si block es true, se bloquea si no se reprograma
  bool blocked = false; // indica que esta alarma/plato dispensador esta bloqueado
  bool shouldStartDispense = false;
  bool waitingForButton = false; // si tiene que dispensar y esta esperando el boton
  bool notifVasoNoDevueltoEnviada = false;
  bool notifBotonNoPresionadoEnviada = false; //si la notif por superar el button threshold fue enviada
  bool notifExpendioNoRealizadoLimiteTiempoEnviada = false;
  bool notifVasoNoRetiradoEnviada = false;
  bool waitingForVaso = false; // si tiene que retirar el vaso
  bool movePlate = false; // si tiene que mover el plato
  char pillName[10];
  DateTime nextDispenseDateTime; //fecha de proximo dispendio
  bool complete = false;
  byte valid; //data valida


};


class Planificador{

   protected:
    List<Alarm> configDataList;
    String notificationArray[MAX_NOTIF_ARRAY] = {"","","","","",""};
    void setInitTime(time_t initTime);
    
    void loadAlarms();
    void saveAlarms();
    time_t getLocalTime(time_t utc);
    void logEvento(String evento, String msg="");
    int plateIDToIndex(int plateID);
    long nextDispense(Alarm* config);
    long _nextDispense(Alarm* config);
    void setNextDispenseDateTime(Alarm* config);
    void blockPlate(Alarm* config);
    void reschedulePlate(Alarm* config);
    bool alarmDispensed(Alarm* config);
    void activarBuzzer();
    void activarBuzzerRetiro();
    void desactivarBuzzer();
    unsigned long previousMillisBuzzer = 0;
    unsigned long previousMillisVaso = 0;
    bool isVasoInPlace();
    bool waitingForOtherPlate();
    int quantity[MAX_SUPPORTED_ALARMS] = {0, 0};
    long vasoInPlaceSec [MAX_SUPPORTED_ALARMS] = {0, 0};
    long buttonPressedSec[MAX_SUPPORTED_ALARMS] = {0, 0};
    long pillThroughTubeSec[MAX_SUPPORTED_ALARMS] = {0, 0};
    String macAddress;
    bool WIFI_OK = false;

    //LEDs
    int ledValue = 0;
    bool showRedLED = false;
    bool showGreenLED = false;
    bool showBlueLED = false;
    bool showOrangeLED = false;
    int ledTimes = 0;
    void activarLED(String color, int times);
    void desactivarLED(String color);
    void resetLED();
    unsigned long previousMillisLED = 0;
    
    //MOTOR
    Servo plate1;
    Servo plate2;
    Servo plates[MAX_SUPPORTED_ALARMS] = {plate1, plate2};
    unsigned long previousMillisMotor[MAX_SUPPORTED_ALARMS] = {0,0};  
    
   
   public: 
    Planificador();
    
    //umbrales (modificables)
    int NO_DISPENSE_THRESHOLD = 30; //segundos a esperar para que efectue el dispendio luego de apretar el boton
    int BUTTON_THRESHOLD = 5; //segundos a esperar para que se presione el boton que inicia el dispendio
    int VASO_THRESHOLD = 20; //segundos a esperar para que se retire el vaso, pasado este tiempo se emite notificacion
  
    int storedAlarms = 0;
    
    bool execute();   
    DateTime getTime();
    String getTimeString(DateTime t);
    void setStock(int stock, int plateID, char* pillName);
    void setAlarm(DateTime startTime, int interval, int quantity, int plateID, bool block);
    void setAlarm(DateTime startTime, int interval, int quantity, int criticalStock, byte periodicity, char* days, bool block, int plateID);
    Alarm getAlarm(int index);
    String getAlarmString(Alarm config);
    void resetAlarms();
    void processPlates();
    int getIndexForPlateID(int plateID);
    void checkAndNotifyCriticalStock(Alarm *config);
    void processCommandsWIFI();
    int sendNotification(int code, Alarm *config);
    int sendPlanificacion(Alarm *config);
    int sendCarga(int containerID, String pillName, int stock);

    //MOTOR
    void initServo();
    void startPlate(Servo plate, int index);
    void stopPlate(Servo plate);
    void stopAllPlates();

    //LED
    void processLED();

    //NOTIFICACIONES
    void processNotifications();

    //DEBUG
    void clearEEPROM();


};

//UTILS
static char* string2char(String str){
    if(str.length()!=0){
        char *p = const_cast<char*>(str.c_str());
        return p;
    }
}

#endif
