#ifndef _CONFIG_h
#define _CONFIG_h

//CONFIGURACION GLOBAL
const int MAIN_LOOP_DELAY = 100;
const int MAX_SUPPORTED_ALARMS = 2;
#define LOGLEVEL LOG_LEVEL_DEBUG


//ALARMA
const int UMBRAL_ALARMA_SEG = 5; //umbral para determinar si es la hora actual coincide con alguna alarma

const int PIN_BUTTON = 5;

const byte PIN_PLATE_MOTOR[2] = {9,9}; 
const byte PIN_DISPENSE_SENSOR[2] = {10,10}; 

#endif
