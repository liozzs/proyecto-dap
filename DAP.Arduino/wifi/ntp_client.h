#ifndef _NTP_CLIENT_h
#define _NTP_CLIENT_h

#include <ESP8266WiFi.h>
#include <WiFiUdp.h>

const unsigned int localPort = 2390;      // local port to listen for UDP packets
const int NTP_PACKET_SIZE = 48; // NTP time stamp is in the first 48 bytes of the message
const String modo = "NORMAL";

class NTPClient{
  
   protected:
    byte packetBuffer[ NTP_PACKET_SIZE]; //buffer to hold incoming and outgoing packets
    WiFiUDP udp;
    IPAddress timeServerIP; 
    const char* ntpServerName = "pool.ntp.org";
    unsigned long sendNTPpacket(IPAddress& address);
          
   public: 
    NTPClient();
    unsigned long getTime();
    void printTime(unsigned long epoch);

};


static void sendToArduino(String str){
  while(str.length() < 128){
    str+='\0';
  }
  str = str.substring(0,128);
  Serial.println(str);
}

static void debug2(String str) {
  if (modo == "TEST") {
     Serial.println(str);
  }
};

//UTILS
static char* string2char(String str){
    if(str.length()!=0){
        char *p = const_cast<char*>(str.c_str());
        return p;
    }
}

#endif
