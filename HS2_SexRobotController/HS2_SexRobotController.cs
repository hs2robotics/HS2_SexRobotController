using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Diagnostics;
using System.IO.Ports;
using UnityEngine.UI;
using IllusionUtility.GetUtility;

namespace HS2_SexRobotController
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]

    public class HS2_SexRobotController : BaseUnityPlugin
    {
        public const string pluginGUID = "hs2robotics.HS2SexRobotController";
        public const string pluginName = "HS2_SexRobotController";
        public const string pluginVersion = "1.7";

        public static HScene hScene;
        public bool inHScene = false;
        public ChaControl[] females;
        public ChaControl[] males;
        public static bool animationChanged = false;
        public static string animationName = "";
        public Transform malePenisBase;
        public Transform malePenisTip;
        public Transform malePenisLeftBall;
        public Transform malePenisRightBall;
        public Transform femaleMouthLipsUpper;
        public Transform femaleMouthLipsLower;
        public Transform femaleMouthLeft;
        public Transform femaleMouthRight;
        public Transform femaleHip;
        public Transform femaleVagina;
        public Transform femaleAnus;
        public Transform femaleMiddleBreastsLeft;
        public Transform femaleMiddleBreastsRight;
        public Transform femaleBreasts;
        public Transform femaleMiddleFingerLeft;
        public Transform femaleRingFingerLeft;
        public Transform femaleHandLeft;
        public Transform femaleMiddleFingerRight;
        public Transform femaleRingFingerRight;
        public Transform femaleHandRight;
        public Transform femaleLegLeft;
        public Transform femaleKneeLeft;
        public Transform femaleLegRight;
        public Transform femaleKneeRight;

        public enum femaleTargetType
        {
            VAGINAL,
            ANAL,
            ORAL,
            BREASTS,
            LEFTHAND,
            RIGHTHAND,
            INTERCRURAL,
            VAGINALSWAP,
            ORALSWAP,
            RIGHTHANDSWAP,
            INTERCRURALSWAP
        }

        femaleTargetType currentFemaleTargetType;

        public Dictionary<string, femaleTargetType> animationFemaleTargetDictionary = new Dictionary<string, femaleTargetType>
        {
            // Schema: {ANIMATION NAME, FEMALE TARGET(S) TO USE IN REGARD TO MAPPING TO THE MALE'S PENIS TARGET}

            // Honey Select 2 Service HScene Category
            {"Blowjob", femaleTargetType.ORAL},
            {"Handjob", femaleTargetType.LEFTHAND},
            //{"Glans Tease", femaleTargetType.},
            {"Boobjob", femaleTargetType.BREASTS},
            {"Licking Boobjob", femaleTargetType.BREASTS},
            {"Sucking Boobjob", femaleTargetType.BREASTS},
            {"Exhausted Handjob", femaleTargetType.RIGHTHAND},
            {"Exhausted Blowjob", femaleTargetType.ORAL},
            {"Standing Handjob", femaleTargetType.RIGHTHAND},
            //{"No-Hands Tip Licking", femaleTargetType.},
            {"No-Hands Blowjob", femaleTargetType.ORAL},
            {"Deepthroat", femaleTargetType.ORAL},
            {"Standing Boobjob", femaleTargetType.BREASTS},
            {"Stand. Lick. Boobjob", femaleTargetType.BREASTS},
            {"Restrained Blowjob", femaleTargetType.ORAL},
            {"Forced Handjob", femaleTargetType.RIGHTHAND},
            {"Irrumatio", femaleTargetType.ORAL},
            {"Chair Handjob", femaleTargetType.LEFTHAND},
            {"Sit. No-Hand Blowjob", femaleTargetType.ORAL},
            {"Sitting Boobjob", femaleTargetType.BREASTS},
            {"Sitting Licking Boobjob", femaleTargetType.BREASTS},
            {"Wall-Trapped Blowjob", femaleTargetType.ORAL},
            {"Crouching Blowjob", femaleTargetType.ORAL},
            {"Desk Handjob", femaleTargetType.RIGHTHAND},
            {"Behind Handjob", femaleTargetType.LEFTHAND},
            {"Sleeping Boobjob", femaleTargetType.BREASTS},
            {"Wall Irramatio", femaleTargetType.ORAL},
            {"Chair Irramatio", femaleTargetType.ORAL},
            {"Pet Blowjob", femaleTargetType.ORAL},
            
            // Honey Select 2 Insert HScene Category
            {"Missionary", femaleTargetType.VAGINAL},
            {"Breast Grope Missionary", femaleTargetType.VAGINAL},
            {"Doggy", femaleTargetType.VAGINAL},
            {"Kneeling Behind", femaleTargetType.VAGINAL},
            {"Spooning", femaleTargetType.VAGINAL},
            {"Cowgirl", femaleTargetType.VAGINAL},
            {"Chest Grope Cowgirl", femaleTargetType.VAGINAL},
            {"Reverse Cowgirl", femaleTargetType.VAGINAL},
            {"Piledriver Missionary", femaleTargetType.VAGINAL},
            {"Bent Missionary", femaleTargetType.VAGINAL},
            {"Double Decker", femaleTargetType.VAGINAL},
            {"Anal Missionary", femaleTargetType.ANAL},
            {"Anal Doggy", femaleTargetType.ANAL},
            {"Floor Bondage Miss.", femaleTargetType.VAGINAL},
            {"Forced Missionary", femaleTargetType.VAGINAL},
            {"Standing", femaleTargetType.VAGINAL},
            {"Standing Behind", femaleTargetType.VAGINAL},
            {"Thrust Behind", femaleTargetType.VAGINAL},
            {"Lifting", femaleTargetType.VAGINAL},
            {"Reverse Lifting", femaleTargetType.VAGINAL},
            {"Thighjob", femaleTargetType.INTERCRURAL},
            {"Stockade", femaleTargetType.VAGINAL},
            {"Wall-Facing Behind", femaleTargetType.VAGINAL},
            {"Wall-Facing Anal", femaleTargetType.ANAL},
            {"Chair Sitting Behind", femaleTargetType.VAGINAL},
            {"Anal Doggy on Chair", femaleTargetType.ANAL},
            {"Desk Missionary", femaleTargetType.VAGINAL},
            {"Desk on Side", femaleTargetType.VAGINAL},
            {"Desk Doggy", femaleTargetType.ANAL},
            {"Against Counter Behind", femaleTargetType.VAGINAL},
            {"Wall-Trapped Doggy", femaleTargetType.VAGINAL},
            {"Crouch Insertion", femaleTargetType.VAGINAL},
            {"Delivery Table Insert", femaleTargetType.VAGINAL},
            {"Hanging", femaleTargetType.VAGINAL},
            {"Tied Up Insertion", femaleTargetType.VAGINAL},
            {"Lying Doggystyle", femaleTargetType.VAGINAL},
            {"Anal Piledriver Miss.", femaleTargetType.ANAL},
            {"Face to Face Sitting", femaleTargetType.VAGINAL},
            {"Chest Grope Sit Behind", femaleTargetType.VAGINAL},
            {"Sitting Hugging", femaleTargetType.VAGINAL},
            {"Restrained Standing", femaleTargetType.VAGINAL},
            {"Clinging Lifting", femaleTargetType.VAGINAL},
            {"Wall-Pressed Behind", femaleTargetType.VAGINAL},
            {"Pet Sex", femaleTargetType.VAGINAL},
            {"Mating Press", femaleTargetType.VAGINAL},

            // Honey Select 2 Woman-led HScene Category
            {"Nipple Licking Blowjob", femaleTargetType.RIGHTHAND},
            {"Rimjob + Handjob", femaleTargetType.RIGHTHAND},
            //{"Standing Footjob", femaleTargetType.},
            //{"Sitting Footjob", femaleTargetType.},
            {"All Fours Handjob", femaleTargetType.RIGHTHAND},
            //{"Restrained Blowjob", femaleTargetType.ORAL}, // Already in the Dictionary above
            //{"Chair Restraint Footjob", femaleTargetType.},
            //{"Cowgirl", femaleTargetType.VAGINAL}, // Already in the Dictionary above
            //{"Reverse Cowgirl", femaleTargetType.VAGINAL}, // Already in the Dictionary above
            {"Reverse Piledriver", femaleTargetType.VAGINAL},
            {"Cowgirl Intercrural", femaleTargetType.INTERCRURAL},
            {"Handjob Intercrural", femaleTargetType.INTERCRURAL},
            {"Chair Intercrural", femaleTargetType.INTERCRURAL},
            {"Piledriver Rev. Cowgirl", femaleTargetType.VAGINAL},
            //{"Standing", femaleTargetType.VAGINAL}, // Already in the Dictionary above
            {"Anal Reverse Cowgirl", femaleTargetType.ANAL},
            {"Male Restrained Stand", femaleTargetType.VAGINAL},
            {"Chair Restraint Sitting", femaleTargetType.VAGINAL},

            // Honey Select 2 Female Group HScene Category
            {"W Cowgirl", femaleTargetType.VAGINAL},
            {"W Cowgirl (swap)", femaleTargetType.VAGINALSWAP},
            {"W Blowjob", femaleTargetType.ORAL},
            {"W Blowjob (swap)", femaleTargetType.ORALSWAP},
            {"Intercrural Sandwich", femaleTargetType.INTERCRURAL},
            {"Interc. Sandwich (swap)", femaleTargetType.INTERCRURALSWAP},
            {"Insertion + Fingering", femaleTargetType.VAGINAL},
            {"Insert. + Fing. (swap)", femaleTargetType.VAGINALSWAP},
            {"W Handjob Licking", femaleTargetType.RIGHTHANDSWAP},
            {"W Handjob Lick. (swap)", femaleTargetType.RIGHTHAND},
            {"Sitting+Cunilingus", femaleTargetType.VAGINAL},
            {"Sit+Cunilingus (swap)", femaleTargetType.VAGINALSWAP},

            // Honey Select 2 Special HScene Category
            {"69", femaleTargetType.ORAL}
        };

        private SerialPort serialPort;
        private Stopwatch sw = Stopwatch.StartNew();
        private ConfigEntry<KeyboardShortcut> toggleSerialPortConnection { get; set; }
        private ConfigEntry<KeyboardShortcut> strokeLengthMultiplierIncrease { get; set; }
        private ConfigEntry<KeyboardShortcut> strokeLengthMultiplierDecrease { get; set; }
        public string[] serialPorts = { "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM10", "COM11", "COM12", "COM13", "COM14", "COM15", "COM16", "COM17", "COM18", "COM19", "COM20", "COM21", "COM22", "COM23", "COM24", "COM25", "COM26", "COM27", "COM28", "COM29", "COM30" };
        public ConfigEntry<string> serialPortConfig;
        public ConfigEntry<float> sexRobotUpdateFrequencyConfig;
        public ConfigEntry<string> serialPortStatus;
        public ConfigEntry<bool> serialPortConnected;
        public ConfigEntry<bool> diagnosticsConfig;
        public ConfigEntry<float> robotL0Multiplier;
        public ConfigEntry<float> robotL0MultiplierStepValue;
        public ConfigEntry<float> robotL0Min;
        public ConfigEntry<float> robotL0Max;
        public ConfigEntry<float> robotL1Min;
        public ConfigEntry<float> robotL1Max;
        public ConfigEntry<float> robotL2Min;
        public ConfigEntry<float> robotL2Max;
        public ConfigEntry<float> robotR0Min;
        public ConfigEntry<float> robotR0Max;
        public ConfigEntry<float> robotR1Min;
        public ConfigEntry<float> robotR1Max;
        public ConfigEntry<float> robotR2Min;
        public ConfigEntry<float> robotR2Max;
        public float autoRangeMin;
        public float autoRangeMid;
        public float autoRangeMax;
        public static bool updateRobotPosition = false;
        public static bool buttonConnectRobotClicked = false;
        public static bool buttonDisconnectRobotClicked = false;
        public static bool buttonStrokeMultiplierIncreaseClicked = false;
        public static bool buttonStrokeMultiplierDecreaseClicked = false;
        public static Transform buttonConnectRobot;
        public static Transform buttonDisconnectRobot;
        public static Transform buttonStrokeMultiplierIncrease;
        public static Transform buttonStrokeMultiplierDecrease;
        public static Text buttonConnectRobotText;
        public static Text buttonDisconnectRobotText;
        public static Text buttonStrokeMultiplierIncreaseText;
        public static Text buttonStrokeMultiplierDecreaseText;

        public void Awake()
        {
            HarmonyLib.Harmony.CreateAndPatchAll(typeof(HS2_SexRobotController));

            // Setup config file entries used in the in game menu
            // Creates a config file in BepInEx/config named hs2robotics.HS2SexRobotController.cfg
            strokeLengthMultiplierIncrease = Config.Bind("Sex Robot Limits", "Increase Stroke Multiplier", new KeyboardShortcut(KeyCode.Z));
            strokeLengthMultiplierDecrease = Config.Bind("Sex Robot Limits", "Decrease Stroke Multiplier", new KeyboardShortcut(KeyCode.X));
            robotL0Multiplier = Config.Bind("Sex Robot Limits", "Sex Robot (L0) Stroke Multiplier", 1.0f, new ConfigDescription("Sex Robot (L0) Stroke Multiplier", new AcceptableValueRange<float>(0.25f, 10.0f)));
            robotL0MultiplierStepValue = Config.Bind("Sex Robot Limits", "Sex Robot (L0) Stroke Multiplier Step Value", 0.25f, new ConfigDescription("Sex Robot (L0) Stroke Multiplier Step Value", new AcceptableValueRange<float>(0.01f, 1.0f)));
            robotL0Min = Config.Bind("Sex Robot Limits", "Sex Robot (L0) Up/Down Min", 0.0f, new ConfigDescription("Sex Robot (L0) Up/Down Min", new AcceptableValueRange<float>(0.0f, 0.5f)));
            robotL0Max = Config.Bind("Sex Robot Limits", "Sex Robot (L0) Up/Down Max", 1.0f, new ConfigDescription("Sex Robot (L0) Up/Down Max", new AcceptableValueRange<float>(0.5f, 1.0f)));
            robotL1Min = Config.Bind("Sex Robot Limits", "Sex Robot (L1) Forward/Backward Min", 0.0f, new ConfigDescription("Sex Robot (L1) Forward/Backward Min", new AcceptableValueRange<float>(0.0f, 0.5f)));
            robotL1Max = Config.Bind("Sex Robot Limits", "Sex Robot (L1) Forward/Backward Max", 1.0f, new ConfigDescription("Sex Robot (L1) Forward/Backward Max", new AcceptableValueRange<float>(0.5f, 1.0f)));
            robotL2Min = Config.Bind("Sex Robot Limits", "Sex Robot (L2) Left/Right Min", 0.0f, new ConfigDescription("Sex Robot (L2) Left/Right Min", new AcceptableValueRange<float>(0.0f, 0.5f)));
            robotL2Max = Config.Bind("Sex Robot Limits", "Sex Robot (L2) Left/Right Max", 1.0f, new ConfigDescription("Sex Robot (L2) Left/Right Max", new AcceptableValueRange<float>(0.5f, 1.0f)));
            robotR0Min = Config.Bind("Sex Robot Limits", "Sex Robot (R0) Twist Min", 0.0f, new ConfigDescription("Sex Robot (R0) Twist Min", new AcceptableValueRange<float>(0.0f, 0.5f)));
            robotR0Max = Config.Bind("Sex Robot Limits", "Sex Robot (R0) Twist Max", 1.0f, new ConfigDescription("Sex Robot (R0) Twist Max", new AcceptableValueRange<float>(0.5f, 1.0f)));
            robotR1Min = Config.Bind("Sex Robot Limits", "Sex Robot (R1) Roll Min", 0.0f, new ConfigDescription("Sex Robot (R1) Roll Min", new AcceptableValueRange<float>(0.0f, 0.5f)));
            robotR1Max = Config.Bind("Sex Robot Limits", "Sex Robot (R1) Roll Max", 1.0f, new ConfigDescription("Sex Robot (R1) Roll Max", new AcceptableValueRange<float>(0.5f, 1.0f)));
            robotR2Min = Config.Bind("Sex Robot Limits", "Sex Robot (R2) Pitch Min", 0.0f, new ConfigDescription("Sex Robot (R2) Pitch Min", new AcceptableValueRange<float>(0.0f, 0.5f)));
            robotR2Max = Config.Bind("Sex Robot Limits", "Sex Robot (R2) Pitch Max", 1.0f, new ConfigDescription("Sex Robot (R2) Pitch Max", new AcceptableValueRange<float>(0.5f, 1.0f)));
            toggleSerialPortConnection = Config.Bind("Sex Robot Connection", "Connect/Disconnect Sex Robot Hotkey", new KeyboardShortcut(KeyCode.S, KeyCode.LeftShift));
            (serialPortConfig = Config.Bind("Sex Robot Connection", "Serial Port For Sex Robot", serialPorts[2], new ConfigDescription("SerialPorts", new AcceptableValueList<string>(serialPorts)))).SettingChanged += (s, e) => { UpdateSerialPort(); };
            sexRobotUpdateFrequencyConfig = Config.Bind("Sex Robot Connection", "Sex Robot Update Frequency", 30.0f, new ConfigDescription("SexRobotUpdateFrequencies", new AcceptableValueRange<float>(1.0f, 120.0f)));
            serialPortStatus = Config.Bind("Sex Robot Connection", "Serial Port Status Information", "Serial Port is not connected.");
            serialPortStatus.Value = serialPortConfig.Value + " port is disconnected.";
            (serialPortConnected = Config.Bind("Sex Robot Connection", "Connection To Sex Robot Via Serial Port", false)).SettingChanged += (s, e) => { UpdateSerialPortConnection(); };
            diagnosticsConfig = Config.Bind("General", "BepInEx Debug Console Output", false);

            if (serialPortConnected.Value)
            {
                UpdateSerialPortConnection();
            }
        }

        // Hook method to grab the HScene instance from
        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "SetStartVoice")]
        public static void HScene_SetStartVoice(HScene __instance)
        {
            hScene = __instance;
        }

        // Hook method to grab the HScene animation name from
        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "ChangeAnimation")]
        private static void HScene_PreChangeAnimation(HScene.AnimationListInfo _info)
        {
            animationChanged = true;

            animationName = _info.nameAnimation;

            updateRobotPosition = false;
        }

        // Hook method to inject UI buttons into the main Honey Select 2 config menu
        [HarmonyPostfix, HarmonyPatch(typeof(Config.ConfigWindow), "Initialize")]
        private static void SetupUIButtons(Config.ConfigWindow __instance, ref Button[] ___buttons)
        {
            // Get main button to instantiate in order to create our new buttons
            Transform btnTitle = __instance.transform.FindLoop("btnTitle").transform;

            // Create connect robot button by instantiating main button, changing it's name, text label, and adding a new listener to handle click events
            buttonConnectRobot = Instantiate(btnTitle, btnTitle.parent);
            buttonConnectRobot.name = "btnConnectRobot";
            buttonConnectRobotText = buttonConnectRobot.GetComponentInChildren<Text>();
            buttonConnectRobotText.text = "Connect Robot";
            buttonConnectRobotText.fontSize = 18;
            Button newButton = buttonConnectRobot.GetComponentInChildren<Button>();
            newButton.onClick = new Button.ButtonClickedEvent();
            newButton.onClick.AddListener(() =>
            {
                buttonConnectRobotClicked = true;
            });

            // Create disconnect robot button by instantiating main button, changing it's name, text label, and adding a new listener to handle click events
            buttonDisconnectRobot = Instantiate(btnTitle, btnTitle.parent);
            buttonDisconnectRobot.name = "btnDisconnectRobot";
            buttonDisconnectRobotText = buttonDisconnectRobot.GetComponentInChildren<Text>();
            buttonDisconnectRobotText.text = "Disconnect Robot";
            buttonDisconnectRobotText.fontSize = 18;
            newButton = buttonDisconnectRobot.GetComponentInChildren<Button>();
            newButton.onClick = new Button.ButtonClickedEvent();
            newButton.onClick.AddListener(() =>
            {
                buttonDisconnectRobotClicked = true;
            });

            // Create robot stroke multiplier increase button by instantiating main button, changing it's name, text label, and adding a new listener to handle click events
            buttonStrokeMultiplierIncrease = Instantiate(btnTitle, btnTitle.parent);
            buttonStrokeMultiplierIncrease.name = "btnStrokeMultiplierIncrease";
            buttonStrokeMultiplierIncreaseText = buttonStrokeMultiplierIncrease.GetComponentInChildren<Text>();
            buttonStrokeMultiplierIncreaseText.text = "Stroke Multiplier +";
            buttonStrokeMultiplierIncreaseText.fontSize = 18;
            newButton = buttonStrokeMultiplierIncrease.GetComponentInChildren<Button>();
            newButton.onClick = new Button.ButtonClickedEvent();
            newButton.onClick.AddListener(() =>
            {
                buttonStrokeMultiplierIncreaseClicked = true;
            });

            // Create robot stroke multiplier decrease button by instantiating main button, changing it's name, text label, and adding a new listener to handle click events
            buttonStrokeMultiplierDecrease = Instantiate(btnTitle, btnTitle.parent);
            buttonStrokeMultiplierDecrease.name = "btnStrokeMultiplierDecrease";
            buttonStrokeMultiplierDecreaseText = buttonStrokeMultiplierDecrease.GetComponentInChildren<Text>();
            buttonStrokeMultiplierDecreaseText.text = "Stroke Multiplier -";
            buttonStrokeMultiplierDecreaseText.fontSize = 18;
            newButton = buttonStrokeMultiplierDecrease.GetComponentInChildren<Button>();
            newButton.onClick = new Button.ButtonClickedEvent();
            newButton.onClick.AddListener(() =>
            {
                buttonStrokeMultiplierDecreaseClicked = true;
            });
        }

        public void UpdateSerialPort()
        {
            serialPortConnected.Value = false;

            Logger.LogInfo("Serial COM port changed to: " + serialPortConfig.Value);

            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    try
                    {
                        // Close the serial port connection
                        serialPort.Close();

                        Logger.LogInfo("Serial port " + serialPort.PortName + " has been disconnected.");
                    }
                    catch (Exception e)
                    {
                        Logger.LogInfo("Error: " + e.ToString());
                    }
                }
            }

            serialPortStatus.Value = serialPortConfig.Value + " port is disconnected.";
        }

        public void UpdateSerialPortConnection()
        {
            // Disconnect serial port if currently connected
            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    try
                    {
                        // Close the serial port connection
                        serialPort.Close();

                        serialPortStatus.Value = serialPort.PortName + " port is disconnected.";

                        Logger.LogInfo("Serial port " + serialPort.PortName + " has been disconnected.");
                    }
                    catch (Exception e)
                    {
                        serialPortStatus.Value = serialPort.PortName + " port is disconnected.";

                        Logger.LogInfo("Serial port " + serialPort.PortName + " has been disconnected.");

                        Logger.LogInfo("Error: " + e.ToString());
                    }
                }
            }

            // Connect to serial port
            if (serialPortConnected.Value)
            {
                // Setup COM port based on updated config selection
                serialPort = new SerialPort("\\\\.\\" + serialPortConfig.Value, 115200);

                try
                {
                    // Open the serial port connection
                    serialPort.Open();

                    if (serialPort.IsOpen)
                    {
                        serialPortStatus.Value = "Connected to serial port " + serialPortConfig.Value + ".";

                        serialPortConnected.Value = true;

                        Logger.LogInfo("Connected to serial port " + serialPort.PortName + ".");
                    }
                    else
                    {
                        serialPortStatus.Value = "Error connecting to serial port " + serialPortConfig.Value + ".";

                        serialPortConnected.Value = false;

                        Logger.LogInfo("Error connecting to serial port " + serialPort.PortName + ".");
                    }
                }
                catch (Exception e)
                {
                    serialPortStatus.Value = "Error connecting to serial port " + serialPortConfig.Value + ".";

                    serialPortConnected.Value = false;

                    Logger.LogInfo("Error: " + e.ToString());
                }
            }
        }

        public async Task UpdateConnectRobotButton()
        {
            await Task.Run(async () =>
            {
                if (serialPort.IsOpen)
                {
                    buttonConnectRobotText.text = "Connected.";
                }
                else
                {
                    buttonConnectRobotText.text = "Can't Connect.";
                }
                await Task.Delay(1000);
                buttonConnectRobotText.text = "Connect Robot";
            });
        }

        public async Task UpdateDisconnectRobotButton()
        {
            await Task.Run(async () =>
            {
                if (!serialPort.IsOpen)
                {
                    buttonDisconnectRobotText.text = "Disconnected.";
                }
                else
                {
                    buttonDisconnectRobotText.text = "Can't Disconnect.";
                }
                await Task.Delay(1000);
                buttonDisconnectRobotText.text = "Disconnect Robot";
            });
        }

        public async Task UpdateStrokeMultiplierIncreaseButton()
        {
            await Task.Run(async () =>
            {
                buttonStrokeMultiplierIncreaseText.text = robotL0Multiplier.Value.ToString();
                await Task.Delay(1000);
                buttonStrokeMultiplierIncreaseText.text = "Stroke Multiplier +";
            });
        }

        public async Task UpdateStrokeMultiplierDecreaseButton()
        {
            await Task.Run(async () =>
            {
                buttonStrokeMultiplierDecreaseText.text = robotL0Multiplier.Value.ToString();
                await Task.Delay(1000);
                buttonStrokeMultiplierDecreaseText.text = "Stroke Multiplier -";
            });
        }

        public void Update()
        {
            // Check if connect robot button was clicked
            if (buttonConnectRobotClicked)
            {
                buttonConnectRobotClicked = false;

                if (serialPortConnected.Value)
                {
                    UpdateSerialPortConnection();
                }
                else
                {
                    serialPortConnected.Value = true;
                }

                Task task = UpdateConnectRobotButton();
            }

            // Check if connect robot button was clicked
            if (buttonDisconnectRobotClicked)
            {
                buttonDisconnectRobotClicked = false;

                if (!serialPortConnected.Value)
                {
                    UpdateSerialPortConnection();
                }
                else
                {
                    serialPortConnected.Value = false;
                }

                Task task = UpdateDisconnectRobotButton();
            }

            // Check if increase stroke multiplier button was clicked
            if (buttonStrokeMultiplierIncreaseClicked)
            {
                buttonStrokeMultiplierIncreaseClicked = false;

                robotL0Multiplier.Value += robotL0MultiplierStepValue.Value;

                Task task = UpdateStrokeMultiplierIncreaseButton();

                Logger.LogInfo("Stroke multiplier: " + robotL0Multiplier.Value);
            }

            // Check if decrease stroke multiplier button was clicked
            if (buttonStrokeMultiplierDecreaseClicked)
            {
                buttonStrokeMultiplierDecreaseClicked = false;

                robotL0Multiplier.Value -= robotL0MultiplierStepValue.Value;

                Task task = UpdateStrokeMultiplierDecreaseButton();

                Logger.LogInfo("Stroke multiplier: " + robotL0Multiplier.Value);
            }

            // Check if increase stroke multiplier hotkey was pressed
            if (strokeLengthMultiplierIncrease.Value.IsDown())
            {
                robotL0Multiplier.Value += robotL0MultiplierStepValue.Value;

                Task task = UpdateStrokeMultiplierIncreaseButton();

                Logger.LogInfo("Stroke multiplier: " + robotL0Multiplier.Value);
            }

            // Check if decrease stroke multiplier hotkey was pressed
            if (strokeLengthMultiplierDecrease.Value.IsDown())
            {
                robotL0Multiplier.Value -= robotL0MultiplierStepValue.Value;

                Task task = UpdateStrokeMultiplierDecreaseButton();

                Logger.LogInfo("Stroke multiplier: " + robotL0Multiplier.Value);
            }

            // Check if serial port connection toggle hotkey was pressed and toggle the serial port on/off if so
            if (toggleSerialPortConnection.Value.IsDown())
            {
                bool connectSerialPort = false;

                if (serialPort != null)
                {
                    if (serialPort.IsOpen)
                    {
                        try
                        {
                            serialPortConnected.Value = false;

                            serialPortStatus.Value = serialPort.PortName + " port is disconnected.";

                            // Close the serial port connection
                            serialPort.Close();

                            Task task = UpdateDisconnectRobotButton();

                            Logger.LogInfo("Serial port " + serialPort.PortName + " has been disconnected.");
                        }
                        catch (Exception e)
                        {
                            serialPortStatus.Value = serialPort.PortName + " port is disconnected.";

                            Task task = UpdateDisconnectRobotButton();

                            Logger.LogInfo("Serial port " + serialPort.PortName + " has been disconnected.");

                            Logger.LogInfo("Error: " + e.ToString());
                        }
                    }
                    else
                    {
                        connectSerialPort = true;
                    }
                }
                else
                {
                    connectSerialPort = true;
                }

                if (connectSerialPort)
                {
                    try
                    {
                        // Setup COM port based on config selection
                        serialPort = new SerialPort("\\\\.\\" + serialPortConfig.Value, 115200);

                        // Open the serial port connection
                        serialPort.Open();

                        if (serialPort.IsOpen)
                        {
                            serialPortConnected.Value = true;

                            serialPortStatus.Value = "Connected to serial port " + serialPortConfig.Value + ".";

                            Task task = UpdateConnectRobotButton();

                            Logger.LogInfo("Connected to serial port " + serialPort.PortName + ".");
                        }
                        else
                        {
                            serialPortStatus.Value = "Error connecting to serial port " + serialPort.PortName + ".";

                            Task task = UpdateDisconnectRobotButton();

                            Logger.LogInfo("Error connecting to serial port " + serialPort.PortName + ".");
                        }
                    }
                    catch (Exception e)
                    {
                        serialPortStatus.Value = "Error connecting to serial port " + serialPort.PortName + ".";

                        Task task = UpdateDisconnectRobotButton();

                        Logger.LogInfo("Error connecting to serial port " + serialPort.PortName + ".");

                        Logger.LogInfo("Error: " + e.ToString());
                    }
                }
            }

            // Return if not in an HScene
            if (hScene == null)
            {
                return;
            }

            // Get ms elapsed since current stopwatch interval
            float msElapsed = sw.ElapsedMilliseconds;

            // If the ms elapsed is greater than the period based on the robot's update frequency then
            // stop the stopwatch, call the robot update function, and restart the stopwatch
            if (msElapsed >= (1000.0 / sexRobotUpdateFrequencyConfig.Value))
            {
                sw.Stop();

                if (animationFemaleTargetDictionary.ContainsKey(animationName))
                {
                    if (animationChanged)
                    {
                        // Find all the female and male chara in the current HScene
                        females = hScene.GetFemales().Where(female => female != null).ToArray();
                        males = hScene.GetMales().Where(male => male != null).ToArray();

                        if (diagnosticsConfig.Value)
                        {
                            Logger.LogInfo("Animation: " + animationName);
                            Logger.LogInfo("Females found: " + females.Length.ToString());
                            Logger.LogInfo("Males found: " + males.Length.ToString());
                        }

                        if (males.Length == 1 && females.Length > 0)
                        {
                            // Find/set all the male transforms needed for the calculations here so it doesn't have to happen each FixedUpdate()
                            // Get the base of the male's penis
                            malePenisBase = males[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cm_J_dan100_00").FirstOrDefault();

                            // Get the tip of the male's penis
                            malePenisTip = males[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cm_J_dan109_00").FirstOrDefault();

                            // Get the male's penis left ball
                            malePenisLeftBall = males[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cm_J_dan_f_L").FirstOrDefault();

                            // Get the male's penis right ball
                            malePenisRightBall = males[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cm_J_dan_f_R").FirstOrDefault();

                            // Lookup in the animation dictionary the female target type for this current animation
                            femaleTargetType femaleTargetTypeCurrent;
                            animationFemaleTargetDictionary.TryGetValue(animationName, out femaleTargetTypeCurrent);
                            currentFemaleTargetType = femaleTargetTypeCurrent;

                            if (currentFemaleTargetType == femaleTargetType.VAGINAL)
                            {
                                // Find/set all the female transforms needed for the VAGINAL calculations here so it doesn't have to happen each FixedUpdate()
                                // Get the base of the female's hip
                                femaleHip = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kosi02").FirstOrDefault();

                                // Get the base of the female's vagina
                                femaleVagina = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kokan").FirstOrDefault();

                                // Get the base of the female's anus
                                femaleAnus = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Ana").FirstOrDefault();
                            }
                            else if (currentFemaleTargetType == femaleTargetType.ANAL)
                            {
                                // Find/set all the female transforms needed for the ANAL calculations here so it doesn't have to happen each FixedUpdate()
                                // Get the base of the female's hip
                                femaleHip = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kosi02").FirstOrDefault();

                                // Get the base of the female's vagina
                                femaleVagina = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kokan").FirstOrDefault();

                                // Get the base of the female's anus
                                femaleAnus = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Ana").FirstOrDefault();
                            }
                            else if (currentFemaleTargetType == femaleTargetType.ORAL)
                            {
                                // Find/set all the female transforms needed for the ORAL calculations here so it doesn't have to happen each FixedUpdate()
                                // Get the female's mouth upper lips
                                femaleMouthLipsUpper = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Mouthup").FirstOrDefault();

                                // Get the female's mouth lower lips
                                femaleMouthLipsLower = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_MouthLow").FirstOrDefault();

                                // Get the female's mouth left
                                femaleMouthLeft = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Mouth_L").FirstOrDefault();

                                // Get the female's mouth right
                                femaleMouthRight = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Mouth_R").FirstOrDefault();
                            }
                            else if (currentFemaleTargetType == femaleTargetType.BREASTS)
                            {
                                // Find/set all the female transforms needed for the BREASTS calculations here so it doesn't have to happen each FixedUpdate()
                                // Get the female's middle of the breasts left
                                femaleMiddleBreastsLeft = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Mune02_L").FirstOrDefault();

                                // Get the female's middle of the breasts right
                                femaleMiddleBreastsRight = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Mune02_R").FirstOrDefault();

                                // Get the female's breasts center on the chest
                                femaleBreasts = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Mune00").FirstOrDefault();
                            }
                            else if (currentFemaleTargetType == femaleTargetType.LEFTHAND)
                            {
                                // Find/set all the female transforms needed for the LEFTHAND calculations here so it doesn't have to happen each FixedUpdate()
                                // Get the female's left hand's middle finger
                                femaleMiddleFingerLeft = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "N_Middle_L").FirstOrDefault();

                                // Get the female's left hand's ring fingers
                                femaleRingFingerLeft = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "N_Ring_L").FirstOrDefault();

                                // Get the female's left hand's center
                                femaleHandLeft = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "N_Hand_L").FirstOrDefault();
                            }
                            else if (currentFemaleTargetType == femaleTargetType.RIGHTHAND)
                            {
                                // Find/set all the female transforms needed for the RIGHTHAND calculations here so it doesn't have to happen each FixedUpdate()
                                // Get the female's right hand's middle finger
                                femaleMiddleFingerRight = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "N_Middle_R").FirstOrDefault();

                                // Get the female's right hand's ring fingers
                                femaleRingFingerRight = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "N_Ring_R").FirstOrDefault();

                                // Get the female's right hand's center
                                femaleHandRight = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "N_Hand_R").FirstOrDefault();
                            }
                            else if (currentFemaleTargetType == femaleTargetType.INTERCRURAL)
                            {
                                // Find/set all the female transforms needed for the INTERCRURAL calculations here so it doesn't have to happen each FixedUpdate()
                                // Get the base of the female's hip
                                femaleHip = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kosi02").FirstOrDefault();

                                // Get the base of the female's vagina
                                femaleVagina = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kokan").FirstOrDefault();

                                // Get the base of the female's anus
                                femaleAnus = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Ana").FirstOrDefault();
                            }
                            else if (currentFemaleTargetType == femaleTargetType.VAGINALSWAP)
                            {
                                if (females.Length == 2)
                                {
                                    // Find/set all the female transforms needed for the VAGINALSWAP calculations here so it doesn't have to happen each FixedUpdate()
                                    // Get the base of the female's hip
                                    femaleHip = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kosi02").FirstOrDefault();

                                    // Get the base of the female's vagina
                                    femaleVagina = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kokan").FirstOrDefault();

                                    // Get the base of the female's anus
                                    femaleAnus = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Ana").FirstOrDefault();
                                }
                                else
                                {
                                    Logger.LogInfo("Error: The current HScene (swap) doesn't have 2 females.");
                                }
                            }
                            else if (currentFemaleTargetType == femaleTargetType.ORALSWAP)
                            {
                                if (females.Length == 2)
                                {
                                    // Find/set all the female transforms needed for the ORALSWAP calculations here so it doesn't have to happen each FixedUpdate()
                                    // Get the female's mouth upper lips
                                    femaleMouthLipsUpper = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Mouthup").FirstOrDefault();

                                    // Get the female's mouth lower lips
                                    femaleMouthLipsLower = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_MouthLow").FirstOrDefault();

                                    // Get the female's mouth left
                                    femaleMouthLeft = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Mouth_L").FirstOrDefault();

                                    // Get the female's mouth right
                                    femaleMouthRight = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Mouth_R").FirstOrDefault();
                                }
                                else
                                {
                                    Logger.LogInfo("Error: The current HScene (swap) doesn't have 2 females.");
                                }
                            }
                            else if (currentFemaleTargetType == femaleTargetType.RIGHTHANDSWAP)
                            {
                                if (females.Length == 2)
                                {
                                    // Find/set all the female transforms needed for the RIGHTHANDSWAP calculations here so it doesn't have to happen each FixedUpdate()
                                    // Get the female's right hand's middle finger
                                    femaleMiddleFingerRight = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "N_Middle_R").FirstOrDefault();

                                    // Get the female's right hand's ring fingers
                                    femaleRingFingerRight = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "N_Ring_R").FirstOrDefault();

                                    // Get the female's right hand's center
                                    femaleHandRight = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "N_Hand_R").FirstOrDefault();
                                }
                                else
                                {
                                    Logger.LogInfo("Error: The current HScene (swap) doesn't have 2 females.");
                                }
                            }
                            else if (currentFemaleTargetType == femaleTargetType.INTERCRURALSWAP)
                            {
                                if (females.Length == 2)
                                {
                                    // Find/set all the female transforms needed for the INTERCRURALSWAP calculations here so it doesn't have to happen each FixedUpdate()
                                    // Get the base of the female's hip
                                    femaleHip = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kosi02").FirstOrDefault();

                                    // Get the base of the female's vagina
                                    femaleVagina = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kokan").FirstOrDefault();

                                    // Get the base of the female's anus
                                    femaleAnus = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Ana").FirstOrDefault();
                                }
                                else
                                {
                                    Logger.LogInfo("Error: The current HScene (swap) doesn't have 2 females.");
                                }
                            }

                            if (diagnosticsConfig.Value)
                            {
                                Logger.LogInfo("Current animation: " + animationName);
                            }

                            animationChanged = false;

                            updateRobotPosition = true;
                        }
                        else
                        {
                            updateRobotPosition = false;

                            Logger.LogInfo("Error: The current HScene doesn't have 1 male and at least 1 female.");
                        }

                        autoRangeMin = 1.0f;
                        autoRangeMax = 0.0f;
                    }

                    if (updateRobotPosition)
                    {
                        // Setup T-code reference coordinate system
                        // X(L0) is up/down in reference to the selected male's penis vector and is positive up
                        // Y(L1) is toward/away orthogonal to the selected male's penis vector and is positive away
                        // Z(L2) is left/right orthogonal to the selected male's penis vector and is positive left
                        // RX(R0) is positive according to the right hand rule around X(L0)
                        // RY(R1) is positive according to the right hand rule around Y(L1)
                        // RZ(R2) is positive according to the right hand rule around Z(L2)

                        // Calculate the center point between the two penis's balls
                        Vector3 malePenisBallsCenterPoint = (malePenisLeftBall.position + malePenisRightBall.position) / 2.0f;

                        // Calculate male's penis length
                        float malePenisLength = Vector3.Distance(malePenisBase.position, malePenisTip.position);

                        // Vector from the selected male's penis's base to tip
                        Vector3 malePenisXAxis = malePenisTip.position - malePenisBase.position;

                        // Use the male's penis's base and the male's penis's balls center point to establish the Z reference axis
                        Vector3 malePenisZAxis = Vector3.Cross(malePenisXAxis, malePenisBallsCenterPoint - malePenisBase.position);
                        malePenisZAxis = (malePenisXAxis.magnitude / malePenisZAxis.magnitude) * malePenisZAxis;

                        // Use the reference X and Z axes to establish the orthogonal Y axis
                        Vector3 malePenisYAxis = Vector3.Cross(malePenisXAxis, malePenisZAxis);
                        malePenisYAxis = (malePenisXAxis.magnitude / malePenisYAxis.magnitude) * malePenisYAxis;

                        Vector3 femaleTargetXAxis = new Vector3(0.0f, 0.0f, 0.0f);
                        Vector3 femaleTargetZAxis = new Vector3(0.0f, 0.0f, 0.0f);
                        Vector3 femaleTargetYAxis = new Vector3(0.0f, 0.0f, 0.0f);
                        Vector3 femaleTargetToMalePenisBase = new Vector3(0.0f, 0.0f, 0.0f);

                        if (currentFemaleTargetType == femaleTargetType.VAGINAL || currentFemaleTargetType == femaleTargetType.VAGINALSWAP)
                        {
                            // Vector from the selected female's vagina to hip
                            femaleTargetXAxis = femaleHip.position - femaleVagina.position;

                            // Use the female's vagina and hip vector and the female's anus to establish the Z reference axis
                            femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleAnus.position - femaleVagina.position);
                            femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                            // Use the reference X and Z axes to establish the orthogonal Y axis
                            femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetZAxis);
                            femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                            // Vector from the female's vagina to the male's penis's base
                            femaleTargetToMalePenisBase = femaleVagina.position - malePenisBase.position;
                        }
                        else if (currentFemaleTargetType == femaleTargetType.ANAL)
                        {
                            // Vector from the selected female's vagina to hip
                            femaleTargetXAxis = femaleHip.position - femaleVagina.position;

                            // Use the female's vagina and hip vector and the female's anus to establish the Z reference axis
                            femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleAnus.position - femaleVagina.position);
                            femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                            // Use the reference X and Z axes to establish the orthogonal Y axis
                            femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetZAxis);
                            femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                            // Vector from the female's vagina to the male's penis's base
                            femaleTargetToMalePenisBase = femaleAnus.position - malePenisBase.position;
                        }
                        else if (currentFemaleTargetType == femaleTargetType.ORAL || currentFemaleTargetType == femaleTargetType.ORALSWAP)
                        {
                            // Calculate the center point between the two lips of the mouth
                            Vector3 femaleMouthLipsCenterPoint = (femaleMouthLipsUpper.position + femaleMouthLipsLower.position) / 2.0f;

                            // Calculate the center point between the left and right sides of the mouth
                            Vector3 femaleMouthCenterPoint = (femaleMouthLeft.position + femaleMouthRight.position) / 2.0f;

                            // Vector from the selected female's mouth lips center point to mouth center point
                            femaleTargetXAxis = femaleMouthCenterPoint - femaleMouthLipsCenterPoint;

                            // Use the female's mouth and lips center points vector and the female's mouth to establish the Y reference axis
                            femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleMouthRight.position - femaleMouthCenterPoint);
                            femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                            // Use the reference X and Y axes to establish the orthogonal Z axis
                            femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetYAxis);
                            femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                            // Vector from the female's mouth center point to the male's penis's base
                            femaleTargetToMalePenisBase = femaleMouthCenterPoint - malePenisBase.position;
                        }
                        else if (currentFemaleTargetType == femaleTargetType.BREASTS)
                        {
                            // Calculate the center point between the two middle breasts
                            Vector3 femaleMiddleBreastsCenterPoint = (femaleMiddleBreastsLeft.position + femaleMiddleBreastsRight.position) / 2.0f;

                            // Vector from the selected female's middle breasts to breasts on chest
                            femaleTargetYAxis = femaleBreasts.position - femaleMiddleBreastsCenterPoint;

                            // Use the female's middle breasts and breasts on chest vector and the female's middle breasts right to establish the X reference axis
                            femaleTargetXAxis = Vector3.Cross(femaleTargetYAxis, femaleMiddleBreastsRight.position - femaleMiddleBreastsCenterPoint);
                            femaleTargetXAxis = (femaleTargetYAxis.magnitude / femaleTargetXAxis.magnitude) * femaleTargetXAxis;

                            // Use the reference X and Y axes to establish the orthogonal Z axis
                            femaleTargetZAxis = Vector3.Cross(femaleTargetYAxis, femaleTargetXAxis);
                            femaleTargetZAxis = (femaleTargetYAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                            // Vector from the female's breasts center point to the male's penis's base
                            femaleTargetToMalePenisBase = femaleMiddleBreastsCenterPoint - malePenisBase.position;
                        }
                        else if (currentFemaleTargetType == femaleTargetType.LEFTHAND)
                        {
                            // Vector from the selected female's middle and ring fingers
                            femaleTargetXAxis = femaleMiddleFingerLeft.position - femaleRingFingerLeft.position;

                            // Use the female's middle and ring fingers vector and the female's hand to establish the Y reference axis
                            femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleHandLeft.position - femaleMiddleFingerLeft.position);
                            femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                            // Use the reference X and Y axes to establish the orthogonal Z axis
                            femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetZAxis);
                            femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                            // Vector from the female's hand to the male's penis's base
                            femaleTargetToMalePenisBase = femaleHandLeft.position - malePenisBase.position;
                        }
                        else if (currentFemaleTargetType == femaleTargetType.RIGHTHAND || currentFemaleTargetType == femaleTargetType.RIGHTHANDSWAP)
                        {
                            // Vector from the selected female's middle and ring fingers
                            femaleTargetXAxis = femaleMiddleFingerRight.position - femaleRingFingerRight.position;

                            // Use the female's middle and ring fingers vector and the female's hand to establish the Y reference axis
                            femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleHandRight.position - femaleMiddleFingerRight.position);
                            femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                            // Use the reference X and Y axes to establish the orthogonal Z axis
                            femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetZAxis);
                            femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                            // Vector from the female's hand to the male's penis's base
                            femaleTargetToMalePenisBase = femaleHandRight.position - malePenisBase.position;
                        }
                        else if (currentFemaleTargetType == femaleTargetType.INTERCRURAL || currentFemaleTargetType == femaleTargetType.INTERCRURALSWAP)
                        {
                            // Vector from the selected female's vagina to anus
                            femaleTargetXAxis = femaleVagina.position - femaleAnus.position;

                            // Use the female's vagina and hip vector and the female's anus to establish the Z reference axis
                            femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleHip.position - femaleVagina.position);
                            femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                            // Use the reference X and Z axes to establish the orthogonal Y axis
                            femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetZAxis);
                            femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                            // Vector from the female's vagina to the male's penis's base
                            femaleTargetToMalePenisBase = femaleVagina.position - malePenisBase.position;
                        }

                        // Calculate X(L0) for robot based on the reference X axis and the vector from the female's vagina's labia trigger to the male's penis's base collider
                        float robotL0 = Vector3.Dot(malePenisXAxis, femaleTargetToMalePenisBase) / (malePenisXAxis.magnitude * malePenisXAxis.magnitude);

                        // Calculate Y(L1) for robot based on the reference Y axis and the vector from the female's vagina's labia trigger to the male's penis's base collider
                        float robotL1 = 0.5f + Vector3.Dot(malePenisYAxis, femaleTargetToMalePenisBase) / (malePenisYAxis.magnitude * malePenisYAxis.magnitude);

                        // Calculate Z(L2) for robot based on the reference Z axis and the vector from the female's vagina's labia trigger to the male's penis's base collider
                        float robotL2 = 0.5f + Vector3.Dot(malePenisZAxis, femaleTargetToMalePenisBase) / (malePenisZAxis.magnitude * malePenisZAxis.magnitude);

                        // Determine the coordinate system orientation between the male and female, used for calculating the R0 rotation
                        bool coordinateAxesMatch = true;

                        if (Vector3.Dot(malePenisZAxis, femaleTargetZAxis) < 0)
                        {
                            coordinateAxesMatch = false;
                        }

                        // Calculate RX(R0) for robot based on the angle between reference Z axis and the female's vagina to anus vector
                        float robotR0Angle = Vector3.Angle(malePenisZAxis, femaleTargetZAxis);

                        // Calculate RY(R1) for robot based on the reference Z axis and the vector from the female's vagina's labia to vagina triggers
                        float robotR1Angle = -(90.0f - Vector3.Angle(malePenisZAxis, femaleTargetXAxis));

                        if (!coordinateAxesMatch)
                        {
                            robotR0Angle = 180.0f - robotR0Angle;
                            robotR1Angle *= -1.0f;
                        }

                        float robotR0 = 0.5f + robotR0Angle / 180.0f;

                        float robotR1 = 0.5f + robotR1Angle / 180.0f;

                        // Calculate RZ(R2) for robot based on the reference Y axis and the vector from the female's vagina's labia to vagina triggers
                        float robotR2Angle = -(90.0f - Vector3.Angle(malePenisYAxis, femaleTargetXAxis));

                        float robotR2 = 0.5f + robotR2Angle / 180.0f;

                        // Calculate automatic range values
                        if (robotL0 >= 0.0f && robotL0 <= 1.0f)
                        {
                            if (robotL0 < autoRangeMin)
                            {
                                autoRangeMin = robotL0;
                            }

                            if (robotL0 > autoRangeMax)
                            {
                                autoRangeMax = robotL0;
                            }
                        }

                        // Get the automatic range midpoint
                        autoRangeMid = (autoRangeMin + autoRangeMax) / 2.0f;

                        // Caclulate modified robotL0
                        robotL0 = 0.5f + (robotL0 - autoRangeMid) * robotL0Multiplier.Value;

                        // Formulate T-Code command string
                        string command = "L0" + GenerateTCode(robotL0, robotL0Min.Value, robotL0Max.Value) + "\n";
                        command += "L1" + GenerateTCode(robotL1, robotL1Min.Value, robotL1Max.Value) + "\n";
                        command += "L2" + GenerateTCode(robotL2, robotL2Min.Value, robotL2Max.Value) + "\n";
                        command += "R0" + GenerateTCode(robotR0, robotR0Min.Value, robotR0Max.Value) + "\n";
                        command += "R1" + GenerateTCode(robotR1, robotR1Min.Value, robotR1Max.Value) + "\n";
                        command += "R2" + GenerateTCode(robotR2, robotR2Min.Value, robotR2Max.Value);

                        if (diagnosticsConfig.Value)
                        {
                            Logger.LogInfo("malePenisBase: " + malePenisBase.position.x.ToString() + ", " + malePenisBase.position.y.ToString() + ", " + malePenisBase.position.z.ToString());
                            Logger.LogInfo("malePenisTip: " + malePenisTip.position.x.ToString() + ", " + malePenisTip.position.y.ToString() + ", " + malePenisTip.position.z.ToString());
                            Logger.LogInfo("malePenisLeftBall: " + malePenisLeftBall.position.x.ToString() + ", " + malePenisLeftBall.position.y.ToString() + ", " + malePenisLeftBall.position.z.ToString());
                            Logger.LogInfo("malePenisRightBall: " + malePenisRightBall.position.x.ToString() + ", " + malePenisRightBall.position.y.ToString() + ", " + malePenisRightBall.position.z.ToString());
                            Logger.LogInfo("malePenisBallsCenterPoint: " + malePenisBallsCenterPoint.x.ToString() + ", " + malePenisBallsCenterPoint.y.ToString() + ", " + malePenisBallsCenterPoint.z.ToString());
                            Logger.LogInfo("malePenisLength: " + malePenisLength.ToString());
                            Logger.LogInfo("malePenisXAxis: " + malePenisXAxis.x.ToString() + ", " + malePenisXAxis.y.ToString() + ", " + malePenisXAxis.z.ToString());
                            Logger.LogInfo("malePenisZAxis: " + malePenisZAxis.x.ToString() + ", " + malePenisZAxis.y.ToString() + ", " + malePenisZAxis.z.ToString());
                            Logger.LogInfo("malePenisYAxis: " + malePenisYAxis.x.ToString() + ", " + malePenisYAxis.y.ToString() + ", " + malePenisYAxis.z.ToString());
                            Logger.LogInfo("Robot L0 Multiplier: " + robotL0Multiplier.Value);
                            Logger.LogInfo("Robot L0: " + robotL0);
                            Logger.LogInfo("Robot L1: " + robotL1);
                            Logger.LogInfo("Robot L2: " + robotL2);
                            Logger.LogInfo("Robot R0: " + robotR0);
                            Logger.LogInfo("Robot R1: " + robotR1);
                            Logger.LogInfo("Robot R2: " + robotR2);
                            Logger.LogInfo("Robot R0 Angle: " + robotR0Angle);
                            Logger.LogInfo("Robot R1 Angle: " + robotR1Angle);
                            Logger.LogInfo("Robot R2 Angle: " + robotR2Angle);
                            Logger.LogInfo("T-Code Command: \n" + command);
                            Logger.LogInfo("autoRangeMin: " + autoRangeMin);
                            Logger.LogInfo("autoRangeMid: " + autoRangeMid);
                            Logger.LogInfo("autoRangeMax: " + autoRangeMax);
                            Logger.LogInfo("robotL0 percent: " + (((robotL0 - autoRangeMin) / (autoRangeMax - autoRangeMin)) * 100.0f));
                        }

                        // Only update the sex robot's position/servos
                        if (robotL0 >= 0.0f && robotL0 <= 1.0f)
                        {
                            try
                            {
                                if (serialPort != null)
                                {
                                    // If serial port is open then send the command to the robot
                                    if (serialPort.IsOpen)
                                    {
                                        serialPort.WriteLine(command);

                                        if (diagnosticsConfig.Value)
                                        {
                                            Logger.LogInfo("Command value sent: " + command);
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.LogInfo("Error: " + e.ToString());
                            }
                        }
                    }
                }

                sw = Stopwatch.StartNew();
            }
        }

        string GenerateTCode(float input, float min, float max)
        {
            if (input > max) input = max;
            if (input < min) input = min;

            input = input * 1000;

            string output;

            if (input >= 999f)
            {
                output = "999";
            }
            else if (input >= 1f)
            {
                output = input.ToString("000");
            }
            else
            {
                output = "000";
            }

            return output;
        }

    }
}

