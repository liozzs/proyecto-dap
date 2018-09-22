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


//NTP
NTPClient *ntp;

//Server app mobile
ESP8266WebServer  server(80);
int reqCount = 0;                // number of requests received

//CLIENT
char * host;
uint16_t port;

//CAMBIAR A TRUE 
bool USE_SMART_CONFIG = false;

void setup() {
  Serial.begin(115200);

if (USE_SMART_CONFIG) {
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
} 
else {
  char ssid[] = "DESKTOP";           
  char pass[] = "lionel12";        
  int status = WL_IDLE_STATUS;  
 
  WiFi.begin(ssid, pass);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());

  Serial.println("You're connected to the network");
}

  while (!msg_in.startsWith("ARDUINO_OK")) {
     msg_in = "";
     if (Serial.available() > 0) {
       msg_in = Serial.readStringUntil('\0');
     }
  }
  msg_in = "";
  debug("Arduino is ready");
  
 
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
  server.on("/", handlePlan);               // Call the 'handleRoot' function when a client requests URI "/"
  server.on("/Plan", handlePlan);              
  server.on("/MAC", handleMAC);              
  server.onNotFound(handleNotFound);        // When a client requests an unknown URI (i.e. something other than "/"), call function "handleNotFound"
  server.begin();

}

void loop() {

  //SERVER (app movil)
  server.handleClient();  

  //CLIENT (notificaciones)
   while (Serial.available()) {
     if (Serial.available() > 0) {
       msg_in = Serial.readStringUntil('\0');

     }
   }
   if (msg_in.length() >= 1) {
        debug("WIFI: msg de arduino length: " + String(msg_in.length()));
        debug("Mensaje: " + String(msg_in));

        StaticJsonBuffer<300> jsonBuffer;
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

        if (root.containsKey("set_server")){
          host = string2char(root["host"].as<String>());
          port = root["port"].as<int>();
          debug("WIFI: cambio modo a: " + modo_operacion);
        } 

        if (root.containsKey("get_MAC")){
          String mac =  String(WiFi.macAddress());
          JsonObject& root = jsonBuffer.createObject();
          root["MAC"] = mac;
          root.printTo(Serial);
          msg_in = "";
        }

        if (root.containsKey("notification")){
          String mac = root["mac"].as<char *>(); 
          int code =  root["code"].as<int>(); 
          int containerID =  root["containerID"].as<int>(); 
          String pillName = root["pillName"].as<char *>(); 
          String time = root["time"].as<char *>(); 
          int stock =  root["stock"].as<int>(); 

          debug("WIFI: procesando notificacion: " + mac + "," + String(code) + "," + String(containerID) + "," + pillName + "," + time + "," + String(stock));
          //ACA ENVIAR NOTIFICACION AL SERVER

          // Use WiFiClient class to create TCP connections
          WiFiClient client;
      
          if (!client.connect(host, port)) {
              Serial.println("connection failed");
              return;
          }
      
          // This will send the request to the server
          //client.print("Send this data to server");
          root.printTo(client);
      
          //read back one line from server
          String line = client.readStringUntil('\r');
          client.println(line);
      
          Serial.println("closing connection");
          client.stop();
          
          msg_in = "";
        }
       msg_in = ""; 
       }
}

//SERVER
void handlePlan() {
  //String arg = server.arg("plan");
  //Serial.println(arg);
  StaticJsonBuffer<350> newBuffer;
  JsonObject& root = newBuffer.parseObject(server.arg("plain"));
  root["plan"] = true;
  root.printTo(Serial);

  server.send(200, "text/plain", "PLAN OK");   // Send HTTP status 200 (Ok) and send some text to the browser/client
}

void handleMAC() {
  server.send(200, "text/plain", WiFi.macAddress());
}

void handleNotFound(){
  server.send(404, "text/plain", "404: Not found"); // Send HTTP status 404 (Not Found) when there's no handler for the URI in the request
}


//CLIENT

void debug(String str) {
  if (modo_operacion == "TEST") {
     Serial.println(str);
  }
};

