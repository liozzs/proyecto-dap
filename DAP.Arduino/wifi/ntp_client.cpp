#include "arduino.h"
#include "ntp_client.h"


NTPClient::NTPClient(){
  udp.begin(localPort);
};

//Devuelve unix time
unsigned long NTPClient::getTime(){
  //get a random server from the pool
  WiFi.hostByName(ntpServerName, timeServerIP); 

  sendNTPpacket(timeServerIP); // send an NTP packet to a time server
  // wait to see if a reply is available
  delay(1000);
  
  int cb = udp.parsePacket();
  if (!cb) {
    debug2("debug2:no packet yet\n");
    return 0;
  }
  else {
    debug2("debug2:packet received, length=");
    debug2(String(cb));
    // We've received a packet, read the data from it
    udp.read(packetBuffer, NTP_PACKET_SIZE); // read the packet into the buffer

    //the timestamp starts at byte 40 of the received packet and is four bytes,
    // or two words, long. First, esxtract the two words:

    unsigned long highWord = word(packetBuffer[40], packetBuffer[41]);
    unsigned long lowWord = word(packetBuffer[42], packetBuffer[43]);
    // combine the four bytes (two words) into a long integer
    // this is NTP time (seconds since Jan 1 1900):
    unsigned long secsSince1900 = highWord << 16 | lowWord;
    debug2("debug2:Seconds since Jan 1 1900 = " );
    debug2(String(secsSince1900));

    // now convert NTP time into everyday time:
    debug2("debug2:Unix time = ");
    // Unix time starts on Jan 1 1970. In seconds, that's 2208988800:
    const unsigned long seventyYears = 2208988800UL;
    // subtract seventy years:
    unsigned long epoch = secsSince1900 - seventyYears;
    // print Unix time:
    debug2("debug2:" + String(epoch));
    return epoch;
  }
}


void NTPClient::printTime(unsigned long epoch){
 
    // print the hour, minute and second:
    debug2("The UTC time is ");       // UTC is the time at Greenwich Meridian (GMT)
    debug2(String((epoch  % 86400L) / 3600)); // print the hour (86400 equals secs per day)
    debug2(":");
    if ( ((epoch % 3600) / 60) < 10 ) {
      // In the first 10 minutes of each hour, we'll want a leading '0'
      debug2("0");
    }
    debug2(String((epoch  % 3600) / 60)); // print the minute (3600 equals secs per minute)
    debug2(":");
    if ( (epoch % 60) < 10 ) {
      // In the first 10 seconds of each minute, we'll want a leading '0'
      debug2("0");
    }
    debug2(String(epoch % 60)); // print the second
}

// send an NTP request to the time server at the given address
unsigned long NTPClient::sendNTPpacket(IPAddress& address)
{
  debug2("debug2:sending NTP packet...");
  // set all bytes in the buffer to 0
  memset(packetBuffer, 0, NTP_PACKET_SIZE);
  // Initialize values needed to form NTP request
  // (see URL above for details on the packets)
  packetBuffer[0] = 0b11100011;   // LI, Version, Mode
  packetBuffer[1] = 0;     // Stratum, or type of clock
  packetBuffer[2] = 6;     // Polling Interval
  packetBuffer[3] = 0xEC;  // Peer Clock Precision
  // 8 bytes of zero for Root Delay & Root Dispersion
  packetBuffer[12]  = 49;
  packetBuffer[13]  = 0x4E;
  packetBuffer[14]  = 49;
  packetBuffer[15]  = 52;

  // all NTP fields have been given values, now
  // you can send a packet requesting a timestamp:
  udp.beginPacket(address, 123); //NTP requests are to port 123
  udp.write(packetBuffer, NTP_PACKET_SIZE);
  udp.endPacket();
}


