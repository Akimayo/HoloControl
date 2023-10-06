#include "output_color_switch.h"
#include "input_color_switch.h"
#include "pico/stdlib.h"
#include "power_pins.h"
#include "../control_globals.h"

void set_color() {
    gpio_put(OUT_COL_R, G_color & COLOR_RED);
    gpio_put(OUT_COL_G, G_color & COLOR_GREEN);
    gpio_put(OUT_COL_B, G_color & COLOR_BLUE);
    gpio_put(OUT_COL_E, G_color & COLOR_EXT);
    gpio_put(OUT_COL_W, G_white);
}