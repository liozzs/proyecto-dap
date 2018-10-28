#include <ESP8266WiFi.h>          //https://github.com/esp8266/Arduino
#include <DNSServer.h>
#include <ESP8266WebServer.h>
#include "ESP8266HTTPClient.h"

//custom imports
#include <ArduinoJson.h>
#include "ntp_client.h"


String msg_in;
String msg_out;
String modo_operacion = "TEST";
DynamicJsonBuffer  jsonBuffer;
//para medir el tiempo y ejecutar GET y POST 
long previousMillis = 0;
String prev_get = "";
unsigned long previousMillisWIFI = 0; 

//NTP
NTPClient *ntp;

//Server app mobile
ESP8266WebServer  server(80);
int reqCount = 0;                // number of requests received

//CLIENT
char * host = "18.219.97.48";
uint16_t port = 50065;

//CAMBIAR A TRUE 
bool USE_SMART_CONFIG = true;
int MAX_RETRIES = 3;

void setup() {
  Serial.begin(115200);
  delay(5000);

  WiFi.setAutoConnect(true);

if (USE_SMART_CONFIG) {
  //Smart config
  int connRes = WiFi.waitForConnectResult();
  if (connRes == WL_CONNECTED){
    Serial.println("Auto connected");
  }
  else {
    Serial.println("Smart config");
  /* Set ESP32 to WiFi Station mode */
    WiFi.mode(WIFI_STA);
    /* start SmartConfig */
    WiFi.beginSmartConfig();
    delay(1000);
    /* Wait for SmartConfig packet from mobile */
    Serial.println("Waiting for SmartConfig.");
    int retries = 0;
    while (!WiFi.smartConfigDone()) {
      if (retries == MAX_RETRIES){
        JsonObject& root = jsonBuffer.createObject();
        root["WIFI_ERROR"] = "could not connect";
        root.printTo(Serial);
        retries = 0;
      }
      delay(500);
      Serial.print(".");
      retries++;
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
} 
else {
  char ssid[] = "TeleCentro-8b3c";           
  char pass[] = "lionel13";        
  int status = WL_IDLE_STATUS;  
 
  WiFi.begin(ssid, pass);
  int retries = 0;
  while (WiFi.status() != WL_CONNECTED) {
    if (retries == MAX_RETRIES){
      JsonObject& root = jsonBuffer.createObject();
      root["WIFI_ERROR"] = "could not connect";
      root.printTo(Serial);
      retries = 0;
    }
    delay(500);
    
    retries++;
  }

  JsonObject& root = jsonBuffer.createObject();
  root["WIFI_OK"] = String(WiFi.localIP());
  root.printTo(Serial);
}
  
  // start the web server on port 80
  server.on("/", handlePlan);               // Call the 'handleRoot' function when a client requests URI "/"
  server.on("/Stock", handleStock);     
  server.on("/Plan", handlePlan);              
  server.on("/MAC", handleMAC);              
  server.on("/ResetWIFI", handleResetWIFI);    

  //umbrales
  server.on("/Umbrales", handleUmbrales);  //puede ser: UmbralNoDispendio, UmbralRetiroVaso, UmbralBoton. En segundos

  server.on("/Time", handleTime);
  
  server.onNotFound(handleNotFound);        // When a client requests an unknown URI (i.e. something other than "/"), call function "handleNotFound"
  server.begin();

}

void loop() {
  unsigned long currentMillis = millis();
  
  int retries = 0;
  while (WiFi.status() != WL_CONNECTED) {
    if (retries == MAX_RETRIES){
      JsonObject& root = jsonBuffer.createObject();
      root["WIFI_ERROR"] = "could not connect";
      root.printTo(Serial);
      retries = 0;
      WiFi.beginSmartConfig();
    }
    delay(500);
    retries++;
  }

  if (currentMillis - previousMillisWIFI >= 10000) {
    JsonObject& root = jsonBuffer.createObject();
    root["WIFI_OK"] = "";
    root.printTo(Serial);
    previousMillisWIFI = currentMillis;
  }

  //SERVER (app movil)
  server.handleClient();  

  //CLIENT (notificaciones)
   while (Serial.available()) {
     if (Serial.available() > 0) {
       msg_in = Serial.readStringUntil('\0');

     }
   }
   if (msg_in.length() >= 1) {
        //debug("WIFI: msg de arduino length: " + String(msg_in.length()));
        debug("WIFI Mensaje: " + String(msg_in));

        StaticJsonBuffer<500> jsonBuffer;
        JsonObject& root = jsonBuffer.parseObject(msg_in);
        
        if (!root.success()) {
          debug("Json parseObject() failed");
        }
      
        if (root.containsKey("modo")){
          modo_operacion = root["modo"].as<char *>();
          debug("WIFI: cambio modo a: " + modo_operacion);
        } 

        if (root.containsKey("estado")){
          sendToArduino("WIFI:ack_test:" + String(WiFi.status()));
        } 

        if (root.containsKey("reset")){
          debug("WIFI: reiniciando");
          ESP.reset();
          delay(5000);
        } 

        if (root.containsKey("disconnect")){
          debug("WIFI: disconnect");
          WiFi.disconnect(true);
          delay(5000);
        } 

        if (root.containsKey("get_MAC")){
          String mac =  String(WiFi.macAddress());
          JsonObject& root = jsonBuffer.createObject();
          root["MAC"] = mac;
          root.printTo(Serial);
          msg_in = "";
        }

        if (root.containsKey("get_Time")){          
          ntp = new NTPClient();
        
          //enviar a arduino primer mensaje con el horario correcto e IP publica
          time_t ntpTime = 0;

          
          while (ntpTime == 0){
             ntpTime = ntp->getTime();
             if (retries == MAX_RETRIES){
              break;
            }
            retries++;
          }
          
          JsonObject& root = jsonBuffer.createObject();
          root["time"] = ntpTime;
          root["ip"] = WiFi.localIP().toString();
          root.printTo(Serial);
        }

        if (root.containsKey("notification")){
          sendToServer("mensajes", &root);
          msg_in = "";
        }
        if (root.containsKey("planificacion")){
          sendToServer("planificacion", &root);
          msg_in = "";
        }
        if (root.containsKey("carga")){
          sendToServer("carga", &root);
          msg_in = "";
        }
        
       msg_in = ""; 
       }
}

void sendToServer(String service, JsonObject* root) {
  // Use WiFiClient class to create TCP connections
  WiFiClient client;

  debug("sending to server: " + String(host) + ":" + String(port));
  if (!client.connect(host, port)) {
      Serial.println("connection failed");
      return;
  }

  //Sending POST request with json content
  String content;
  root->printTo(content);

   String post = String("POST ") + "/api/dispensers/" + service + " HTTP/1.1\r\n" +
     "Host: 18.219.97.48\r\n" +
     "User-Agent: Arduino/1.0\r\n" +
     "Accept: application/json\r\n" +
     "Content-Type: application/json\r\n" +
     "Accept-Encoding: gzip, deflate, br\r\n" +
     "Connection: close\r\n" +
     "Content-Length: " + content.length() + "\r\n\r\n" + content;
  
  client.print(post);
  debug(post);
  client.stop();
}

//SERVER
void handleStock() {

  StaticJsonBuffer<350> newBuffer;
  JsonObject& root = newBuffer.parseObject(server.arg("plain"));
  root.printTo(Serial);

  server.send(200, "text/plain", "OK");   // Send HTTP status 200 (Ok) and send some text to the browser/client
}

void handlePlan() {
  //String arg = server.arg("plan");
  //Serial.println(arg);
  StaticJsonBuffer<350> newBuffer;
  JsonObject& root = newBuffer.parseObject(server.arg("plain"));
  root["plan"] = true;
  root.printTo(Serial);

  server.send(200, "text/plain", "OK");   // Send HTTP status 200 (Ok) and send some text to the browser/client
}

void handleMAC() {
  server.send(200, "text/plain", WiFi.macAddress());
}

void handleResetWIFI() {
  server.send(200, "text/plain", "OK");
  ESP.reset();
  delay(5000);
}

void handleUmbrales() {
  StaticJsonBuffer<100> newBuffer;
  JsonObject& root = newBuffer.parseObject(server.arg("plain"));
  root.printTo(Serial);
  server.send(200, "text/plain", "OK");
}

void handleNotFound(){
  server.send(404, "text/plain", "404: Not found"); // Send HTTP status 404 (Not Found) when there's no handler for the URI in the request
}

void handleTime(){
  StaticJsonBuffer<350> newBuffer;
  JsonObject& root = newBuffer.parseObject(server.arg("plain"));
  root.printTo(Serial);
  server.send(200, "text/plain", "OK");
}

void debug(String str) {
  if (modo_operacion == "TEST") {
     Serial.println(str);
  }
};


