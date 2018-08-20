#ifndef _SEGDISPLAY_h
#define _SEGDISPLAY_h

#include "arduino.h"

const int NB_DIGITS     = 4;  // 4-digit display
const int FIRST_SEGMENT   = 40;  // on pins 40..43
const int FIRST_CONTROL = 36;  // on pins 36..39

extern Planificador* planif;

 // Our 7-segment "font".
static const uint8_t font[10] = {
    0b0000, // 0
    0b0001, // 1
    0b0010, // 2
    0b0011, // 3
    0b0100, // 4
    0b0101, // 5
    0b0110, // 6
    0b0111, // 7
    0b1000, // 8
    0b1001  // 9
};

// Digits to display, from right to left.
uint8_t digits[NB_DIGITS];

// Set all the used pins as outputs.
void init_display()
{
    for (int i = 0; i < 4; i++)
        pinMode(FIRST_SEGMENT + i, OUTPUT);
    for (int i = 0; i < NB_DIGITS; i++)
        pinMode(FIRST_CONTROL + i, OUTPUT);
}

// This should be called periodically.
void refresh_display()
{ 
   uint32_t now = micros();

    // Change the number displayed every second.
    static uint32_t last_change;
    if (now - last_change >= 1000*1000) {
        
        int min = planif->getTime().hour();
        int min_1 = (min / 10U) % 10;
        int min_2 = (min / 1U) % 10;

        int seg = planif->getTime().minute();
        int seg_1 = (seg / 10U) % 10;
        int seg_2 = (seg / 1U) % 10;

        digits[3] = seg_2;
        digits[2] = seg_1;
        digits[1] = min_2;
        digits[0] = min_1;
        last_change = now;
    }
    
    // Wait for 2.5 ms before switching digits.
    static uint32_t last_switch;
  
    if (now - last_switch < 2500) return;
    last_switch = now;

    // Switch off the current digit.
    static uint8_t pos;
    digitalWrite(FIRST_CONTROL + pos, LOW);

    // Set the anodes for the next digit.
    pos = (pos + 1) % NB_DIGITS;
    uint8_t glyph = font[digits[pos]];
    for (int i = 0; i < 4; i++){
      
        digitalWrite(FIRST_SEGMENT + i, glyph & 1 << (3-i));
    }
    // Switch digit on.
    digitalWrite(FIRST_CONTROL + pos, HIGH);
}

#endif
