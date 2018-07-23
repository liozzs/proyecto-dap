#include "arduino.h"
#include "planificador.h"
#include "interrupcion.h"

RTC_DS3231 rtc;

Planificador::Planificador(){
  pciSetup(PIN_BUTTON);
  for (int i=0; i < MAX_SUPPORTED_ALARMS; i++)
     pciSetup(PIN_DISPENSE_SENSOR[i]);
  
  loadAlarms();
  setInitTime(0);

  //MOTOR
  initServo();
    
  //
};

void Planificador::setAlarm(DateTime startTime, int interval, int quantity, int plateID)
{    
  this->setAlarm(startTime, interval, quantity, 1000, 0, "1111111", true, plateID);
}

/*
 * Agrega una alarma (configuracion de dispendio) asociada a un plateID. Si ya existe el plateID, reemplaza la configuracion, de lo contrario agrega una
 * nueva configuracion.
 */
void Planificador::setAlarm(DateTime startTime, int interval, int quantity, int criticalStock, byte periodicity, char* days, bool block, int plateID)
{    
  Alarm config;

  config.plateID = plateID;
  config.startTime = startTime;
  config.interval = interval;
  config.quantity = quantity;
  config.criticalStock = criticalStock;
  config.periodicity = periodicity; 
  strcpy( config.days, days);
  config.block = block;
  config.blocked = false;
  config.waitingForButton = false;
  config.times = 0;
  config.movePlate = false;
  config.valid = 100;

  Log.Debug("seteando %d,%d,%d,%d, %s,%d\n", interval, quantity, criticalStock, periodicity, days, plateID);
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

/*
 * Carga las configuraciones de alarma guardadas en la memoria EEPROM
 */
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

void Planificador::resetAlarms(){
  this->configDataList.Clear();
}
/*
 * Guarda las configuraciones de alarma en la memoria EEPROM
 */
void Planificador::saveAlarms(){
   //guardo la configuracion
   EEPROM.put(0, this->storedAlarms);
   Log.Debug("Guardando %d alarmas:\n", this->storedAlarms); 
   
   for (int i = 0; i < this->configDataList.Count(); i++) {
    EEPROM.put(sizeof(int) +  sizeof(Alarm) * i, this->configDataList[i]);
    Log.Debug("EEPROM put config para plateID %d:\n", this->configDataList[i].plateID); 
   }
}

/*
 * Retorna la posicion en la lista de configuraciones de alarma para un plateID determinado
 */
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

/*
 * Inicializa la hora del modulo RTC
 */
void Planificador::setInitTime(time_t initTime) {
  if (! rtc.begin()) {
      Serial.println("Couldn't find RTC");
  while (1);
  }
  
  if (initTime == 0)
    rtc.adjust(DateTime(F(__DATE__), F(__TIME__)));
  else
    rtc.adjust(DateTime(initTime));
}

DateTime Planificador::getTime(){
  return rtc.now();
}

String Planificador::getTimeString(DateTime t){
  String str = String(t.year()) + '/' + t.month() + '/' + t.day() + ' ';
  str = str + t.hour() + ':' + t.minute() + ':' + t.second() ;

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

/*
 * Indica si hay que dispensar en este preciso momento al comaparar las alarmas de los platos con la hora actual del RTC
 * (en proceso)
 */
bool Planificador::execute(){
 
  if (this->configDataList.Count() == 0) {
    Log.Debug("Verificando dispendio: No hay alarmas configuradas\n");
  }

  for (int i = 0; i < this->configDataList.Count(); i++)
  {
    Alarm* config = &this->configDataList[i];
 
    Log.Debug("Verificando dispendio platoID: %d, veces a dispensado: %d, cantidad: %d\n", config->plateID, config->times, config->quantity);

    if (config->blocked == true) {
      Log.Debug("Este plato se encuentra BLOQUEADO\n");
      continue;
    }

    //verificar si existe dispendio pendiente - presionar boton
    if (config->waitingForButton == true){
      Log.Debug("Alarma %d esperando por boton\n", config->plateID);
      if (isButtonPressed()){
        Log.Debug("BOTON PRESIONADO\n");
        config->times++;
        config->waitingForButton = false;
        config->movePlate = true;
        saveAlarms();
        setButtonPressed(false);
        continue;
      }
    }

    //verificar si corresponde el dia. Nosotros tomamos 1er dia de la semana el lunes.
    int day = getTime().dayOfTheWeek() - 1;
    if (day == -1) day = 6;

    if (config->days[day] != '1') {
       Log.Debug("Dispendio no configurado para este dia");
       continue;
    }

    long sec = nextDispense(config);

    if (sec <= UMBRAL_ALARMA_SEG && config->waitingForButton == false && config->movePlate == false) {
      Log.Debug("Dispendio SI: segundos de diferencia: %l\n", sec);
      setButtonReady(true); //habilita el presionado del boton    
      config->waitingForButton = true;
      saveAlarms(); //guardar el cambio 

    }
    if (sec > UMBRAL_ALARMA_SEG && config->waitingForButton == true) {
       config->waitingForButton = false;
       if (config->block == true) {
        config->blocked = true; // se bloquea
       } else {
        config->times++; // se reprograma 
       }
       setButtonReady(false); // se paso el tiempo de espera de presionado del boton
       saveAlarms();
    }

  }
}

//Devuelve el tiempo en segundos para el proximo dispendio de la alarma 'config'
long Planificador::nextDispense(Alarm* config) {
  DateTime nextDispense;

  // Si es periodicidad diaria o semanal tiene un intervalo de toma
  if (config->periodicity == PERIODICIDAD_DIARIA || config->periodicity == PERIODICIDAD_PERSONALIZADA ) {
    nextDispense = config->startTime + TimeSpan(config->times * config->interval); 
  // Si es semanal, existe un solo horario de toma
  } else if (config->periodicity == PERIODICIDAD_SEMANAL) {
    nextDispense = config->startTime + TimeSpan(config->times * 1,0,0,0); // intervalo seria 1 dia
  } else {
    Log.Debug("Configuracion de alarma incorrecta");
  }
  TimeSpan diff = nextDispense - this->getTime();

  Log.Debug("Proximo dispendio: dia:%d, hora:%d, min:%d, seg:%d\n", diff.days(), diff.hours(), diff.minutes(), diff.seconds());
  long sec = diff.days()*86400L + diff.hours()*3600L + diff.minutes()*60L + diff.seconds();
  
  return sec;
}

//SECCION MANEJO MOTOR

void Planificador::processPlates(){
  for (int i = 0; i < this->configDataList.Count(); i++)
  {
    Alarm* config = &this->configDataList[i];

    if (config->movePlate == true)
      startPlate(plates[plateIDToIndex(config->plateID)]);

  //Serial.println(sensorDetected);
    if (getSensorDetected() != -1) {
      stopPlate(plates[getSensorDetected()]);
      setSensorDetected(-1);
      config->movePlate = false;
    }
  }

  
}

int Planificador::plateIDToIndex(int plateID) {
  for (int i=0; i< MAX_SUPPORTED_ALARMS; i++) {
    if (plateID == PLATE_IDS[i])
      return i;
  }
  return -1;
}


void Planificador::initServo(){
  for (int i=0; i< MAX_SUPPORTED_ALARMS; i++) 
     plates[i].attach(PIN_PLATE_MOTOR[i]);
}
void Planificador::startPlate(Servo plate) {

    plate.write(180);
    delay(20);
    plate.writeMicroseconds(1500);
    delay(90); 
}

void Planificador::stopPlate(Servo plate) {
  plate.write(90);                
}


//Devuelve unix time en horario local
time_t Planificador::getLocalTime(time_t utc){
  TimeChangeRule ART = { "ART", First, Sun, Jan, 0, -180 }; 
  Timezone artTimezone(ART);
  
  return artTimezone.toLocal(utc);
}

//Procesar mensajes y acciones recibidas del server
void Planificador::processCommandsWIFI()
{
  
  //Lee mensajes de WIFI 
  String str = readFromWIFI();
  
  //Log.Debug("Recibido de WIFI: %s\n", string2char(str));
  
  if (str != "" && !str.startsWith("debug:", 0)) {
    StaticJsonBuffer<130> jsonBuffer;
    JsonObject& root = jsonBuffer.parseObject(str);
    if (!root.success()) {
      Log.Debug("Json parseObject() failed");
    }

    if (root.containsKey("time")){
      Log.Debug("Recibido time_t de wifi! %l", root["time"].as<time_t>());
      this->setInitTime(root["time"].as<time_t>());
    } 
 }
}

//UTILS
char* string2char(String str){
    if(str.length()!=0){
        char *p = const_cast<char*>(str.c_str());
        return p;
    }
}


