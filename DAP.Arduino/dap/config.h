#ifndef _CONFIG_h
#define _CONFIG_h

//CONFIGURACION GLOBAL

const int MAIN_LOOP_DELAY = 1000;
const int LOOP_DELAY_WIFI = 500;
const int MAX_SUPPORTED_ALARMS = 2;

const int PIN_BUTTON = 53;
const int PIN_BUZZER = 2;
const int PIN_VASO_LASER = 7;
const int PIN_VASO_PHOTO = 0;
const int PIN_LEDRED = 4;
const int PIN_LEDGREEN = 3;
const int PIN_LEDBLUE = 5;

const byte PIN_PLATE_MOTOR[MAX_SUPPORTED_ALARMS] = {10,11}; 
const byte PIN_DISPENSE_SENSOR[MAX_SUPPORTED_ALARMS] = {51, 52}; //interrupcion
const byte PLATE_IDS[MAX_SUPPORTED_ALARMS] = {100, 200}; 



#endif
