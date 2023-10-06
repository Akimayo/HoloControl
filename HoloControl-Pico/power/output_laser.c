#include "output_laser.h"
#include "input_color_switch.h"
#include "power_pins.h"
#include "pico/stdlib.h"
#include "../control_globals.h"

void set_lasers(void)
{
    gpio_put(TRA_LSR_R, G_color & COLOR_RED);
    gpio_put(TRA_LSR_G, G_color & COLOR_GREEN);
    gpio_put(TRA_LSR_B, G_color & COLOR_BLUE);
    gpio_put(OUT_TTL, G_color & COLOR_EXT); // FIXME: Validate that the controller works this way
}