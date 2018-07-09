#include "arduino.h"
#include "planificador.h"
// Date and time functions using a DS3231 RTC connected via I2C and Wire lib

RTC_DS3231 rtc;

Planificador::Planificador(){
  setInitTime();
};

void Planificador::setAlarm(DateTime startTime, int interval, int quantity, int plateID)
{    
  PlateConfig config;

  config.plateID = plateID;
  config.startTime = startTime;
  config.interval = interval;
  config.quantity = quantity;
  config.times = 0;

  this->configDataList.Add(config);
}

PlateConfig Planificador::getAlarm(int index){
  return this->configDataList[index];
}

void Planificador::setInitTime() {
  if (! rtc.begin()) {
      Serial.println("Couldn't find RTC");
  while (1);
  }

  //TODO: Por ahora setea la hora inicial con la fecha de compilación.
  rtc.adjust(DateTime(F(__DATE__), F(__TIME__)));
}

DateTime Planificador::getTime(){
  return rtc.now();
}

String Planificador::getTimeString(){
  DateTime t = this->getTime();
  String str = String(t.year()) + '/' + t.month() + '/' + t.day() + ' ';
  str = str + t.hour() + ':' + t.minute() + ':' + t.second() + '\n';

  return str;
}

String Planificador::getPlateConfigString(PlateConfig config){
  DateTime t = config.startTime;
  
  String str = "\nPlateID: " + String(config.plateID) + '\n';
  str = str + "Start time: " + t.year() + '/' + t.month() + '/' + t.day() + ' ';
  str = str + t.hour() + ':' + t.minute() + ':' + t.second() + '\n';

  str = str + "Intervalo: " + config.interval + '\n';
  str = str + "Cantidad: " + config.quantity + '\n';
  str = str + "Repeticion: " + config.times + '\n';
  
  return str;
}

void Planificador::loadAlarms(){
  
}

void Planificador::saveAlarms(){
  
}

bool Planificador::isDispenseTime(){
  DateTime nextDispense;

  if (this->configDataList.Count() == 0) {
    Log.Debug("Verificando dispendio: No hay alarmas configuradas\n");
  }

  for (int i = 0; i < this->configDataList.Count(); i++)
  {
    PlateConfig config = this->configDataList[i];
    Log.Debug("Verificando dispendio platoID: %d\n", config.plateID);
    nextDispense = config.startTime + TimeSpan(config.times * config.interval);
    TimeSpan diff = nextDispense - this->getTime();
    int sec=abs(diff.seconds());

    if (sec <= UMBRAL_ALARMA_SEG) {
      Log.Debug("Dispendio SI: segundos de diferencia: %d\n", sec);
      return true;
    }
    Log.Debug("Dispendio NO: segundos de diferencia: %d\n", sec);
  }
  return false;
}


//SECCION MANEJOR MOTOR
void Planificador::initServo(Servo servo, int plateID){
  servo.attach(PIN_PLATE_MOTOR[plateID]);
}
void Planificador::startPlate(Servo plate) {
   plate.write(65);             
}

void Planificador::stopPlate(Servo plate) {
  plate.write(90);                
}

bool Planificador::isButtonPressed(){
  if (digitalRead(PIN_BUTTON) == 0)
    return true;
  return false;
}
bool Planificador::isDispensed(int plateID){
  int pin = PIN_DISPENSE_SENSOR[plateID];
  return digitalRead(pin);
      
}
