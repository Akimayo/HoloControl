#include "control_globals.h"
#include "modes.h"
#include "colors.h"

char G_mode = MODE_OFF, G_prev_mode = MODE_OFF, G_color = COLOR_OFF, G_white = 0, G_auto_run = 0;
long unsigned int G_exposures[4] = {0, 0, 0, 0};
long unsigned int G_finishing = 0, G_waiting = 0;