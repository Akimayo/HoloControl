#include "output_status.h"
#include "power_pins.h"
#include "pico/stdlib.h"
#include "../control_globals.h"

#define COUNTER_LOOP 15

int counter = 0;
void trigger_status(void)
{
    gpio_put(OUT_STA_MAN, counter < 1 * (G_mode & MODE_MANUAL));
    gpio_put(OUT_STA_AUT, counter < 1 * (G_mode & MODE_AUTO));
    gpio_put(OUT_STA_ERR, counter < 1 * (G_mode & MODE_ERROR));
    counter = (counter + 1) & COUNTER_LOOP;
}
