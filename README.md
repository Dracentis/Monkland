Monkland

# Packet Formats
This a temporal set up of the packet format. The Creature and Object packets could be combined. The sizes are not accurate.


Physical Object Packet
| type  | field              | size (bytes) |
|-------|--------------------|:------------:|
| float | bounce             |       4      |
| bool  | canBeHitByWeapons  |       1      |
| byte  | numberOfChunks     |       1      |
| pkt   | BODYCHUNKPCKT      |              |
| byte  | numberOfChunkConns |       1      |
| pkt   | BODYCHUNKSCONNPKT  |              |

Abstract Creature Packet
| type       | field                             | size (bytes) |
|------------|-----------------------------------|:------------:|
| pkt        | Abstract Physical Object   Packet |              |
| byte       | Creature template type            |       1      |
| int        | remainInDenCounter                |       4      |
| worldCoord | spawnDen                          |      16      |

Abstract Physical Object
| type | field                          | size (bytes) |
|------|--------------------------------|:------------:|
| pkt  | Abstract World Entity   Packet |              |
| byte | AbsPhysType                    |       1      |
|      | (optional)                     |              |
| pkt  | AbstractSpear                  |              |

Abstract World Entity Packet
| type       | field         | size (bytes) |
|------------|---------------|:------------:|
| EntityID   | ID            |       8      |
| WorldCoord | pos           |      16      |
| bool       | inDen         |       1      |
| int        | timeSpentHere |       4      |

Physical Entity Packet
| type   | field                    | size (bytes) |
|--------|--------------------------|:------------:|
| byte   | packetType               |       1      |
| string | roonName                 |     ~26?     |
| int    | distinguisher            |       4      |
| byte   | AbsPhyObj type           |       1      |
| pkt    | Abstract Physical Object |              |
| pkt    | Physical Object Packet   |              |
| pkt    | Especific Obj Packet     |              |

Creature Entity Packet
| type   | field                    | size (bytes) |
|--------|--------------------------|:------------:|
| byte   | packetType               |       1      |
| string | roonName                 |     ~26?     |
| int    | distinguisher            |       4      |
| byte   | AbsPhyObj type           |       1      |
| pkt    | Abstract Physical Object |              |
| pkt    | Physical Object Packet   |              |
| pkt    | Especific Obj Packet     |              |
