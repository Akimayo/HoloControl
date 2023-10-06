| Instruction  | Code  |  Hex   | Parameter |
|:-------------|:-----:|:------:|:----------|
| Start        | `"x"` | `0x18` | _none_    |
| Pause        | `"y"` | `0x19` | _none_    |
| Stop         | `"z"` | `0x1A` | _none_    |
| Set colors   | `"c"` | `0x03` | Every non-zero byte turns on its corresponding LED (first byte red, second green, third blue) |
| Set external   | `"i"` | `0x09` | Non-zero value turns on external laser |
| Get colors   | `"d"` | `0x04` | _none_    |
| Toggle white | `"e"` | `0x05` | Non-zero value turns on finishing white LED |
| Get mode     | `"s"` | `0x13` | _none_    |
| Buzz | `"u"` | `0x15` | 3-byte time to buzz for in milliseconds |
| Set red time | `"r"` | `0x12` | 3-byte time of red laser in milliseconds |
| Set green time | `"g"` | `0x07` | 3-byte time of green laser in milliseconds |
| Set blue time | `"b"` | `0x02` | 3-byte time of blue laser in milliseconds |
| Set external time | `"h"` | `0x08` | 3-byte time of external laser in milliseconds |
| Set finishing time | `"f"` | `0x06` | 3-byte time of white light in milliseconds after exposing |
| Set wait time | `"w"` | `0x17` | 3-byte time of waiting before exposing |
| Show all times | `"t"` | `0x14` | _none_ |
| Reset all times | `"v"` | `0x16` | _none_