#include "output_finishing.h"
#include "power_pins.h"
#include "pico/stdlib.h"
#include "../control_globals.h"

void set_finishing(void)
{
    gpio_put(TRA_FIN_PWR, !(G_white & 1));
}