#include "esp_event.h"
#include "esp_log.h"
#include "esp_netif.h"
#include "esp_system.h"
#include "esp_wifi.h"
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "led_strip.h"
#include "nvs_flash.h"
#include <string.h>
#include <sys/param.h>
#include <wifi/WiFiConfig.hpp>
#include <wifi/WiFiManager.hpp>

#include "lwip/err.h"
#include "lwip/sockets.h"
#include "lwip/sys.h"
#include <lwip/netdb.h>

#include <snox/logging/Logger.hpp>

static snox::logging::Logger<> logger(LOGGER_FILE_NAME);

struct UdpServerSocket {
  UdpServerSocket() {
    sockaddr_in dest_addr;
    dest_addr.sin_addr.s_addr = htonl(INADDR_ANY);
    dest_addr.sin_family = AF_INET;
    dest_addr.sin_port = htons(6996);

    socketHandle = socket(AF_INET, SOCK_DGRAM, IPPROTO_IP);

    if (socketHandle < 0) {
      logger.Error("Unable to create socket: errno {}", errno);
      return;
    }

    auto errorStatus =
        bind(socketHandle, (struct sockaddr *)&dest_addr, sizeof(dest_addr));

    if (errorStatus < 0) {
      logger.Error("Socket unable to bind: errno {}", errno);

      closesocket(socketHandle);
      socketHandle = 0;
      return;
    }
  }

  ~UdpServerSocket() {
    if (socketHandle != 0) {
      closesocket(socketHandle);
    }
  }

  int socketHandle;
};

void self_test(led_strip_handle_t handle, std::uint8_t r, std::uint8_t g,
               std::uint8_t b) {
  for (int index = 0; index < LedCount; index++) {
    ESP_ERROR_CHECK(led_strip_set_pixel(handle, index, r, g, b));
  }

  ESP_ERROR_CHECK(led_strip_refresh(handle));
  vTaskDelay(500 / portTICK_PERIOD_MS);
}

extern "C" void app_main() {

  led_strip_config_t ledConfiguration{
      .strip_gpio_num = 7,
      .max_leds = LedCount,
      .led_pixel_format = LED_PIXEL_FORMAT_GRB,
      .led_model = LED_MODEL_WS2812,
      .flags = {.invert_out = false},
  };

  led_strip_rmt_config_t rmtConfiguration{
      .clk_src = rmt_clock_source_t::RMT_CLK_SRC_APB,
      .resolution_hz = 4 * 1000 * 1000,
      .mem_block_symbols = 64,
      .flags = {.with_dma = false},
  };

  led_strip_handle_t handle;
  ESP_ERROR_CHECK(
      led_strip_new_rmt_device(&ledConfiguration, &rmtConfiguration, &handle));

  self_test(handle, 255, 0, 0);
  self_test(handle, 0, 255, 0);
  self_test(handle, 0, 0, 255);
  self_test(handle, 255, 255, 255);

  for (int index = 0; index < LedCount; index++) {
    auto step = index % 3;
    ESP_ERROR_CHECK(led_strip_set_pixel(handle, index, step == 0 ? 255 : 0,
                                        step == 1 ? 255 : 0,
                                        step == 2 ? 255 : 0));
  }

  ESP_ERROR_CHECK(led_strip_refresh(handle));

  vTaskDelay(1500 / portTICK_PERIOD_MS);
  ESP_ERROR_CHECK(led_strip_clear(handle));

  std::array<std::uint8_t, 1024> receiveBuffer{};

  auto wifiManager = WiFiManager::CreateDefault();

  wifiManager->Connect();

  while (1) {
    UdpServerSocket serverSocket;
    std::uint64_t lastSequenceNumber{};
    TickType_t lastPayloadAccepted{xTaskGetTickCount()};

    while (1) {
      const auto bytesTransferred =
          recv(serverSocket.socketHandle, receiveBuffer.data(),
               receiveBuffer.size(), 0);

      if (bytesTransferred < 0) {
        logger.Error("recvfrom failed: errno {}", errno);
        break;
      }

      if (bytesTransferred < 14 || receiveBuffer[0] != 'W' ||
          receiveBuffer[1] != 'R' || receiveBuffer[2] != 'G' ||
          receiveBuffer[3] != 'B') {
        logger.Warning("Invalid payload");
        continue;
      }

      auto pixelCount =
          *reinterpret_cast<std::uint16_t *>(&receiveBuffer.data()[4]);
      auto sequenceNumber =
          *reinterpret_cast<std::uint64_t *>(&receiveBuffer.data()[6]);

      if (pixelCount != LedCount) {
        logger.Warning("Invalid pixel count");
        continue;
      }

      if (bytesTransferred < 14 + pixelCount * 3) {
        logger.Warning("Invalid payload size");
        continue;
      }

      if (sequenceNumber <= lastSequenceNumber) {
        if (xTaskGetTickCount() - lastPayloadAccepted > 1000) {
          logger.Information("Resetting sequence number");
          lastSequenceNumber = sequenceNumber;
          lastPayloadAccepted = xTaskGetTickCount();
        }

        continue;
      }

      lastSequenceNumber = sequenceNumber;

      auto pixelData = &receiveBuffer.data()[14];

      for (int index = 0; index < LedCount; index++) {
        auto red = pixelData[0];
        auto green = pixelData[1];
        auto blue = pixelData[2];

        pixelData += 3;

        ESP_ERROR_CHECK_WITHOUT_ABORT(
            led_strip_set_pixel(handle, index, red, green, blue));
      }

      ESP_ERROR_CHECK(led_strip_refresh(handle));
    }
  }

  vTaskDelete(NULL);
}