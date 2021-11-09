# HS2_SexRobotController
Honey Select 2 Sex Robot Controller Plugin v1.2

![HS2_SexRobotControllerConfigMenu](https://user-images.githubusercontent.com/93683226/140925711-6f79036d-284d-4cf5-b353-cdee4a8366ab.png)

This plugin outputs the positional data from a total of 94 (currently) of the 'HScenes' (sex scenes) in Honey Select 2 with full 6 degrees of freedom (6DOF) in a simple text format known as T-Code (Toy-Code) which is then sent over a serial link (COM port) to drive an open source sex robot (OSR2, OSR2+, SR6, etc).

The 6 total degrees of freedom are:
- L0 (X) Up/Down
- L1 (Y) Forward/Backward
- L2 (Z) Left/Right
- R0 (RX) Twist
- R1 (RY) Roll
- R2 (RZ) Pitch

The male's penis in a given HScene is always aligned with the L0 (X) Up/Down axis, and depending on the HScene/animation in Honey Select 2, 3+ specific 'bones' of the female's vagina, anus, breasts, mouth, or hands are used to calculate and export the necessary 6DOF information to drive the sex robot.

The T-Code format and open source sex robots (OSR2, OSR2+, SR6) were all created/developed by TempestVR. You can find the full/free open sourced OSR2 here: https://www.patreon.com/posts/osr2-1-year-47041804
