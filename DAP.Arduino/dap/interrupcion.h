#ifndef _INTERRUPCION_h
#define _INTERRUPCION_h
 
#include "arduino.h"
#include "config.h"

void pciSetup(byte pin);
ISR (PCINT0_vect);

void setButtonReady(bool state);
bool isButtonReady();
void setButtonPressed(bool state);
bool isButtonPressed();
void setSensorDetected(int i, int value);
int  getSensorDetected(int i); //retorna el pin detectado

#endif
