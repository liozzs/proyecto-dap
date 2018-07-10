#include "arduino.h"
#include "planificador.h"
// Date and time functions using a DS3231 RTC connected via I2C and Wire lib

RTC_DS3231 rtc;

Planificador::Planificador(){
  loadAlarms();
  setInitTime();
};

void Planificador::setAlarm(DateTime startTime, int interval, int quantity, int plateID)
{    
  Alarm config;

  config.plateID = plateID;
  config.startTime = startTime;
  config.interval = interval;
  config.quantity = quantity;
  config.times = 0;
  config.valid = 100;
  
  //verifico si ya existe ese plateID para reemplazar la alarma o agregar nueva
  int index = getIndexForPlateID(plateID);
  if (index != -1) {
    this->configDataList.Replace(index, config);
    Log.Debug("Reemplazo alarma para plateID: %d, index: %d", plateID, index);
  } else {
    this->configDataList.Add(config);
    this->storedAlarms = this->storedAlarms +  1;   
    Log.Debug("Agregado de alarma para plateID: %d", plateID);
  }
  saveAlarms();
}

void Planificador::loadAlarms(){
  //Cargar datos guardados
  EEPROM.get(0, this->storedAlarms);
  Log.Debug("Alarmas guardadas: %d\n", this->storedAlarms);
  if (storedAlarms == 0) {
    Log.Debug("No hay alarmas en guardadas en EEPROM\n");
  } else {
    for (int i = 0; i < this->storedAlarms; i++)
      {
        Alarm configData;
        EEPROM.get(sizeof(int) +  sizeof(Alarm) * i, configData);
        this->configDataList.Add(configData);
        Log.Debug("EEPROM get config para plateID %d:\n", this->configDataList[i].plateID); 
      }
  } 
}

void Planificador::saveAlarms(){
   //guardo la configuracion
   EEPROM.put(0, this->storedAlarms);
   Log.Debug("Guardando %d alarmas:\n", this->storedAlarms); 
   for (int i = 0; i < this->configDataList.Count(); i++) {
    EEPROM.put(sizeof(int) +  sizeof(Alarm) * i, this->configDataList[i]);
    Log.Debug("EEPROM put config para plateID %d:\n", this->configDataList[i].plateID); 
   }
}


int Planificador::getIndexForPlateID(int plateID) {
  for (int i = 0; i < this->configDataList.Count(); i++) {
    if (this->configDataList[i].plateID == plateID) 
      return i;
  }
  return -1;
}

Alarm Planificador::getAlarm(int index){
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

String Planificador::getAlarmString(Alarm config){
  DateTime t = config.startTime;
  
  String str = "\nPlateID: " + String(config.plateID) + '\n';
  str = str + "Start time: " + t.year() + '/' + t.month() + '/' + t.day() + ' ';
  str = str + t.hour() + ':' + t.minute() + ':' + t.second() + '\n';

  str = str + "Intervalo: " + config.interval + '\n';
  str = str + "Cantidad: " + config.quantity + '\n';
  str = str + "Repeticion: " + config.times + '\n';
  
  return str;
}

bool Planificador::isDispenseTime(){
  DateTime nextDispense;

  if (this->configDataList.Count() == 0) {
    Log.Debug("Verificando dispendio: No hay alarmas configuradas\n");
  }

  for (int i = 0; i < this->configDataList.Count(); i++)
  {
    Alarm* config = &this->configDataList[i];
    Log.Debug("Verificando dispendio platoID: %d, veces a dispensado: %d, cantidad: %d\n", config->plateID, config->times, config->quantity);
    if (config->times < config->quantity) {
      nextDispense = config->startTime + TimeSpan(config->times * config->interval);
      TimeSpan diff = nextDispense - this->getTime();
      Log.Debug("Horario: dia:%d, hora:%d, min:%d, seg:%d", diff.days(), diff.hours(), diff.minutes(), diff.seconds());
      long sec=abs(diff.days()*86400 + diff.hours()*3600 + diff.minutes()*60 + diff.seconds());

      if (sec <= UMBRAL_ALARMA_SEG) {
        Log.Debug("Dispendio SI: segundos de diferencia: %l\n", sec);
        config->times++;
        return true;
      }
      Log.Debug("Dispendio NO: segundos de diferencia: %l\n", sec);
    } else {
      Log.Debug("Plan terminado\n");
    }
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
