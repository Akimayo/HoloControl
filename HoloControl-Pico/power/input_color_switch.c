#include "input_color_switch.h"
#include "pico/stdlib.h"
#include "power_pins.h"
#include "../control_globals.h"
#include <stdio.h>

char last_input = 0;
void get_color(void)
{
    char input = gpio_get(IN_COL);
    char change = (input ^ last_input) & input;
    G_color = ((G_color << change) + change * (G_color < 1)) & 15;
    last_input = input;
}

char get_change(char prev)
{
    char input = gpio_get(IN_COL);
    char change = (input ^ last_input) & input;
    last_input = input;
    return (prev + change) & 1;
}
