#include <ESP8266WiFi.h>          //https://github.com/esp8266/Arduino
#include <DNSServer.h>
#include <ESP8266WebServer.h>
#include <WiFiManager.h>          //https://github.com/tzapu/WiFiManager
#include "ESP8266HTTPClient.h"

//custom imports
#include <ArduinoJson.h>
#include "ntp_client.h"

String msg_in;
String msg_out;
DynamicJsonBuffer  jsonBuffer;
//para medir el tiempo y ejecutar GET y POST 
long previousMillis = 0;
String prev_get = "";


//NTP
NTPClient *ntp;

void setup() {
  Serial.begin(115200);
  
  //Inicialización WiFi
  WiFiManager wifiManager;
  wifiManager.setTimeout(180);
  wifiManager.setMinimumSignalQuality(60);

  if (modo_operacion == "TEST")
    wifiManager.setDebugOutput(true);
  else
    wifiManager.setDebugOutput(false);
  //DEBUG para que siempre muestre el portal
  //wifiManager.resetSettings();
  
  if(!wifiManager.autoConnect("DAP WiFi")) {
    Serial.println("failed to connect and hit timeout");
    ESP.reset();
    delay(5000);
  } 

  debug("WIFI: connected...\n");

  ntp = new NTPClient();

  //enviar a arduino primer mensaje con el horario correcto e IP publica
  time_t ntpTime = 0;

  while(ntpTime == 0) {
    ntpTime = ntp->getTime();
  }

  // Obtener IP publica para enviar al arduino
  String publicIP;
  HTTPClient http;
  http.begin("http://ipv4bot.whatismyipaddress.com/");
  int httpCode = http.GET();
  if(httpCode > 0) {
    if(httpCode == HTTP_CODE_OK)
      publicIP = http.getString();
  }
  http.end();
  
  JsonObject& root = jsonBuffer.createObject();
  root["time"] = ntpTime;
  root["ip"] = WiFi.localIP().toString();
  root.printTo(Serial);


}

void loop() {
  delay(10000); 
}



