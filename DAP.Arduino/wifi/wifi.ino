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

//Server app mobile
ESP8266WebServer  server(80);
int reqCount = 0;                // number of requests received

void setup() {
  Serial.begin(115200);

//Smart config

  if (!WiFi.setAutoConnect(true)) {
  /* Set ESP32 to WiFi Station mode */
    WiFi.mode(WIFI_STA);
    /* start SmartConfig */
    WiFi.beginSmartConfig();
    delay(1000);
    /* Wait for SmartConfig packet from mobile */
    Serial.println("Waiting for SmartConfig.");
    while (!WiFi.smartConfigDone()) {
      delay(500);
      Serial.print(".");
    }
    Serial.println("");
    Serial.println("SmartConfig done.");
  
    /* Wait for WiFi to connect to AP */
    Serial.println("Waiting for WiFi");
    while (WiFi.status() != WL_CONNECTED) {
      delay(500);
      Serial.print(".");
    }
    Serial.println("WiFi Connected.");
    Serial.print("IP Address: ");
    Serial.println(WiFi.localIP());
  }

//Wifi Manager deshabilitado  
/*  
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


*/
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


  // start the web server on port 80
  server.on("/", handleRoot);               // Call the 'handleRoot' function when a client requests URI "/"
  server.onNotFound(handleNotFound);        // When a client requests an unknown URI (i.e. something other than "/"), call function "handleNotFound"
  server.begin();

}

void loop() {

  //SERVER
  
  server.handleClient();  

  
}


void handleRoot() {
  //String arg = server.arg("plan");
  //Serial.println(arg);
  StaticJsonBuffer<350> newBuffer;
  JsonObject& root = newBuffer.parseObject(server.arg("plain"));
  root["plan"] = true;
  root.printTo(Serial);

  server.send(200, "text/plain", "Hello world!");   // Send HTTP status 200 (Ok) and send some text to the browser/client
}

void handleNotFound(){
  server.send(404, "text/plain", "404: Not found"); // Send HTTP status 404 (Not Found) when there's no handler for the URI in the request
}

