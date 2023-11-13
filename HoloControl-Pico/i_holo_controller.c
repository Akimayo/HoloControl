#include <stdio.h>
#include <time.h>
#include "pico/stdlib.h"
#include "control_globals.h"
#include "power/power_main.h"
#include "power/output_status.h"
#include "power/input_mode_select.h"
#include "power/input_color_switch.h"
#include "power/input_run_button.h"
#include "power/output_color_switch.h"
#include "power/output_laser.h"
#include "power/output_buzzer.h"
#include "power/output_finishing.h"
#include "serial/serial_main.h"
#include "control.h"
#include "control_globals.h"

char show_status = 1;

long unsigned int exposure_until[4] = {0, 0, 0, 0};
long unsigned int waiting_until = 0, finishing_until = 0;
long unsigned int start_time = 0;

void on_mode_change(void)
{
    G_color = COLOR_OFF;
    show_status = 1;
    G_auto_run = 0;
    exposure_until[0] = exposure_until[1] = exposure_until[2] = exposure_until[3] = finishing_until = 0;
    G_white = 0;
    cancel_buzzer();
    set_lasers();
    set_color();
    set_finishing();
    if (G_mode & MODE_OFF)
        trigger_status();
}

void start(void)
{
    enable_buzzer(2000);
    show_status = 0;
    if (G_auto_run & 4)
    {
        long unsigned int new_start = time_us_64(), diff = new_start - start_time;
        if (G_auto_run & 2)
        {
            // Restore from pause when finishing
            finishing_until = finishing_until + diff;
            G_auto_run = 11;
        }
        else
        {
            // Restore from pause when exposing
            waiting_until = new_start + G_waiting;
            exposure_until[0] = exposure_until[0] + diff + G_waiting;
            exposure_until[1] = exposure_until[1] + diff + G_waiting;
            exposure_until[2] = exposure_until[2] + diff + G_waiting;
            exposure_until[3] = exposure_until[3] + diff + G_waiting;
            G_auto_run = 13;
        }
        start_time = new_start;
    }
    else
    {
        // Fresh start
        start_time = time_us_64();
        waiting_until = G_waiting + start_time;
        exposure_until[0] = G_exposures[0] + waiting_until;
        exposure_until[1] = G_exposures[1] + waiting_until;
        exposure_until[2] = G_exposures[2] + waiting_until;
        exposure_until[3] = G_exposures[3] + waiting_until;
        finishing_until = 0;
        G_auto_run = 13;
    }
}
void pause(void)
{
    enable_buzzer(1000);
    G_auto_run &= 14;
}
void reset(void)
{
    enable_buzzer(500);
    start_time = 0;
    G_auto_run = 0;
}

int cycle = 0;
bool any_exposure = 0;
int main()
{
    power_init();
    stdio_init_all();
    long unsigned int start = time_us_64(), sleep;
    while (1)
    {
        // CONTROL LOOP
        // - Mode selection
        get_mode();
        if (G_mode != G_prev_mode)
            on_mode_change();
        G_prev_mode = G_mode;
        // - End loop early when OFF
        if (G_mode & MODE_OFF)
        {
            sleep_ms(CONTROL_LOOP_OFF);
            continue;
        }
        // - Status indication
        if (!(cycle & CONTROL_STATUS_EVERY) && (G_mode & MODE_MANUAL || (show_status && !(G_auto_run & 8))))
            trigger_status();
        // - End loop early when ERROR
        if (G_mode & MODE_ERROR)
            continue;
        // - Timing
        if (G_mode & MODE_AUTO)
        {
            if (G_auto_run & 1) // If exposure is running
            {
                hide_status();
                if (G_auto_run & 4) // If waiting for start
                {
                    // Wait for start
                    if (start >= waiting_until)
                    {
                        G_auto_run &= 11;
                        enable_buzzer(4000);
                    }
                }
                else if (G_auto_run & 2) // If finishing exposure
                {
                    // Finishing light
                    G_color = COLOR_OFF;
                    G_white = start < finishing_until;
                    if (start >= finishing_until)
                    {
                        G_auto_run = 0;
                        enable_buzzer(2000);
                    }
                }
                else
                {
                    // Laser exposing
                    G_color = (COLOR_RED | COLOR_GREEN | COLOR_BLUE | COLOR_EXT) ^
                              (((start >= exposure_until[0]) * COLOR_RED) | ((start >= exposure_until[1]) * COLOR_GREEN) | ((start >= exposure_until[2]) * COLOR_BLUE) | ((start >= exposure_until[3]) * COLOR_EXT));
                    G_white = 0;
                    any_exposure = start < exposure_until[0] || start < exposure_until[1] || start < exposure_until[2] || start < exposure_until[3];
                    if (!any_exposure)
                    {
                        G_auto_run |= 2;
                        finishing_until = start + G_finishing;
                        enable_buzzer(4000);
                    }
                }
            }
            else
            {
                G_color = COLOR_OFF;
                G_white = 0;
                show_status = get_change(show_status);
            }
            detect_action();
        }
        // - Color selection & status LED control
        else if (G_mode & MODE_MANUAL && !(cycle & CONTROL_STATUS_EVERY))
        {
            get_color();
            G_white = get_run_change(G_white);
            set_color();
        }
        // - Holo lights
        set_lasers();
        set_finishing();
        // - Buzzer
        if (!(cycle & CONTROL_BUZZER_EVERY))
            trigger_buzzer();
        // - Serial comms
        if (load_serial() == BUFFER_LENGTH)
        {
            if (execute())
                G_mode = MODE_ERROR;
        }
        // END

        cycle = (cycle + 1) & 0xFFFFFF;
        sleep = time_us_64() - start;
        sleep_us((sleep < CONTROL_LOOP_uS) * (CONTROL_LOOP_uS - sleep));
        start = time_us_64();
    }
    return 0;
}