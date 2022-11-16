#include <Arduino.h>
#include <FastLED.h>
#include "WiFi.h"
#include "AsyncUDP.h"

#define CONFIG 1 // 0 = Matrix Display, 1 = Tree

#if CONFIG == 0

#define AP_SSID "BKW Display"
#define AP_PASS "aF1cDjotT0FOL$G2Z8Yuk"
#define LED_COUNT 35
#define DATA_PIN 18

#elif CONFIG == 1

#define AP_SSID "BKW Weihnachtsbaum"
#define AP_PASS "zuQDoyOCba6JQWmO4IFLIz"
#define LED_COUNT 100
#define DATA_PIN 18

#else
#error Invalid configuration index
#endif

AsyncUDP udp;
CRGB leds[LED_COUNT];
bool update{false};

void process(AsyncUDPPacket packet)
{
  uint8_t *pointer = packet.data();
  size_t length = packet.length();

  if (length < 3)
  {
    return;
  }

  const uint8_t opCode = pointer[0];
  const uint8_t tableLength = pointer[1];
  const uint8_t dataLength = pointer[2];

  if (opCode != 0x01 || tableLength > dataLength)
  {
    return;
  }

  const CRGB *colorTable = (CRGB *)(pointer + 3);
  const uint8_t *dataTable = pointer + 3 + (3 * tableLength);

  for (uint8_t index = 0; index < dataLength && index < LED_COUNT; index++)
  {
    const uint8_t offset = dataTable[index];

    if (offset >= dataLength)
    {
      continue;
    }

    leds[index] = colorTable[offset];
  }

  update = true;
}

void setup()
{
  WiFi.mode(WIFI_AP);
  WiFi.softAP(AP_SSID, AP_PASS);

  memset(leds, 0, sizeof(leds));

  for (int index = 0; index < 100; index++)
  {
    leds[index] = CRGB(index * 20, index * 30, index * 40);
  }

  FastLED.addLeds<WS2812B, DATA_PIN, BRG>(leds, LED_COUNT);
  FastLED.show();

  if (udp.listen(8996))
  {
    udp.onPacket(process);
  }

  Serial.begin(115200);
  Serial.println("Ready.");
}

void loop()
{
  if (update)
  {
    FastLED.show();
    update = false;
  }

  delay(10);
}