//Modulo para conectectarse a WiFi

#include "conectividad.h"

void sendToWIFI(String str){
  while(str.length() < 128){
    str+='\0';
  }
  Serial1.println(str);
}

String readFromWIFI(){
  
  String msg_in_wifi = "";
  //Serial.println("INIT: " + String(msg_in_wifi.length()));
  while (Serial1.available()) {
     if (Serial1.available()  > 0 ) {
       char c = Serial1.read();
       msg_in_wifi += c;
      }
  }
  Serial.println("readFromWiFI: " + String(msg_in_wifi));
  // Serial.println("LENGTH:" + String(msg_in_wifi.length()));
   if (msg_in_wifi != "") {
  
      if (msg_in_wifi.startsWith("debug:", 0)) {
        //Serial.println(msg_in_wifi);
      }
      else {
        return  msg_in_wifi;
      }
   }
   return "";
}


