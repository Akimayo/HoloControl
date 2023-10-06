#include "power_main.h"
#include "power_pins.h"
#include "pico/stdlib.h"

#define PIN(gpio, dir) gpio_init(gpio);gpio_set_dir(gpio, dir)

void power_init(void) {
    // Finishing LED
    PIN(TRA_FIN_PWR, GPIO_OUT);
    // Mode Select
    PIN(OUT_SEL_PWR, GPIO_OUT);
    gpio_put(OUT_SEL_PWR, 1);
    PIN(IN_SEL_MAN, GPIO_IN);
    PIN(IN_SEL_AUT, GPIO_IN);
    // Buzzer
    PIN(OUT_BZR, GPIO_OUT);
    // Status
    PIN(OUT_STA_ERR, GPIO_OUT);
    PIN(OUT_STA_AUT, GPIO_OUT);
    PIN(OUT_STA_MAN, GPIO_OUT);
    // TTL
    PIN(OUT_TTL, GPIO_OUT);
    // Manual Color Select
    PIN(IN_COL, GPIO_IN);
    PIN(OUT_COL_R, GPIO_OUT);
    PIN(OUT_COL_G, GPIO_OUT);
    PIN(OUT_COL_B, GPIO_OUT);
    PIN(OUT_COL_W, GPIO_OUT);
    PIN(OUT_COL_E, GPIO_OUT);
    // Lasers
    PIN(TRA_LSR_R, GPIO_OUT);
    PIN(TRA_LSR_G, GPIO_OUT);
    PIN(TRA_LSR_B, GPIO_OUT);
    // Run Button
    PIN(IN_RUN, GPIO_IN);
    // Power pins
    PIN(OUT_BTN, GPIO_OUT);
    gpio_put(OUT_BTN, 1);
    PIN(POWER_PIN, GPIO_OUT);
    gpio_put(POWER_PIN, ENABLE_POWER_PIN);
}