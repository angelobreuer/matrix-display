#include "WiFiManager.hpp"
#include "WiFiConfig.hpp"

#include <snox/logging/Logger.hpp>
#include <snox/logging/Stopwatch.hpp>

#include <cstring>
#include <esp_err.h>
#include <esp_log.h>
#include <esp_wifi.h>
#include <freertos/event_groups.h>
#include <lwip/apps/sntp.h>
#include <nvs.h>
#include <nvs_flash.h>

static snox::logging::Logger<> logger(LOGGER_FILE_NAME);

constexpr const auto ConnectedBit = BIT0;

std::unique_ptr<WiFiManager> WiFiManager::CreateDefault() {
  return std::make_unique<WiFiManager>();
}

void WiFiManager::Connect() {
  ESP_ERROR_CHECK(nvs_flash_init());
  ESP_ERROR_CHECK(esp_netif_init());
  ESP_ERROR_CHECK(esp_event_loop_create_default());

  if (_eventGroupHandle != nullptr) {
    ESP_ERROR_CHECK(ESP_ERR_INVALID_STATE);
  }

  _eventGroupHandle = xEventGroupCreate();

  wifi_init_config_t wifiInitConfiguration = WIFI_INIT_CONFIG_DEFAULT();
  ESP_ERROR_CHECK(esp_wifi_init(&wifiInitConfiguration));

  esp_netif_config_t interfaceConfiguration = ESP_NETIF_DEFAULT_WIFI_STA();
  _networkInterface = esp_netif_new(&interfaceConfiguration);

  esp_netif_attach_wifi_station(_networkInterface);
  esp_wifi_set_default_wifi_sta_handlers();

  ESP_ERROR_CHECK(esp_event_handler_register(
      WIFI_EVENT, WIFI_EVENT_STA_DISCONNECTED, &DoHandleDisconnect, this));
  ESP_ERROR_CHECK(esp_event_handler_register(IP_EVENT, IP_EVENT_STA_GOT_IP,
                                             &DoHandleStationGotIp, this));
  ESP_ERROR_CHECK(esp_wifi_set_storage(WIFI_STORAGE_RAM));

  wifi_config_t wifiConfiguration;

  memset(&wifiConfiguration, 0, sizeof(wifiConfiguration));

  strcpy((char *)wifiConfiguration.sta.ssid, WIFI_SSID);
  strcpy((char *)wifiConfiguration.sta.password, WIFI_PASSPHRASE);

  logger.Information("Connectin to " WIFI_SSID "...");

  ESP_ERROR_CHECK(esp_wifi_set_mode(WIFI_MODE_STA));
  ESP_ERROR_CHECK(esp_wifi_set_config(WIFI_IF_STA, &wifiConfiguration));
  ESP_ERROR_CHECK(esp_wifi_start());
  ESP_ERROR_CHECK(esp_wifi_connect());

  logger.Information("Waiting for IP address to be assigned...");

  snox::logging::Stopwatch stopwatch{};
  xEventGroupWaitBits(_eventGroupHandle, ConnectedBit, true, true,
                      portMAX_DELAY);

  logger.Information("WiFi connection to '" WIFI_SSID
                     "' was established in {}.",
                     stopwatch.ToString());

  const auto ipAddressBytes =
      reinterpret_cast<const std::uint8_t *>(&_ipAddress.addr);

  logger.Information("Assigned IP address: {}.{}.{}.{}", ipAddressBytes[0],
                     ipAddressBytes[1], ipAddressBytes[2], ipAddressBytes[3]);
}

void WiFiManager::Disconnect() {
  if (_eventGroupHandle == nullptr) {
    ESP_ERROR_CHECK(ESP_ERR_INVALID_STATE);
  }

  vEventGroupDelete(_eventGroupHandle);
  _eventGroupHandle = nullptr;

  ESP_ERROR_CHECK(esp_event_handler_unregister(
      WIFI_EVENT, WIFI_EVENT_STA_DISCONNECTED, &DoHandleDisconnect));
  ESP_ERROR_CHECK(esp_event_handler_unregister(IP_EVENT, IP_EVENT_STA_GOT_IP,
                                               &DoHandleStationGotIp));

  const auto errorStatus = esp_wifi_stop();

  if (errorStatus == ESP_ERR_WIFI_NOT_INIT) {
    return;
  }

  ESP_ERROR_CHECK(errorStatus);
  ESP_ERROR_CHECK(esp_wifi_deinit());
  ESP_ERROR_CHECK(
      esp_wifi_clear_default_wifi_driver_and_handlers(_networkInterface));
  esp_netif_destroy(_networkInterface);
  _networkInterface = nullptr;

  logger.Information("Disconnected from " WIFI_SSID ".");
}

void WiFiManager::HandleDisconnect(esp_event_base_t eventBase, int32_t eventId,
                                   void *eventData) {
  logger.Information("Wi-Fi disconnected, trying to reconnect...");

  const auto errorStatus = esp_wifi_connect();

  if (errorStatus == ESP_ERR_WIFI_NOT_STARTED) {
    return;
  }

  ESP_ERROR_CHECK(errorStatus);
}

void WiFiManager::DoHandleDisconnect(void *arg, esp_event_base_t eventBase,
                                     int32_t eventId, void *eventData) {
  static_cast<WiFiManager *>(arg)->HandleDisconnect(eventBase, eventId,
                                                    eventData);
}

void WiFiManager::HandleStationGotIp(esp_event_base_t eventBase,
                                     int32_t eventId, void *eventData) {
  ip_event_got_ip_t *event = (ip_event_got_ip_t *)eventData;
  memcpy(&_ipAddress, &event->ip_info.ip, sizeof(_ipAddress));
  xEventGroupSetBits(_eventGroupHandle, ConnectedBit);
}

void WiFiManager::DoHandleStationGotIp(void *arg, esp_event_base_t eventBase,
                                       int32_t eventId, void *eventData) {
  static_cast<WiFiManager *>(arg)->HandleStationGotIp(eventBase, eventId,
                                                      eventData);
}