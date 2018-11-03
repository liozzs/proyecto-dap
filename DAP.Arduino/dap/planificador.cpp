#include "arduino.h"
#include "planificador.h"
#include "interrupcion.h"

RTC_DS3231 rtc;
DynamicJsonBuffer  jsonBuffer;

//Constructor Planificador
Planificador::Planificador(){
  pciSetup(PIN_BUTTON);
  for (int i=0; i < MAX_SUPPORTED_ALARMS; i++){
     pciSetup(PIN_DISPENSE_SENSOR[i]);
  }
  
  loadAlarms();
  rtc.begin();

  //MOTOR
  initServo();

  //VASO
  pinMode(PIN_VASO_LASER, OUTPUT);
  digitalWrite(PIN_VASO_LASER, HIGH);

  //LED RGB
  pinMode(PIN_LEDRED, OUTPUT);
  pinMode(PIN_LEDGREEN, OUTPUT);
  pinMode(PIN_LEDBLUE, OUTPUT);

  //WIFI
//  while (WIFI_OK == false) {
//    Log.Debug("Waiting for WIFI\n");
//    processCommandsWIFI();
//    delay(10);
//  }
  
  JsonObject& root = jsonBuffer.createObject();

  root["get_MAC"] = ""; //para pedir la MAC
  root["get_Time"] = ""; //para pedir time / IP
  root.printTo(Serial1);
 
};

/*
 * Seteo de carga de pastillas. Si ya existe el plateID, reemplaza la configuracion, de lo contrario agrega una
 * nueva configuracion.
 */
void Planificador::setStock(int stock, int plateID, char* pillName)
{    

  Alarm* config;

  Log.Debug("seteando %d,%s\n", plateID, pillName);
  //verifico si ya existe ese plateID para reemplazar la configuracion o agregar nueva
  int index = getIndexForPlateID(plateID);
  if (index != -1) {
    config = &configDataList[index];
    config->stock = config->stock + stock;
    strcpy( config->pillName, pillName);
    
    Log.Debug("Reemplazo stock para plateID: %d, index: %d", plateID, index);
  } else {
    Alarm config;
    config.plateID = plateID;
    config.stock =  stock;
    strcpy( config.pillName, pillName);
    config.complete = false;
    config.valid = 100;
  
    this->configDataList.Add(config);
    this->storedAlarms = this->storedAlarms +  1;   
    Log.Debug("Agregado de stock para plateID: %d", plateID);
  }
  sendCarga(plateID, pillName, stock);
  saveAlarms();
}

void Planificador::setAlarm(DateTime startTime, int interval, int quantity, int plateID, bool block)
{    
  this->setAlarm(startTime, interval, quantity, 5, 0, "1111111", block, plateID);
}

/*
 * Agrega una alarma (configuracion de dispendio) asociada a un plateID. Si ya existe el plateID, reemplaza la configuracion, de lo contrario agrega una
 * nueva configuracion.
 */
