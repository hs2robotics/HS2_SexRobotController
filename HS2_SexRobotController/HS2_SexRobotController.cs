using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIChara;
using BepInEx;
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
        public static bool inHScene = false;
        public static ChaControl[] females;
        public static ChaControl[] males;
        public static bool animationChanged = false;
        public static string animationName = "";
        public static bool updateRobotPosition = false;
        public static List<Transform> femaleTargets;
        public static Transform malePenisTarget;
        public static string malePenisBaseName = "cm_J_dan_s";
        public static string malePenisTipName = "cm_J_dan109_00";
        public static float malePenisLength;
        public static Vector3 autoRangeMin;
        public static Vector3 autoRangeMax;
        public static Dictionary<string, string[]> animationFemaleTargetDictionary = new Dictionary<string, string[]>
        {
            // Schema: {ANIMATION NAME, FEMALE TARGET(S) TO USE IN REGARD TO MAPPING TO THE MALE'S PENIS TARGET}
            {"Blowjob", new string[]{"cf_J_MouthCavity"} },
            {"Handjob", new string[]{"cf_J_Hand_s_L"}},
            {"Boobjob", new string[]{"cf_J_Mune00_s_L", "cf_J_Mune00_s_R"}},
            {"Standing Handjob", new string[]{"cf_J_Hand_s_R"}},
            {"Irrumatio", new string[]{"cf_J_MouthCavity"}},
            {"Chair Handjob", new string[]{"cf_J_Hand_s_L"}},
            {"Sitting Boobjob", new string[]{"cf_J_Mune00_s_L", "cf_J_Mune00_s_R"}},
            {"Desk Handjob", new string[]{"cf_J_Hand_s_R"}},
            {"Missionary", new string[]{"cf_J_Kokan"}},
            {"Doggy", new string[]{"cf_J_Kokan"}},
            {"Spooning", new string[]{"cf_J_Kokan"}},
            {"Cowgirl", new string[]{"cf_J_Kokan"}},
            {"Reverse Cowgirl", new string[]{"cf_J_Kokan"}},
            {"Anal Missionary", new string[]{"cf_J_Ana"}},
            {"Anal Doggy", new string[]{"cf_J_Ana"}},
            {"Standing Behind", new string[]{"cf_J_Kokan"}},
            {"Lifting", new string[]{"cf_J_Kokan"}},
            {"Wall-Facing Behind", new string[]{"cf_J_Kokan"}},
            {"Desk Missionary", new string[]{"cf_J_Kokan"}},
            {"Desk Doggy", new string[]{"cf_J_Ana"}},
            {"Against Counter Behind", new string[]{"cf_J_Kokan"}},
            {"Crouch Insertion", new string[]{"cf_J_Kokan"}},
            {"Hanging", new string[]{"cf_J_Kokan"}},
            {"Restrained Standing", new string[]{"cf_J_Kokan"}},
            {"Clinging Lifting", new string[]{"cf_J_Kokan"}},
            {"Mating Press", new string[]{"cf_J_Kokan"}},
            {"All Fours Handjob", new string[]{"cf_J_Hand_s_R"}},
            {"Restrained Blowjob", new string[]{"cf_J_MouthCavity"}},
            {"Piledriver Rev. Cowgirl", new string[]{"cf_J_Kokan"}},
            {"Male Restrained Stand", new string[]{"cf_J_Kokan"}}
        };
        public static bool diagnostics = true;
        // Make sure you edit the line below and change the COM port number to the one
        // used by your ESP32 driven sex robot
        private SerialPort serialPort = new SerialPort("\\\\.\\COM3", 115200);
        private float robotUpdateFrequency = 50.0f;
        private Stopwatch sw = Stopwatch.StartNew();

        public void Awake()
        {
            HarmonyLib.Harmony.CreateAndPatchAll(typeof(HS2_SexRobotController));

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
        }

        private void SceneManager_sceneUnloaded(Scene scene)
        {
            if (scene.name != "HScene")
                return;

            // Leaving HScene
            inHScene = false;
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name != "HScene")
                return;

            // Entering HScene
            inHScene = true;
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
        }

        public void FixedUpdate()
        {
            // Only proceed if we are currently in an HScene
            if (hScene == null || inHScene == false)
                return;

            // Open and close the serial port connection when Control+K is pressed on the keyboard
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.K))
            {
                try
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
                catch (Exception e)
                {
                    Logger.LogInfo("Error: " + e.ToString());
                }
            }

            // Get ms elapsed since current stopwatch interval
            float msElapsed = sw.ElapsedMilliseconds;

            // If the ms elapsed is greater than the period based on the robot's update frequency then
            // stop the stopwatch, call the robot update function, and restart the stopwatch
            if (msElapsed >= (1000.0 / robotUpdateFrequency))
            {
                sw.Stop();

                if (diagnostics)
                    //Logger.LogInfo("Time taken: " + msElapsed + "ms, Frequency: " + (1000.0 / msElapsed) + "Hz");

                if (animationChanged)
                {
                    if (animationFemaleTargetDictionary.ContainsKey(animationName))
                    {
                        // Find all the female and male chara in the current HScene
                        females = hScene.GetFemales().Where(female => female != null).ToArray();
                        males = hScene.GetMales().Where(male => male != null).ToArray();

                        if (diagnostics)
                        {
                            Logger.LogInfo("Females found: " + females.Length.ToString());
                            Logger.LogInfo("Males found: " + males.Length.ToString());
                        }

                        // Currently only HScenes with 1 male and 1 female are allowed
                        if (females.Length == 1 && males.Length == 1)
                        {
                            femaleTargets = null;

                            femaleTargets = new List<Transform>();

                            string[] femaleTargetNames;

                            animationFemaleTargetDictionary.TryGetValue(animationName, out femaleTargetNames);

                            if (diagnostics)
                                Logger.LogInfo("femaleTargetNames found: " + femaleTargetNames[0]);

                            foreach (string femaleTargetName in femaleTargetNames)
                            {
                                Transform femaleTarget = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == femaleTargetName).FirstOrDefault();

                                if (femaleTarget != null)
                                {
                                    femaleTargets.Add(femaleTarget);

                                    if (diagnostics)
                                        Logger.LogInfo("femaleTarget found: " + femaleTarget.name);
                                }
                            }

                            // Find/use the base of the male's penis for the main target reference
                            malePenisTarget = males[0].GetComponentsInChildren<Transform>().Where(x => x.name == malePenisBaseName).FirstOrDefault();

                            if (malePenisTarget != null)
                            {
                                // Calculate male's penis length
                                Vector3 penisTip = males[0].GetComponentsInChildren<Transform>().Where(x => x.name == malePenisTipName).FirstOrDefault().position;

                                malePenisLength = Vector3.Distance(malePenisTarget.position, penisTip);

                                if (diagnostics)
                                {
                                    Logger.LogInfo("malePenisTarget found: " + malePenisTarget.name);
                                    Logger.LogInfo("Penis Length: " + malePenisLength.ToString());
                                }
                            }

                            if (diagnostics)
                                Logger.LogInfo("Current animation: " + animationName);

                            animationChanged = false;

                            updateRobotPosition = true;
                        }
                    }
                }

                if (updateRobotPosition)
                {
                    Vector3 femaleTargetPosition;

                    if (femaleTargets.Count == 1)
                    {
                        femaleTargetPosition = femaleTargets[0].position;
                    }
                    else
                    {
                        // Average the female target positions if there is more than one target

                        femaleTargetPosition.x = 0.0f;
                        femaleTargetPosition.y = 0.0f;
                        femaleTargetPosition.z = 0.0f;

                        for (int i = 0; i < femaleTargets.Count; i++)
                        {
                            femaleTargetPosition.x += femaleTargets[i].position.x;
                            femaleTargetPosition.y += femaleTargets[i].position.y;
                            femaleTargetPosition.z += femaleTargets[i].position.z;
                        }

                        femaleTargetPosition.x /= (float)femaleTargets.Count;
                        femaleTargetPosition.y /= (float)femaleTargets.Count;
                        femaleTargetPosition.z /= (float)femaleTargets.Count;
                    }

                    // Calculate the distance from the female target(s) per the current HScene and the male's penis target
                    float distance = Vector3.Distance(femaleTargetPosition, malePenisTarget.position);

                    // Only update the sex robot's position/servos when the distance from the female target(s) to the
                    // male's penis target is less than the length of the male's penis
                    if (distance <= malePenisLength)
                    {
                        try
                        {
                            // Convert the distance value to an int value between 0 and 100
                            string command = ((int)((distance / malePenisLength) * 100.0f)).ToString();

                            // If serial port is open then send the command to the robot
                            if (serialPort.IsOpen)
                            {
                                serialPort.WriteLine(command);

                                if (diagnostics)
                                    Logger.LogInfo("Command value sent: " + command);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.LogInfo("Error: " + e.ToString());
                        }

                        if (diagnostics)
                        {
                            string[] femaleTargetNames;

                            animationFemaleTargetDictionary.TryGetValue(animationName, out femaleTargetNames);

                            if (diagnostics)
                            {
                                Logger.LogInfo("Distance from Female chara's " + femaleTargetNames[0] + " to Male chara's penis is " + distance.ToString());
                                Logger.LogInfo("Distance by % from Female chara's " + femaleTargetNames[0] + " to Male chara's penis is " + ((distance / malePenisLength) * 100.0f).ToString());
                            }
                        }
                    }
                }

                sw = Stopwatch.StartNew();
            }
        }
    }
}
