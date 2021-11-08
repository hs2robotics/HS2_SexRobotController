using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIChara;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.IO.Ports;

namespace HS2_SexRobotController
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]

    public class HS2_SexRobotController : BaseUnityPlugin
    {
        public const string pluginGUID = "hs2robotics.HS2SexRobotController";
        public const string pluginName = "HS2_SexRobotController";
        public const string pluginVersion = "1.0";

        public static HScene hScene;
        public bool inHScene = false;
        public ChaControl[] females;
        public ChaControl[] males;
        public static bool animationChanged = false;
        public static string animationName = "";
        Transform malePenisBase;
        Transform malePenisTip;
        Transform malePenisLeftBall;
        Transform malePenisRightBall;
        Transform femaleMouthLipsUpper;
        Transform femaleMouthLipsLower;
        Transform femaleMouthLeft;
        Transform femaleMouthRight;
        Transform femaleHip;
        Transform femaleVagina;
        Transform femaleAnus;
        Transform femaleMiddleBreastsLeft;
        Transform femaleMiddleBreastsRight;
        Transform femaleBreasts;
        Transform femaleMiddleFingerLeft;
        Transform femaleRingFingerLeft;
        Transform femaleHandLeft;
        Transform femaleMiddleFingerRight;
        Transform femaleRingFingerRight;
        Transform femaleHandRight;

        public enum femaleTargetType
        {
            VAGINAL,
            ANAL,
            ORAL,
            BREASTS,
            LEFTHAND,
            RIGHTHAND
        }

        femaleTargetType currentFemaleTargetType;

        public Dictionary<string, femaleTargetType> animationFemaleTargetDictionary = new Dictionary<string, femaleTargetType>
        {
            // Schema: {ANIMATION NAME, FEMALE TARGET(S) TO USE IN REGARD TO MAPPING TO THE MALE'S PENIS TARGET}
            {"Blowjob", femaleTargetType.ORAL},
            {"Handjob", femaleTargetType.LEFTHAND},
            {"Boobjob", femaleTargetType.BREASTS},
            {"Standing Handjob", femaleTargetType.RIGHTHAND},
            {"Irrumatio", femaleTargetType.ORAL},
            {"Chair Handjob", femaleTargetType.LEFTHAND},
            {"Sitting Boobjob", femaleTargetType.BREASTS},
            {"Wall-Trapped Blowjob", femaleTargetType.ORAL},
            {"Desk Handjob", femaleTargetType.RIGHTHAND},
            {"Missionary", femaleTargetType.VAGINAL},
            {"Doggy", femaleTargetType.VAGINAL},
            {"Spooning", femaleTargetType.VAGINAL},
            {"Cowgirl", femaleTargetType.VAGINAL},
            {"Reverse Cowgirl", femaleTargetType.VAGINAL},
            {"Anal Missionary", femaleTargetType.ANAL},
            {"Anal Doggy", femaleTargetType.ANAL},
            {"Standing Behind", femaleTargetType.VAGINAL},
            {"Lifting", femaleTargetType.VAGINAL},
            {"Wall-Facing Behind", femaleTargetType.VAGINAL},
            {"Desk Missionary", femaleTargetType.VAGINAL},
            {"Desk Doggy", femaleTargetType.ANAL},
            {"Against Counter Behind", femaleTargetType.VAGINAL},
            {"Crouch Insertion", femaleTargetType.VAGINAL},
            {"Delivery Table Insert", femaleTargetType.VAGINAL},
            {"Hanging", femaleTargetType.VAGINAL},
            {"Restrained Standing", femaleTargetType.VAGINAL},
            {"Clinging Lifting", femaleTargetType.VAGINAL},
            {"Wall-Pressed Behind", femaleTargetType.VAGINAL},
            {"Mating Press", femaleTargetType.VAGINAL},
            {"All Fours Handjob", femaleTargetType.RIGHTHAND},
            {"Restrained Blowjob", femaleTargetType.ORAL},
            {"Piledriver Rev. Cowgirl", femaleTargetType.VAGINAL},
            {"Male Restrained Stand", femaleTargetType.VAGINAL},
            {"Chair Restraint Sitting", femaleTargetType.VAGINAL}
        };

        private SerialPort serialPort;
        private Stopwatch sw = Stopwatch.StartNew();
        private ConfigEntry<KeyboardShortcut> toggleSerialPortConnection { get; set; }
        public string[] serialPorts = { "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM10"};
        public ConfigEntry<string> serialPortConfig;
        public ConfigEntry<float> sexRobotUpdateFrequencyConfig;
        public ConfigEntry<bool> diagnosticsConfig;
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
        public static bool updateRobotPosition = false;

        public void Awake()
        {
            HarmonyLib.Harmony.CreateAndPatchAll(typeof(HS2_SexRobotController));

            // Setup config file entries used in the in game menu
            // Creates a config file in BepInEx/config named hs2robotics.HS2SexRobotController.cfg
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
            diagnosticsConfig = Config.Bind("General", "Diagnostics/Debug Console Output", false);
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

        public void UpdateSerialPort()
        {
            Logger.LogInfo("Serial COM port changed to: " + serialPortConfig.Value);

            bool connectSerialPort = false;

            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    try
                    {
                        // Close the serial port connection
                        serialPort.Close();

                        Logger.LogInfo("Serial port " + serialPort.PortName + " is closed.");

                        connectSerialPort = true;
                    }
                    catch (Exception e)
                    {
                        Logger.LogInfo("Error: " + e.ToString());
                    }
                }
            }

            // Setup COM port based on updated config selection
            serialPort = new SerialPort("\\\\.\\" + serialPortConfig.Value, 115200);

            // Reconnect since the serial port was already open before the COM port config selection changed
            if (connectSerialPort)
            {
                try
                {
                    // Open the serial port connection
                    serialPort.Open();

                    if (serialPort.IsOpen)
                    {
                        Logger.LogInfo("Serial port " + serialPort.PortName + " is open.");
                    }
                    else
                    {
                        Logger.LogInfo("Serial port " + serialPort.PortName + " is closed.");
                    }
                }
                catch (Exception e)
                {
                    Logger.LogInfo("Error: " + e.ToString());
                }
            }
        }

        public void FixedUpdate()
        {
            if (hScene == null)
            {
                return;
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
                            // Close the serial port connection
                            serialPort.Close();

                            Logger.LogInfo("Serial port " + serialPort.PortName + " is closed.");
                        }
                        catch (Exception e)
                        {
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
                            Logger.LogInfo("Serial port " + serialPort.PortName + " is open.");
                        }
                        else
                        {
                            Logger.LogInfo("Serial port " + serialPort.PortName + " is closed.");
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogInfo("Error: " + e.ToString());
                    }
                }
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

                        if (males.Length == 1 && females.Length == 1)
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

                            Logger.LogInfo("Error: The current HScene does not have exactly 1 female and 1 male.");
                        }
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

                        if (currentFemaleTargetType == femaleTargetType.VAGINAL)
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
                        else if (currentFemaleTargetType == femaleTargetType.ORAL)
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
                        else if (currentFemaleTargetType == femaleTargetType.RIGHTHAND)
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

                        // Formulate T-Code v0.2 command string
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
                        }

                        // Only update the sex robot's position/servos
                        if (robotL0 > 0.0f && robotL0 < 1.0f)
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