void Planificador::setAlarm(DateTime startTime, int interval, int quantity, int criticalStock, byte periodicity, char* days, bool block, int plateID)
{    
  Alarm* config;

  Log.Debug("seteando %d,%d,%d,%d, %d, %d\n", interval, quantity, criticalStock, periodicity, days, plateID);
  //verifico si ya existe ese plateID para reemplazar la alarma o agregar nueva
  int index = getIndexForPlateID(plateID);
  if (index != -1) {
    config = &configDataList[index];
    
    config->startTime = startTime;
    config->interval = interval;
    config->quantity = quantity;
    config->criticalStock = criticalStock;
    config->periodicity = periodicity; 
    strcpy( config->days, days);
    config->block = block;
    config->blocked = false;
    config->waitingForButton = false;
    config->waitingForVaso = false;
    config->dispensedTimes = 0;
    config->times = 0;
    config->movePlate = false;
    config->complete = true;
    config->valid = 100;
  
    //this->configDataList.Replace(index, *config);
    Log.Debug("Seteo/Reemplazo alarma para plateID: %d, index: %d", plateID, index);
    sendPlanificacion(config);
  } else {
    //this->configDataList.Add(config);
    //this->storedAlarms = this->storedAlarms +  1;   
    Log.Debug("Se debe configurar la carga de pastilla antes para plateID: %d", plateID);
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
     Log.Debug("Couldn't find RTC");
  while (1);
  }
  
  if (initTime == 0)
    rtc.adjust(DateTime(F(__DATE__), F(__TIME__)));
  else {
    rtc.adjust(DateTime(getLocalTime(initTime)));
  }
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
    Log.Debug("No hay alarmas configuradas\n");
  }

  for (int i = 0; i < this->configDataList.Count(); i++)
  {
    Alarm* config = &this->configDataList[i];

    if (config->complete == false) {
      Log.Debug("Verificando configuracion stock platoID: %d, stock: %d, pasti: %s\n", config->plateID, config->stock, config->pillName);
      continue;
    }
    
    Log.Debug("Verificando dispendio platoID: %d, veces activado: %d, veces dispensado: %d, cantidad: %d, pasti: %s\n", config->plateID, config->times, config->dispensedTimes,config->quantity, config->pillName);

    if (config->blocked == true) {
      Log.Debug("Este plato se encuentra BLOQUEADO\n");
      continue;
    }

    if (config->stock < config->quantity){
      Log.Debug("stock (%d) no suficiente para proximo dispendio (%d), enviando notificacion\n", config->stock, config->quantity);
      sendNotification(NOTIF_EXPENDIO_NO_REALIZADO_NO_PASTILLAS, config->plateID, config->pillName, getTimeString(nextDispenseDateTime(config)), config->stock); 
      blockPlate(config);
      continue;
    }
    
    long sec = nextDispense(config);
    
    //verificar si existe dispendio pendiente - presionar boton
    if (config->waitingForButton == true){
      Log.Debug("Alarma %d esperando por boton\n", config->plateID);
      if (isButtonPressed()){
        Log.Debug("BOTON PRESIONADO\n");
        if (sec > BUTTON_THRESHOLD) {
          Log.Debug("Fuera de umbral\n");
        }
        buttonPressedSec[i] = sec;
        config->dispensedTimes++;
        config->stock--;
        config->waitingForButton = false;
        config->movePlate = true;

        if (checkCriticalStock(config)) {
          Log.Debug("Alarma %d supero tiene stock critico de %d\n", config->plateID, config->criticalStock);
          sendNotification(NOTIF_STOCK_CRITICO, config->plateID, config->pillName, getTimeString(nextDispenseDateTime(config)), config->stock);
        }
      
        desactivarBuzzer();
        saveAlarms();
        continue;
      }
    }

    //verificar si ya se hizo el dispendio
    if (alarmDispensed(config) == true && config->movePlate == true) {
       quantity[i]++;
       setSensorDetected(-1);
      
       if (config->quantity == quantity[i]) {
         pillThroughTubeSec[i] = sec;
         config->movePlate = false;
         config->waitingForVaso = true;
         previousMillisVaso = millis();
         quantity[i] = 0;
         saveAlarms();
       } else {
        Log.Debug("Dispendio multiple de pastillas, esperando..");
       }
    }

    //verificar si corresponde el dia. Nosotros tomamos 1er dia de la semana el lunes.
    if (config->periodicity != PERIODICIDAD_DIARIA) {
      int day = getTime().dayOfTheWeek() - 1;
      if (day == -1) day = 6;
  
      if (config->days[day] != '1') {
         Log.Debug("Dispendio no configurado para este dia");
         continue;
      }
    }

    //verificar el retiro del vaso
    if (config->waitingForVaso && !waitingForOtherPlate()){
      Log.Debug("Chequando si el vaso se retiro\n");
      activarBuzzerRetiro();
      activarLED("green", 50);
      if (!isVasoInPlace()){

        if (config->notifVasoNoDevueltoEnviada || config->notifExpendioNoRealizadoLimiteTiempoEnviada || 
                config->notifBotonNoPresionadoEnviada || config->notifVasoNoRetiradoEnviada) {
          reschedulePlate(config);
        } else {
            config->times++;
        }

        Log.Debug("VASO retirado\n");
        config->waitingForVaso = false;
        desactivarLED("green");
      }
    }
    else if (config->waitingForVaso == true && waitingForOtherPlate()) {
      Log.Debug("Esperando el dispendio de otro plato antes de retirar vaso\n");
    }

    if (sec <= UMBRAL_ALARMA_SEG && !config->waitingForButton && !config->movePlate && !config->waitingForVaso) {
      config->shouldStartDispense = true;
    }

    if (config->shouldStartDispense && !config->waitingForButton && !config->movePlate && !config->waitingForVaso) {
      if (isVasoInPlace()) {
        Log.Debug("Dispendio SI: segundos de diferencia: %l\n", sec);
        vasoInPlaceSec[i] = sec;
        setButtonReady(true); //habilita el presionado del boton    
        config->shouldStartDispense = false;
        config->waitingForButton = true;
        config->notifVasoNoDevueltoEnviada = false;
        config->notifBotonNoPresionadoEnviada=false;
        config->notifExpendioNoRealizadoLimiteTiempoEnviada=false;
        config->notifVasoNoRetiradoEnviada=false;
        saveAlarms(); //guardar el cambio 
      } else {
        if (sec > VASO_THRESHOLD && !config->notifVasoNoDevueltoEnviada) {
          Log.Debug("VASO no DEVUELTO, enviando notificacion\n"); //TODO que mas se hace? se sigue como si nada?
          sendNotification(NOTIF_VASO_NO_DEVUELTO, config->plateID, config->pillName, getTimeString(nextDispenseDateTime(config)), config->stock); 
          config->notifVasoNoDevueltoEnviada = true;
    
          if (config->block) {
            blockPlate(config);
          }

          saveAlarms(); //guardar el cambio 
        }
      }
 
    }

    //Se considera BUTTON_THRESHOLD como el umbral para el envio de notificacion si no presiona boton
    if (sec > vasoInPlaceSec[i] + BUTTON_THRESHOLD && config->waitingForButton && !config->notifBotonNoPresionadoEnviada) {
       Log.Debug("Boton no presionado, enviando notificacion\n"); 
       sendNotification(NOTIF_BOTON_NO_PRESIONADO, config->plateID, config->pillName, getTimeString(nextDispenseDateTime(config)), config->stock);
       config->notifBotonNoPresionadoEnviada=true;
       
       if (config->block) {
        blockPlate(config);
       }      
       
       saveAlarms();
    }

    if (sec > buttonPressedSec[i] + NO_DISPENSE_THRESHOLD && config->movePlate && !config->notifExpendioNoRealizadoLimiteTiempoEnviada) {
       Log.Debug("Tiempo de dispendio excedido, enviando notificacion\n"); 
       sendNotification(NOTIF_EXPENDIO_NO_REALIZADO_LIMITE_TIEMPO, config->plateID, config->pillName, getTimeString(nextDispenseDateTime(config)), config->stock);
       config->notifExpendioNoRealizadoLimiteTiempoEnviada = true;

       if (config->block){
        blockPlate(config);
       }
       
       saveAlarms();
    }

    //Accion en caso de no retiro del vaso. No haria mas q enviar la notificacion, el dispendio ya esta hecho (no tiene sentido bloquear o reprogramar
    if (sec > pillThroughTubeSec[i] + VASO_THRESHOLD && config->waitingForVaso && !config->notifVasoNoRetiradoEnviada) {
    //if (millis() - previousMillisVaso > VASO_THRESHOLD * 1000 && config->waitingForVaso == true){
      Log.Debug("VASO no retirado, enviando notificacion\n");
      sendNotification(NOTIF_VASO_NO_RETIRADO, config->plateID, config->pillName, getTimeString(nextDispenseDateTime(config)), config->stock); 
      config->notifVasoNoRetiradoEnviada = true;

      if (config->block) {
        blockPlate(config);
      }

      saveAlarms();
    }

    //TODO revisar logica, se activo esto cuando no se presiono el boton
    //En caseo de apagarse el DAP, verifica si se perdieron dispendios en el medio y en tal caso bloquea
//    if (_nextDispense(config) < (BUTTON_THRESHOLD * -2)){
//      Log.Debug("DAP desconectado, se omitieron dispendios. Bloqueando plato\n"); 
//      config->blocked = true;
//    }

    if (config->shouldStartDispense || config->waitingForButton == true) {
      activarBuzzer();
      activarLED("blue", 50);
    } else {
      desactivarLED("blue");
    }

    if(config->movePlate == true) {
      activarLED("red", 50);
    } else {
      desactivarLED("red");
    }

    if (!isVasoInPlace())
      activarLED("orange", 50);
    else 
      desactivarLED("orange");
    

  }

  //Acciones despues del for
   if (isButtonPressed()){
    setButtonPressed(false);
   }

}

