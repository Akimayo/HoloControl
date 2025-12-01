# == DO NOT EDIT THE FOLLOWING LINES for the Raspberry Pi Pico VS Code Extension to work ==
if(WIN32)
    set(USERHOME $ENV{USERPROFILE})
else()
    set(USERHOME $ENV{HOME})
endif()
set(sdkVersion 2.2.0)
set(toolchainVersion 14_2_Rel1)
set(picotoolVersion 2.2.0-a4)
set(picoVscode ${USERHOME}/.pico-sdk/cmake/pico-vscode.cmake)
if (EXISTS ${picoVscode})
    include(${picoVscode})
endif()
# ====================================================================================
set(PICO_BOARD pico CACHE STRING "Board type")

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