# HS2_SexRobotController
**Honey Select 2 Sex Robot Controller Plugin v1.7**

**Main configuration menu accessed by hitting F1 and then clicking the Plugin settings button**

![plugin](https://user-images.githubusercontent.com/93683226/141469348-13a16e15-cbf2-45cc-9ec7-334a3e93203e.png)


**Quick access buttons (works in VR) to connect / disconnect your sex robot and to increase / decrease the stroke multiplier**

![pluginmenu](https://user-images.githubusercontent.com/93683226/141469532-d0db17df-e333-477b-b0db-08027204de6f.png)


**Pressing the hotkeys or the quick access buttons to connect / disconnect your sex robot and to increase / decrease the stroke multiplier show feedback text when pressed (especially helpful in VR)**

![buttonfeedback](https://user-images.githubusercontent.com/93683226/141486554-17f9b57a-e50f-435d-9612-085e2722fa70.png)

This plugin outputs the positional data from a total of 100 (currently) of the 'HScenes' (sex scenes) in Honey Select 2 with full 6 degrees of freedom (6DOF) in a simple text format known as T-Code (Toy-Code) which is then sent over a serial link (COM port) to drive an open source sex robot (OSR2, OSR2+, SR6, etc).

The 6 total degrees of freedom are:
- L0 (X) Up/Down
- L1 (Y) Forward/Backward
- L2 (Z) Left/Right
- R0 (RX) Twist
- R1 (RY) Roll
- R2 (RZ) Pitch

The male's penis in a given HScene is always aligned with the L0 (X) Up/Down axis, and depending on the HScene/animation in Honey Select 2, 3+ specific 'bones' of the female's vagina, anus, breasts, mouth, or hands are used to calculate and export the necessary 6DOF information to drive the sex robot.

The T-Code format and open source sex robots (OSR2, OSR2+, SR6) were all created/developed by TempestVR. You can find the full/free open sourced OSR2 here: https://www.patreon.com/posts/osr2-1-year-47041804

## Supported Scenes In Honey Select 2

#### Honey Select 2 Service HScene Category
|Animation Name|Female Target Type|Supported|
|---|---|---|
|Blowjob|ORAL|Yes|
|Handjob|LEFTHAND|Yes|
|Glans Tease|(in progress)|No|
|Boobjob|BREASTS|Yes|
|Licking Boobjob|BREASTS|Yes|
|Sucking Boobjob|BREASTS|Yes|
|Exhausted Handjob|RIGHTHAND|Yes|
|Exhausted Blowjob|ORAL|Yes|
|Standing Handjob|RIGHTHAND|Yes|
|No-Hands Tip Licking|(in progress)|No|
|No-Hands Blowjob|ORAL|Yes|
|Deepthroat|ORAL|Yes|
|Standing Boobjob|BREASTS|Yes|
|Stand. Lick. Boobjob|BREASTS|Yes|
|Restrained Blowjob|ORAL|Yes|
|Forced Handjob|RIGHTHAND|Yes|
|Irrumatio|ORAL|Yes|
|Chair Handjob|LEFTHAND|Yes|
|Sit. No-Hand Blowjob|ORAL|Yes|
|Sitting Boobjob|BREASTS|Yes|
|Sitting Licking Boobjob|BREASTS|Yes|
|Wall-Trapped Blowjob|ORAL|Yes|
|Crouching Blowjob|ORAL|Yes|
|Desk Handjob|RIGHTHAND|Yes|
|Behind Handjob|LEFTHAND|Yes|
|Sleeping Boobjob|BREASTS|Yes|
|Wall Irramatio|ORAL|Yes|
|Chair Irramatio|ORAL|Yes|
|Pet Blowjob|ORAL|Yes|

#### Honey Select 2 Insert HScene Category
|Animation Name|Female Target Type|Supported|
|---|---|---|
|Missionary|VAGINAL|Yes|
|Breast Grope Missionary|VAGINAL|Yes|
|Doggy|VAGINAL|Yes|
|Kneeling Behind|VAGINAL|Yes|
|Spooning|VAGINAL|Yes|
|Cowgirl|VAGINAL|Yes|
|Chest Grope Cowgirl|VAGINAL|Yes|
|Reverse Cowgirl|VAGINAL|Yes|
|Piledriver Missionary|VAGINAL|Yes|
|Bent Missionary|VAGINAL|Yes|
|Double Decker|VAGINAL|Yes|
|Anal Missionary|ANAL|Yes|
|Anal Doggy|ANAL|Yes|
|Floor Bondage Miss.|VAGINAL|Yes|
|Forced Missionary|VAGINAL|Yes|
|Standing|VAGINAL|Yes|
|Standing Behind|VAGINAL|Yes|
|Thrust Behind|VAGINAL|Yes|
|Lifting|VAGINAL|Yes|
|Reverse Lifting|VAGINAL|Yes|
|Thighjob|INTERCRURAL|Yes|
|Stockade|VAGINAL|Yes|
|Wall-Facing Behind|VAGINAL|Yes|
|Wall-Facing Anal|ANAL|Yes|
|Chair Sitting Behind|VAGINAL|Yes|
|Anal Doggy on Chair|ANAL|Yes|
|Desk Missionary|VAGINAL|Yes|
|Desk on Side|VAGINAL|Yes|
|Desk Doggy|ANAL|Yes|
|Against Counter Behind|VAGINAL|Yes|
|Wall-Trapped Doggy|VAGINAL|Yes|
|Crouch Insertion|VAGINAL|Yes|
|Delivery Table Insert|VAGINAL|Yes|
|Hanging|VAGINAL|Yes|
|Tied Up Insertion|VAGINAL|Yes|
|Lying Doggystyle|VAGINAL|Yes|
|Anal Piledriver Miss.|ANAL|Yes|
|Face to Face Sitting|VAGINAL|Yes|
|Chest Grope Sit Behind|VAGINAL|Yes|
|Sitting Hugging|VAGINAL|Yes|
|Restrained Standing|VAGINAL|Yes|
|Clinging Lifting|VAGINAL|Yes|
|Wall-Pressed Behind|VAGINAL|Yes|
|Pet Sex|VAGINAL|Yes|
|Mating Press|VAGINAL|Yes|

#### Honey Select 2 Woman-led HScene Category
|Animation Name|Female Target Type|Supported|
|---|---|---|
|Face Sit Cunnilingus|(in progress)|No|
|Nipple Licking Blowjob|RIGHTHAND|Yes|
|Rimjob + Handjob|RIGHTHAND|Yes|
|Standing Footjob|(in progress)|No|
|Sitting Footjob|(in progress)|No|
|All Fours Handjob|RIGHTHAND|Yes|
|Restrained Blowjob|ORAL|Yes|
|Chair Restraint Footjob|(in progress)|No|
|Cowgirl|VAGINAL|Yes|
|Reverse Cowgirl|VAGINAL|Yes|
|Reverse Piledriver|VAGINAL|Yes|
|Cowgirl Intercrural|INTERCRURAL|Yes|
|Handjob Intercrural|INTERCRURAL|Yes|
|Chair Intercrural|INTERCRURAL|Yes|
|Piledriver Rev. Cowgirl|VAGINAL|Yes|
|Standing|VAGINAL|Yes|
|Anal Reverse Cowgirl|ANAL|Yes|
|Male Restrained Stand|VAGINAL|Yes|
|Chair Restraint Sitting|VAGINAL|Yes|

#### Honey Select 2 Female Group HScene Category
|Animation Name|Female Target Type|Supported|
|---|---|---|
|W Cowgirl|VAGINAL|Yes|
|W Cowgirl (swap)|VAGINALSWAP|Yes|
|W Blowjob|ORAL|Yes|
|W Blowjob (swap)|ORALSWAP|Yes|
|Intercrural Sandwich|INTERCRURAL|Yes|
|Interc. Sandwich (swap)|INTERCRURAL|Yes|
|Insertion + Fingering|VAGINAL|Yes|
|Insert. + Fing. (swap)|VAGINALSWAP|Yes|
|W Handjob Licking|RIGHTHANDSWAP|Yes|
|W Handjob Lick. (swap)|RIGHTHAND|Yes|
|Sitting+Cunilingus|VAGINAL|Yes|
|Sit+Cunilingus (swap)|VAGINALSWAP|Yes|

#### Honey Select 2 Special HScene Category
|Animation Name|Female Target Type|Supported|
|---|---|---|
|69|ORAL|Yes|
