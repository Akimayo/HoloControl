#include "serial_main.h"
#include "pico/stdlib.h"
#include "parser.h"
#include "../control_globals.h"

char buffer_index = 0;
char buffer[BUFFER_LENGTH];
char load_serial(void)
{
    int c;
    while (1)
    {
        c = getchar_timeout_us(!(G_auto_run & 1) * 100);
        if (c != PICO_ERROR_TIMEOUT && buffer_index < BUFFER_LENGTH)
            buffer[buffer_index++] = c;
        else
            break;
    }
    return buffer_index;
}

char execute(void)
{
    buffer_index = 0;
    if (executors[buffer[0] & 31] == NULL)
        return 1;
    return executors[buffer[0] & 31]((buffer[1] << 16) + (buffer[2] << 8) + buffer[3]);
}
