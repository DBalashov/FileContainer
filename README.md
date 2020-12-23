# FileContainer

### Speed tests

#### Single requests - write

|Page   |ms  |   MB     | MB/sec  |Lost space(%)|
|---:|---:|---:|---:|---:|
|128    |7454|   373,9  | 50,2    |3,20   |
|256    |3818|   368,1  | 96,4    |1,67   |
|512    |2195|   365,4  | 166,5   |0,94   |
|1024   |1393|   364,4  | 261,6   |0,65   |
|2048   |1089|   364,5  | 334,7   |0,69   |
|4096   | 802|    366,1 |  456,5  | 1,13  |
|8192   | 622|    370,9 |  596,3  | 2,41  |
|16384  | 511|    382,6 |  748,7  | 5,39  |
|32768  | 547|    407,8 |  745,5  | 11,23 |
|65536  | 535|    460,8 |  861,3  | 21,45 |

#### Batch requests - batch: 8

|Page  |   ms  |    MB/sec|
|-----:|------:|----------:|
|128   |4728   | 79,1|
|256   |2438   | 151,0|
|512   |1357   | 269,3|
|1024  | 819   |  444,9|
|2048  | 561   |  649,7|
|4096  | 402   |  910,7|
|8192  | 276   |  1343,9|
|16384 |  240  |   1594,2|
|32768 |  239  |   1706,2|
|65536 |  270  |   1706,7|

#### Batch requests - batch: 16
|Page  |   ms  |    MB/sec|
|-----:|------:|---------:|
|128   |6509   | 57,4|
|256   |2751   | 133,8|
|512   |1314   | 278,1|
|1024  | 893   |  408,0|
|2048  | 842   |  432,9|
|4096  | 501   |  730,7|
|8192  | 291   |  1274,6|
|16384 |  223  |   1715,7|
|32768 |  286  |   1425,8|
|65536 |  259  |   1779,2|

#### Batch requests - batch: 32
|Page  |   ms  |    MB/sec|
|-----:|------:|----------:|
|128   |4405   | 84,9|
|256   |2596   | 141,8|
|512   |1204   | 303,5|
|1024  | 726   |  501,9|
|2048  | 485   |  751,5|
|4096  | 483   |  758,0|
|8192  | 275   |  1348,8|
|16384 |  203  |   1884,8|
|32768 |  177  |   2303,8|
|65536 |  187  |   2464,2|

#### Batch requests - batch: 64
|Page  |   ms  |    MB/sec|
|-----:|------:|----------:|
|128   |4251   |88,0|
|256   |2265   |162,5|
|512   |1225   |298,3|
|1024  | 760   | 479,4|
|2048  | 490   | 743,8|
|4096  | 358   | 1022,6|
|8192  | 236   | 1571,6|
|16384 |  336  |  1138,7|
|32768 |  182  |  2240,6|
|65536 |  166  |  2776,0|

#### Batch requests - batch: 128
|Page  |   ms  |    MB/sec|
|-----:|------:|----------:|
|128   |7710   | 48,5|
|256   |2202   | 167,2|
|512   |1228   | 297,6|
|1024  | 735   |  495,7|
|2048  | 515   |  707,7|
|4096  | 326   |  1123,0|
|8192  | 224   |  1655,8|
|16384 |  180  |   2125,6|
|32768 |  167  |   2441,8|
|65536 |  160  |   2880,1|