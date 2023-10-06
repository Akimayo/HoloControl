#include "power_pins.h"
#include "output_status.h"
#include "pico/stdlib.h"
#include "../control_globals.h"

char get_mode(void)
{
    char m = gpio_get(IN_SEL_MAN) * MODE_MANUAL + gpio_get(IN_SEL_AUT) * MODE_AUTO;
    G_mode = m + (!m) * G_prev_mode + (G_mode & MODE_ERROR) * MODE_ERROR; // FIXME: Does this MODE_ERROR?
    return G_mode;
}