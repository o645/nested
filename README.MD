## NESTED

WIP NES hardware emulator, written in C#.

## TODO

1. [ ] CPU (2A03, modified 6502 Chip)
   1. [ ] All 256 Opcodes
      2. [ ] Official Opcodes | **Working On**
         3. [x] Transfer Instructions
         4. [x] Stack Instructions
         5. [ ] Decrements & Increments
         6. [ ] Arithmetic Operations
         7. [ ] Logical Operations
         8. [ ] Shift & Rotate Instructions
         9. [ ] Flag Instructions
         10. [ ] Comparisons
         11. [ ] Conditional Branch Instructions
         12. [ ] Jumps & Subroutines
         13. [ ] Interrupts
         14. [ ] Bit-test and NOP
      3. [ ] Unofficial Opcodes
   2. [ ] Run Snake Game
      1. [ ] SDL Setup for debug I/O
   3. [ ] Bus Emulation
2. [ ] Cartridge Emulation
   1. [ ] Parse ROM Header
   2. [ ] Run test ROMs
3. [ ] PPU (2C02 Ricoh Chip)
   4. [ ] Memory Map
   5. [ ] Mirroring
   6. [ ] NMI Interrupts
   7. [ ] Rendering CHR ROM Tiles
   8. [ ] Palette Setup
   9. [ ] Render to Screen
   10. [ ] Scrolling
5. [ ] Gamepad Emulation
4. [ ] APU (Part of 2A03 Chip)
