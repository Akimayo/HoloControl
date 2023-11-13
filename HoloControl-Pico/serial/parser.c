#include "parser.h"
#include <stdio.h>
#include "../control_globals.h"
#include "../i_holo_controller.h"
#include "../power/output_status.h"
#include "../power/input_color_switch.h"
#include "../power/output_buzzer.h"
#include "pico/unique_id.h"

#define COLOR_STRING(_buffer, _color) snprintf(_buffer, 5, "%c%c%c%c", 45 + ((_color & COLOR_RED) > 0) * 37, 45 + ((_color & COLOR_GREEN) > 0) * 26, 45 + ((_color & COLOR_BLUE) > 0) * 21, 45 + ((_color & COLOR_EXT) > 0) * 24)

#pragma region Replies
void reply_d(char *name, long unsigned int value)
{
    printf("\n✅  %s: %lu\n", name, value);
}
void reply_s(char *name, char *value)
{
    printf("\n✅  %s: %s\n", name, value);
}
void error(char *message)
{
    printf("\n❌  %s\n", message);
}
#pragma endregion

#pragma region Instructions
char send_init_data(int _) //   [0]
{
    char id[2 * PICO_UNIQUE_BOARD_ID_SIZE_BYTES + 1];
    pico_get_unique_board_id_string(id, 2 * PICO_UNIQUE_BOARD_ID_SIZE_BYTES + 1);
    printf("HoloControl;b:%s;v:%s\n", id, __TIMESTAMP__);
}

char set_blue_timing(int value) // b [2]
{
    if (G_auto_run & 8)
        error("Cannot change timing while exposure is in progress");
    else
    {
        G_exposures[2] = value * 1000;
        reply_d("Blue exp. time [ms]", value);
    }
    return 0;
}

char set_manual_colors(int value) // c [3]
{
    if (G_mode == MODE_MANUAL)
    {
        G_color = ((value & 0xFF0000) > 0) * COLOR_RED + ((value & 0x00FF00) > 0) * COLOR_GREEN + ((value & 0x0000FF) > 0) * COLOR_BLUE + (G_color & COLOR_EXT);
        char code[5];
        COLOR_STRING(code, G_color);
        reply_s("New color", code);
    }
    else
        error("Only available in manual mode");
    return 0;
}

char get_current_color(int _) // d [4]
{
    if (G_mode == MODE_MANUAL)
    {
        char code[5];
        COLOR_STRING(code, G_color);
        reply_s("Color", code);
    }
    else
        error("Only available in manual mode");
    return 0;
}

char *on_off[] = {[0] = "OFF", [1] = "ON"};
char set_finish_power(int value) // e [5]
{
    if (G_mode == MODE_MANUAL)
    {
        G_white = value > 0;
        reply_s("Finishing LED", on_off[G_white]);
    }
    else
        error("Only available in manual mode");
    return 0;
}

char set_finish_timing(int value) // f [6]
{
    if (G_auto_run & 8)
        error("Cannot change timing while exposure is in progress");
    else
    {
        G_finishing = value * 1000;
        reply_d("Finishing time [ms]", value);
    }
    return 0;
}

char set_green_timing(int value) // g [7]
{
    if (G_auto_run & 8)
        error("Cannot change timing while exposure is in progress");
    else
    {
        G_exposures[1] = value * 1000;
        reply_d("Green exp. time [ms]", value);
    }
    return 0;
}

char set_external_timing(int value) // h [8]
{
    if (G_auto_run & 8)
        error("Cannot change timing while exposure is in progress");
    else
    {
        G_exposures[3] = value * 1000;
        reply_d("External exp. time [ms]", value);
    }
    return 0;
}

char set_manual_external(int value) // i [9]
{
    if (G_mode == MODE_MANUAL)
    {
        G_color = (G_color & (COLOR_RED | COLOR_BLUE | COLOR_GREEN)) + (value > 0) * COLOR_EXT; // FIXME: This turns off all other colors
        char code[5];
        COLOR_STRING(code, G_color);
        reply_s("New color", code);
    }
    else
        error("Only available in manual mode");
    return 0;
}

