#ifndef _TEST_h
#define _TEST_h

#include <string.h>
#include <stdlib.h>
#include "interrupcion.h"


#define CR '\r'
#define LF '\n'
#define BS '\b'
#define NULLCHAR '\0'
#define SPACE ' '

#define COMMAND_BUFFER_LENGTH        50                        //length of serial buffer for incoming commands
char   CommandLine[COMMAND_BUFFER_LENGTH + 1];                 //Read commands into this buffer from Serial.  +1 in length for a termination char

const char *delimiters            = ", \n";                    //commands can be separated by return, space or comma

extern Planificador* planif;

//this following macro is good for debugging, e.g.  print2("myVar= ", myVar);
#define print2(x,y) (Serial.print(x), Serial.println(y))

/*************************************************************************************************************
     Agregar aca los metodos/comandos que se quieren testear
*/

//Imprime la hora
const char *testGetTimeStringToken       = "getTimeString";       

//setea una alarma. setAlarm 2018(año) 7(mes) 22(dia) 30(min) 0(seg) 20(intervalo) 1(cantidad) 1(plateID)
const char *testSetAlarmToken            = "setAlarm";                

//Imprime la configuracion de la alarma. getAlarm 0(indice)
const char *testGetAlarmToken            = "getAlarm";     

//Borra las alarmas guardas en EEPROM
const char *testClearEEPROMToken         = "clearEEPROM";

//setea en true el sensor que detecta el dispendio. setDispenseTrue {PIN}
const char *testSetDispenseTrueToken          = "setDispenseTrue"; 




/*************************************************************************************************************
    getTestCommand()
      Return the string of the next command. Commands are delimited by return"
      Handle BackSpace character
      Make all chars lowercase
*************************************************************************************************************/

bool
getTestCommand(char * commandLine)
{
  static uint8_t charsRead = 0;                      //note: COMAND_BUFFER_LENGTH must be less than 255 chars long
  //read asynchronously until full command input
  while (Serial.available()) {
    char c = Serial.read();
    switch (c) {
      case CR:      //likely have full command in buffer now, commands are terminated by CR and/or LS
      case LF:
        commandLine[charsRead] = NULLCHAR;       //null terminate our command char array
        if (charsRead > 0)  {
          charsRead = 0;                           //charsRead is static, so have to reset
          Serial.println(commandLine);
          return true;
        }
        break;
      case BS:                                    // handle backspace in input: put a space in last char
        if (charsRead > 0) {                        //and adjust commandLine and charsRead
          commandLine[--charsRead] = NULLCHAR;
          Serial << byte(BS) << byte(SPACE) << byte(BS);  //no idea how this works, found it on the Internet
        }
        break;
      default:
        // c = tolower(c);
        if (charsRead < COMMAND_BUFFER_LENGTH) {
          commandLine[charsRead++] = c;
        }
        commandLine[charsRead] = NULLCHAR;     //just in case
        break;
    }
  }
  return false;
}


/* ****************************
   readNumber: return a 16bit (for Arduino Uno) signed integer from the command line
   readWord: get a text word from the command line

*/
int
readNumber () {
  char * numTextPtr = strtok(NULL, delimiters);         //K&R string.h  pg. 250
  return atoi(numTextPtr);                              //K&R string.h  pg. 251
}

char * readWord() {
  char * word = strtok(NULL, delimiters);               //K&R string.h  pg. 250
  return word;
}

void
nullCommand(char * ptrToCommandName) {
  print2("Command not found: ", ptrToCommandName);      //see above for macro print2
}

/****************************************************
   Nuestras funciones de testing / Agregar aca la implementacion de los nombres definidos arriba
*/

String testGetTimeString() {                                      
  return planif->getTimeString(planif->getTime());
}

void testSetAlarm() {                            
  int anio = readNumber();
  int mes = readNumber();
  int dia = readNumber();
  int hora = readNumber();
  int minuto = readNumber();
  int segundo = readNumber();

  int period = readNumber();
  int times = readNumber();
  int plateID = readNumber();
  String block = readWord();

  DateTime t = DateTime(anio,mes,dia,hora,minuto,segundo);
  planif->setAlarm(t, period, times, plateID, block == "true");

}

String testGetAlarm() {    
  int index = readNumber();
  
  Alarm config;

  config = planif->getAlarm(index);
  return planif->getAlarmString(config);                     

}

void testClearEEPROM() {
  for (int i = 0 ; i < EEPROM.length() ; i++) {
    EEPROM.write(i, 0);
  }
  planif->storedAlarms = 0;
  planif->resetAlarms();
}

void testSetDispenseTrue() {
  int plateID = readNumber();

  setSensorDetected(planif->getIndexForPlateID(plateID));
 
}

/****************************************************
   DoMyCommand
*/
bool executeTestCommand(char * commandLine) {

  char * ptrToCommandName = strtok(commandLine, delimiters);

  if (strcmp(ptrToCommandName, testGetTimeStringToken) == 0) {
    String result;
    result = testGetTimeString();
    print2(">    La hora es = ", result);
    return true;

  } 

  if (strcmp(ptrToCommandName, testSetAlarmToken) == 0) {
    testSetAlarm();
    print2(">    Alarma seteada", "");
    return true;
  } 

  if (strcmp(ptrToCommandName, testGetAlarmToken) == 0) {
    String result;
    result = testGetAlarm();
    print2(">    La alarma es = ", result);
    return true; 
  } 

  if (strcmp(ptrToCommandName, testClearEEPROMToken) == 0) {
    testClearEEPROM();
    print2(">    EEPROM borrada = ", "");
    return true;
  } 

  if (strcmp(ptrToCommandName, testSetDispenseTrueToken) == 0) {
    testSetDispenseTrue();
    print2(">    Seteado dispendio para sensor = ", "");
    return true;
  } 
  
 
  nullCommand(ptrToCommandName);
  
  
}
#endif
