#include "input_run_button.h"
#include "pico/stdlib.h"
#include "power_pins.h"
#include "../i_holo_controller.h"
#include "../control_globals.h"

#define COUNTS_AS_RESET 32

char last = 0;
unsigned int count = 0;
void detect_action(void)
{
    char press = gpio_get(IN_RUN);
    count += (press > 0) & (press && last);
    if (!press && last && count > 0 && (G_mode & MODE_AUTO))
    {
        if (count > COUNTS_AS_RESET)
            reset();
        else if (G_auto_run & 1)
            pause();
        else
            start();
        count = 0;
    }
    last = press;
}

char get_run_change(char prev)
{
    char input = gpio_get(IN_RUN);
    char change = (input ^ last) & input;
    last = input;
    return (prev + change) & 1;
}