void Planificador::reschedulePlate(Alarm* config) {
  config->startTime = getTime();
  config->times = 1; // se reprograma
}

void Planificador::blockPlate(Alarm* config) {
  config->blocked = true;

  setButtonReady(false); // se paso el tiempo de espera de presionado del boton
  config->waitingForButton = false;
  config->movePlate = false;
  config->waitingForVaso = false;

  desactivarLED("red");
  desactivarLED("green");
  desactivarLED("blue");
  desactivarLED("orange");
  
  Log.Debug("Bloqueando plato, enviando notificacion\n");
  sendNotification(NOTIF_BLOQUEO, config->plateID, config->pillName, getTimeString(nextDispenseDateTime(config)), config->stock); 
}

//Devuelve DateTime para el proximo dispendio de la alarma 'config'
DateTime Planificador::nextDispenseDateTime(Alarm* config){
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
  
  return nextDispense;
}

//Devuelve el tiempo en segundos ABSOLUTOS para el proximo dispendio de la alarma 'config'
long Planificador::nextDispense(Alarm* config) {
  long nextDispense = _nextDispense(config);
  return abs(nextDispense);
}

//Devuelve el tiempo en segundos para el proximo dispendio de la alarma 'config'
long Planificador::_nextDispense(Alarm* config) {
  DateTime nextDispense;

  nextDispense = nextDispenseDateTime(config);
  TimeSpan diff = nextDispense - this->getTime();

  Log.Debug("Proximo dispendio: %s\n", string2char(getTimeString(nextDispense)));
  Log.Debug("Resta para Proximo dispendio: dia:%d, hora:%d, min:%d, seg:%d\n", diff.days(), diff.hours(), diff.minutes(), diff.seconds());
  long sec = diff.days()*86400L + diff.hours()*3600L + diff.minutes()*60L + diff.seconds();
  
  return sec;
}


