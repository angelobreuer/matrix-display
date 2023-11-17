#pragma once

#include <freertos/FreeRTOS.h>

#include <cstdint>
#include <esp_event_base.h>
#include <esp_netif_ip_addr.h>
#include <esp_netif_types.h>
#include <freertos/event_groups.h>
#include <memory>

class WiFiManager {
public:
  static std::unique_ptr<WiFiManager> CreateDefault();

  void Connect();
  void Disconnect();

private:
  EventGroupHandle_t _eventGroupHandle;
  esp_netif_t *_networkInterface;
  esp_ip4_addr_t _ipAddress;

  void HandleDisconnect(esp_event_base_t eventBase, std::int32_t eventId,
                        void *eventData);

  static void DoHandleDisconnect(void *arg, esp_event_base_t eventBase,
                                 std::int32_t eventId, void *eventData);

  void HandleStationGotIp(esp_event_base_t eventBase, std::int32_t eventId,
                          void *eventData);

  static void DoHandleStationGotIp(void *arg, esp_event_base_t eventBase,
                                   std::int32_t eventId, void *eventData);
};