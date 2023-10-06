cmake_minimum_required(VERSION 3.13)

# initialize the SDK based on PICO_SDK_PATH
# note: this must happen before project()
include(pico_sdk_import.cmake)

project(holo_controller)

# initialize the Raspberry Pi Pico SDK
pico_sdk_init()

add_executable(holo_controller
    i_holo_controller.c
    control_globals.c
    power/input_color_switch.c
    power/input_mode_select.c
    power/input_run_button.c
    power/output_buzzer.c
    power/output_color_switch.c
    power/output_finishing.c
    power/output_laser.c
    power/output_status.c
    power/power_main.c
    serial/parser.c
    serial/serial_main.c
)

target_link_libraries(holo_controller pico_stdlib)

pico_enable_stdio_usb(holo_controller 1)
pico_enable_stdio_uart(holo_controller 0)

pico_add_extra_outputs(holo_controller)