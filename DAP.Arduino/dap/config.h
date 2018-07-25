#ifndef _CONFIG_h
#define _CONFIG_h

//CONFIGURACION GLOBAL
const int MAIN_LOOP_DELAY = 1000;
const int LOOP_DELAY_WIFI = 500;
const int MAX_SUPPORTED_ALARMS = 2;
#define LOGLEVEL LOG_LEVEL_DEBUG

const int PIN_BUTTON = 53;

const byte PIN_PLATE_MOTOR[MAX_SUPPORTED_ALARMS] = {9,9}; 
const byte PIN_DISPENSE_SENSOR[MAX_SUPPORTED_ALARMS] = {51, 52}; //interrupcion
const byte PLATE_IDS[MAX_SUPPORTED_ALARMS] = {100, 200}; 

#endif