bool Planificador::alarmDispensed(Alarm *config) {
  if (getSensorDetected() == -1)
    return false;
  return plateIDToIndex(config->plateID) == getSensorDetected();  

}

bool Planificador::checkCriticalStock(Alarm *config) {

   return (config->stock <= config->criticalStock);
}

bool Planificador::isVasoInPlace() {
  int value = 0;

  pinMode(PIN_VASO_LASER, OUTPUT);
  digitalWrite(PIN_VASO_LASER, HIGH);

  value = analogRead(PIN_VASO_PHOTO);
  
  //Log.Debug("VasoInPlace: %d\n", value);
  
  if (value < 80)
    return false;
  return true;
  
}

//chequea si otro plato se está moviendo 
bool Planificador::waitingForOtherPlate() {
  int count = 0;
  for (int i = 0; i < this->configDataList.Count(); i++)
  { 
    Alarm* config = &this->configDataList[i];

    if (config->movePlate == true)
      count++;
  }

  return count > 0;
}

//SECCION MANEJO MOTOR

void Planificador::processPlates(){
  for (int i = 0; i < this->configDataList.Count(); i++)
  { 
    
    Alarm* config = &this->configDataList[i];

    if (config->movePlate == true){
      startPlate(plates[plateIDToIndex(config->plateID)], i);
    }
      

    else if (getSensorDetected() == -1)
      stopPlate(plates[plateIDToIndex(config->plateID)]);
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


void Planificador::startPlate(Servo plate, int index) {
    //Importante: esta configuracion funciona bien si la alimentacion es la del arduino.
    unsigned long currentMillis = millis();
    
    if (currentMillis - previousMillisMotor[index] >= 100) {
      plate.write(180);
    }
   
    if (currentMillis - previousMillisMotor[index] >= 100 + 20) { 
        plate.writeMicroseconds(1300);
        previousMillisMotor[index] = currentMillis;
    }
      
    //plate.write(180);
    //delay(20);
    //plate.writeMicroseconds(1500);
    //delay(90); 
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

//Procesar mensajes y acciones recibidas del wifi
void Planificador::processCommandsWIFI()
{
  
  //Lee mensajes de WIFI 
  String str = readFromWIFI();

  if (str != "")
   Log.Debug("Recibido de WIFI: %s\n", string2char(str));
  
  if (str != "" && !str.startsWith("debug:", 0)) {
    StaticJsonBuffer<300> jsonBuffer;
    JsonObject& root = jsonBuffer.parseObject(str);
    if (!root.success()) {
      Log.Debug("Json parseObject() failed");
    }

    if (root.containsKey("time")){
      desactivarLED("red");
      activarLED("green", 2);
      Log.Debug("WIFI: time_t! %l\n", root["time"].as<time_t>());
      this->setInitTime(root["time"].as<time_t>());
    } 

    if (root.containsKey("ip")){
      Log.Debug("WIFI: IP ! %s\n", root["ip"].as<char *>());
      //this->setInitTime(root["ip"].as<String>());
    } 

    if (root.containsKey("stock")){
     
      int stock = root["stock"].as<int>();
      int plateID = root["plateID"].as<int>();
      char* pillName = root["pillName"].as<char *>();

      this->setStock(stock, plateID, pillName);

    } 

    if (root.containsKey("plan")){
      
      DateTime startTime;
      String startTimeString = root["startTime"];
      int anio = startTimeString.substring(0,4).toInt();
      int mes = startTimeString.substring(4,6).toInt();
      int dia = startTimeString.substring(6,8).toInt();
      int hora = startTimeString.substring(8,10).toInt();
      int minuto = startTimeString.substring(10,12).toInt();
      int segundo = startTimeString.substring(12,14).toInt();

      startTime = DateTime(anio,mes,dia,hora,minuto,segundo);
      
      int interval = root["interval"].as<int>();
      int quantity = root["quantity"].as<int>();
      int criticalStock = root["criticalStock"].as<int>();
      byte periodicity = root["periodicity"].as<byte>();
      char *days = root["days"];
      bool block = root["block"].as<int>() == 2; // 1 replinificar, 2 bloquear
      int plateID = root["plateID"].as<int>();
      
      this->setAlarm(startTime, interval, quantity, criticalStock, periodicity, days, block, plateID);

    } 
    
    if (root.containsKey("MAC")){
      macAddress = root["MAC"].as<String>();
      Log.Debug("WIFI: MAC ! %s\n", string2char(macAddress));
    } 

    if (root.containsKey("UmbralNoDispendio")){
      NO_DISPENSE_THRESHOLD = root["UmbralNoDispendio"].as<int>();
      Log.Debug("WIFI: NO_DISPENSE_THRESHOLD ! %d\n", NO_DISPENSE_THRESHOLD);
    } 
    if (root.containsKey("UmbralRetiroVaso")){
      VASO_THRESHOLD = root["UmbralRetiroVaso"].as<int>();
      Log.Debug("WIFI: VASO_THRESHOLD ! %d\n", VASO_THRESHOLD);
    } 
    if (root.containsKey("UmbralButton")){
      BUTTON_THRESHOLD = root["UmbralButton"].as<int>();
      Log.Debug("WIFI: BUTTON_THRESHOLD ! %d\n", BUTTON_THRESHOLD);
    }

    if (root.containsKey("WIFI_ERROR")){
      activarLED("red", 2);
      Log.Debug("WIFI: Error connecting !\n");
    }
    if (root.containsKey("WIFI_OK")){
      WIFI_OK = true;
    }
 }
}

int Planificador::sendNotification(int code, int containerID, String pillName, String time, int stock){
    JsonObject& root = jsonBuffer.createObject();
    root["notification"] = "";
    root["DireccionMAC"] = "60:01:94:4A:8C:A4";
    root["Codigo"] = code;
    root["Receptaculo"] = containerID;
    root["Pastilla"] = pillName;
    root["Horario"] = time;
    root["CantidadRestante"] = stock;

    root.printTo(Serial1);

}

int Planificador::sendPlanificacion(Alarm *config){


    JsonObject& root = jsonBuffer.createObject();
    root["planificacion"] = "";
    root["DireccionMAC"] = "60:01:94:4A:8C:A4";
    root["Receptaculo"] = config->plateID;
    root["Pastilla"] = config->pillName;
    root["HorarioInicio"] = getTimeString(config->startTime);
    root["Intervalo"] = String(config->interval);
    root["StockCritico"] = String(config->criticalStock);
    root["Periodicidad"] = String(config->periodicity);
    root["Dias"] = String(config->days);
    root["Bloqueo"] = String(config->block);
    root["Cantidad"] = String(config->quantity);

    root.printTo(Serial1);
}

int Planificador::sendCarga(int containerID, String pillName, int stock){
    JsonObject& root = jsonBuffer.createObject();
    root["carga"] = "";
    root["DireccionMAC"] = "60:01:94:4A:8C:A4";
    root["Receptaculo"] = containerID;
    root["Pastilla"] = pillName;
    root["Stock"] = String(stock);

    root.printTo(Serial1);

}

void Planificador::activarBuzzer()
{
  if (millis() - previousMillisBuzzer >= 800)
  {
    previousMillisBuzzer += 800;
    NewTone(PIN_BUZZER, 800, 500); // play 800 Hz tone in background for 'onDuration'
  }
}

void Planificador::activarBuzzerRetiro()
{
  if (millis() - previousMillisBuzzer >= 800)
  {
    previousMillisBuzzer += 800;
    NewTone(PIN_BUZZER, 200, 500); // play 800 Hz tone in background for 'onDuration'
  }
}

void Planificador::desactivarBuzzer()
{
  noNewTone(PIN_BUZZER);
}

void Planificador::activarLED(String color, int times)
{

  if (color == "red") 
    showRedLED = true;

  if (color == "green") 
    showGreenLED = true;

  if (color == "blue") 
    showBlueLED = true;

  if (color == "orange"){ 
    showOrangeLED = true;
  }

  ledTimes = times * 2;
  
}

void Planificador::processLED(){
  
  if (millis() - previousMillisLED >= 1000)
  {
    ledTimes--;
    if (ledTimes == 0) {
        resetLED();
        desactivarLED("red");
        desactivarLED("green");
        desactivarLED("blue");
        desactivarLED("orange");
    }
    
      previousMillisLED = millis();

      if (ledValue == 0)
        ledValue = 128;
      else
        ledValue = 0;
        
      if (showRedLED) {
        analogWrite(PIN_LEDRED, ledValue); 
        analogWrite(PIN_LEDGREEN, 0); 
        analogWrite(PIN_LEDBLUE, 0); 
        return;
      }
      else{
        analogWrite(PIN_LEDRED, 0); 
      }
        
      if (showGreenLED) {
        analogWrite(PIN_LEDGREEN, ledValue); 
        analogWrite(PIN_LEDBLUE, 0); 
        analogWrite(PIN_LEDRED, 0); 
        return;
      }
      else {
        analogWrite(PIN_LEDGREEN, 0); 
      }
        
      if (showBlueLED) {
        analogWrite(PIN_LEDBLUE, ledValue); 
        analogWrite(PIN_LEDRED, 0); 
        analogWrite(PIN_LEDGREEN, 0); 
        return;
      }
      else {
        analogWrite(PIN_LEDBLUE, 0); 
      }

      if (showOrangeLED) {
         analogWrite(PIN_LEDRED, ledValue); 
         analogWrite(PIN_LEDGREEN,  (ledValue == 128) ? ledValue - 127 : ledValue); 
         analogWrite(PIN_LEDBLUE, 0); 
         return;
      } else {
        analogWrite(PIN_LEDRED, 0); 
        analogWrite(PIN_LEDGREEN, 0); 
      }
      
    
  }
  
}

void Planificador::resetLED(){
  ledTimes = 0;
}

void Planificador::desactivarLED(String color){
   if (color == "red") 
    showRedLED = false;

  if (color == "green") 
    showGreenLED = false;

  if (color == "blue") 
    showBlueLED = false;

  if (color == "orange") 
    showOrangeLED = false;

}




