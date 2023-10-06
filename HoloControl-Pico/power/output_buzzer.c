#include "../control.h"
#include "output_buzzer.h"
#include "power_pins.h"
#include "pico/stdlib.h"

int hold = 0, holdFor = 0;
bool state = 0;
void trigger_buzzer()
{
    gpio_put(OUT_BZR, (hold < holdFor) * (state = !state));
    hold += hold < holdFor;
}

void enable_buzzer(int time)
{
    holdFor = time / (CONTROL_BUZZER_EVERY + 1);
    hold = 0;
}

void cancel_buzzer()
{
    holdFor = 0;
    hold = 0;
}
