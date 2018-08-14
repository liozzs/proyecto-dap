#include "interrupcion.h"

//variables globales necesarias para interrupciones
bool buttonReady = false; 
bool buttonPressed = false; 
int sensorDetected = -1;

/*
 * Uso de interrupciones por grupo (50-53), ya que no quedan pines libres de interrupcion (rtc, wifi y sensores)
 * https://playground.arduino.cc/Main/PinChangeInterrupt
 */
void pciSetup(byte pin)
{
    *digitalPinToPCMSK(pin) |= bit (digitalPinToPCMSKbit(pin));  // enable pin
    PCIFR  |= bit (digitalPinToPCICRbit(pin)); // clear any outstanding interrupt
    PCICR  |= bit (digitalPinToPCICRbit(pin)); // enable interrupt for the group

    digitalWrite(pin, HIGH);
}

ISR (PCINT0_vect) // handle pin change interrupt for D0 to D7 here
{
  if (buttonReady && digitalRead(PIN_BUTTON) == 0) {
    buttonPressed = true;
    buttonReady = false;
  }
  else {
    for (int i=0; i < MAX_SUPPORTED_ALARMS; i++) {
      if (digitalRead(PIN_DISPENSE_SENSOR[i]) == 0){
        Serial.println( PIN_DISPENSE_SENSOR[i]  );
       sensorDetected = i;
      }
    }
  }  
}


void setButtonReady(bool state){
  buttonReady = state;
}
bool isButtonReady(){
  return buttonReady;
}
void setButtonPressed(bool state){
  buttonPressed = state;
}
bool isButtonPressed(){
  return buttonPressed;
}
void setSensorDetected(int value){
  sensorDetected = value;
}
int  getSensorDetected() {
  return sensorDetected;
}