char set_red_timing(int value) // r [18]
{
    if (G_auto_run & 8)
        error("Cannot change timing while exposure is in progress");
    else
    {
        G_exposures[0] = value * 1000;
        reply_d("Red exp. time [ms]", value);
    }
    return 0;
}

char *status_names[] = {[MODE_MANUAL] = "manual", [MODE_AUTO] = "auto", [MODE_ERROR] = "error"};
char get_current_status(int _) // s [19]
{
    reply_s("Status", status_names[G_mode]);
    return 0;
}

char get_current_timing(int _) // t [20]
{
    reply_d("Waiting time [ms]", G_waiting / 1000);
    reply_d("Red exp. time [ms]", G_exposures[0] / 1000);
    reply_d("Green exp. time [ms]", G_exposures[1] / 1000);
    reply_d("Blue exp. time [ms]", G_exposures[2] / 1000);
    reply_d("External exp. time [ms]", G_exposures[3] / 1000);
    reply_d("Finishing time [ms]", G_finishing / 1000);
}

char set_buzzer(int value) // u [21]
{
    if (G_mode == MODE_MANUAL)
    {
        enable_buzzer(value);
        reply_d("Buzzer enabled for [ms]", value);
    }
    else
        error("Only available in manual mode");
    return 0;
}

char reset_timing(int _) // v [22]
{
    if (G_auto_run & 8)
        error("Cannot change timing while exposure is in progress");
    else
    {
        G_exposures[0] = G_exposures[1] = G_exposures[2] = G_exposures[3] = G_finishing = G_waiting = 0;
        reply_s("Reset", "Exposure times, finishing times and waiting time have been reset.");
    }
    return 0;
}

char set_wait_timing(int value) // w [23]
{
    if (G_auto_run & 8)
        error("Cannot change timing while exposure is in progress");
    else
    {
        G_waiting = value * 1000;
        reply_d("Waiting time [ms]", value);
    }
    return 0;
}

char run_start(int _) // x [24]
{
    if (G_auto_run & 1)
        error("Exposure is already in progress");
    else if (G_mode & MODE_AUTO)
    {
        printf("\n▶️  Exposure will %sSTART after waiting for %lu s\n", G_auto_run & 8 ? "re-" : "", G_waiting/1000000);
        start();
    }
    else
        error("Switch the device into AUTO mode first");
    return 0;
}

char run_pause(int _) // y [25]
{
    if (G_auto_run & 1)
    {
        pause();
        printf("\n⏸️  Exposure paused\n");
    }
    else
        error("Exposure is already paused");
    return 0;
}

char run_reset(int _) // z [26]
{
    if (G_auto_run & 1)
        error("Exposure in progress, please pause it first");
    else if (G_auto_run & 8)
    {
        reset();
        printf("\n⏹️  Exposure stopped and reset\n");
    }
    else
        error("Exposure not started, nothing to reset");
    return 0;
}
#pragma endregion

char (*executors[32])(int) = {
    [0] = send_init_data,
    [2] = set_blue_timing,     // "b"
    [3] = set_manual_colors,   // "c"
    [4] = get_current_color,   // "d"
    [5] = set_finish_power,    // "e"
    [6] = set_finish_timing,   // "f"
    [7] = set_green_timing,    // "g"
    [8] = set_external_timing, // "h"
    [9] = set_manual_external, // "i"








    [18] = set_red_timing,     // "r"
    [19] = get_current_status, // "s"
    [20] = get_current_timing, // "t"
    [21] = set_buzzer,         // "u"
    [22] = reset_timing,       // "v"
    [23] = set_wait_timing,    // "w"
    [24] = run_start,          // "x"
    [25] = run_pause,          // "y"
    [26] = run_reset           // "z"
};