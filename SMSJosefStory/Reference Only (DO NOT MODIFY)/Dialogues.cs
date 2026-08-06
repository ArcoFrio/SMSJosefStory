using BepInEx;
using GameCreator;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.Audio;
using GameCreator.Runtime.Common.UnityUI;
using GameCreator.Runtime.Dialogue;
using GameCreator.Runtime.Dialogue.UnityUI;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using TMPro;
using TransitionsPlusDemos;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace SMSAndroidsCore
{
    [BepInPlugin(pluginGuid, Core.pluginName + " - Dialogues", Core.pluginVersion)]
    internal class Dialogues : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.dialogues";
        #endregion
        public static GameObject overrideSpeechSkinBlue;
        public static GameObject overrideSpeechSkinGreen;
        public static GameObject overrideSpeechSkinPink;
        public static GameObject overrideSpeechSkinYellow;

        public static GameObject badWeatherDialogue;
        public static GameObject badWeatherDialogueActivator;
        public static GameObject badWeatherDialogueFinisher;

        public static GameObject giftUI;

        // Gift Item Template - stores a disabled template for cloning
        public static GameObject giftItemTemplate;

        // Track gift types for visibility checks (name -> isVanilla)
        public static Dictionary<string, bool> giftVanillaMap = new Dictionary<string, bool>();

        // SFX System - Maps text patterns to audio clip data
        public class SFXMapping
        {
            public string TextPattern;
            public List<AudioClip> AudioClips; // Can be multiple variants
            public float Volume;

            public SFXMapping(string textPattern, List<AudioClip> audioClips, float volume)
            {
                TextPattern = textPattern;
                AudioClips = audioClips;
                Volume = volume;
            }
        }

        public static Dictionary<string, SFXMapping> textToSFX = new Dictionary<string, SFXMapping>();

        #region Secret Beach Variables
        public static GameObject sBDialogueMainFirst;
        public static GameObject sBDialogueMainFirstScene1;
        public static GameObject sBDialogueMainFirstScene2;
        public static GameObject sBDialogueMainFirstScene3;
        public static GameObject sBDialogueMainFirstScene4;
        public static GameObject sBDialogueMainFirstScene5;
        public static GameObject sBDialogueMainFirstDialogueActivator;
        public static GameObject sBDialogueMainFirstDialogueFinisher;
        public static GameObject sBDialogueMainFirstMouthActivator;
        public static GameObject sBDialogueMainFirstSpriteFocus;

        public static GameObject sBDialogueMain;
        public static GameObject sBDialogueMainScene1;
        public static GameObject sBDialogueMainScene2;
        public static GameObject sBDialogueMainScene3;
        public static GameObject sBDialogueMainScene4;
        public static GameObject sBDialogueMainScene5;
        public static GameObject sBDialogueMainDialogueActivator;
        public static GameObject sBDialogueMainDialogueFinisher;
        public static GameObject sBDialogueMainMouthActivator;
        public static GameObject sBDialogueMainSpriteFocus;

        public static GameObject sBDialogueMainGK;
        public static GameObject sBDialogueMainGKScene1;
        public static GameObject sBDialogueMainGKScene2;
        public static GameObject sBDialogueMainGKScene3;
        public static GameObject sBDialogueMainGKScene4;
        public static GameObject sBDialogueMainGKScene5;
        public static GameObject sBDialogueMainGKDialogueActivator;
        public static GameObject sBDialogueMainGKDialogueFinisher;
        public static GameObject sBDialogueMainGKMouthActivator;
        public static GameObject sBDialogueMainGKSpriteFocus;

        public static GameObject sBDialogueStory01;
        public static GameObject sBDialogueStory01Scene1;
        public static GameObject sBDialogueStory01Scene2;
        public static GameObject sBDialogueStory01Scene3;
        public static GameObject sBDialogueStory01Scene4;
        public static GameObject sBDialogueStory01Scene5;
        public static GameObject sBDialogueStory01DialogueActivator;
        public static GameObject sBDialogueStory01DialogueFinisher;
        public static GameObject sBDialogueStory01MouthActivator;
        public static GameObject sBDialogueStory01SpriteFocus;
        #endregion
        #region Mountain Lab Variables
        public static GameObject mLDialogueMainFirst;
        public static GameObject mLDialogueMainFirstScene1;
        public static GameObject mLDialogueMainFirstScene2;
        public static GameObject mLDialogueMainFirstScene3;
        public static GameObject mLDialogueMainFirstScene4;
        public static GameObject mLDialogueMainFirstScene5;
        public static GameObject mLDialogueMainFirstDialogueActivator;
        public static GameObject mLDialogueMainFirstDialogueFinisher;
        public static GameObject mLDialogueMainFirstMouthActivator;
        public static GameObject mLDialogueMainFirstSpriteFocus;

        public static GameObject amberDefaultDialogue;
        public static GameObject amberDefaultDialogueScene1;
        public static GameObject amberDefaultDialogueScene2;
        public static GameObject amberDefaultDialogueScene3;
        public static GameObject amberDefaultDialogueScene4;
        public static GameObject amberDefaultDialogueScene5;
        public static GameObject amberDefaultDialogueDialogueActivator;
        public static GameObject amberDefaultDialogueDialogueFinisher;
        public static GameObject amberDefaultDialogueMouthActivator;
        public static GameObject amberDefaultDialogueSpriteFocus;

        public static GameObject mLDialogueMainStory03;
        public static GameObject mLDialogueMainStory03Scene1;
        public static GameObject mLDialogueMainStory03Scene2;
        public static GameObject mLDialogueMainStory03Scene3;
        public static GameObject mLDialogueMainStory03Scene4;
        public static GameObject mLDialogueMainStory03Scene5;
        public static GameObject mLDialogueMainStory03DialogueActivator;
        public static GameObject mLDialogueMainStory03DialogueFinisher;
        public static GameObject mLDialogueMainStory03MouthActivator;
        public static GameObject mLDialogueMainStory03SpriteFocus;

        public static GameObject mLDialogueMainStory04;
        public static GameObject mLDialogueMainStory04Scene1;
        public static GameObject mLDialogueMainStory04Scene2;
        public static GameObject mLDialogueMainStory04Scene3;
        public static GameObject mLDialogueMainStory04Scene4;
        public static GameObject mLDialogueMainStory04Scene5;
        public static GameObject mLDialogueMainStory04DialogueActivator;
        public static GameObject mLDialogueMainStory04DialogueFinisher;
        public static GameObject mLDialogueMainStory04MouthActivator;
        public static GameObject mLDialogueMainStory04SpriteFocus;
        #endregion
        #region Gift Shop Variables
        public static GameObject gSDialogueMainFirst;
        public static GameObject gSDialogueMainFirstScene1;
        public static GameObject gSDialogueMainFirstScene2;
        public static GameObject gSDialogueMainFirstScene3;
        public static GameObject gSDialogueMainFirstScene4;
        public static GameObject gSDialogueMainFirstScene5;
        public static GameObject gSDialogueMainFirstDialogueActivator;
        public static GameObject gSDialogueMainFirstDialogueFinisher;
        public static GameObject gSDialogueMainFirstMouthActivator;
        public static GameObject gSDialogueMainFirstSpriteFocus;

        public static GameObject claireDefaultDialogue;
        public static GameObject claireDefaultDialogueScene1;
        public static GameObject claireDefaultDialogueScene2;
        public static GameObject claireDefaultDialogueScene3;
        public static GameObject claireDefaultDialogueScene4;
        public static GameObject claireDefaultDialogueScene5;
        public static GameObject claireDefaultDialogueDialogueActivator;
        public static GameObject claireDefaultDialogueDialogueFinisher;
        public static GameObject claireDefaultDialogueMouthActivator;
        public static GameObject claireDefaultDialogueSpriteFocus;
        #endregion
        #region Harbor Home Variables
        public static GameObject sarahDialogueBuyHH;
        public static GameObject sarahDialogueBuyHHScene1;
        public static GameObject sarahDialogueBuyHHScene2;
        public static GameObject sarahDialogueBuyHHScene3;
        public static GameObject sarahDialogueBuyHHScene4;
        public static GameObject sarahDialogueBuyHHScene5;
        public static GameObject sarahDialogueBuyHHDialogueActivator;
        public static GameObject sarahDialogueBuyHHDialogueFinisher;
        public static GameObject sarahDialogueBuyHHMouthActivator;
        public static GameObject sarahDialogueBuyHHSpriteFocus;
        #endregion
        #region Character Variables
        public static GameObject anisDefaultDialogue;
        public static GameObject anisDefaultDialogueScene1;
        public static GameObject anisDefaultDialogueScene2;
        public static GameObject anisDefaultDialogueScene3;
        public static GameObject anisDefaultDialogueScene4;
        public static GameObject anisDefaultDialogueScene5;
        public static GameObject anisDefaultDialogueDialogueActivator;
        public static GameObject anisDefaultDialogueDialogueFinisher;
        public static GameObject anisDefaultDialogueMouthActivator;
        public static GameObject anisDefaultDialogueSpriteFocus;

        public static GameObject anisDefaultHHDialogue;
        public static GameObject anisDefaultHHDialogueScene1;
        public static GameObject anisDefaultHHDialogueScene2;
        public static GameObject anisDefaultHHDialogueScene3;
        public static GameObject anisDefaultHHDialogueScene4;
        public static GameObject anisDefaultHHDialogueScene5;
        public static GameObject anisDefaultHHDialogueDialogueActivator;
        public static GameObject anisDefaultHHDialogueDialogueFinisher;
        public static GameObject anisDefaultHHDialogueMouthActivator;
        public static GameObject anisDefaultHHDialogueOutfitDefault;
        public static GameObject anisDefaultHHDialogueOutfitSwim;
        public static GameObject anisDefaultHHDialogueSpriteFocus;

        public static GameObject anisGiftDialogue;
        public static GameObject anisGiftDialogueScene1;
        public static GameObject anisGiftDialogueScene2;
        public static GameObject anisGiftDialogueScene3;
        public static GameObject anisGiftDialogueScene4;
        public static GameObject anisGiftDialogueScene5;
        public static GameObject anisGiftDialogueDialogueActivator;
        public static GameObject anisGiftDialogueDialogueFinisher;
        public static GameObject anisGiftDialogueMouthActivator;
        public static GameObject anisGiftDialogueSpriteFocus;

        public static GameObject anisRandomDialogue67;
        public static GameObject anisRandomDialogue67Scene1;
        public static GameObject anisRandomDialogue67Scene2;
        public static GameObject anisRandomDialogue67Scene3;
        public static GameObject anisRandomDialogue67Scene4;
        public static GameObject anisRandomDialogue67Scene5;
        public static GameObject anisRandomDialogue67DialogueActivator;
        public static GameObject anisRandomDialogue67DialogueFinisher;
        public static GameObject anisRandomDialogue67MouthActivator;
        public static GameObject anisRandomDialogue67SpriteFocus;

        public static GameObject anisAffection01Dialogue;
        public static GameObject anisAffection01DialogueScene1;
        public static GameObject anisAffection01DialogueScene2;
        public static GameObject anisAffection01DialogueScene3;
        public static GameObject anisAffection01DialogueScene4;
        public static GameObject anisAffection01DialogueScene5;
        public static GameObject anisAffection01DialogueDialogueActivator;
        public static GameObject anisAffection01DialogueDialogueFinisher;
        public static GameObject anisAffection01DialogueMouthActivator;
        public static GameObject anisAffection01DialogueSpriteFocus;

        public static GameObject anisAffection02Dialogue;
        public static GameObject anisAffection02DialogueScene1;
        public static GameObject anisAffection02DialogueScene2;
        public static GameObject anisAffection02DialogueScene3;
        public static GameObject anisAffection02DialogueScene4;
        public static GameObject anisAffection02DialogueScene5;
        public static GameObject anisAffection02DialogueScene6;
        public static GameObject anisAffection02DialogueScene7;
        public static GameObject anisAffection02DialogueScene8;
        public static GameObject anisAffection02DialogueDialogueActivator;
        public static GameObject anisAffection02DialogueDialogueFinisher;
        public static GameObject anisAffection02DialogueMouthActivator;
        public static GameObject anisAffection02DialogueSpriteFocus;

        public static GameObject anisAffection03Dialogue;
        public static GameObject anisAffection03DialogueScene1;
        public static GameObject anisAffection03DialogueScene2;
        public static GameObject anisAffection03DialogueScene3;
        public static GameObject anisAffection03DialogueScene4;
        public static GameObject anisAffection03DialogueScene5;
        public static GameObject anisAffection03DialogueDialogueActivator;
        public static GameObject anisAffection03DialogueDialogueFinisher;
        public static GameObject anisAffection03DialogueMouthActivator;
        public static GameObject anisAffection03DialogueSpriteFocus;

        public static GameObject anisRandomDialogueLabRoomChill01Dialogue;
        public static GameObject anisRandomDialogueLabRoomChill01DialogueScene1;
        public static GameObject anisRandomDialogueLabRoomChill01DialogueScene2;
        public static GameObject anisRandomDialogueLabRoomChill01DialogueScene3;
        public static GameObject anisRandomDialogueLabRoomChill01DialogueScene4;
        public static GameObject anisRandomDialogueLabRoomChill01DialogueScene5;
        public static GameObject anisRandomDialogueLabRoomChill01DialogueScene6;
        public static GameObject anisRandomDialogueLabRoomChill01DialogueDialogueActivator;
        public static GameObject anisRandomDialogueLabRoomChill01DialogueDialogueFinisher;
        public static GameObject anisRandomDialogueLabRoomChill01DialogueMouthActivator;
        public static GameObject anisRandomDialogueLabRoomChill01DialogueSpriteFocus;


        public static GameObject centiDefaultDialogue;
        public static GameObject centiDefaultDialogueScene1;
        public static GameObject centiDefaultDialogueScene2;
        public static GameObject centiDefaultDialogueScene3;
        public static GameObject centiDefaultDialogueScene4;
        public static GameObject centiDefaultDialogueScene5;
        public static GameObject centiDefaultDialogueDialogueActivator;
        public static GameObject centiDefaultDialogueDialogueFinisher;
        public static GameObject centiDefaultDialogueMouthActivator;
        public static GameObject centiDefaultDialogueSpriteFocus;

        public static GameObject dorothyDefaultDialogue;
        public static GameObject dorothyDefaultDialogueScene1;
        public static GameObject dorothyDefaultDialogueScene2;
        public static GameObject dorothyDefaultDialogueScene3;
        public static GameObject dorothyDefaultDialogueScene4;
        public static GameObject dorothyDefaultDialogueScene5;
        public static GameObject dorothyDefaultDialogueDialogueActivator;
        public static GameObject dorothyDefaultDialogueDialogueFinisher;
        public static GameObject dorothyDefaultDialogueMouthActivator;
        public static GameObject dorothyDefaultDialogueSpriteFocus;

        public static GameObject eleggDefaultDialogue;
        public static GameObject eleggDefaultDialogueScene1;
        public static GameObject eleggDefaultDialogueScene2;
        public static GameObject eleggDefaultDialogueScene3;
        public static GameObject eleggDefaultDialogueScene4;
        public static GameObject eleggDefaultDialogueScene5;
        public static GameObject eleggDefaultDialogueDialogueActivator;
        public static GameObject eleggDefaultDialogueDialogueFinisher;
        public static GameObject eleggDefaultDialogueMouthActivator;
        public static GameObject eleggDefaultDialogueSpriteFocus;

        public static GameObject frimaDefaultDialogue;
        public static GameObject frimaDefaultDialogueScene1;
        public static GameObject frimaDefaultDialogueScene2;
        public static GameObject frimaDefaultDialogueScene3;
        public static GameObject frimaDefaultDialogueScene4;
        public static GameObject frimaDefaultDialogueScene5;
        public static GameObject frimaDefaultDialogueDialogueActivator;
        public static GameObject frimaDefaultDialogueDialogueFinisher;
        public static GameObject frimaDefaultDialogueMouthActivator;
        public static GameObject frimaDefaultDialogueSpriteFocus;

        public static GameObject guiltyDefaultDialogue;
        public static GameObject guiltyDefaultDialogueScene1;
        public static GameObject guiltyDefaultDialogueScene2;
        public static GameObject guiltyDefaultDialogueScene3;
        public static GameObject guiltyDefaultDialogueScene4;
        public static GameObject guiltyDefaultDialogueScene5;
        public static GameObject guiltyDefaultDialogueDialogueActivator;
        public static GameObject guiltyDefaultDialogueDialogueFinisher;
        public static GameObject guiltyDefaultDialogueMouthActivator;
        public static GameObject guiltyDefaultDialogueSpriteFocus;

        public static GameObject helmDefaultDialogue;
        public static GameObject helmDefaultDialogueScene1;
        public static GameObject helmDefaultDialogueScene2;
        public static GameObject helmDefaultDialogueScene3;
        public static GameObject helmDefaultDialogueScene4;
        public static GameObject helmDefaultDialogueScene5;
        public static GameObject helmDefaultDialogueDialogueActivator;
        public static GameObject helmDefaultDialogueDialogueFinisher;
        public static GameObject helmDefaultDialogueMouthActivator;
        public static GameObject helmDefaultDialogueSpriteFocus;

        public static GameObject maidenDefaultDialogue;
        public static GameObject maidenDefaultDialogueScene1;
        public static GameObject maidenDefaultDialogueScene2;
        public static GameObject maidenDefaultDialogueScene3;
        public static GameObject maidenDefaultDialogueScene4;
        public static GameObject maidenDefaultDialogueScene5;
        public static GameObject maidenDefaultDialogueDialogueActivator;
        public static GameObject maidenDefaultDialogueDialogueFinisher;
        public static GameObject maidenDefaultDialogueMouthActivator;
        public static GameObject maidenDefaultDialogueSpriteFocus;

        public static GameObject maryDefaultDialogue;
        public static GameObject maryDefaultDialogueScene1;
        public static GameObject maryDefaultDialogueScene2;
        public static GameObject maryDefaultDialogueScene3;
        public static GameObject maryDefaultDialogueScene4;
        public static GameObject maryDefaultDialogueScene5;
        public static GameObject maryDefaultDialogueDialogueActivator;
        public static GameObject maryDefaultDialogueDialogueFinisher;
        public static GameObject maryDefaultDialogueMouthActivator;
        public static GameObject maryDefaultDialogueSpriteFocus;

        public static GameObject mastDefaultDialogue;
        public static GameObject mastDefaultDialogueScene1;
        public static GameObject mastDefaultDialogueScene2;
        public static GameObject mastDefaultDialogueScene3;
        public static GameObject mastDefaultDialogueScene4;
        public static GameObject mastDefaultDialogueScene5;
        public static GameObject mastDefaultDialogueDialogueActivator;
        public static GameObject mastDefaultDialogueDialogueFinisher;
        public static GameObject mastDefaultDialogueMouthActivator;
        public static GameObject mastDefaultDialogueSpriteFocus;

        public static GameObject neonDefaultDialogue;
        public static GameObject neonDefaultDialogueScene1;
        public static GameObject neonDefaultDialogueScene2;
        public static GameObject neonDefaultDialogueScene3;
        public static GameObject neonDefaultDialogueScene4;
        public static GameObject neonDefaultDialogueScene5;
        public static GameObject neonDefaultDialogueDialogueActivator;
        public static GameObject neonDefaultDialogueDialogueFinisher;
        public static GameObject neonDefaultDialogueMouthActivator;
        public static GameObject neonDefaultDialogueSpriteFocus;

        public static GameObject pepperDefaultDialogue;
        public static GameObject pepperDefaultDialogueScene1;
        public static GameObject pepperDefaultDialogueScene2;
        public static GameObject pepperDefaultDialogueScene3;
        public static GameObject pepperDefaultDialogueScene4;
        public static GameObject pepperDefaultDialogueScene5;
        public static GameObject pepperDefaultDialogueDialogueActivator;
        public static GameObject pepperDefaultDialogueDialogueFinisher;
        public static GameObject pepperDefaultDialogueMouthActivator;
        public static GameObject pepperDefaultDialogueSpriteFocus;

        public static GameObject rapiDefaultDialogue;
        public static GameObject rapiDefaultDialogueScene1;
        public static GameObject rapiDefaultDialogueScene2;
        public static GameObject rapiDefaultDialogueScene3;
        public static GameObject rapiDefaultDialogueScene4;
        public static GameObject rapiDefaultDialogueScene5;
        public static GameObject rapiDefaultDialogueDialogueActivator;
        public static GameObject rapiDefaultDialogueDialogueFinisher;
        public static GameObject rapiDefaultDialogueMouthActivator;
        public static GameObject rapiDefaultDialogueSpriteFocus;

        public static GameObject rosannaDefaultDialogue;
        public static GameObject rosannaDefaultDialogueScene1;
        public static GameObject rosannaDefaultDialogueScene2;
        public static GameObject rosannaDefaultDialogueScene3;
        public static GameObject rosannaDefaultDialogueScene4;
        public static GameObject rosannaDefaultDialogueScene5;
        public static GameObject rosannaDefaultDialogueDialogueActivator;
        public static GameObject rosannaDefaultDialogueDialogueFinisher;
        public static GameObject rosannaDefaultDialogueMouthActivator;
        public static GameObject rosannaDefaultDialogueSpriteFocus;

        public static GameObject sakuraDefaultDialogue;
        public static GameObject sakuraDefaultDialogueScene1;
        public static GameObject sakuraDefaultDialogueScene2;
        public static GameObject sakuraDefaultDialogueScene3;
        public static GameObject sakuraDefaultDialogueScene4;
        public static GameObject sakuraDefaultDialogueScene5;
        public static GameObject sakuraDefaultDialogueDialogueActivator;
        public static GameObject sakuraDefaultDialogueDialogueFinisher;
        public static GameObject sakuraDefaultDialogueMouthActivator;
        public static GameObject sakuraDefaultDialogueSpriteFocus;

        public static GameObject toveDefaultDialogue;
        public static GameObject toveDefaultDialogueScene1;
        public static GameObject toveDefaultDialogueScene2;
        public static GameObject toveDefaultDialogueScene3;
        public static GameObject toveDefaultDialogueScene4;
        public static GameObject toveDefaultDialogueScene5;
        public static GameObject toveDefaultDialogueDialogueActivator;
        public static GameObject toveDefaultDialogueDialogueFinisher;
        public static GameObject toveDefaultDialogueMouthActivator;
        public static GameObject toveDefaultDialogueSpriteFocus;

        public static GameObject viperDefaultDialogue;
        public static GameObject viperDefaultDialogueScene1;
        public static GameObject viperDefaultDialogueScene2;
        public static GameObject viperDefaultDialogueScene3;
        public static GameObject viperDefaultDialogueScene4;
        public static GameObject viperDefaultDialogueScene5;
        public static GameObject viperDefaultDialogueDialogueActivator;
        public static GameObject viperDefaultDialogueDialogueFinisher;
        public static GameObject viperDefaultDialogueMouthActivator;
        public static GameObject viperDefaultDialogueSpriteFocus;

        public static GameObject yanDefaultDialogue;
        public static GameObject yanDefaultDialogueScene1;
        public static GameObject yanDefaultDialogueScene2;
        public static GameObject yanDefaultDialogueScene3;
        public static GameObject yanDefaultDialogueScene4;
        public static GameObject yanDefaultDialogueScene5;
        public static GameObject yanDefaultDialogueDialogueActivator;
        public static GameObject yanDefaultDialogueDialogueFinisher;
        public static GameObject yanDefaultDialogueMouthActivator;
        public static GameObject yanDefaultDialogueSpriteFocus;
        #endregion
        #region Event Variables
        public static GameObject amberHospitalhallwayEvent01Dialogue;
        public static GameObject amberHospitalhallwayEvent01DialogueScene1;
        public static GameObject amberHospitalhallwayEvent01DialogueScene2;
        public static GameObject amberHospitalhallwayEvent01DialogueScene3;
        public static GameObject amberHospitalhallwayEvent01DialogueScene4;
        public static GameObject amberHospitalhallwayEvent01DialogueScene5;
        public static GameObject amberHospitalhallwayEvent01DialogueDialogueActivator;
        public static GameObject amberHospitalhallwayEvent01DialogueDialogueFinisher;
        public static GameObject amberHospitalhallwayEvent01DialogueMouthActivator;
        public static GameObject amberHospitalhallwayEvent01DialogueSpriteFocus;

        public static GameObject anisMallEvent01Dialogue;
        public static GameObject anisMallEvent01DialogueScene1;
        public static GameObject anisMallEvent01DialogueScene2;
        public static GameObject anisMallEvent01DialogueScene3;
        public static GameObject anisMallEvent01DialogueScene4;
        public static GameObject anisMallEvent01DialogueScene5;
        public static GameObject anisMallEvent01DialogueDialogueActivator;
        public static GameObject anisMallEvent01DialogueDialogueFinisher;
        public static GameObject anisMallEvent01DialogueMouthActivator;
        public static GameObject anisMallEvent01DialogueSpriteFocus;

        public static GameObject centiKenshomeEvent01Dialogue;
        public static GameObject centiKenshomeEvent01DialogueScene1;
        public static GameObject centiKenshomeEvent01DialogueScene2;
        public static GameObject centiKenshomeEvent01DialogueScene3;
        public static GameObject centiKenshomeEvent01DialogueScene4;
        public static GameObject centiKenshomeEvent01DialogueScene5;
        public static GameObject centiKenshomeEvent01DialogueDialogueActivator;
        public static GameObject centiKenshomeEvent01DialogueDialogueFinisher;
        public static GameObject centiKenshomeEvent01DialogueMouthActivator;
        public static GameObject centiKenshomeEvent01DialogueSpriteFocus;

        public static GameObject dorothyParkEvent01Dialogue;
        public static GameObject dorothyParkEvent01DialogueScene1;
        public static GameObject dorothyParkEvent01DialogueScene2;
        public static GameObject dorothyParkEvent01DialogueScene3;
        public static GameObject dorothyParkEvent01DialogueScene4;
        public static GameObject dorothyParkEvent01DialogueScene5;
        public static GameObject dorothyParkEvent01DialogueDialogueActivator;
        public static GameObject dorothyParkEvent01DialogueDialogueFinisher;
        public static GameObject dorothyParkEvent01DialogueMouthActivator;
        public static GameObject dorothyParkEvent01DialogueSpriteFocus;

        public static GameObject eleggDowntownEvent01Dialogue;
        public static GameObject eleggDowntownEvent01DialogueScene1;
        public static GameObject eleggDowntownEvent01DialogueScene2;
        public static GameObject eleggDowntownEvent01DialogueScene3;
        public static GameObject eleggDowntownEvent01DialogueScene4;
        public static GameObject eleggDowntownEvent01DialogueScene5;
        public static GameObject eleggDowntownEvent01DialogueDialogueActivator;
        public static GameObject eleggDowntownEvent01DialogueDialogueFinisher;
        public static GameObject eleggDowntownEvent01DialogueMouthActivator;
        public static GameObject eleggDowntownEvent01DialogueSpriteFocus;

        public static GameObject frimaHotelEvent01Dialogue;
        public static GameObject frimaHotelEvent01DialogueScene1;
        public static GameObject frimaHotelEvent01DialogueScene2;
        public static GameObject frimaHotelEvent01DialogueScene3;
        public static GameObject frimaHotelEvent01DialogueScene4;
        public static GameObject frimaHotelEvent01DialogueScene5;
        public static GameObject frimaHotelEvent01DialogueDialogueActivator;
        public static GameObject frimaHotelEvent01DialogueDialogueFinisher;
        public static GameObject frimaHotelEvent01DialogueMouthActivator;
        public static GameObject frimaHotelEvent01DialogueSpriteFocus;

        public static GameObject guiltyParkinglotEvent01Dialogue;
        public static GameObject guiltyParkinglotEvent01DialogueScene1;
        public static GameObject guiltyParkinglotEvent01DialogueScene2;
        public static GameObject guiltyParkinglotEvent01DialogueScene3;
        public static GameObject guiltyParkinglotEvent01DialogueScene4;
        public static GameObject guiltyParkinglotEvent01DialogueScene5;
        public static GameObject guiltyParkinglotEvent01DialogueDialogueActivator;
        public static GameObject guiltyParkinglotEvent01DialogueDialogueFinisher;
        public static GameObject guiltyParkinglotEvent01DialogueMouthActivator;
        public static GameObject guiltyParkinglotEvent01DialogueSpriteFocus;

        public static GameObject helmBeachEvent01Dialogue;
        public static GameObject helmBeachEvent01DialogueScene1;
        public static GameObject helmBeachEvent01DialogueScene2;
        public static GameObject helmBeachEvent01DialogueScene3;
        public static GameObject helmBeachEvent01DialogueScene4;
        public static GameObject helmBeachEvent01DialogueScene5;
        public static GameObject helmBeachEvent01DialogueDialogueActivator;
        public static GameObject helmBeachEvent01DialogueDialogueFinisher;
        public static GameObject helmBeachEvent01DialogueMouthActivator;
        public static GameObject helmBeachEvent01DialogueSpriteFocus;

        public static GameObject maidenAlleyEvent01Dialogue;
        public static GameObject maidenAlleyEvent01DialogueScene1;
        public static GameObject maidenAlleyEvent01DialogueScene2;
        public static GameObject maidenAlleyEvent01DialogueScene3;
        public static GameObject maidenAlleyEvent01DialogueScene4;
        public static GameObject maidenAlleyEvent01DialogueScene5;
        public static GameObject maidenAlleyEvent01DialogueDialogueActivator;
        public static GameObject maidenAlleyEvent01DialogueDialogueFinisher;
        public static GameObject maidenAlleyEvent01DialogueMouthActivator;
        public static GameObject maidenAlleyEvent01DialogueSpriteFocus;

        public static GameObject maryHospitalhallwayEvent01Dialogue;
        public static GameObject maryHospitalhallwayEvent01DialogueScene1;
        public static GameObject maryHospitalhallwayEvent01DialogueScene2;
        public static GameObject maryHospitalhallwayEvent01DialogueScene3;
        public static GameObject maryHospitalhallwayEvent01DialogueScene4;
        public static GameObject maryHospitalhallwayEvent01DialogueScene5;
        public static GameObject maryHospitalhallwayEvent01DialogueDialogueActivator;
        public static GameObject maryHospitalhallwayEvent01DialogueDialogueFinisher;
        public static GameObject maryHospitalhallwayEvent01DialogueMouthActivator;
        public static GameObject maryHospitalhallwayEvent01DialogueSpriteFocus;

        public static GameObject mastBeachEvent01Dialogue;
        public static GameObject mastBeachEvent01DialogueScene1;
        public static GameObject mastBeachEvent01DialogueScene2;
        public static GameObject mastBeachEvent01DialogueScene3;
        public static GameObject mastBeachEvent01DialogueScene4;
        public static GameObject mastBeachEvent01DialogueScene5;
        public static GameObject mastBeachEvent01DialogueDialogueActivator;
        public static GameObject mastBeachEvent01DialogueDialogueFinisher;
        public static GameObject mastBeachEvent01DialogueMouthActivator;
        public static GameObject mastBeachEvent01DialogueSpriteFocus;

        public static GameObject neonTempleEvent01Dialogue;
        public static GameObject neonTempleEvent01DialogueScene1;
        public static GameObject neonTempleEvent01DialogueScene2;
        public static GameObject neonTempleEvent01DialogueScene3;
        public static GameObject neonTempleEvent01DialogueScene4;
        public static GameObject neonTempleEvent01DialogueScene5;
        public static GameObject neonTempleEvent01DialogueDialogueActivator;
        public static GameObject neonTempleEvent01DialogueDialogueFinisher;
        public static GameObject neonTempleEvent01DialogueMouthActivator;
        public static GameObject neonTempleEvent01DialogueSpriteFocus;

        public static GameObject pepperHospitalEvent01Dialogue;
        public static GameObject pepperHospitalEvent01DialogueScene1;
        public static GameObject pepperHospitalEvent01DialogueScene2;
        public static GameObject pepperHospitalEvent01DialogueScene3;
        public static GameObject pepperHospitalEvent01DialogueScene4;
        public static GameObject pepperHospitalEvent01DialogueScene5;
        public static GameObject pepperHospitalEvent01DialogueDialogueActivator;
        public static GameObject pepperHospitalEvent01DialogueDialogueFinisher;
        public static GameObject pepperHospitalEvent01DialogueMouthActivator;
        public static GameObject pepperHospitalEvent01DialogueSpriteFocus;

        public static GameObject rapiGasstationEvent01Dialogue;
        public static GameObject rapiGasstationEvent01DialogueScene1;
        public static GameObject rapiGasstationEvent01DialogueScene2;
        public static GameObject rapiGasstationEvent01DialogueScene3;
        public static GameObject rapiGasstationEvent01DialogueScene4;
        public static GameObject rapiGasstationEvent01DialogueScene5;
        public static GameObject rapiGasstationEvent01DialogueDialogueActivator;
        public static GameObject rapiGasstationEvent01DialogueDialogueFinisher;
        public static GameObject rapiGasstationEvent01DialogueMouthActivator;
        public static GameObject rapiGasstationEvent01DialogueSpriteFocus;

        public static GameObject rosannaGabrielsmansionEvent01Dialogue;
        public static GameObject rosannaGabrielsmansionEvent01DialogueScene1;
        public static GameObject rosannaGabrielsmansionEvent01DialogueScene2;
        public static GameObject rosannaGabrielsmansionEvent01DialogueScene3;
        public static GameObject rosannaGabrielsmansionEvent01DialogueScene4;
        public static GameObject rosannaGabrielsmansionEvent01DialogueScene5;
        public static GameObject rosannaGabrielsmansionEvent01DialogueDialogueActivator;
        public static GameObject rosannaGabrielsmansionEvent01DialogueDialogueFinisher;
        public static GameObject rosannaGabrielsmansionEvent01DialogueMouthActivator;
        public static GameObject rosannaGabrielsmansionEvent01DialogueSpriteFocus;

        public static GameObject sakuraForestEvent01Dialogue;
        public static GameObject sakuraForestEvent01DialogueScene1;
        public static GameObject sakuraForestEvent01DialogueScene2;
        public static GameObject sakuraForestEvent01DialogueScene3;
        public static GameObject sakuraForestEvent01DialogueScene4;
        public static GameObject sakuraForestEvent01DialogueScene5;
        public static GameObject sakuraForestEvent01DialogueDialogueActivator;
        public static GameObject sakuraForestEvent01DialogueDialogueFinisher;
        public static GameObject sakuraForestEvent01DialogueMouthActivator;
        public static GameObject sakuraForestEvent01DialogueSpriteFocus;

        public static GameObject toveTrailEvent01Dialogue;
        public static GameObject toveTrailEvent01DialogueScene1;
        public static GameObject toveTrailEvent01DialogueScene2;
        public static GameObject toveTrailEvent01DialogueScene3;
        public static GameObject toveTrailEvent01DialogueScene4;
        public static GameObject toveTrailEvent01DialogueScene5;
        public static GameObject toveTrailEvent01DialogueDialogueActivator;
        public static GameObject toveTrailEvent01DialogueDialogueFinisher;
        public static GameObject toveTrailEvent01DialogueMouthActivator;
        public static GameObject toveTrailEvent01DialogueSpriteFocus;

        public static GameObject viperVillaEvent01Dialogue;
        public static GameObject viperVillaEvent01DialogueScene1;
        public static GameObject viperVillaEvent01DialogueScene2;
        public static GameObject viperVillaEvent01DialogueScene3;
        public static GameObject viperVillaEvent01DialogueScene4;
        public static GameObject viperVillaEvent01DialogueScene5;
        public static GameObject viperVillaEvent01DialogueDialogueActivator;
        public static GameObject viperVillaEvent01DialogueDialogueFinisher;
        public static GameObject viperVillaEvent01DialogueMouthActivator;
        public static GameObject viperVillaEvent01DialogueSpriteFocus;

        public static GameObject yanMallEvent01Dialogue;
        public static GameObject yanMallEvent01DialogueScene1;
        public static GameObject yanMallEvent01DialogueScene2;
        public static GameObject yanMallEvent01DialogueScene3;
        public static GameObject yanMallEvent01DialogueScene4;
        public static GameObject yanMallEvent01DialogueScene5;
        public static GameObject yanMallEvent01DialogueDialogueActivator;
        public static GameObject yanMallEvent01DialogueDialogueFinisher;
        public static GameObject yanMallEvent01DialogueMouthActivator;
        public static GameObject yanMallEvent01DialogueSpriteFocus;
        #endregion
        #region Voyeur Variables
        public static GameObject anisSecretbeachVoyeur01Dialogue;
        public static GameObject anisSecretbeachVoyeur01DialogueScene1;
        public static GameObject anisSecretbeachVoyeur01DialogueScene2;
        public static GameObject anisSecretbeachVoyeur01DialogueScene3;
        public static GameObject anisSecretbeachVoyeur01DialogueScene4;
        public static GameObject anisSecretbeachVoyeur01DialogueScene5;
        public static GameObject anisSecretbeachVoyeur01DialogueActivator;
        public static GameObject anisSecretbeachVoyeur01DialogueFinisher;
        public static GameObject anisSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject anisSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject centiSecretbeachVoyeur01Dialogue;
        public static GameObject centiSecretbeachVoyeur01DialogueScene1;
        public static GameObject centiSecretbeachVoyeur01DialogueScene2;
        public static GameObject centiSecretbeachVoyeur01DialogueScene3;
        public static GameObject centiSecretbeachVoyeur01DialogueScene4;
        public static GameObject centiSecretbeachVoyeur01DialogueScene5;
        public static GameObject centiSecretbeachVoyeur01DialogueActivator;
        public static GameObject centiSecretbeachVoyeur01DialogueFinisher;
        public static GameObject centiSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject centiSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject dorothySecretbeachVoyeur01Dialogue;
        public static GameObject dorothySecretbeachVoyeur01DialogueScene1;
        public static GameObject dorothySecretbeachVoyeur01DialogueScene2;
        public static GameObject dorothySecretbeachVoyeur01DialogueScene3;
        public static GameObject dorothySecretbeachVoyeur01DialogueScene4;
        public static GameObject dorothySecretbeachVoyeur01DialogueScene5;
        public static GameObject dorothySecretbeachVoyeur01DialogueActivator;
        public static GameObject dorothySecretbeachVoyeur01DialogueFinisher;
        public static GameObject dorothySecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject dorothySecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject eleggSecretbeachVoyeur01Dialogue;
        public static GameObject eleggSecretbeachVoyeur01DialogueScene1;
        public static GameObject eleggSecretbeachVoyeur01DialogueScene2;
        public static GameObject eleggSecretbeachVoyeur01DialogueScene3;
        public static GameObject eleggSecretbeachVoyeur01DialogueScene4;
        public static GameObject eleggSecretbeachVoyeur01DialogueScene5;
        public static GameObject eleggSecretbeachVoyeur01DialogueScene6;
        public static GameObject eleggSecretbeachVoyeur01DialogueScene7;
        public static GameObject eleggSecretbeachVoyeur01DialogueScene8;
        public static GameObject eleggSecretbeachVoyeur01DialogueActivator;
        public static GameObject eleggSecretbeachVoyeur01DialogueFinisher;
        public static GameObject eleggSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject eleggSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject frimaSecretbeachVoyeur01Dialogue;
        public static GameObject frimaSecretbeachVoyeur01DialogueScene1;
        public static GameObject frimaSecretbeachVoyeur01DialogueScene2;
        public static GameObject frimaSecretbeachVoyeur01DialogueScene3;
        public static GameObject frimaSecretbeachVoyeur01DialogueScene4;
        public static GameObject frimaSecretbeachVoyeur01DialogueScene5;
        public static GameObject frimaSecretbeachVoyeur01DialogueActivator;
        public static GameObject frimaSecretbeachVoyeur01DialogueFinisher;
        public static GameObject frimaSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject frimaSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject guiltySecretbeachVoyeur01Dialogue;
        public static GameObject guiltySecretbeachVoyeur01DialogueScene1;
        public static GameObject guiltySecretbeachVoyeur01DialogueScene2;
        public static GameObject guiltySecretbeachVoyeur01DialogueScene3;
        public static GameObject guiltySecretbeachVoyeur01DialogueScene4;
        public static GameObject guiltySecretbeachVoyeur01DialogueScene5;
        public static GameObject guiltySecretbeachVoyeur01DialogueActivator;
        public static GameObject guiltySecretbeachVoyeur01DialogueFinisher;
        public static GameObject guiltySecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject guiltySecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject helmSecretbeachVoyeur01Dialogue;
        public static GameObject helmSecretbeachVoyeur01DialogueScene1;
        public static GameObject helmSecretbeachVoyeur01DialogueScene2;
        public static GameObject helmSecretbeachVoyeur01DialogueScene3;
        public static GameObject helmSecretbeachVoyeur01DialogueScene4;
        public static GameObject helmSecretbeachVoyeur01DialogueScene5;
        public static GameObject helmSecretbeachVoyeur01DialogueActivator;
        public static GameObject helmSecretbeachVoyeur01DialogueFinisher;
        public static GameObject helmSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject helmSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject maidenSecretbeachVoyeur01Dialogue;
        public static GameObject maidenSecretbeachVoyeur01DialogueScene1;
        public static GameObject maidenSecretbeachVoyeur01DialogueScene2;
        public static GameObject maidenSecretbeachVoyeur01DialogueScene3;
        public static GameObject maidenSecretbeachVoyeur01DialogueScene4;
        public static GameObject maidenSecretbeachVoyeur01DialogueScene5;
        public static GameObject maidenSecretbeachVoyeur01DialogueActivator;
        public static GameObject maidenSecretbeachVoyeur01DialogueFinisher;
        public static GameObject maidenSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject maidenSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject marySecretbeachVoyeur01Dialogue;
        public static GameObject marySecretbeachVoyeur01DialogueScene1;
        public static GameObject marySecretbeachVoyeur01DialogueScene2;
        public static GameObject marySecretbeachVoyeur01DialogueScene3;
        public static GameObject marySecretbeachVoyeur01DialogueScene4;
        public static GameObject marySecretbeachVoyeur01DialogueScene5;
        public static GameObject marySecretbeachVoyeur01DialogueActivator;
        public static GameObject marySecretbeachVoyeur01DialogueFinisher;
        public static GameObject marySecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject marySecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject mastSecretbeachVoyeur01Dialogue;
        public static GameObject mastSecretbeachVoyeur01DialogueScene1;
        public static GameObject mastSecretbeachVoyeur01DialogueScene2;
        public static GameObject mastSecretbeachVoyeur01DialogueScene3;
        public static GameObject mastSecretbeachVoyeur01DialogueScene4;
        public static GameObject mastSecretbeachVoyeur01DialogueScene5;
        public static GameObject mastSecretbeachVoyeur01DialogueActivator;
        public static GameObject mastSecretbeachVoyeur01DialogueFinisher;
        public static GameObject mastSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject mastSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject neonSecretbeachVoyeur01Dialogue;
        public static GameObject neonSecretbeachVoyeur01DialogueScene1;
        public static GameObject neonSecretbeachVoyeur01DialogueScene2;
        public static GameObject neonSecretbeachVoyeur01DialogueScene3;
        public static GameObject neonSecretbeachVoyeur01DialogueScene4;
        public static GameObject neonSecretbeachVoyeur01DialogueScene5;
        public static GameObject neonSecretbeachVoyeur01DialogueActivator;
        public static GameObject neonSecretbeachVoyeur01DialogueFinisher;
        public static GameObject neonSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject neonSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject pepperSecretbeachVoyeur01Dialogue;
        public static GameObject pepperSecretbeachVoyeur01DialogueScene1;
        public static GameObject pepperSecretbeachVoyeur01DialogueScene2;
        public static GameObject pepperSecretbeachVoyeur01DialogueScene3;
        public static GameObject pepperSecretbeachVoyeur01DialogueScene4;
        public static GameObject pepperSecretbeachVoyeur01DialogueScene5;
        public static GameObject pepperSecretbeachVoyeur01DialogueActivator;
        public static GameObject pepperSecretbeachVoyeur01DialogueFinisher;
        public static GameObject pepperSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject pepperSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject rapiSecretbeachVoyeur01Dialogue;
        public static GameObject rapiSecretbeachVoyeur01DialogueScene1;
        public static GameObject rapiSecretbeachVoyeur01DialogueScene2;
        public static GameObject rapiSecretbeachVoyeur01DialogueScene3;
        public static GameObject rapiSecretbeachVoyeur01DialogueScene4;
        public static GameObject rapiSecretbeachVoyeur01DialogueScene5;
        public static GameObject rapiSecretbeachVoyeur01DialogueActivator;
        public static GameObject rapiSecretbeachVoyeur01DialogueFinisher;
        public static GameObject rapiSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject rapiSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject rosannaSecretbeachVoyeur01Dialogue;
        public static GameObject rosannaSecretbeachVoyeur01DialogueScene1;
        public static GameObject rosannaSecretbeachVoyeur01DialogueScene2;
        public static GameObject rosannaSecretbeachVoyeur01DialogueScene3;
        public static GameObject rosannaSecretbeachVoyeur01DialogueScene4;
        public static GameObject rosannaSecretbeachVoyeur01DialogueScene5;
        public static GameObject rosannaSecretbeachVoyeur01DialogueActivator;
        public static GameObject rosannaSecretbeachVoyeur01DialogueFinisher;
        public static GameObject rosannaSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject rosannaSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject sakuraSecretbeachVoyeur01Dialogue;
        public static GameObject sakuraSecretbeachVoyeur01DialogueScene1;
        public static GameObject sakuraSecretbeachVoyeur01DialogueScene2;
        public static GameObject sakuraSecretbeachVoyeur01DialogueScene3;
        public static GameObject sakuraSecretbeachVoyeur01DialogueScene4;
        public static GameObject sakuraSecretbeachVoyeur01DialogueScene5;
        public static GameObject sakuraSecretbeachVoyeur01DialogueActivator;
        public static GameObject sakuraSecretbeachVoyeur01DialogueFinisher;
        public static GameObject sakuraSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject sakuraSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject toveSecretbeachVoyeur01Dialogue;
        public static GameObject toveSecretbeachVoyeur01DialogueScene1;
        public static GameObject toveSecretbeachVoyeur01DialogueScene2;
        public static GameObject toveSecretbeachVoyeur01DialogueScene3;
        public static GameObject toveSecretbeachVoyeur01DialogueScene4;
        public static GameObject toveSecretbeachVoyeur01DialogueScene5;
        public static GameObject toveSecretbeachVoyeur01DialogueActivator;
        public static GameObject toveSecretbeachVoyeur01DialogueFinisher;
        public static GameObject toveSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject toveSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject viperSecretbeachVoyeur01Dialogue;
        public static GameObject viperSecretbeachVoyeur01DialogueScene1;
        public static GameObject viperSecretbeachVoyeur01DialogueScene2;
        public static GameObject viperSecretbeachVoyeur01DialogueScene3;
        public static GameObject viperSecretbeachVoyeur01DialogueScene4;
        public static GameObject viperSecretbeachVoyeur01DialogueScene5;
        public static GameObject viperSecretbeachVoyeur01DialogueActivator;
        public static GameObject viperSecretbeachVoyeur01DialogueFinisher;
        public static GameObject viperSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject viperSecretbeachVoyeur01DialogueSpriteFocus;

        public static GameObject yanSecretbeachVoyeur01Dialogue;
        public static GameObject yanSecretbeachVoyeur01DialogueScene1;
        public static GameObject yanSecretbeachVoyeur01DialogueScene2;
        public static GameObject yanSecretbeachVoyeur01DialogueScene3;
        public static GameObject yanSecretbeachVoyeur01DialogueScene4;
        public static GameObject yanSecretbeachVoyeur01DialogueScene5;
        public static GameObject yanSecretbeachVoyeur01DialogueActivator;
        public static GameObject yanSecretbeachVoyeur01DialogueFinisher;
        public static GameObject yanSecretbeachVoyeur01DialogueMouthActivator;
        public static GameObject yanSecretbeachVoyeur01DialogueSpriteFocus;
        #endregion

        public static GameObject backupHaus;

        public static GameObject vanillaDiagNoOneThere;
        public static GameObject vanillaDiagRiverRepeat;

        public static GameObject snekForestEgg01Dialogue;
        public static GameObject snekForestEgg01DialogueScene1;
        public static GameObject snekForestEgg01DialogueActivator;
        public static GameObject snekForestEgg01DialogueFinisher;

        public static GameObject audioHarborHomeMusic;
        public static GameObject audioShower;
        public static GameObject audioShowerQuiet;

        #region Harbor Home Talk Panel
        public static GameObject hhTalkPanel;
        public static GameObject hhTalkButton1;
        public static GameObject hhTalkButton2;
        private static GameObject vanillaTalkButton;
        private static bool hhTalkPanelInitialized = false;
        private static string hhTalkLastActiveRoom = null;
        private static float hhTalkLastUpdateTime = 0f;
        private static float hhTalkUpdateInterval = 0.3f;
        private static int hhTalkLastScheduleVersion = -1;

        // Cached rounded-corner sprites
        private static Sprite hhPanelSprite;
        private static Sprite hhButtonLeftSprite;   // rounded left corners, flat right
        private static Sprite hhButtonRightSprite;  // flat left corners, rounded right
        private static Sprite hhButtonFullSprite;   // all 4 corners rounded (single button)
        private static Sprite hhLabelLeftSprite;    // rounded bottom-left only
        private static Sprite hhLabelRightSprite;   // rounded bottom-right only
        private static Sprite hhLabelFullSprite;    // both bottom corners rounded (single button)

        // Maps room names to their level GameObjects for quick lookup
        private static readonly string[] hhTalkRoomNames = new string[]
        {
            "HarborHomeBathroom", "HarborHomeBedroom", "HarborHomeCloset",
            "HarborHomeKitchen", "HarborHomeLivingRoom", "HarborHomePool"
        };

        // Character names that can appear in HH rooms
        private static readonly string[] hhTalkCharacterNames = new string[]
        {
            "Amber", "Claire", "Sarah",
            "Anis", "Centi", "Dorothy", "Elegg", "Frima", "Guilty", "Helm",
            "Maiden", "Mary", "Mast", "Neon", "Pepper", "Rapi", "Rosanna",
            "Sakura", "Tove", "Viper", "Yan"
        };
        #endregion

        public static bool loadedDialogues = false;
        public static bool dialoguePlaying = false;
        public static bool dialoguePlayingVanilla = false;

        private static bool dialoguePlayingVanillaInvokeRepeatingRunning = false;

        private void MonitorRoomTalkChildren()
        {
            if (Core.roomTalk == null)
            {
                Debug.LogWarning("Core.roomTalk is null. Cannot monitor children.");
                return;
            }

            bool anyChildActive = false;
            foreach (Transform child in Core.roomTalk)
            {
                if (child.name != "Always_Active" && child.gameObject.activeSelf)
                {
                    anyChildActive = true;
                    break;
                }
            }

            //Debug.Log("Monitoring.");
            dialoguePlayingVanilla = anyChildActive;
            //Debug.Log("dialoguePlayingVanilla = " + dialoguePlayingVanilla);
        }

        
        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedDialogues && Places.loadedPlaces)
                {
                    backupHaus = GameObject.Instantiate(new GameObject());
                    backupHaus.name = "BackupHaus";

                    vanillaDiagNoOneThere = Places.roomTalkParkingLot.transform.Find("NoOneThere").gameObject;
                    vanillaDiagRiverRepeat = Places.roomTalkPark.transform.Find("RiverRepeat").gameObject;

                    badWeatherDialogue = CreateNewDialogue("BadWeather", Places.secretBeachRoomtalk.transform);
                    badWeatherDialogueActivator = badWeatherDialogue.transform.Find("DialogueActivator").gameObject;
                    badWeatherDialogueFinisher = badWeatherDialogue.transform.Find("DialogueFinisher").gameObject;

                    #region Secret Beach Initialization
                    sBDialogueMainFirst = CreateNewDialogue("SBDialogueMainFirst", Places.secretBeachRoomtalk.transform);
                    sBDialogueMainFirstScene1 = sBDialogueMainFirst.transform.Find("Scene1").gameObject;
                    sBDialogueMainFirstScene2 = sBDialogueMainFirst.transform.Find("Scene2").gameObject;
                    sBDialogueMainFirstScene3 = sBDialogueMainFirst.transform.Find("Scene3").gameObject;
                    sBDialogueMainFirstScene4 = sBDialogueMainFirst.transform.Find("Scene4").gameObject;
                    sBDialogueMainFirstScene5 = sBDialogueMainFirst.transform.Find("Scene5").gameObject;
                    sBDialogueMainFirstDialogueActivator = sBDialogueMainFirst.transform.Find("DialogueActivator").gameObject;
                    sBDialogueMainFirstDialogueFinisher = sBDialogueMainFirst.transform.Find("DialogueFinisher").gameObject;
                    sBDialogueMainFirstMouthActivator = sBDialogueMainFirst.transform.Find("MouthActivator").gameObject;
                    sBDialogueMainFirstSpriteFocus = sBDialogueMainFirst.transform.Find("SpriteFocus").gameObject;

                    sBDialogueMain = CreateNewDialogue("SBDialogueMain", Places.secretBeachRoomtalk.transform);
                    sBDialogueMainScene1 = sBDialogueMain.transform.Find("Scene1").gameObject;
                    sBDialogueMainScene2 = sBDialogueMain.transform.Find("Scene2").gameObject;
                    sBDialogueMainScene3 = sBDialogueMain.transform.Find("Scene3").gameObject;
                    sBDialogueMainScene4 = sBDialogueMain.transform.Find("Scene4").gameObject;
                    sBDialogueMainScene5 = sBDialogueMain.transform.Find("Scene5").gameObject;
                    sBDialogueMainDialogueActivator = sBDialogueMain.transform.Find("DialogueActivator").gameObject;
                    sBDialogueMainDialogueFinisher = sBDialogueMain.transform.Find("DialogueFinisher").gameObject;
                    sBDialogueMainMouthActivator = sBDialogueMain.transform.Find("MouthActivator").gameObject;
                    sBDialogueMainSpriteFocus = sBDialogueMain.transform.Find("SpriteFocus").gameObject;

                    sBDialogueMainGK = CreateNewDialogue("SBDialogueMainGatekeeper", Places.secretBeachRoomtalk.transform);
                    sBDialogueMainGKScene1 = sBDialogueMainGK.transform.Find("Scene1").gameObject;
                    sBDialogueMainGKScene2 = sBDialogueMainGK.transform.Find("Scene2").gameObject;
                    sBDialogueMainGKScene3 = sBDialogueMainGK.transform.Find("Scene3").gameObject;
                    sBDialogueMainGKScene4 = sBDialogueMainGK.transform.Find("Scene4").gameObject;
                    sBDialogueMainGKScene5 = sBDialogueMainGK.transform.Find("Scene5").gameObject;
                    sBDialogueMainGKDialogueActivator = sBDialogueMainGK.transform.Find("DialogueActivator").gameObject;
                    sBDialogueMainGKDialogueFinisher = sBDialogueMainGK.transform.Find("DialogueFinisher").gameObject;
                    sBDialogueMainGKMouthActivator = sBDialogueMainGK.transform.Find("MouthActivator").gameObject;
                    sBDialogueMainGKSpriteFocus = sBDialogueMainGK.transform.Find("SpriteFocus").gameObject;

                    sBDialogueStory01 = CreateNewDialogue("SBDialogueStory01", Places.secretBeachRoomtalk.transform);
                    sBDialogueStory01Scene1 = sBDialogueStory01.transform.Find("Scene1").gameObject;
                    sBDialogueStory01Scene2 = sBDialogueStory01.transform.Find("Scene2").gameObject;
                    sBDialogueStory01Scene3 = sBDialogueStory01.transform.Find("Scene3").gameObject;
                    sBDialogueStory01Scene4 = sBDialogueStory01.transform.Find("Scene4").gameObject;
                    sBDialogueStory01Scene5 = sBDialogueStory01.transform.Find("Scene5").gameObject;
                    sBDialogueStory01DialogueActivator = sBDialogueStory01.transform.Find("DialogueActivator").gameObject;
                    sBDialogueStory01DialogueFinisher = sBDialogueStory01.transform.Find("DialogueFinisher").gameObject;
                    sBDialogueStory01MouthActivator = sBDialogueStory01.transform.Find("MouthActivator").gameObject;
                    sBDialogueStory01SpriteFocus = sBDialogueStory01.transform.Find("SpriteFocus").gameObject;
                    #endregion
                    #region Mountain Lab Initialization
                    mLDialogueMainFirst = CreateNewDialogue("MLDialogueStory02", Places.mountainLabRoomtalk.transform);
                    mLDialogueMainFirstScene1 = mLDialogueMainFirst.transform.Find("Scene1").gameObject;
                    mLDialogueMainFirstScene2 = mLDialogueMainFirst.transform.Find("Scene2").gameObject;
                    mLDialogueMainFirstScene3 = mLDialogueMainFirst.transform.Find("Scene3").gameObject;
                    mLDialogueMainFirstScene4 = mLDialogueMainFirst.transform.Find("Scene4").gameObject;
                    mLDialogueMainFirstScene5 = mLDialogueMainFirst.transform.Find("Scene5").gameObject;
                    mLDialogueMainFirstDialogueActivator = mLDialogueMainFirst.transform.Find("DialogueActivator").gameObject;
                    mLDialogueMainFirstDialogueFinisher = mLDialogueMainFirst.transform.Find("DialogueFinisher").gameObject;
                    mLDialogueMainFirstMouthActivator = mLDialogueMainFirst.transform.Find("MouthActivator").gameObject;
                    mLDialogueMainFirstSpriteFocus = mLDialogueMainFirst.transform.Find("SpriteFocus").gameObject;

                    mLDialogueMainStory03 = CreateNewDialogue("MLDialogueStory03", Places.mountainLabRoomtalk.transform);
                    mLDialogueMainStory03Scene1 = mLDialogueMainStory03.transform.Find("Scene1").gameObject;
                    mLDialogueMainStory03Scene2 = mLDialogueMainStory03.transform.Find("Scene2").gameObject;
                    mLDialogueMainStory03Scene3 = mLDialogueMainStory03.transform.Find("Scene3").gameObject;
                    mLDialogueMainStory03Scene4 = mLDialogueMainStory03.transform.Find("Scene4").gameObject;
                    mLDialogueMainStory03Scene5 = mLDialogueMainStory03.transform.Find("Scene5").gameObject;
                    mLDialogueMainStory03DialogueActivator = mLDialogueMainStory03.transform.Find("DialogueActivator").gameObject;
                    mLDialogueMainStory03DialogueFinisher = mLDialogueMainStory03.transform.Find("DialogueFinisher").gameObject;
                    mLDialogueMainStory03MouthActivator = mLDialogueMainStory03.transform.Find("MouthActivator").gameObject;
                    mLDialogueMainStory03SpriteFocus = mLDialogueMainStory03.transform.Find("SpriteFocus").gameObject;

                    mLDialogueMainStory04 = CreateNewDialogue("MLDialogueStory04", Places.mountainLabRoomtalk.transform);
                    mLDialogueMainStory04Scene1 = mLDialogueMainStory04.transform.Find("Scene1").gameObject;
                    mLDialogueMainStory04Scene2 = mLDialogueMainStory04.transform.Find("Scene2").gameObject;
                    mLDialogueMainStory04Scene3 = mLDialogueMainStory04.transform.Find("Scene3").gameObject;
                    mLDialogueMainStory04Scene4 = mLDialogueMainStory04.transform.Find("Scene4").gameObject;
                    mLDialogueMainStory04Scene5 = mLDialogueMainStory04.transform.Find("Scene5").gameObject;
                    mLDialogueMainStory04DialogueActivator = mLDialogueMainStory04.transform.Find("DialogueActivator").gameObject;
                    mLDialogueMainStory04DialogueFinisher = mLDialogueMainStory04.transform.Find("DialogueFinisher").gameObject;
                    mLDialogueMainStory04MouthActivator = mLDialogueMainStory04.transform.Find("MouthActivator").gameObject;
                    mLDialogueMainStory04SpriteFocus = mLDialogueMainStory04.transform.Find("SpriteFocus").gameObject;
                    #endregion
                    #region Gift Shop Initialization
                    gSDialogueMainFirst = CreateNewDialogue("GSDialogueStory05", Places.giftShopRoomtalk.transform);
                    gSDialogueMainFirstScene1 = gSDialogueMainFirst.transform.Find("Scene1").gameObject;
                    gSDialogueMainFirstScene2 = gSDialogueMainFirst.transform.Find("Scene2").gameObject;
                    gSDialogueMainFirstScene3 = gSDialogueMainFirst.transform.Find("Scene3").gameObject;
                    gSDialogueMainFirstScene4 = gSDialogueMainFirst.transform.Find("Scene4").gameObject;
                    gSDialogueMainFirstScene5 = gSDialogueMainFirst.transform.Find("Scene5").gameObject;
                    gSDialogueMainFirstDialogueActivator = gSDialogueMainFirst.transform.Find("DialogueActivator").gameObject;
                    gSDialogueMainFirstDialogueFinisher = gSDialogueMainFirst.transform.Find("DialogueFinisher").gameObject;
                    gSDialogueMainFirstMouthActivator = gSDialogueMainFirst.transform.Find("MouthActivator").gameObject;
                    gSDialogueMainFirstSpriteFocus = gSDialogueMainFirst.transform.Find("SpriteFocus").gameObject;
                    #endregion
                    #region Harbor Home Initialization
                    sarahDialogueBuyHH = CreateNewDialogue("SarahDialogueBuyHH", Places.harborHouseEntranceRoomtalk.transform);
                    sarahDialogueBuyHHScene1 = sarahDialogueBuyHH.transform.Find("Scene1").gameObject;
                    sarahDialogueBuyHHScene2 = sarahDialogueBuyHH.transform.Find("Scene2").gameObject;
                    sarahDialogueBuyHHScene3 = sarahDialogueBuyHH.transform.Find("Scene3").gameObject;
                    sarahDialogueBuyHHScene4 = sarahDialogueBuyHH.transform.Find("Scene4").gameObject;
                    sarahDialogueBuyHHScene5 = sarahDialogueBuyHH.transform.Find("Scene5").gameObject;
                    sarahDialogueBuyHHDialogueActivator = sarahDialogueBuyHH.transform.Find("DialogueActivator").gameObject;
                    sarahDialogueBuyHHDialogueFinisher = sarahDialogueBuyHH.transform.Find("DialogueFinisher").gameObject;
                    sarahDialogueBuyHHMouthActivator = sarahDialogueBuyHH.transform.Find("MouthActivator").gameObject;
                    sarahDialogueBuyHHSpriteFocus = sarahDialogueBuyHH.transform.Find("SpriteFocus").gameObject;
                    #endregion
                    #region Character Initialization

                    amberDefaultDialogue = CreateNewDialogue("AmberDialogueDefault", Places.mountainLabRoomtalk.transform);
                    amberDefaultDialogueScene1 = amberDefaultDialogue.transform.Find("Scene1").gameObject;
                    amberDefaultDialogueScene2 = amberDefaultDialogue.transform.Find("Scene2").gameObject;
                    amberDefaultDialogueScene3 = amberDefaultDialogue.transform.Find("Scene3").gameObject;
                    amberDefaultDialogueScene4 = amberDefaultDialogue.transform.Find("Scene4").gameObject;
                    amberDefaultDialogueScene5 = amberDefaultDialogue.transform.Find("Scene5").gameObject;
                    amberDefaultDialogueDialogueActivator = amberDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    amberDefaultDialogueDialogueFinisher = amberDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    amberDefaultDialogueMouthActivator = amberDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    amberDefaultDialogueSpriteFocus = amberDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    claireDefaultDialogue = CreateNewDialogue("ClaireDialogueDefault", Places.giftShopInteriorRoomtalk.transform);
                    claireDefaultDialogueScene1 = claireDefaultDialogue.transform.Find("Scene1").gameObject;
                    claireDefaultDialogueScene2 = claireDefaultDialogue.transform.Find("Scene2").gameObject;
                    claireDefaultDialogueScene3 = claireDefaultDialogue.transform.Find("Scene3").gameObject;
                    claireDefaultDialogueScene4 = claireDefaultDialogue.transform.Find("Scene4").gameObject;
                    claireDefaultDialogueScene5 = claireDefaultDialogue.transform.Find("Scene5").gameObject;
                    claireDefaultDialogueDialogueActivator = claireDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    claireDefaultDialogueDialogueFinisher = claireDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    claireDefaultDialogueMouthActivator = claireDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    claireDefaultDialogueSpriteFocus = claireDefaultDialogue.transform.Find("SpriteFocus").gameObject;



                    anisDefaultDialogue = CreateNewDialogue("AnisDialogueDefault", Places.mountainLabRoomNikkeAnisRoomtalk.transform);
                    anisDefaultDialogueScene1 = anisDefaultDialogue.transform.Find("Scene1").gameObject;
                    anisDefaultDialogueScene2 = anisDefaultDialogue.transform.Find("Scene2").gameObject;
                    anisDefaultDialogueScene3 = anisDefaultDialogue.transform.Find("Scene3").gameObject;
                    anisDefaultDialogueScene4 = anisDefaultDialogue.transform.Find("Scene4").gameObject;
                    anisDefaultDialogueScene5 = anisDefaultDialogue.transform.Find("Scene5").gameObject;
                    anisDefaultDialogueDialogueActivator = anisDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    anisDefaultDialogueDialogueFinisher = anisDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    anisDefaultDialogueMouthActivator = anisDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    anisDefaultDialogueSpriteFocus = anisDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    anisDefaultHHDialogue = CreateNewDialogue("AnisDialogueHHDefault", Places.harborHomeLivingroomRoomtalk.transform);
                    anisDefaultHHDialogueScene1 = anisDefaultHHDialogue.transform.Find("Scene1").gameObject;
                    anisDefaultHHDialogueScene2 = anisDefaultHHDialogue.transform.Find("Scene2").gameObject;
                    anisDefaultHHDialogueScene3 = anisDefaultHHDialogue.transform.Find("Scene3").gameObject;
                    anisDefaultHHDialogueScene4 = anisDefaultHHDialogue.transform.Find("Scene4").gameObject;
                    anisDefaultHHDialogueScene5 = anisDefaultHHDialogue.transform.Find("Scene5").gameObject;
                    anisDefaultHHDialogueDialogueActivator = anisDefaultHHDialogue.transform.Find("DialogueActivator").gameObject;
                    anisDefaultHHDialogueDialogueFinisher = anisDefaultHHDialogue.transform.Find("DialogueFinisher").gameObject;
                    anisDefaultHHDialogueMouthActivator = anisDefaultHHDialogue.transform.Find("MouthActivator").gameObject;
                    anisDefaultHHDialogueOutfitDefault = anisDefaultHHDialogue.transform.Find("OutfitDefault").gameObject;
                    anisDefaultHHDialogueOutfitSwim = anisDefaultHHDialogue.transform.Find("OutfitSwim").gameObject;
                    anisDefaultHHDialogueSpriteFocus = anisDefaultHHDialogue.transform.Find("SpriteFocus").gameObject;

                    anisGiftDialogue = CreateNewDialogue("AnisDialogueGift", Places.mountainLabRoomNikkeAnisRoomtalk.transform);
                    anisGiftDialogueScene1 = anisGiftDialogue.transform.Find("Scene1").gameObject;
                    anisGiftDialogueScene2 = anisGiftDialogue.transform.Find("Scene2").gameObject;
                    anisGiftDialogueScene3 = anisGiftDialogue.transform.Find("Scene3").gameObject;
                    anisGiftDialogueScene4 = anisGiftDialogue.transform.Find("Scene4").gameObject;
                    anisGiftDialogueScene5 = anisGiftDialogue.transform.Find("Scene5").gameObject;
                    anisGiftDialogueDialogueActivator = anisGiftDialogue.transform.Find("DialogueActivator").gameObject;
                    anisGiftDialogueDialogueFinisher = anisGiftDialogue.transform.Find("DialogueFinisher").gameObject;
                    anisGiftDialogueMouthActivator = anisGiftDialogue.transform.Find("MouthActivator").gameObject;
                    anisGiftDialogueSpriteFocus = anisGiftDialogue.transform.Find("SpriteFocus").gameObject;

                    anisRandomDialogue67 = CreateNewDialogue("AnisRandom67", Places.roomTalkMall.transform);
                    anisRandomDialogue67Scene1 = anisRandomDialogue67.transform.Find("Scene1").gameObject;
                    anisRandomDialogue67Scene2 = anisRandomDialogue67.transform.Find("Scene2").gameObject;
                    anisRandomDialogue67Scene3 = anisRandomDialogue67.transform.Find("Scene3").gameObject;
                    anisRandomDialogue67Scene4 = anisRandomDialogue67.transform.Find("Scene4").gameObject;
                    anisRandomDialogue67Scene5 = anisRandomDialogue67.transform.Find("Scene5").gameObject;
                    anisRandomDialogue67DialogueActivator = anisRandomDialogue67.transform.Find("DialogueActivator").gameObject;
                    anisRandomDialogue67DialogueFinisher = anisRandomDialogue67.transform.Find("DialogueFinisher").gameObject;
                    anisRandomDialogue67MouthActivator = anisRandomDialogue67.transform.Find("MouthActivator").gameObject;
                    anisRandomDialogue67SpriteFocus = anisRandomDialogue67.transform.Find("SpriteFocus").gameObject;

                    anisRandomDialogueLabRoomChill01Dialogue = CreateNewDialogue("AnisRandomChill01", Places.mountainLabRoomNikkeAnisRoomtalk.transform);
                    anisRandomDialogueLabRoomChill01DialogueScene1 = anisRandomDialogueLabRoomChill01Dialogue.transform.Find("Scene1").gameObject;
                    anisRandomDialogueLabRoomChill01DialogueScene2 = anisRandomDialogueLabRoomChill01Dialogue.transform.Find("Scene2").gameObject;
                    anisRandomDialogueLabRoomChill01DialogueScene3 = anisRandomDialogueLabRoomChill01Dialogue.transform.Find("Scene3").gameObject;
                    anisRandomDialogueLabRoomChill01DialogueScene4 = anisRandomDialogueLabRoomChill01Dialogue.transform.Find("Scene4").gameObject;
                    anisRandomDialogueLabRoomChill01DialogueScene5 = anisRandomDialogueLabRoomChill01Dialogue.transform.Find("Scene5").gameObject;
                    anisRandomDialogueLabRoomChill01DialogueScene6 = anisRandomDialogueLabRoomChill01Dialogue.transform.Find("Scene6").gameObject;
                    anisRandomDialogueLabRoomChill01DialogueDialogueActivator = anisRandomDialogueLabRoomChill01Dialogue.transform.Find("DialogueActivator").gameObject;
                    anisRandomDialogueLabRoomChill01DialogueDialogueFinisher = anisRandomDialogueLabRoomChill01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    anisRandomDialogueLabRoomChill01DialogueMouthActivator = anisRandomDialogueLabRoomChill01Dialogue.transform.Find("MouthActivator").gameObject;
                    anisRandomDialogueLabRoomChill01DialogueSpriteFocus = anisRandomDialogueLabRoomChill01Dialogue.transform.Find("SpriteFocus").gameObject;

                    anisAffection01Dialogue = CreateNewDialogue("AnisDialogueAffection01", Places.roomTalkDowntown.transform);
                    anisAffection01DialogueScene1 = anisAffection01Dialogue.transform.Find("Scene1").gameObject;
                    anisAffection01DialogueScene2 = anisAffection01Dialogue.transform.Find("Scene2").gameObject;
                    anisAffection01DialogueScene3 = anisAffection01Dialogue.transform.Find("Scene3").gameObject;
                    anisAffection01DialogueScene4 = anisAffection01Dialogue.transform.Find("Scene4").gameObject;
                    anisAffection01DialogueScene5 = anisAffection01Dialogue.transform.Find("Scene5").gameObject;
                    anisAffection01DialogueDialogueActivator = anisAffection01Dialogue.transform.Find("DialogueActivator").gameObject;
                    anisAffection01DialogueDialogueFinisher = anisAffection01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    anisAffection01DialogueMouthActivator = anisAffection01Dialogue.transform.Find("MouthActivator").gameObject;
                    anisAffection01DialogueSpriteFocus = anisAffection01Dialogue.transform.Find("SpriteFocus").gameObject;

                    anisAffection02Dialogue = CreateNewDialogue("AnisDialogueAffection02", Places.roomTalkMall.transform);
                    anisAffection02DialogueScene1 = anisAffection02Dialogue.transform.Find("Scene1").gameObject;
                    anisAffection02DialogueScene2 = anisAffection02Dialogue.transform.Find("Scene2").gameObject;
                    anisAffection02DialogueScene3 = anisAffection02Dialogue.transform.Find("Scene3").gameObject;
                    anisAffection02DialogueScene4 = anisAffection02Dialogue.transform.Find("Scene4").gameObject;
                    anisAffection02DialogueScene5 = anisAffection02Dialogue.transform.Find("Scene5").gameObject;
                    anisAffection02DialogueScene6 = anisAffection02Dialogue.transform.Find("Scene6").gameObject;
                    anisAffection02DialogueScene7 = anisAffection02Dialogue.transform.Find("Scene7").gameObject;
                    anisAffection02DialogueScene8 = anisAffection02Dialogue.transform.Find("Scene8").gameObject;
                    anisAffection02DialogueDialogueActivator = anisAffection02Dialogue.transform.Find("DialogueActivator").gameObject;
                    anisAffection02DialogueDialogueFinisher = anisAffection02Dialogue.transform.Find("DialogueFinisher").gameObject;
                    anisAffection02DialogueMouthActivator = anisAffection02Dialogue.transform.Find("MouthActivator").gameObject;
                    anisAffection02DialogueSpriteFocus = anisAffection02Dialogue.transform.Find("SpriteFocus").gameObject;

                    anisAffection03Dialogue = CreateNewDialogue("AnisDialogueAffection03", Places.secretBeachRoomtalk.transform);
                    anisAffection03DialogueScene1 = anisAffection03Dialogue.transform.Find("Scene1").gameObject;
                    anisAffection03DialogueScene2 = anisAffection03Dialogue.transform.Find("Scene2").gameObject;
                    anisAffection03DialogueScene3 = anisAffection03Dialogue.transform.Find("Scene3").gameObject;
                    anisAffection03DialogueScene4 = anisAffection03Dialogue.transform.Find("Scene4").gameObject;
                    anisAffection03DialogueScene5 = anisAffection03Dialogue.transform.Find("Scene5").gameObject;
                    anisAffection03DialogueDialogueActivator = anisAffection03Dialogue.transform.Find("DialogueActivator").gameObject;
                    anisAffection03DialogueDialogueFinisher = anisAffection03Dialogue.transform.Find("DialogueFinisher").gameObject;
                    anisAffection03DialogueMouthActivator = anisAffection03Dialogue.transform.Find("MouthActivator").gameObject;
                    anisAffection03DialogueSpriteFocus = anisAffection03Dialogue.transform.Find("SpriteFocus").gameObject;



                    centiDefaultDialogue = CreateNewDialogue("CentiDialogueDefault", Places.mountainLabRoomNikkeCentiRoomtalk.transform);
                    centiDefaultDialogueScene1 = centiDefaultDialogue.transform.Find("Scene1").gameObject;
                    centiDefaultDialogueScene2 = centiDefaultDialogue.transform.Find("Scene2").gameObject;
                    centiDefaultDialogueScene3 = centiDefaultDialogue.transform.Find("Scene3").gameObject;
                    centiDefaultDialogueScene4 = centiDefaultDialogue.transform.Find("Scene4").gameObject;
                    centiDefaultDialogueScene5 = centiDefaultDialogue.transform.Find("Scene5").gameObject;
                    centiDefaultDialogueDialogueActivator = centiDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    centiDefaultDialogueDialogueFinisher = centiDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    centiDefaultDialogueMouthActivator = centiDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    centiDefaultDialogueSpriteFocus = centiDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    dorothyDefaultDialogue = CreateNewDialogue("DorothyDialogueDefault", Places.mountainLabRoomNikkeDorothyRoomtalk.transform);
                    dorothyDefaultDialogueScene1 = dorothyDefaultDialogue.transform.Find("Scene1").gameObject;
                    dorothyDefaultDialogueScene2 = dorothyDefaultDialogue.transform.Find("Scene2").gameObject;
                    dorothyDefaultDialogueScene3 = dorothyDefaultDialogue.transform.Find("Scene3").gameObject;
                    dorothyDefaultDialogueScene4 = dorothyDefaultDialogue.transform.Find("Scene4").gameObject;
                    dorothyDefaultDialogueScene5 = dorothyDefaultDialogue.transform.Find("Scene5").gameObject;
                    dorothyDefaultDialogueDialogueActivator = dorothyDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    dorothyDefaultDialogueDialogueFinisher = dorothyDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    dorothyDefaultDialogueMouthActivator = dorothyDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    dorothyDefaultDialogueSpriteFocus = dorothyDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    eleggDefaultDialogue = CreateNewDialogue("EleggDialogueDefault", Places.mountainLabRoomNikkeEleggRoomtalk.transform);
                    eleggDefaultDialogueScene1 = eleggDefaultDialogue.transform.Find("Scene1").gameObject;
                    eleggDefaultDialogueScene2 = eleggDefaultDialogue.transform.Find("Scene2").gameObject;
                    eleggDefaultDialogueScene3 = eleggDefaultDialogue.transform.Find("Scene3").gameObject;
                    eleggDefaultDialogueScene4 = eleggDefaultDialogue.transform.Find("Scene4").gameObject;
                    eleggDefaultDialogueScene5 = eleggDefaultDialogue.transform.Find("Scene5").gameObject;
                    eleggDefaultDialogueDialogueActivator = eleggDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    eleggDefaultDialogueDialogueFinisher = eleggDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    eleggDefaultDialogueMouthActivator = eleggDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    eleggDefaultDialogueSpriteFocus = eleggDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    frimaDefaultDialogue = CreateNewDialogue("FrimaDialogueDefault", Places.mountainLabRoomNikkeFrimaRoomtalk.transform);
                    frimaDefaultDialogueScene1 = frimaDefaultDialogue.transform.Find("Scene1").gameObject;
                    frimaDefaultDialogueScene2 = frimaDefaultDialogue.transform.Find("Scene2").gameObject;
                    frimaDefaultDialogueScene3 = frimaDefaultDialogue.transform.Find("Scene3").gameObject;
                    frimaDefaultDialogueScene4 = frimaDefaultDialogue.transform.Find("Scene4").gameObject;
                    frimaDefaultDialogueScene5 = frimaDefaultDialogue.transform.Find("Scene5").gameObject;
                    frimaDefaultDialogueDialogueActivator = frimaDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    frimaDefaultDialogueDialogueFinisher = frimaDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    frimaDefaultDialogueMouthActivator = frimaDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    frimaDefaultDialogueSpriteFocus = frimaDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    guiltyDefaultDialogue = CreateNewDialogue("GuiltyDialogueDefault", Places.mountainLabRoomNikkeGuiltyRoomtalk.transform);
                    guiltyDefaultDialogueScene1 = guiltyDefaultDialogue.transform.Find("Scene1").gameObject;
                    guiltyDefaultDialogueScene2 = guiltyDefaultDialogue.transform.Find("Scene2").gameObject;
                    guiltyDefaultDialogueScene3 = guiltyDefaultDialogue.transform.Find("Scene3").gameObject;
                    guiltyDefaultDialogueScene4 = guiltyDefaultDialogue.transform.Find("Scene4").gameObject;
                    guiltyDefaultDialogueScene5 = guiltyDefaultDialogue.transform.Find("Scene5").gameObject;
                    guiltyDefaultDialogueDialogueActivator = guiltyDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    guiltyDefaultDialogueDialogueFinisher = guiltyDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    guiltyDefaultDialogueMouthActivator = guiltyDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    guiltyDefaultDialogueSpriteFocus = guiltyDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    helmDefaultDialogue = CreateNewDialogue("HelmDialogueDefault", Places.mountainLabRoomNikkeHelmRoomtalk.transform);
                    helmDefaultDialogueScene1 = helmDefaultDialogue.transform.Find("Scene1").gameObject;
                    helmDefaultDialogueScene2 = helmDefaultDialogue.transform.Find("Scene2").gameObject;
                    helmDefaultDialogueScene3 = helmDefaultDialogue.transform.Find("Scene3").gameObject;
                    helmDefaultDialogueScene4 = helmDefaultDialogue.transform.Find("Scene4").gameObject;
                    helmDefaultDialogueScene5 = helmDefaultDialogue.transform.Find("Scene5").gameObject;
                    helmDefaultDialogueDialogueActivator = helmDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    helmDefaultDialogueDialogueFinisher = helmDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    helmDefaultDialogueMouthActivator = helmDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    helmDefaultDialogueSpriteFocus = helmDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    maidenDefaultDialogue = CreateNewDialogue("MaidenDialogueDefault", Places.mountainLabRoomNikkeMaidenRoomtalk.transform);
                    maidenDefaultDialogueScene1 = maidenDefaultDialogue.transform.Find("Scene1").gameObject;
                    maidenDefaultDialogueScene2 = maidenDefaultDialogue.transform.Find("Scene2").gameObject;
                    maidenDefaultDialogueScene3 = maidenDefaultDialogue.transform.Find("Scene3").gameObject;
                    maidenDefaultDialogueScene4 = maidenDefaultDialogue.transform.Find("Scene4").gameObject;
                    maidenDefaultDialogueScene5 = maidenDefaultDialogue.transform.Find("Scene5").gameObject;
                    maidenDefaultDialogueDialogueActivator = maidenDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    maidenDefaultDialogueDialogueFinisher = maidenDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    maidenDefaultDialogueMouthActivator = maidenDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    maidenDefaultDialogueSpriteFocus = maidenDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    maryDefaultDialogue = CreateNewDialogue("MaryDialogueDefault", Places.mountainLabRoomNikkeMaryRoomtalk.transform);
                    maryDefaultDialogueScene1 = maryDefaultDialogue.transform.Find("Scene1").gameObject;
                    maryDefaultDialogueScene2 = maryDefaultDialogue.transform.Find("Scene2").gameObject;
                    maryDefaultDialogueScene3 = maryDefaultDialogue.transform.Find("Scene3").gameObject;
                    maryDefaultDialogueScene4 = maryDefaultDialogue.transform.Find("Scene4").gameObject;
                    maryDefaultDialogueScene5 = maryDefaultDialogue.transform.Find("Scene5").gameObject;
                    maryDefaultDialogueDialogueActivator = maryDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    maryDefaultDialogueDialogueFinisher = maryDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    maryDefaultDialogueMouthActivator = maryDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    maryDefaultDialogueSpriteFocus = maryDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    mastDefaultDialogue = CreateNewDialogue("MastDialogueDefault", Places.mountainLabRoomNikkeMastRoomtalk.transform);
                    mastDefaultDialogueScene1 = mastDefaultDialogue.transform.Find("Scene1").gameObject;
                    mastDefaultDialogueScene2 = mastDefaultDialogue.transform.Find("Scene2").gameObject;
                    mastDefaultDialogueScene3 = mastDefaultDialogue.transform.Find("Scene3").gameObject;
                    mastDefaultDialogueScene4 = mastDefaultDialogue.transform.Find("Scene4").gameObject;
                    mastDefaultDialogueScene5 = mastDefaultDialogue.transform.Find("Scene5").gameObject;
                    mastDefaultDialogueDialogueActivator = mastDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    mastDefaultDialogueDialogueFinisher = mastDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    mastDefaultDialogueMouthActivator = mastDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    mastDefaultDialogueSpriteFocus = mastDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    neonDefaultDialogue = CreateNewDialogue("NeonDialogueDefault", Places.mountainLabRoomNikkeNeonRoomtalk.transform);
                    neonDefaultDialogueScene1 = neonDefaultDialogue.transform.Find("Scene1").gameObject;
                    neonDefaultDialogueScene2 = neonDefaultDialogue.transform.Find("Scene2").gameObject;
                    neonDefaultDialogueScene3 = neonDefaultDialogue.transform.Find("Scene3").gameObject;
                    neonDefaultDialogueScene4 = neonDefaultDialogue.transform.Find("Scene4").gameObject;
                    neonDefaultDialogueScene5 = neonDefaultDialogue.transform.Find("Scene5").gameObject;
                    neonDefaultDialogueDialogueActivator = neonDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    neonDefaultDialogueDialogueFinisher = neonDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    neonDefaultDialogueMouthActivator = neonDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    neonDefaultDialogueSpriteFocus = neonDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    pepperDefaultDialogue = CreateNewDialogue("PepperDialogueDefault", Places.mountainLabRoomNikkePepperRoomtalk.transform);
                    pepperDefaultDialogueScene1 = pepperDefaultDialogue.transform.Find("Scene1").gameObject;
                    pepperDefaultDialogueScene2 = pepperDefaultDialogue.transform.Find("Scene2").gameObject;
                    pepperDefaultDialogueScene3 = pepperDefaultDialogue.transform.Find("Scene3").gameObject;
                    pepperDefaultDialogueScene4 = pepperDefaultDialogue.transform.Find("Scene4").gameObject;
                    pepperDefaultDialogueScene5 = pepperDefaultDialogue.transform.Find("Scene5").gameObject;
                    pepperDefaultDialogueDialogueActivator = pepperDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    pepperDefaultDialogueDialogueFinisher = pepperDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    pepperDefaultDialogueMouthActivator = pepperDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    pepperDefaultDialogueSpriteFocus = pepperDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    rapiDefaultDialogue = CreateNewDialogue("RapiDialogueDefault", Places.mountainLabRoomNikkeRapiRoomtalk.transform);
                    rapiDefaultDialogueScene1 = rapiDefaultDialogue.transform.Find("Scene1").gameObject;
                    rapiDefaultDialogueScene2 = rapiDefaultDialogue.transform.Find("Scene2").gameObject;
                    rapiDefaultDialogueScene3 = rapiDefaultDialogue.transform.Find("Scene3").gameObject;
                    rapiDefaultDialogueScene4 = rapiDefaultDialogue.transform.Find("Scene4").gameObject;
                    rapiDefaultDialogueScene5 = rapiDefaultDialogue.transform.Find("Scene5").gameObject;
                    rapiDefaultDialogueDialogueActivator = rapiDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    rapiDefaultDialogueDialogueFinisher = rapiDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    rapiDefaultDialogueMouthActivator = rapiDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    rapiDefaultDialogueSpriteFocus = rapiDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    rosannaDefaultDialogue = CreateNewDialogue("RosannaDialogueDefault", Places.mountainLabRoomNikkeRosannaRoomtalk.transform);
                    rosannaDefaultDialogueScene1 = rosannaDefaultDialogue.transform.Find("Scene1").gameObject;
                    rosannaDefaultDialogueScene2 = rosannaDefaultDialogue.transform.Find("Scene2").gameObject;
                    rosannaDefaultDialogueScene3 = rosannaDefaultDialogue.transform.Find("Scene3").gameObject;
                    rosannaDefaultDialogueScene4 = rosannaDefaultDialogue.transform.Find("Scene4").gameObject;
                    rosannaDefaultDialogueScene5 = rosannaDefaultDialogue.transform.Find("Scene5").gameObject;
                    rosannaDefaultDialogueDialogueActivator = rosannaDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    rosannaDefaultDialogueDialogueFinisher = rosannaDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    rosannaDefaultDialogueMouthActivator = rosannaDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    rosannaDefaultDialogueSpriteFocus = rosannaDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    sakuraDefaultDialogue = CreateNewDialogue("SakuraDialogueDefault", Places.mountainLabRoomNikkeSakuraRoomtalk.transform);
                    sakuraDefaultDialogueScene1 = sakuraDefaultDialogue.transform.Find("Scene1").gameObject;
                    sakuraDefaultDialogueScene2 = sakuraDefaultDialogue.transform.Find("Scene2").gameObject;
                    sakuraDefaultDialogueScene3 = sakuraDefaultDialogue.transform.Find("Scene3").gameObject;
                    sakuraDefaultDialogueScene4 = sakuraDefaultDialogue.transform.Find("Scene4").gameObject;
                    sakuraDefaultDialogueScene5 = sakuraDefaultDialogue.transform.Find("Scene5").gameObject;
                    sakuraDefaultDialogueDialogueActivator = sakuraDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    sakuraDefaultDialogueDialogueFinisher = sakuraDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    sakuraDefaultDialogueMouthActivator = sakuraDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    sakuraDefaultDialogueSpriteFocus = sakuraDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    toveDefaultDialogue = CreateNewDialogue("ToveDialogueDefault", Places.mountainLabRoomNikkeToveRoomtalk.transform);
                    toveDefaultDialogueScene1 = toveDefaultDialogue.transform.Find("Scene1").gameObject;
                    toveDefaultDialogueScene2 = toveDefaultDialogue.transform.Find("Scene2").gameObject;
                    toveDefaultDialogueScene3 = toveDefaultDialogue.transform.Find("Scene3").gameObject;
                    toveDefaultDialogueScene4 = toveDefaultDialogue.transform.Find("Scene4").gameObject;
                    toveDefaultDialogueScene5 = toveDefaultDialogue.transform.Find("Scene5").gameObject;
                    toveDefaultDialogueDialogueActivator = toveDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    toveDefaultDialogueDialogueFinisher = toveDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    toveDefaultDialogueMouthActivator = toveDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    toveDefaultDialogueSpriteFocus = toveDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    viperDefaultDialogue = CreateNewDialogue("ViperDialogueDefault", Places.mountainLabRoomNikkeViperRoomtalk.transform);
                    viperDefaultDialogueScene1 = viperDefaultDialogue.transform.Find("Scene1").gameObject;
                    viperDefaultDialogueScene2 = viperDefaultDialogue.transform.Find("Scene2").gameObject;
                    viperDefaultDialogueScene3 = viperDefaultDialogue.transform.Find("Scene3").gameObject;
                    viperDefaultDialogueScene4 = viperDefaultDialogue.transform.Find("Scene4").gameObject;
                    viperDefaultDialogueScene5 = viperDefaultDialogue.transform.Find("Scene5").gameObject;
                    viperDefaultDialogueDialogueActivator = viperDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    viperDefaultDialogueDialogueFinisher = viperDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    viperDefaultDialogueMouthActivator = viperDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    viperDefaultDialogueSpriteFocus = viperDefaultDialogue.transform.Find("SpriteFocus").gameObject;

                    yanDefaultDialogue = CreateNewDialogue("YanDialogueDefault", Places.mountainLabRoomNikkeYanRoomtalk.transform);
                    yanDefaultDialogueScene1 = yanDefaultDialogue.transform.Find("Scene1").gameObject;
                    yanDefaultDialogueScene2 = yanDefaultDialogue.transform.Find("Scene2").gameObject;
                    yanDefaultDialogueScene3 = yanDefaultDialogue.transform.Find("Scene3").gameObject;
                    yanDefaultDialogueScene4 = yanDefaultDialogue.transform.Find("Scene4").gameObject;
                    yanDefaultDialogueScene5 = yanDefaultDialogue.transform.Find("Scene5").gameObject;
                    yanDefaultDialogueDialogueActivator = yanDefaultDialogue.transform.Find("DialogueActivator").gameObject;
                    yanDefaultDialogueDialogueFinisher = yanDefaultDialogue.transform.Find("DialogueFinisher").gameObject;
                    yanDefaultDialogueMouthActivator = yanDefaultDialogue.transform.Find("MouthActivator").gameObject;
                    yanDefaultDialogueSpriteFocus = yanDefaultDialogue.transform.Find("SpriteFocus").gameObject;
                    #endregion
                    #region Event Initialization
                    amberHospitalhallwayEvent01Dialogue = CreateNewDialogue("AmberDialogueHospitalhallway01", Places.roomTalkHospitalHallway.transform);
                    amberHospitalhallwayEvent01DialogueScene1 = amberHospitalhallwayEvent01Dialogue.transform.Find("Scene1").gameObject;
                    amberHospitalhallwayEvent01DialogueScene2 = amberHospitalhallwayEvent01Dialogue.transform.Find("Scene2").gameObject;
                    amberHospitalhallwayEvent01DialogueScene3 = amberHospitalhallwayEvent01Dialogue.transform.Find("Scene3").gameObject;
                    amberHospitalhallwayEvent01DialogueScene4 = amberHospitalhallwayEvent01Dialogue.transform.Find("Scene4").gameObject;
                    amberHospitalhallwayEvent01DialogueScene5 = amberHospitalhallwayEvent01Dialogue.transform.Find("Scene5").gameObject;
                    amberHospitalhallwayEvent01DialogueDialogueActivator = amberHospitalhallwayEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    amberHospitalhallwayEvent01DialogueDialogueFinisher = amberHospitalhallwayEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    amberHospitalhallwayEvent01DialogueMouthActivator = amberHospitalhallwayEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    amberHospitalhallwayEvent01DialogueSpriteFocus = amberHospitalhallwayEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    anisMallEvent01Dialogue = CreateNewDialogue("AnisDialogueMall01", Places.roomTalkMall.transform);
                    anisMallEvent01DialogueScene1 = anisMallEvent01Dialogue.transform.Find("Scene1").gameObject;
                    anisMallEvent01DialogueScene2 = anisMallEvent01Dialogue.transform.Find("Scene2").gameObject;
                    anisMallEvent01DialogueScene3 = anisMallEvent01Dialogue.transform.Find("Scene3").gameObject;
                    anisMallEvent01DialogueScene4 = anisMallEvent01Dialogue.transform.Find("Scene4").gameObject;
                    anisMallEvent01DialogueScene5 = anisMallEvent01Dialogue.transform.Find("Scene5").gameObject;
                    anisMallEvent01DialogueDialogueActivator = anisMallEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    anisMallEvent01DialogueDialogueFinisher = anisMallEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    anisMallEvent01DialogueMouthActivator = anisMallEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    anisMallEvent01DialogueSpriteFocus = anisMallEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    centiKenshomeEvent01Dialogue = CreateNewDialogue("CentiDialogueKenshome01", Places.roomTalkKensHome.transform);
                    centiKenshomeEvent01DialogueScene1 = centiKenshomeEvent01Dialogue.transform.Find("Scene1").gameObject;
                    centiKenshomeEvent01DialogueScene2 = centiKenshomeEvent01Dialogue.transform.Find("Scene2").gameObject;
                    centiKenshomeEvent01DialogueScene3 = centiKenshomeEvent01Dialogue.transform.Find("Scene3").gameObject;
                    centiKenshomeEvent01DialogueScene4 = centiKenshomeEvent01Dialogue.transform.Find("Scene4").gameObject;
                    centiKenshomeEvent01DialogueScene5 = centiKenshomeEvent01Dialogue.transform.Find("Scene5").gameObject;
                    centiKenshomeEvent01DialogueDialogueActivator = centiKenshomeEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    centiKenshomeEvent01DialogueDialogueFinisher = centiKenshomeEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    centiKenshomeEvent01DialogueMouthActivator = centiKenshomeEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    centiKenshomeEvent01DialogueSpriteFocus = centiKenshomeEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    dorothyParkEvent01Dialogue = CreateNewDialogue("DorothyDialoguePark01", Places.roomTalkPark.transform);
                    dorothyParkEvent01DialogueScene1 = dorothyParkEvent01Dialogue.transform.Find("Scene1").gameObject;
                    dorothyParkEvent01DialogueScene2 = dorothyParkEvent01Dialogue.transform.Find("Scene2").gameObject;
                    dorothyParkEvent01DialogueScene3 = dorothyParkEvent01Dialogue.transform.Find("Scene3").gameObject;
                    dorothyParkEvent01DialogueScene4 = dorothyParkEvent01Dialogue.transform.Find("Scene4").gameObject;
                    dorothyParkEvent01DialogueScene5 = dorothyParkEvent01Dialogue.transform.Find("Scene5").gameObject;
                    dorothyParkEvent01DialogueDialogueActivator = dorothyParkEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    dorothyParkEvent01DialogueDialogueFinisher = dorothyParkEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    dorothyParkEvent01DialogueMouthActivator = dorothyParkEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    dorothyParkEvent01DialogueSpriteFocus = dorothyParkEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    eleggDowntownEvent01Dialogue = CreateNewDialogue("EleggDialogueDowntown01", Places.roomTalkDowntown.transform);
                    eleggDowntownEvent01DialogueScene1 = eleggDowntownEvent01Dialogue.transform.Find("Scene1").gameObject;
                    eleggDowntownEvent01DialogueScene2 = eleggDowntownEvent01Dialogue.transform.Find("Scene2").gameObject;
                    eleggDowntownEvent01DialogueScene3 = eleggDowntownEvent01Dialogue.transform.Find("Scene3").gameObject;
                    eleggDowntownEvent01DialogueScene4 = eleggDowntownEvent01Dialogue.transform.Find("Scene4").gameObject;
                    eleggDowntownEvent01DialogueScene5 = eleggDowntownEvent01Dialogue.transform.Find("Scene5").gameObject;
                    eleggDowntownEvent01DialogueDialogueActivator = eleggDowntownEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    eleggDowntownEvent01DialogueDialogueFinisher = eleggDowntownEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    eleggDowntownEvent01DialogueMouthActivator = eleggDowntownEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    eleggDowntownEvent01DialogueSpriteFocus = eleggDowntownEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    frimaHotelEvent01Dialogue = CreateNewDialogue("FrimaDialogueHotel01", Places.roomTalkHotel.transform);
                    frimaHotelEvent01DialogueScene1 = frimaHotelEvent01Dialogue.transform.Find("Scene1").gameObject;
                    frimaHotelEvent01DialogueScene2 = frimaHotelEvent01Dialogue.transform.Find("Scene2").gameObject;
                    frimaHotelEvent01DialogueScene3 = frimaHotelEvent01Dialogue.transform.Find("Scene3").gameObject;
                    frimaHotelEvent01DialogueScene4 = frimaHotelEvent01Dialogue.transform.Find("Scene4").gameObject;
                    frimaHotelEvent01DialogueScene5 = frimaHotelEvent01Dialogue.transform.Find("Scene5").gameObject;
                    frimaHotelEvent01DialogueDialogueActivator = frimaHotelEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    frimaHotelEvent01DialogueDialogueFinisher = frimaHotelEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    frimaHotelEvent01DialogueMouthActivator = frimaHotelEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    frimaHotelEvent01DialogueSpriteFocus = frimaHotelEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    guiltyParkinglotEvent01Dialogue = CreateNewDialogue("GuiltyDialogueParkinglot01", Places.roomTalkParkingLot.transform);
                    guiltyParkinglotEvent01DialogueScene1 = guiltyParkinglotEvent01Dialogue.transform.Find("Scene1").gameObject;
                    guiltyParkinglotEvent01DialogueScene2 = guiltyParkinglotEvent01Dialogue.transform.Find("Scene2").gameObject;
                    guiltyParkinglotEvent01DialogueScene3 = guiltyParkinglotEvent01Dialogue.transform.Find("Scene3").gameObject;
                    guiltyParkinglotEvent01DialogueScene4 = guiltyParkinglotEvent01Dialogue.transform.Find("Scene4").gameObject;
                    guiltyParkinglotEvent01DialogueScene5 = guiltyParkinglotEvent01Dialogue.transform.Find("Scene5").gameObject;
                    guiltyParkinglotEvent01DialogueDialogueActivator = guiltyParkinglotEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    guiltyParkinglotEvent01DialogueDialogueFinisher = guiltyParkinglotEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    guiltyParkinglotEvent01DialogueMouthActivator = guiltyParkinglotEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    guiltyParkinglotEvent01DialogueSpriteFocus = guiltyParkinglotEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    helmBeachEvent01Dialogue = CreateNewDialogue("HelmDialogueBeach01", Places.roomTalkBeach.transform);
                    helmBeachEvent01DialogueScene1 = helmBeachEvent01Dialogue.transform.Find("Scene1").gameObject;
                    helmBeachEvent01DialogueScene2 = helmBeachEvent01Dialogue.transform.Find("Scene2").gameObject;
                    helmBeachEvent01DialogueScene3 = helmBeachEvent01Dialogue.transform.Find("Scene3").gameObject;
                    helmBeachEvent01DialogueScene4 = helmBeachEvent01Dialogue.transform.Find("Scene4").gameObject;
                    helmBeachEvent01DialogueScene5 = helmBeachEvent01Dialogue.transform.Find("Scene5").gameObject;
                    helmBeachEvent01DialogueDialogueActivator = helmBeachEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    helmBeachEvent01DialogueDialogueFinisher = helmBeachEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    helmBeachEvent01DialogueMouthActivator = helmBeachEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    helmBeachEvent01DialogueSpriteFocus = helmBeachEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    maidenAlleyEvent01Dialogue = CreateNewDialogue("MaidenDialogueAlley01", Places.roomTalkAlley.transform);
                    maidenAlleyEvent01DialogueScene1 = maidenAlleyEvent01Dialogue.transform.Find("Scene1").gameObject;
                    maidenAlleyEvent01DialogueScene2 = maidenAlleyEvent01Dialogue.transform.Find("Scene2").gameObject;
                    maidenAlleyEvent01DialogueScene3 = maidenAlleyEvent01Dialogue.transform.Find("Scene3").gameObject;
                    maidenAlleyEvent01DialogueScene4 = maidenAlleyEvent01Dialogue.transform.Find("Scene4").gameObject;
                    maidenAlleyEvent01DialogueScene5 = maidenAlleyEvent01Dialogue.transform.Find("Scene5").gameObject;
                    maidenAlleyEvent01DialogueDialogueActivator = maidenAlleyEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    maidenAlleyEvent01DialogueDialogueFinisher = maidenAlleyEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    maidenAlleyEvent01DialogueMouthActivator = maidenAlleyEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    maidenAlleyEvent01DialogueSpriteFocus = maidenAlleyEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    maryHospitalhallwayEvent01Dialogue = CreateNewDialogue("MaryDialogueHospitalhallway01", Places.roomTalkHospitalHallway.transform);
                    maryHospitalhallwayEvent01DialogueScene1 = maryHospitalhallwayEvent01Dialogue.transform.Find("Scene1").gameObject;
                    maryHospitalhallwayEvent01DialogueScene2 = maryHospitalhallwayEvent01Dialogue.transform.Find("Scene2").gameObject;
                    maryHospitalhallwayEvent01DialogueScene3 = maryHospitalhallwayEvent01Dialogue.transform.Find("Scene3").gameObject;
                    maryHospitalhallwayEvent01DialogueScene4 = maryHospitalhallwayEvent01Dialogue.transform.Find("Scene4").gameObject;
                    maryHospitalhallwayEvent01DialogueScene5 = maryHospitalhallwayEvent01Dialogue.transform.Find("Scene5").gameObject;
                    maryHospitalhallwayEvent01DialogueDialogueActivator = maryHospitalhallwayEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    maryHospitalhallwayEvent01DialogueDialogueFinisher = maryHospitalhallwayEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    maryHospitalhallwayEvent01DialogueMouthActivator = maryHospitalhallwayEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    maryHospitalhallwayEvent01DialogueSpriteFocus = maryHospitalhallwayEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    mastBeachEvent01Dialogue = CreateNewDialogue("MastDialogueBeach01", Places.roomTalkBeach.transform);
                    mastBeachEvent01DialogueScene1 = mastBeachEvent01Dialogue.transform.Find("Scene1").gameObject;
                    mastBeachEvent01DialogueScene2 = mastBeachEvent01Dialogue.transform.Find("Scene2").gameObject;
                    mastBeachEvent01DialogueScene3 = mastBeachEvent01Dialogue.transform.Find("Scene3").gameObject;
                    mastBeachEvent01DialogueScene4 = mastBeachEvent01Dialogue.transform.Find("Scene4").gameObject;
                    mastBeachEvent01DialogueScene5 = mastBeachEvent01Dialogue.transform.Find("Scene5").gameObject;
                    mastBeachEvent01DialogueDialogueActivator = mastBeachEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    mastBeachEvent01DialogueDialogueFinisher = mastBeachEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    mastBeachEvent01DialogueMouthActivator = mastBeachEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    mastBeachEvent01DialogueSpriteFocus = mastBeachEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    neonTempleEvent01Dialogue = CreateNewDialogue("NeonDialogueTemple01", Places.roomTalkTemple.transform);
                    neonTempleEvent01DialogueScene1 = neonTempleEvent01Dialogue.transform.Find("Scene1").gameObject;
                    neonTempleEvent01DialogueScene2 = neonTempleEvent01Dialogue.transform.Find("Scene2").gameObject;
                    neonTempleEvent01DialogueScene3 = neonTempleEvent01Dialogue.transform.Find("Scene3").gameObject;
                    neonTempleEvent01DialogueScene4 = neonTempleEvent01Dialogue.transform.Find("Scene4").gameObject;
                    neonTempleEvent01DialogueScene5 = neonTempleEvent01Dialogue.transform.Find("Scene5").gameObject;
                    neonTempleEvent01DialogueDialogueActivator = neonTempleEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    neonTempleEvent01DialogueDialogueFinisher = neonTempleEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    neonTempleEvent01DialogueMouthActivator = neonTempleEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    neonTempleEvent01DialogueSpriteFocus = neonTempleEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    pepperHospitalEvent01Dialogue = CreateNewDialogue("PepperDialogueHospital01", Places.roomTalkHospital.transform);
                    pepperHospitalEvent01DialogueScene1 = pepperHospitalEvent01Dialogue.transform.Find("Scene1").gameObject;
                    pepperHospitalEvent01DialogueScene2 = pepperHospitalEvent01Dialogue.transform.Find("Scene2").gameObject;
                    pepperHospitalEvent01DialogueScene3 = pepperHospitalEvent01Dialogue.transform.Find("Scene3").gameObject;
                    pepperHospitalEvent01DialogueScene4 = pepperHospitalEvent01Dialogue.transform.Find("Scene4").gameObject;
                    pepperHospitalEvent01DialogueScene5 = pepperHospitalEvent01Dialogue.transform.Find("Scene5").gameObject;
                    pepperHospitalEvent01DialogueDialogueActivator = pepperHospitalEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    pepperHospitalEvent01DialogueDialogueFinisher = pepperHospitalEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    pepperHospitalEvent01DialogueMouthActivator = pepperHospitalEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    pepperHospitalEvent01DialogueSpriteFocus = pepperHospitalEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    rapiGasstationEvent01Dialogue = CreateNewDialogue("RapiDialogueGasstation01", Places.roomTalkGasStation.transform);
                    rapiGasstationEvent01DialogueScene1 = rapiGasstationEvent01Dialogue.transform.Find("Scene1").gameObject;
                    rapiGasstationEvent01DialogueScene2 = rapiGasstationEvent01Dialogue.transform.Find("Scene2").gameObject;
                    rapiGasstationEvent01DialogueScene3 = rapiGasstationEvent01Dialogue.transform.Find("Scene3").gameObject;
                    rapiGasstationEvent01DialogueScene4 = rapiGasstationEvent01Dialogue.transform.Find("Scene4").gameObject;
                    rapiGasstationEvent01DialogueScene5 = rapiGasstationEvent01Dialogue.transform.Find("Scene5").gameObject;
                    rapiGasstationEvent01DialogueDialogueActivator = rapiGasstationEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    rapiGasstationEvent01DialogueDialogueFinisher = rapiGasstationEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    rapiGasstationEvent01DialogueMouthActivator = rapiGasstationEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    rapiGasstationEvent01DialogueSpriteFocus = rapiGasstationEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    rosannaGabrielsmansionEvent01Dialogue = CreateNewDialogue("RosannaDialogueGabrielsmansion01", Places.roomTalkGabrielsMansion.transform);
                    rosannaGabrielsmansionEvent01DialogueScene1 = rosannaGabrielsmansionEvent01Dialogue.transform.Find("Scene1").gameObject;
                    rosannaGabrielsmansionEvent01DialogueScene2 = rosannaGabrielsmansionEvent01Dialogue.transform.Find("Scene2").gameObject;
                    rosannaGabrielsmansionEvent01DialogueScene3 = rosannaGabrielsmansionEvent01Dialogue.transform.Find("Scene3").gameObject;
                    rosannaGabrielsmansionEvent01DialogueScene4 = rosannaGabrielsmansionEvent01Dialogue.transform.Find("Scene4").gameObject;
                    rosannaGabrielsmansionEvent01DialogueScene5 = rosannaGabrielsmansionEvent01Dialogue.transform.Find("Scene5").gameObject;
                    rosannaGabrielsmansionEvent01DialogueDialogueActivator = rosannaGabrielsmansionEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    rosannaGabrielsmansionEvent01DialogueDialogueFinisher = rosannaGabrielsmansionEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    rosannaGabrielsmansionEvent01DialogueMouthActivator = rosannaGabrielsmansionEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    rosannaGabrielsmansionEvent01DialogueSpriteFocus = rosannaGabrielsmansionEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    sakuraForestEvent01Dialogue = CreateNewDialogue("SakuraDialogueForest01", Places.roomTalkForest.transform);
                    sakuraForestEvent01DialogueScene1 = sakuraForestEvent01Dialogue.transform.Find("Scene1").gameObject;
                    sakuraForestEvent01DialogueScene2 = sakuraForestEvent01Dialogue.transform.Find("Scene2").gameObject;
                    sakuraForestEvent01DialogueScene3 = sakuraForestEvent01Dialogue.transform.Find("Scene3").gameObject;
                    sakuraForestEvent01DialogueScene4 = sakuraForestEvent01Dialogue.transform.Find("Scene4").gameObject;
                    sakuraForestEvent01DialogueScene5 = sakuraForestEvent01Dialogue.transform.Find("Scene5").gameObject;
                    sakuraForestEvent01DialogueDialogueActivator = sakuraForestEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    sakuraForestEvent01DialogueDialogueFinisher = sakuraForestEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    sakuraForestEvent01DialogueMouthActivator = sakuraForestEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    sakuraForestEvent01DialogueSpriteFocus = sakuraForestEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    toveTrailEvent01Dialogue = CreateNewDialogue("ToveDialogueTrail01", Places.roomTalkTrail.transform);
                    toveTrailEvent01DialogueScene1 = toveTrailEvent01Dialogue.transform.Find("Scene1").gameObject;
                    toveTrailEvent01DialogueScene2 = toveTrailEvent01Dialogue.transform.Find("Scene2").gameObject;
                    toveTrailEvent01DialogueScene3 = toveTrailEvent01Dialogue.transform.Find("Scene3").gameObject;
                    toveTrailEvent01DialogueScene4 = toveTrailEvent01Dialogue.transform.Find("Scene4").gameObject;
                    toveTrailEvent01DialogueScene5 = toveTrailEvent01Dialogue.transform.Find("Scene5").gameObject;
                    toveTrailEvent01DialogueDialogueActivator = toveTrailEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    toveTrailEvent01DialogueDialogueFinisher = toveTrailEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    toveTrailEvent01DialogueMouthActivator = toveTrailEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    toveTrailEvent01DialogueSpriteFocus = toveTrailEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    viperVillaEvent01Dialogue = CreateNewDialogue("ViperDialogueVilla01", Places.roomTalkVilla.transform);
                    viperVillaEvent01DialogueScene1 = viperVillaEvent01Dialogue.transform.Find("Scene1").gameObject;
                    viperVillaEvent01DialogueScene2 = viperVillaEvent01Dialogue.transform.Find("Scene2").gameObject;
                    viperVillaEvent01DialogueScene3 = viperVillaEvent01Dialogue.transform.Find("Scene3").gameObject;
                    viperVillaEvent01DialogueScene4 = viperVillaEvent01Dialogue.transform.Find("Scene4").gameObject;
                    viperVillaEvent01DialogueScene5 = viperVillaEvent01Dialogue.transform.Find("Scene5").gameObject;
                    viperVillaEvent01DialogueDialogueActivator = viperVillaEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    viperVillaEvent01DialogueDialogueFinisher = viperVillaEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    viperVillaEvent01DialogueMouthActivator = viperVillaEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    viperVillaEvent01DialogueSpriteFocus = viperVillaEvent01Dialogue.transform.Find("SpriteFocus").gameObject;

                    yanMallEvent01Dialogue = CreateNewDialogue("YanDialogueMall01", Places.roomTalkMall.transform);
                    yanMallEvent01DialogueScene1 = yanMallEvent01Dialogue.transform.Find("Scene1").gameObject;
                    yanMallEvent01DialogueScene2 = yanMallEvent01Dialogue.transform.Find("Scene2").gameObject;
                    yanMallEvent01DialogueScene3 = yanMallEvent01Dialogue.transform.Find("Scene3").gameObject;
                    yanMallEvent01DialogueScene4 = yanMallEvent01Dialogue.transform.Find("Scene4").gameObject;
                    yanMallEvent01DialogueScene5 = yanMallEvent01Dialogue.transform.Find("Scene5").gameObject;
                    yanMallEvent01DialogueDialogueActivator = yanMallEvent01Dialogue.transform.Find("DialogueActivator").gameObject;
                    yanMallEvent01DialogueDialogueFinisher = yanMallEvent01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    yanMallEvent01DialogueMouthActivator = yanMallEvent01Dialogue.transform.Find("MouthActivator").gameObject;
                    yanMallEvent01DialogueSpriteFocus = yanMallEvent01Dialogue.transform.Find("SpriteFocus").gameObject;
                    #endregion
                    #region Voyeur Initialization
                    anisSecretbeachVoyeur01Dialogue = CreateNewDialogue("AnisDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    anisSecretbeachVoyeur01DialogueScene1 = anisSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    anisSecretbeachVoyeur01DialogueScene2 = anisSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    anisSecretbeachVoyeur01DialogueScene3 = anisSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    anisSecretbeachVoyeur01DialogueScene4 = anisSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    anisSecretbeachVoyeur01DialogueScene5 = anisSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    anisSecretbeachVoyeur01DialogueActivator = anisSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    anisSecretbeachVoyeur01DialogueFinisher = anisSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    anisSecretbeachVoyeur01DialogueMouthActivator = anisSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    anisSecretbeachVoyeur01DialogueSpriteFocus = anisSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    centiSecretbeachVoyeur01Dialogue = CreateNewDialogue("CentiDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    centiSecretbeachVoyeur01DialogueScene1 = centiSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    centiSecretbeachVoyeur01DialogueScene2 = centiSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    centiSecretbeachVoyeur01DialogueScene3 = centiSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    centiSecretbeachVoyeur01DialogueScene4 = centiSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    centiSecretbeachVoyeur01DialogueScene5 = centiSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    centiSecretbeachVoyeur01DialogueActivator = centiSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    centiSecretbeachVoyeur01DialogueFinisher = centiSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    centiSecretbeachVoyeur01DialogueMouthActivator = centiSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    centiSecretbeachVoyeur01DialogueSpriteFocus = centiSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    dorothySecretbeachVoyeur01Dialogue = CreateNewDialogue("DorothyDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    dorothySecretbeachVoyeur01DialogueScene1 = dorothySecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    dorothySecretbeachVoyeur01DialogueScene2 = dorothySecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    dorothySecretbeachVoyeur01DialogueScene3 = dorothySecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    dorothySecretbeachVoyeur01DialogueScene4 = dorothySecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    dorothySecretbeachVoyeur01DialogueScene5 = dorothySecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    dorothySecretbeachVoyeur01DialogueActivator = dorothySecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    dorothySecretbeachVoyeur01DialogueFinisher = dorothySecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    dorothySecretbeachVoyeur01DialogueMouthActivator = dorothySecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    dorothySecretbeachVoyeur01DialogueSpriteFocus = dorothySecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    eleggSecretbeachVoyeur01Dialogue = CreateNewDialogue("EleggDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    eleggSecretbeachVoyeur01DialogueScene1 = eleggSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    eleggSecretbeachVoyeur01DialogueScene2 = eleggSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    eleggSecretbeachVoyeur01DialogueScene3 = eleggSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    eleggSecretbeachVoyeur01DialogueScene4 = eleggSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    eleggSecretbeachVoyeur01DialogueScene5 = eleggSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    eleggSecretbeachVoyeur01DialogueScene6 = eleggSecretbeachVoyeur01Dialogue.transform.Find("Scene6").gameObject;
                    eleggSecretbeachVoyeur01DialogueScene7 = eleggSecretbeachVoyeur01Dialogue.transform.Find("Scene7").gameObject;
                    eleggSecretbeachVoyeur01DialogueScene8 = eleggSecretbeachVoyeur01Dialogue.transform.Find("Scene8").gameObject;
                    eleggSecretbeachVoyeur01DialogueActivator = eleggSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    eleggSecretbeachVoyeur01DialogueFinisher = eleggSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    eleggSecretbeachVoyeur01DialogueMouthActivator = eleggSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    eleggSecretbeachVoyeur01DialogueSpriteFocus = eleggSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    frimaSecretbeachVoyeur01Dialogue = CreateNewDialogue("FrimaDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    frimaSecretbeachVoyeur01DialogueScene1 = frimaSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    frimaSecretbeachVoyeur01DialogueScene2 = frimaSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    frimaSecretbeachVoyeur01DialogueScene3 = frimaSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    frimaSecretbeachVoyeur01DialogueScene4 = frimaSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    frimaSecretbeachVoyeur01DialogueScene5 = frimaSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    frimaSecretbeachVoyeur01DialogueActivator = frimaSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    frimaSecretbeachVoyeur01DialogueFinisher = frimaSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    frimaSecretbeachVoyeur01DialogueMouthActivator = frimaSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    frimaSecretbeachVoyeur01DialogueSpriteFocus = frimaSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    guiltySecretbeachVoyeur01Dialogue = CreateNewDialogue("GuiltyDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    guiltySecretbeachVoyeur01DialogueScene1 = guiltySecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    guiltySecretbeachVoyeur01DialogueScene2 = guiltySecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    guiltySecretbeachVoyeur01DialogueScene3 = guiltySecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    guiltySecretbeachVoyeur01DialogueScene4 = guiltySecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    guiltySecretbeachVoyeur01DialogueScene5 = guiltySecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    guiltySecretbeachVoyeur01DialogueActivator = guiltySecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    guiltySecretbeachVoyeur01DialogueFinisher = guiltySecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    guiltySecretbeachVoyeur01DialogueMouthActivator = guiltySecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    guiltySecretbeachVoyeur01DialogueSpriteFocus = guiltySecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    helmSecretbeachVoyeur01Dialogue = CreateNewDialogue("HelmDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    helmSecretbeachVoyeur01DialogueScene1 = helmSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    helmSecretbeachVoyeur01DialogueScene2 = helmSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    helmSecretbeachVoyeur01DialogueScene3 = helmSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    helmSecretbeachVoyeur01DialogueScene4 = helmSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    helmSecretbeachVoyeur01DialogueScene5 = helmSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    helmSecretbeachVoyeur01DialogueActivator = helmSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    helmSecretbeachVoyeur01DialogueFinisher = helmSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    helmSecretbeachVoyeur01DialogueMouthActivator = helmSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    helmSecretbeachVoyeur01DialogueSpriteFocus = helmSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    maidenSecretbeachVoyeur01Dialogue = CreateNewDialogue("MaidenDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    maidenSecretbeachVoyeur01DialogueScene1 = maidenSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    maidenSecretbeachVoyeur01DialogueScene2 = maidenSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    maidenSecretbeachVoyeur01DialogueScene3 = maidenSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    maidenSecretbeachVoyeur01DialogueScene4 = maidenSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    maidenSecretbeachVoyeur01DialogueScene5 = maidenSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    maidenSecretbeachVoyeur01DialogueActivator = maidenSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    maidenSecretbeachVoyeur01DialogueFinisher = maidenSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    maidenSecretbeachVoyeur01DialogueMouthActivator = maidenSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    maidenSecretbeachVoyeur01DialogueSpriteFocus = maidenSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    marySecretbeachVoyeur01Dialogue = CreateNewDialogue("MaryDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    marySecretbeachVoyeur01DialogueScene1 = marySecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    marySecretbeachVoyeur01DialogueScene2 = marySecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    marySecretbeachVoyeur01DialogueScene3 = marySecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    marySecretbeachVoyeur01DialogueScene4 = marySecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    marySecretbeachVoyeur01DialogueScene5 = marySecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    marySecretbeachVoyeur01DialogueActivator = marySecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    marySecretbeachVoyeur01DialogueFinisher = marySecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    marySecretbeachVoyeur01DialogueMouthActivator = marySecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    marySecretbeachVoyeur01DialogueSpriteFocus = marySecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    mastSecretbeachVoyeur01Dialogue = CreateNewDialogue("MastDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    mastSecretbeachVoyeur01DialogueScene1 = mastSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    mastSecretbeachVoyeur01DialogueScene2 = mastSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    mastSecretbeachVoyeur01DialogueScene3 = mastSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    mastSecretbeachVoyeur01DialogueScene4 = mastSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    mastSecretbeachVoyeur01DialogueScene5 = mastSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    mastSecretbeachVoyeur01DialogueActivator = mastSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    mastSecretbeachVoyeur01DialogueFinisher = mastSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    mastSecretbeachVoyeur01DialogueMouthActivator = mastSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    mastSecretbeachVoyeur01DialogueSpriteFocus = mastSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    neonSecretbeachVoyeur01Dialogue = CreateNewDialogue("NeonDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    neonSecretbeachVoyeur01DialogueScene1 = neonSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    neonSecretbeachVoyeur01DialogueScene2 = neonSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    neonSecretbeachVoyeur01DialogueScene3 = neonSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    neonSecretbeachVoyeur01DialogueScene4 = neonSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    neonSecretbeachVoyeur01DialogueScene5 = neonSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    neonSecretbeachVoyeur01DialogueActivator = neonSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    neonSecretbeachVoyeur01DialogueFinisher = neonSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    neonSecretbeachVoyeur01DialogueMouthActivator = neonSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    neonSecretbeachVoyeur01DialogueSpriteFocus = neonSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    pepperSecretbeachVoyeur01Dialogue = CreateNewDialogue("PepperDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    pepperSecretbeachVoyeur01DialogueScene1 = pepperSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    pepperSecretbeachVoyeur01DialogueScene2 = pepperSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    pepperSecretbeachVoyeur01DialogueScene3 = pepperSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    pepperSecretbeachVoyeur01DialogueScene4 = pepperSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    pepperSecretbeachVoyeur01DialogueScene5 = pepperSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    pepperSecretbeachVoyeur01DialogueActivator = pepperSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    pepperSecretbeachVoyeur01DialogueFinisher = pepperSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    pepperSecretbeachVoyeur01DialogueMouthActivator = pepperSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    pepperSecretbeachVoyeur01DialogueSpriteFocus = pepperSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    rapiSecretbeachVoyeur01Dialogue = CreateNewDialogue("RapiDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    rapiSecretbeachVoyeur01DialogueScene1 = rapiSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    rapiSecretbeachVoyeur01DialogueScene2 = rapiSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    rapiSecretbeachVoyeur01DialogueScene3 = rapiSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    rapiSecretbeachVoyeur01DialogueScene4 = rapiSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    rapiSecretbeachVoyeur01DialogueScene5 = rapiSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    rapiSecretbeachVoyeur01DialogueActivator = rapiSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    rapiSecretbeachVoyeur01DialogueFinisher = rapiSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    rapiSecretbeachVoyeur01DialogueMouthActivator = rapiSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    rapiSecretbeachVoyeur01DialogueSpriteFocus = rapiSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    rosannaSecretbeachVoyeur01Dialogue = CreateNewDialogue("RosannaDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    rosannaSecretbeachVoyeur01DialogueScene1 = rosannaSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    rosannaSecretbeachVoyeur01DialogueScene2 = rosannaSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    rosannaSecretbeachVoyeur01DialogueScene3 = rosannaSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    rosannaSecretbeachVoyeur01DialogueScene4 = rosannaSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    rosannaSecretbeachVoyeur01DialogueScene5 = rosannaSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    rosannaSecretbeachVoyeur01DialogueActivator = rosannaSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    rosannaSecretbeachVoyeur01DialogueFinisher = rosannaSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    rosannaSecretbeachVoyeur01DialogueMouthActivator = rosannaSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    rosannaSecretbeachVoyeur01DialogueSpriteFocus = rosannaSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    sakuraSecretbeachVoyeur01Dialogue = CreateNewDialogue("SakuraDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    sakuraSecretbeachVoyeur01DialogueScene1 = sakuraSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    sakuraSecretbeachVoyeur01DialogueScene2 = sakuraSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    sakuraSecretbeachVoyeur01DialogueScene3 = sakuraSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    sakuraSecretbeachVoyeur01DialogueScene4 = sakuraSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    sakuraSecretbeachVoyeur01DialogueScene5 = sakuraSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    sakuraSecretbeachVoyeur01DialogueActivator = sakuraSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    sakuraSecretbeachVoyeur01DialogueFinisher = sakuraSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    sakuraSecretbeachVoyeur01DialogueMouthActivator = sakuraSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    sakuraSecretbeachVoyeur01DialogueSpriteFocus = sakuraSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    toveSecretbeachVoyeur01Dialogue = CreateNewDialogue("ToveDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    toveSecretbeachVoyeur01DialogueScene1 = toveSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    toveSecretbeachVoyeur01DialogueScene2 = toveSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    toveSecretbeachVoyeur01DialogueScene3 = toveSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    toveSecretbeachVoyeur01DialogueScene4 = toveSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    toveSecretbeachVoyeur01DialogueScene5 = toveSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    toveSecretbeachVoyeur01DialogueActivator = toveSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    toveSecretbeachVoyeur01DialogueFinisher = toveSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    toveSecretbeachVoyeur01DialogueMouthActivator = toveSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    toveSecretbeachVoyeur01DialogueSpriteFocus = toveSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    viperSecretbeachVoyeur01Dialogue = CreateNewDialogue("ViperDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    viperSecretbeachVoyeur01DialogueScene1 = viperSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    viperSecretbeachVoyeur01DialogueScene2 = viperSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    viperSecretbeachVoyeur01DialogueScene3 = viperSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    viperSecretbeachVoyeur01DialogueScene4 = viperSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    viperSecretbeachVoyeur01DialogueScene5 = viperSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    viperSecretbeachVoyeur01DialogueActivator = viperSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    viperSecretbeachVoyeur01DialogueFinisher = viperSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    viperSecretbeachVoyeur01DialogueMouthActivator = viperSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    viperSecretbeachVoyeur01DialogueSpriteFocus = viperSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;

                    yanSecretbeachVoyeur01Dialogue = CreateNewDialogue("YanDialogueSecretbeach01", Places.secretBeachRoomtalk.transform);
                    yanSecretbeachVoyeur01DialogueScene1 = yanSecretbeachVoyeur01Dialogue.transform.Find("Scene1").gameObject;
                    yanSecretbeachVoyeur01DialogueScene2 = yanSecretbeachVoyeur01Dialogue.transform.Find("Scene2").gameObject;
                    yanSecretbeachVoyeur01DialogueScene3 = yanSecretbeachVoyeur01Dialogue.transform.Find("Scene3").gameObject;
                    yanSecretbeachVoyeur01DialogueScene4 = yanSecretbeachVoyeur01Dialogue.transform.Find("Scene4").gameObject;
                    yanSecretbeachVoyeur01DialogueScene5 = yanSecretbeachVoyeur01Dialogue.transform.Find("Scene5").gameObject;
                    yanSecretbeachVoyeur01DialogueActivator = yanSecretbeachVoyeur01Dialogue.transform.Find("DialogueActivator").gameObject;
                    yanSecretbeachVoyeur01DialogueFinisher = yanSecretbeachVoyeur01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    yanSecretbeachVoyeur01DialogueMouthActivator = yanSecretbeachVoyeur01Dialogue.transform.Find("MouthActivator").gameObject;
                    yanSecretbeachVoyeur01DialogueSpriteFocus = yanSecretbeachVoyeur01Dialogue.transform.Find("SpriteFocus").gameObject;
                    #endregion

                    snekForestEgg01Dialogue = CreateNewDialogue("SnekForest", Places.roomTalkForest.transform);
                    snekForestEgg01DialogueScene1 = snekForestEgg01Dialogue.transform.Find("Scene1").gameObject;
                    snekForestEgg01DialogueActivator = snekForestEgg01Dialogue.transform.Find("DialogueActivator").gameObject;
                    snekForestEgg01DialogueFinisher = snekForestEgg01Dialogue.transform.Find("DialogueFinisher").gameObject;
                    var actorColors = new Dictionary<string, (byte r, byte g, byte b, byte a)>
                    {
                        { "Amber", (234, 207, 162, 255) },
                        { "Claire", (58, 147, 211, 255) },
                        { "Sarah", (56, 76, 93, 255) },

                        { "Anis", (245, 177, 74, 255) },
                        { "Centi", (245, 159, 41, 255) },
                        { "Dorothy", (248, 189, 203, 255) },
                        { "Elegg", (253, 223, 69, 255) },
                        { "Frima", (192, 199, 205, 255) },
                        { "Guilty", (176, 194, 63, 255) },
                        { "Helm", (129, 190, 233, 255) },
                        { "Maiden", (74, 72, 86, 255) },
                        { "Mary", (115, 200, 231, 255) },
                        { "Mast", (234, 129, 143, 255) },
                        { "Neon", (54, 160, 231, 255) },
                        { "Pepper", (242, 159, 178, 255) },
                        { "Rapi", (236, 60, 31, 255) },
                        { "Rosanna", (170, 182, 200, 255) },
                        { "Sakura", (246, 73, 57, 255) },
                        { "Tove", (238, 143, 53, 255) },
                        { "Viper", (247, 206, 222, 255) },
                        { "Yan", (231, 69, 50, 255) },

                        { "John Dick", (50, 50, 50, 255) },
                        { "Solid Snake", (74, 90, 97, 80) },
                    };
                    AddActorColorsToSpeechUI(GetActorOverrideSpeechSkinValue(Core.roomTalk.Find("Bath").Find("NoOneInShower").gameObject, "You"), actorColors);

                    CreateSFX("*buzz*", "Buzz");
                    CreateSFX("*clink*", "Clink");
                    CreateSFX("*crack*", "Crack");
                    CreateSFX("*grawr*", "Grawr");
                    CreateSFX("*graaawr*", "Graaawr");
                    CreateSFX("*hooooonk*", "Hooooonk", 0.5f);
                    CreateSFX("*smooch*", "Smooch", 0.85f);
                    CreateSFX("*plap*", "Plap", 0.75f);
                    CreateSFX("*plop*", "Plop");
                    CreateSFX("*snap*", "Snap");
                    CreateSFX("*snip*", "Snip");
                    CreateSFX("*splash*", "Splash");
                    CreateSFX("*squelch*", "Squelch");
                    CreateSFX("*swish*", "Swish");
                    CreateSFX("*swoosh*", "Swoosh", 0.5f);
                    CreateSFX("*thud*", "Thud");
                    CreateSFX("*thunk*", "Thunk", 0.9f);
                    CreateSFX("*thwack*", "Thwack");
                    CreateSFX("*yank*", "Yank");
                    CreateSFX("*yeet*", "Yank");
                    CreateSFX("*zip*", "Zip");

                    audioHarborHomeMusic = CreateMusicPlayer("HarborHomeMusic");
                    audioShower = Core.audioPlayer.Find("Shower").gameObject;
                    audioShowerQuiet = Core.audioPlayer.Find("ShowerQuiet").gameObject;

                    // Initialize Gift UI Canvas
                    giftUI = InitializeGiftUICanvas();
                    AddGiftItem("Gift_Action-Figure", "Action Figure", false, "Figure.PNG");
                    AddGiftItem("Beer", "Beer", true, "Beer.PNG");
                    AddGiftItem("Gift_Bikini", "Bikini", false, "Bikini.PNG");
                    AddGiftItem("Body-Oil", "Body Oil", true, "Body Oil.PNG");
                    AddGiftItem("Gift_Bonsai-Tree", "Bonsai Tree", false, "Bonsai.PNG");
                    AddGiftItem("Chocolate", "Chocolate", true, "Chocolate.PNG");
                    AddGiftItem("Inv-energydrink", "Energy Drink", true, "Energy Drink.PNG");
                    AddGiftItem("Flowers", "Flowers", true, "Flowers.PNG");
                    AddGiftItem("inv-lovegum", "Love Gum", true, "Love Gum.PNG");
                    AddGiftItem("Gift_Parasol", "Parasol", false, "Parasol.PNG");
                    AddGiftItem("red-meat", "Red Meat", true, "Red Meat.PNG");
                    AddGiftItem("Gift_Ring", "Ring", false, "Ring.PNG");
                    AddGiftItem("Gift_Shark-Tooth-Necklace", "Shark Tooth Necklace", false, "Necklace.PNG");
                    AddGiftItem("Gift_Sunglasses", "Sunglasses", false, "Sunglasses.PNG");
                    AddGiftItem("Gift_Sunscreen", "Sunscreen", false, "Sunscreen.PNG");
                    AddGiftItem("Gift_Tropical-Flower-Bouquet", "Tropical Flower Bouquet", false, "Bouquet.PNG");
                    AddGiftItem("inv-vape", "Vape", true, "Vape.PNG");
                    AddGiftItem("Whiskey", "Whiskey", true, "Whiskey.PNG");
                    AddGiftItem("Wine", "Wine", true, "Wine.PNG");

                    // Initialize Harbor Home Talk Panel
                    InitializeHHTalkPanel();

                    Logger.LogInfo("----- DIALOGUES LOADED -----");
                    loadedDialogues = true;
                }
                
            }
            if (Core.currentScene.name == "GameStart")
            {
                if (loadedDialogues)
                {
                    hhTalkPanelInitialized = false;
                    hhTalkLastActiveRoom = null;
                    Logger.LogInfo("----- DIALOGUES UNLOADED -----");
                    loadedDialogues = false;
                }
            }

            // ROOMTALK MONITORING
            if (loadedDialogues)
            {
                if (!dialoguePlayingVanillaInvokeRepeatingRunning)
                {
                    InvokeRepeating("MonitorRoomTalkChildren", 0f, 0.5f);
                    dialoguePlayingVanillaInvokeRepeatingRunning = true;
                }

                if (dialoguePlaying && !Core.GetProxyVariableBool("Checks_Dialogue-is-playing")) { Core.FindAndModifyProxyVariableBool("Checks_Dialogue-is-playing", true); }
                if (!dialoguePlaying && Core.GetProxyVariableBool("Checks_Dialogue-is-playing")) { Core.FindAndModifyProxyVariableBool("Checks_Dialogue-is-playing", false); }
                // Update Harbor Home talk panel
                UpdateHHTalkPanel();
                // Update gift UI visibility based on GNV values
                UpdateGiftUIVisibility();
            }
            else
            {
                if (dialoguePlayingVanillaInvokeRepeatingRunning)
                {
                    CancelInvoke("MonitorRoomTalkChildren");
                    dialoguePlayingVanillaInvokeRepeatingRunning = false;
                }
            }
        }

        private static float lastGiftUICheckTime = 0f;
        private static float giftUICheckInterval = 0.5f;

        private void UpdateGiftUIVisibility()
        {
            // Throttle checks every 0.5 seconds
            if (Time.time - lastGiftUICheckTime < giftUICheckInterval)
            {
                return;
            }
            lastGiftUICheckTime = Time.time;

            // Find the gift canvas
            Transform giftCanvasTransform = Core.FindInActiveObjectByName("UI_GiftItem_Androids_Canvas");
            if (giftCanvasTransform == null)
            {
                return;
            }

            // Check all three gift lists
            Transform[] giftLists = new Transform[]
            {
                giftCanvasTransform.Find("gift_list"),
                giftCanvasTransform.Find("gift_list (1)"),
                giftCanvasTransform.Find("gift_list (2)")
            };

            foreach (Transform giftList in giftLists)
            {
                if (giftList == null) continue;

                for (int i = 0; i < giftList.childCount; i++)
                {
                    Transform giftItem = giftList.GetChild(i);
                    string giftName = giftItem.name;

                    // Check if we know about this gift's vanilla/proxy status
                    if (!giftVanillaMap.ContainsKey(giftName))
                    {
                        continue;
                    }

                    bool isVanilla = giftVanillaMap[giftName];
                    bool giftValue = false;

                    if (isVanilla)
                    {
                        // Get vanilla GNV value
                        giftValue = Core.GetVariableBool(giftName);
                    }
                    else
                    {
                        // Get proxy GNV value
                        if (Core.proxyVariables != null && Core.proxyVariables.Exists(giftName))
                        {
                            giftValue = (bool)Core.proxyVariables.Get(giftName);
                        }
                    }

                    // Set active state based on GNV value (true = active, false = inactive)
                    giftItem.gameObject.SetActive(giftValue);
                }
            }
        }

        public GameObject CreateNewDialogue(string bundleAsset, Transform roomTalk)
        {
            // Check if dialogue bundle is loaded
            if (Core.dialogueBundle == null)
            {
                Debug.LogError($"Core.dialogueBundle is null! Cannot create dialogue '{bundleAsset}'");
                return null;
            }

            // Try to load the asset from the bundle
            GameObject dialogue = Core.dialogueBundle.LoadAsset<GameObject>(bundleAsset);
            
            // Check if the asset was found
            if (dialogue == null)
            {
                Debug.LogError($"Failed to load asset '{bundleAsset}' from dialogue bundle. Bundle may be missing this asset.");
                
                // List all available assets in the bundle for debugging
                Debug.Log($"Available assets in dialogue bundle:");
                foreach (var assetName in Core.dialogueBundle.GetAllAssetNames())
                {
                    Debug.Log($"- {assetName}");
                }
                return null;
            }

            // Check if roomTalk transform is valid
            if (roomTalk == null)
            {
                Debug.LogError($"roomTalk transform is null! Cannot instantiate dialogue '{bundleAsset}'");
                return null;
            }

            // Instantiate the dialogue
            GameObject dialogueInstance = GameObject.Instantiate(dialogue, roomTalk);
            dialogueInstance.name = bundleAsset;
            
            // Set the dialogue skin
            try
            {
                var referenceDialogue = Core.coreEvents.Find("SmallTalks")?.Find("FailedGroceries")?.Find("GameObject")?.GetComponent<Dialogue>();
                if (referenceDialogue != null)
                {
                    dialogueInstance.GetComponent<Dialogue>().Story.Content.DialogueSkin = referenceDialogue.Story.Content.DialogueSkin;
                }
                else
                {
                    Debug.LogWarning($"Could not find reference dialogue for skin. Dialogue '{bundleAsset}' may not have proper skin.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error setting dialogue skin for '{bundleAsset}': {e.Message}");
            }
            
            // Add to the tracking list
            // allDialogues.Add(dialogueInstance); // This line is removed as per the edit hint
            
            Debug.Log($"Successfully created dialogue: {bundleAsset}");
            return dialogueInstance;
        }
        public static GameObject GetActorOverrideSpeechSkinValue(GameObject dialogueGO, string actorName)
        {
            var dialogue = dialogueGO.GetComponent(typeof(GameCreator.Runtime.Dialogue.Dialogue));
            if (dialogue == null) return null;

            var storyProp = dialogue.GetType().GetProperty("Story", BindingFlags.Public | BindingFlags.Instance);
            var story = storyProp.GetValue(dialogue);
            var contentProp = story.GetType().GetProperty("Content", BindingFlags.Public | BindingFlags.Instance);
            var content = contentProp.GetValue(story);

            var rolesField = content.GetType().GetField("m_Roles", BindingFlags.NonPublic | BindingFlags.Instance);
            var roles = rolesField.GetValue(content) as Array;
            if (roles == null) return null;

            foreach (var roleObj in roles)
            {
                var actorField = roleObj.GetType().GetField("m_Actor", BindingFlags.NonPublic | BindingFlags.Instance);
                var actor = actorField.GetValue(roleObj);
                if (actor != null)
                {
                    var nameProp = actor.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
                    string thisActorName = nameProp?.GetValue(actor) as string;
                    if (thisActorName == actorName)
                    {
                        var overrideSpeechSkinField = actor.GetType().GetField("m_OverrideSpeechSkin", BindingFlags.NonPublic | BindingFlags.Instance);
                        var speechSkin = overrideSpeechSkinField.GetValue(actor);
                        if (speechSkin != null)
                        {
                            var mValueField = speechSkin.GetType().GetField("m_Value", BindingFlags.NonPublic | BindingFlags.Instance);
                            var prefab = mValueField.GetValue(speechSkin) as GameObject;
                            return prefab;
                        }
                    }
                }
            }
            return null;
        }

        public static void SetActorOverrideSpeechSkinValue(GameObject dialogueGO, string actorName, GameObject newSkinPrefab)
        {
            var dialogue = dialogueGO.GetComponent(typeof(GameCreator.Runtime.Dialogue.Dialogue));
            if (dialogue == null)
            {
                Debug.LogError($"No Dialogue component found on {dialogueGO.name}");
                return;
            }

            var storyProp = dialogue.GetType().GetProperty("Story", BindingFlags.Public | BindingFlags.Instance);
            var story = storyProp.GetValue(dialogue);
            if (story == null)
            {
                Debug.LogError("Dialogue.Story is null.");
                return;
            }

            var contentProp = story.GetType().GetProperty("Content", BindingFlags.Public | BindingFlags.Instance);
            var content = contentProp.GetValue(story);
            if (content == null)
            {
                Debug.LogError("Dialogue.Story.Content is null.");
                return;
            }

            var rolesField = content.GetType().GetField("m_Roles", BindingFlags.NonPublic | BindingFlags.Instance);
            var roles = rolesField.GetValue(content) as Array;
            if (roles == null)
            {
                Debug.LogError("Could not find Roles in Dialogue content.");
                return;
            }

            bool actorFound = false;
            foreach (var roleObj in roles)
            {
                if (roleObj == null) continue;

                var actorField = roleObj.GetType().GetField("m_Actor", BindingFlags.NonPublic | BindingFlags.Instance);
                var actor = actorField.GetValue(roleObj);
                if (actor != null)
                {
                    var nameProp = actor.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
                    string thisActorName = nameProp?.GetValue(actor) as string;
                    if (thisActorName == actorName)
                    {
                        actorFound = true;
                        var overrideSpeechSkinField = actor.GetType().GetField("m_OverrideSpeechSkin", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (overrideSpeechSkinField == null)
                        {
                            Debug.LogError($"Could not find m_OverrideSpeechSkin field on Actor object for '{actorName}'.");
                            break; 
                        }

                        var speechSkin = overrideSpeechSkinField.GetValue(actor);

                        if (speechSkin == null)
                        {
                            try
                            {
                                Type speechSkinType = overrideSpeechSkinField.FieldType;
                                if (typeof(ScriptableObject).IsAssignableFrom(speechSkinType))
                                {
                                    speechSkin = ScriptableObject.CreateInstance(speechSkinType);
                                }
                                else
                                {
                                    speechSkin = Activator.CreateInstance(speechSkinType);
                                    Debug.Log($"Created regular instance of {speechSkinType.Name} for actor '{actorName}'.");
                                }
                                overrideSpeechSkinField.SetValue(actor, speechSkin);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Failed to create an instance of the OverrideSpeechSkin for actor '{actorName}'. Exception: {e}");
                                break;
                            }
                        }
                        
                        var mValueField = speechSkin.GetType().GetField("m_Value", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (mValueField != null)
                        {
                            mValueField.SetValue(speechSkin, newSkinPrefab);
                            Debug.Log($"Set override speech skin for actor '{actorName}' to '{(newSkinPrefab ? newSkinPrefab.name : "null")}'.");
                        }
                        else
                        {
                            Debug.LogError($"Could not find m_Value field on SpeechSkin object for actor '{actorName}'.");
                        }

                        break; 
                    }
                }
            }

            if (!actorFound)
            {
                Debug.LogError($"Actor with name '{actorName}' not found in dialogue '{dialogueGO.name}'.");
            }
            else
            {
                Debug.Log($"Set speech skin for '{actorName}' to '{newSkinPrefab.name}'");
            }
        }

        /// <summary>
        /// Converts 0-255 color values to 0f-1f normalized color.
        /// </summary>
        public static Color ColorFromBytes(byte r, byte g, byte b, byte a = 255)
        {
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        /// <summary>
        /// Adds or updates actor name colors in the TMPWordColorizer component at runtime.
        /// </summary>
        public static void AddActorColorToSpeechUI(GameObject speechPrefab, string actorName, Color color)
        {
            if (speechPrefab == null)
            {
                Debug.LogError($"Speech prefab is null when trying to add actor color for '{actorName}'.");
                return;
            }

            // Find the TMPWordColorizer component in the speech UI
            TMPWordColorizer colorizer = speechPrefab.GetComponentInChildren<TMPWordColorizer>();
            if (colorizer == null)
            {
                Debug.LogWarning($"TMPWordColorizer component not found in {speechPrefab.name} for actor '{actorName}'.");
                return;
            }

            // Get the wordColors list via reflection
            FieldInfo wordColorsField = typeof(TMPWordColorizer).GetField("wordColors", BindingFlags.NonPublic | BindingFlags.Instance);
            if (wordColorsField == null)
            {
                Debug.LogError("Could not find wordColors field in TMPWordColorizer.");
                return;
            }

            var wordColors = wordColorsField.GetValue(colorizer) as List<TMPWordColorizer.WordColorPair>;
            if (wordColors == null)
            {
                Debug.LogError("wordColors list is null in TMPWordColorizer.");
                return;
            }

            // Check if actor color already exists and remove it
            wordColors.RemoveAll(x => x.word.Equals(actorName, System.StringComparison.OrdinalIgnoreCase));

            // Create new WordColorPair and add it
            TMPWordColorizer.WordColorPair newPair = new TMPWordColorizer.WordColorPair
            {
                word = actorName,
                color = color
            };
            wordColors.Add(newPair);

            Debug.Log($"Added/Updated actor color for '{actorName}' to {color} in TMPWordColorizer");
        }

        /// <summary>
        /// Adds or updates actor name colors using 0-255 byte values instead of 0f-1f normalized values.
        /// </summary>
        public static void AddActorColorToSpeechUI(GameObject speechPrefab, string actorName, byte r, byte g, byte b, byte a = 255)
        {
            Color color = ColorFromBytes(r, g, b, a);
            AddActorColorToSpeechUI(speechPrefab, actorName, color);
        }

        /// <summary>
        /// Adds multiple actor colors to the speech UI at once.
        /// </summary>
        public static void AddActorColorsToSpeechUI(GameObject speechPrefab, Dictionary<string, Color> actorColors)
        {
            if (speechPrefab == null || actorColors == null)
            {
                Debug.LogError("Speech prefab or actorColors dictionary is null.");
                return;
            }

            foreach (var kvp in actorColors)
            {
                AddActorColorToSpeechUI(speechPrefab, kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Adds multiple actor colors using 0-255 byte values (RGBA tuple format).
        /// </summary>
        public static void AddActorColorsToSpeechUI(GameObject speechPrefab, Dictionary<string, (byte r, byte g, byte b, byte a)> actorColors)
        {
            if (speechPrefab == null || actorColors == null)
            {
                Debug.LogError("Speech prefab or actorColors dictionary is null.");
                return;
            }

            foreach (var kvp in actorColors)
            {
                AddActorColorToSpeechUI(speechPrefab, kvp.Key, kvp.Value.r, kvp.Value.g, kvp.Value.b, kvp.Value.a);
            }
        }

        /// <summary>
        /// Adds multiple actor colors using 0-255 byte values (RGB tuple format, alpha defaults to 255).
        /// </summary>
        public static void AddActorColorsToSpeechUI(GameObject speechPrefab, Dictionary<string, (byte r, byte g, byte b)> actorColors)
        {
            if (speechPrefab == null || actorColors == null)
            {
                Debug.LogError("Speech prefab or actorColors dictionary is null.");
                return;
            }

            foreach (var kvp in actorColors)
            {
                AddActorColorToSpeechUI(speechPrefab, kvp.Key, kvp.Value.r, kvp.Value.g, kvp.Value.b);
            }
        }

        public class ExpressionInfo
        {
            public string Id;
            public string Name;
            public Sprite Sprite; // or Texture2D, if that's what is used
        }

        public static List<ExpressionInfo> GetAllActorExpressions(Actor actor)
        {
            var result = new List<ExpressionInfo>();
            if (actor == null) return result;

            var expressions = actor.GetType().GetField("m_Expressions", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(actor);
            if (expressions == null) return result;

            var lengthProp = expressions.GetType().GetProperty("Length", BindingFlags.Public | BindingFlags.Instance);
            int length = (int)(lengthProp?.GetValue(expressions) ?? 0);

            var fromIndexMethod = expressions.GetType().GetMethod("FromIndex", BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < length; i++)
            {
                var expression = fromIndexMethod.Invoke(expressions, new object[] { i });
                if (expression != null)
                {
                    var idProp = expression.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
                    var nameProp = expression.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
                    var spriteProp = expression.GetType().GetProperty("Sprite", BindingFlags.Public | BindingFlags.Instance); // or Texture

                    result.Add(new ExpressionInfo
                    {
                        Id = idProp?.GetValue(expression)?.ToString(),
                        Name = nameProp?.GetValue(expression)?.ToString(),
                        Sprite = spriteProp?.GetValue(expression) as Sprite
                    });
                }
            }
            return result;
        }

        public static void AddExpressionSetInstructionToOnStart(GameObject dialogueGO, string actorName, int expressionIndex, int value)
        {
            if (dialogueGO == null)
            {
                Debug.LogError("[AddExpressionSetInstructionToOnStart] dialogueGO is null.");
                return;
            }

            // 1. Extract character name from actor name (remove "Actor" suffix)
            string characterName = actorName;
            if (actorName.EndsWith("Actor"))
            {
                characterName = actorName.Substring(0, actorName.Length - 5); // Remove "Actor" suffix
            }

            // 2. Compose the variable name
            string variableName = characterName + "-expression";

            // 3. Check if the global name variable exists and create it if it doesn't
            var manager = GlobalNameVariablesManager.Instance;
            if (manager == null)
            {
                Debug.LogError("[AddExpressionSetInstructionToOnStart] GlobalNameVariablesManager not initialized");
                return;
            }

            PropertyInfo valuesProp = typeof(GlobalNameVariablesManager).GetProperty("Values", BindingFlags.NonPublic | BindingFlags.Instance);
            var values = valuesProp.GetValue(manager) as Dictionary<IdString, NameVariableRuntime>;
            bool exists = false;
            NameVariableRuntime targetRuntime = null;
            
            if (values != null)
            {
                foreach (var pair in values)
                {
                    PropertyInfo varsProp = typeof(NameVariableRuntime).GetProperty("Variables", BindingFlags.NonPublic | BindingFlags.Instance);
                    var variables = varsProp.GetValue(pair.Value) as Dictionary<string, NameVariable>;
                    if (variables != null && variables.ContainsKey(variableName))
                    {
                        exists = true;
                        targetRuntime = pair.Value;
                        break;
                    }
                }
            }

            // 4. Create the variable if it doesn't exist
            if (!exists)
            {
                Debug.Log($"[AddExpressionSetInstructionToOnStart] Creating global variable '{variableName}'");
                
                // Get the first available runtime to add the variable to
                if (values != null && values.Count > 0)
                {
                    targetRuntime = values.First().Value;
                    PropertyInfo varsProp = typeof(NameVariableRuntime).GetProperty("Variables", BindingFlags.NonPublic | BindingFlags.Instance);
                    var variables = varsProp.GetValue(targetRuntime) as Dictionary<string, NameVariable>;
                    
                    if (variables != null)
                    {
                        // Create a new NameVariable for numbers
                        var nameVarType = Type.GetType("GameCreator.Runtime.Variables.NameVariable, GameCreator.Runtime.Variables");
                        if (nameVarType != null)
                        {
                            var newVariable = Activator.CreateInstance(nameVarType);
                            
                            // Create a proper TValue for numbers
                            var tValueType = Type.GetType("GameCreator.Runtime.Variables.TValue, GameCreator.Runtime.Variables");
                            var createValueMethod = tValueType?.GetMethod("CreateValue", BindingFlags.Public | BindingFlags.Static);
                            if (createValueMethod != null)
                            {
                                // Find the type ID for numbers (usually "number" or similar)
                                var typeIDType = Type.GetType("GameCreator.Runtime.Common.IdString, GameCreator.Runtime.Common");
                                var typeID = Activator.CreateInstance(typeIDType, "number");
                                var tValue = createValueMethod.Invoke(null, new object[] { typeID, 0.0 });
                                
                                // Set the m_Value field
                                var mValueField = newVariable.GetType().GetField("m_Value", BindingFlags.NonPublic | BindingFlags.Instance);
                                mValueField?.SetValue(newVariable, tValue);
                                
                                // Set the m_Name field
                                var mNameField = newVariable.GetType().GetField("m_Name", BindingFlags.NonPublic | BindingFlags.Instance);
                                var nameIdString = Activator.CreateInstance(typeIDType, variableName);
                                mNameField?.SetValue(newVariable, nameIdString);
                                
                                variables[variableName] = newVariable as NameVariable;
                                exists = true;
                                Debug.Log($"[AddExpressionSetInstructionToOnStart] Successfully created global variable '{variableName}'");
                            }
                        }
                    }
                }
                
                if (!exists)
                {
                    Debug.LogError($"[AddExpressionSetInstructionToOnStart] Failed to create global variable '{variableName}'");
                    return;
                }
            }

            // 3. Get the Actor from the dialogue (similar to SetActorOverrideSpeechSkinValue)
            var dialogue = dialogueGO.GetComponent(typeof(GameCreator.Runtime.Dialogue.Dialogue));
            if (dialogue == null)
            {
                Debug.LogError($"[AddExpressionSetInstructionToOnStart] Dialogue component not found on {dialogueGO.name}");
                return;
            }

            var storyProp = dialogue.GetType().GetProperty("Story", BindingFlags.Public | BindingFlags.Instance);
            var story = storyProp?.GetValue(dialogue);
            if (story == null)
            {
                Debug.LogError("[AddExpressionSetInstructionToOnStart] Dialogue.Story is null.");
                return;
            }

            var contentProp = story.GetType().GetProperty("Content", BindingFlags.Public | BindingFlags.Instance);
            var content = contentProp?.GetValue(story);
            if (content == null)
            {
                Debug.LogError("[AddExpressionSetInstructionToOnStart] Dialogue.Story.Content is null");
                return;
            }

            var rolesField = content.GetType().GetField("m_Roles", BindingFlags.NonPublic | BindingFlags.Instance);
            var roles = rolesField?.GetValue(content) as Array;
            if (roles == null)
            {
                Debug.LogError("[AddExpressionSetInstructionToOnStart] Could not find Roles in Dialogue content.");
                return;
            }

            Actor targetActor = null;
            foreach (var roleObj in roles)
            {
                if (roleObj == null) continue;

                var actorField = roleObj.GetType().GetField("m_Actor", BindingFlags.NonPublic | BindingFlags.Instance);
                var actor = actorField?.GetValue(roleObj) as Actor;
                if (actor != null)
                {
                    var nameProp = actor.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
                    string thisActorName = nameProp?.GetValue(actor) as string;
                    if (thisActorName == actorName)
                    {
                        targetActor = actor;
                        break;
                    }
                }
            }

            if (targetActor == null)
            {
                Debug.LogError($"[AddExpressionSetInstructionToOnStart] Actor '{actorName}' not found in dialogue '{dialogueGO.name}'.");
                return;
            }

            // 4. Get the expressions from the actor
            var expressionsField = targetActor.GetType().GetField("m_Expressions", BindingFlags.NonPublic | BindingFlags.Instance);
            var expressions = expressionsField?.GetValue(targetActor);
            if (expressions == null)
            {
                Debug.LogError($"[AddExpressionSetInstructionToOnStart] No expressions found for actor: {actorName}");
                return;
            }

            // 5. Get the specific expression by index
            var fromIndexMethod = expressions.GetType().GetMethod("FromIndex", BindingFlags.Public | BindingFlags.Instance);
            var expression = fromIndexMethod?.Invoke(expressions, new object[] { expressionIndex });
            if (expression == null)
            {
                Debug.LogError($"[AddExpressionSetInstructionToOnStart] Expression at index {expressionIndex} not found for actor: {actorName}");
                return;
            }

            try
            {
                // 6. Create a new InstructionArithmeticSetNumber
                var instrType = Type.GetType("GameCreator.Runtime.VisualScripting.InstructionArithmeticSetNumber, GameCreator.Runtime.VisualScripting, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                if (instrType == null)
                {
                    // Try alternative type names
                    instrType = Type.GetType("GameCreator.Runtime.VisualScripting.InstructionArithmeticSetNumber");
                    if (instrType == null)
                    {
                        // Try to find it in loaded assemblies
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (assembly.FullName.Contains("GameCreator"))
                            {
                                instrType = assembly.GetType("GameCreator.Runtime.VisualScripting.InstructionArithmeticSetNumber");
                                if (instrType != null) break;
                            }
                        }
                    }
                }
                
                if (instrType == null)
                {
                    Debug.LogError("[AddExpressionSetInstructionToOnStart] Could not find InstructionArithmeticSetNumber type");
                    return;
                }
                var newInstr = Activator.CreateInstance(instrType);

                // 7. Set up m_Set (SetNumberGlobalName)
                var setType = Type.GetType("GameCreator.Runtime.Variables.SetNumberGlobalName, GameCreator.Runtime.Variables");
                if (setType == null)
                {
                    // Try alternative type names
                    setType = Type.GetType("GameCreator.Runtime.Variables.SetNumberGlobalName");
                    if (setType == null)
                    {
                        // Try to find it in loaded assemblies
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (assembly.FullName.Contains("GameCreator"))
                            {
                                setType = assembly.GetType("GameCreator.Runtime.Variables.SetNumberGlobalName");
                                if (setType != null) break;
                            }
                        }
                    }
                }
                
                if (setType == null)
                {
                    Debug.LogError("[AddExpressionSetInstructionToOnStart] Could not find SetNumberGlobalName type");
                    return;
                }
                var setInstance = Activator.CreateInstance(setType);
                var mVarField = setType.GetField("m_Variable", BindingFlags.NonPublic | BindingFlags.Instance);
                if (mVarField != null)
                {
                    var fieldSetGlobalNameType = mVarField.FieldType;
                    var fieldSetGlobalName = Activator.CreateInstance(fieldSetGlobalNameType, 2); // 2 = TYPE_ID for numbers
                    var mNameField = fieldSetGlobalNameType.GetField("m_Name", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (mNameField != null)
                    {
                        mNameField.SetValue(fieldSetGlobalName, variableName);
                    }
                    mVarField.SetValue(setInstance, fieldSetGlobalName);
                }

                var propertySetNumberType = Type.GetType("GameCreator.Runtime.Common.PropertySetNumber, GameCreator.Runtime.Common");
                if (propertySetNumberType == null)
                {
                    // Try alternative type names
                    propertySetNumberType = Type.GetType("GameCreator.Runtime.Common.PropertySetNumber");
                    if (propertySetNumberType == null)
                    {
                        // Try to find it in loaded assemblies
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (assembly.FullName.Contains("GameCreator"))
                            {
                                propertySetNumberType = assembly.GetType("GameCreator.Runtime.Common.PropertySetNumber");
                                if (propertySetNumberType != null) break;
                            }
                        }
                    }
                }
                
                if (propertySetNumberType == null)
                {
                    Debug.LogError("[AddExpressionSetInstructionToOnStart] Could not find PropertySetNumber type");
                    return;
                }
                var propertySetNumber = Activator.CreateInstance(propertySetNumberType, setInstance);

                var setField = instrType.GetField("m_Set", BindingFlags.NonPublic | BindingFlags.Instance);
                if (setField != null)
                {
                    setField.SetValue(newInstr, propertySetNumber);
                }

                // 8. Set up m_From (GetDecimalDecimal)
                var getDecimalType = Type.GetType("GameCreator.Runtime.Common.GetDecimalDecimal, GameCreator.Runtime.Common");
                if (getDecimalType == null)
                {
                    // Try alternative type names
                    getDecimalType = Type.GetType("GameCreator.Runtime.Common.GetDecimalDecimal");
                    if (getDecimalType == null)
                    {
                        // Try to find it in loaded assemblies
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (assembly.FullName.Contains("GameCreator"))
                            {
                                getDecimalType = assembly.GetType("GameCreator.Runtime.Common.GetDecimalDecimal");
                                if (getDecimalType != null) break;
                            }
                        }
                    }
                }
                
                if (getDecimalType == null)
                {
                    Debug.LogError("[AddExpressionSetInstructionToOnStart] Could not find GetDecimalDecimal type");
                    return;
                }
                
                var getDecimal = Activator.CreateInstance(getDecimalType, (double)value);
                var propertyGetDecimalType = Type.GetType("GameCreator.Runtime.Common.PropertyGetDecimal, GameCreator.Runtime.Common");
                if (propertyGetDecimalType == null)
                {
                    // Try alternative type names
                    propertyGetDecimalType = Type.GetType("GameCreator.Runtime.Common.PropertyGetDecimal");
                    if (propertyGetDecimalType == null)
                    {
                        // Try to find it in loaded assemblies
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (assembly.FullName.Contains("GameCreator"))
                            {
                                propertyGetDecimalType = assembly.GetType("GameCreator.Runtime.Common.PropertyGetDecimal");
                                if (propertyGetDecimalType != null) break;
                            }
                        }
                    }
                }
                
                if (propertyGetDecimalType == null)
                {
                    Debug.LogError("[AddExpressionSetInstructionToOnStart] Could not find PropertyGetDecimal type");
                    return;
                }
                var propertyGetDecimal = Activator.CreateInstance(propertyGetDecimalType, getDecimal);

                var fromField = instrType.GetField("m_From", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fromField != null)
                {
                    fromField.SetValue(newInstr, propertyGetDecimal);
                }

                // 9. Add to the On Start InstructionList
                var onStartField = expression.GetType().GetField("m_InstructionsOnStart", BindingFlags.NonPublic | BindingFlags.Instance);
                var onStart = onStartField?.GetValue(expression);
                if (onStart == null)
                {
                    Debug.LogError("[AddExpressionSetInstructionToOnStart] Could not access m_InstructionsOnStart field");
                    return;
                }

                var instrListField = onStart.GetType().GetField("m_Instructions", BindingFlags.NonPublic | BindingFlags.Instance);
                var instrList = instrListField?.GetValue(onStart);
                if (instrList == null)
                {
                    Debug.LogError("[AddExpressionSetInstructionToOnStart] Could not access m_Instructions field");
                    return;
                }

                var addMethod = instrList.GetType().GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);
                if (addMethod != null)
                {
                    addMethod.Invoke(instrList, new object[] { newInstr });
                    Debug.Log($"[AddExpressionSetInstructionToOnStart] Successfully added InstructionArithmeticSetNumber to OnStart for variable '{variableName}' with value {value} on {actorName} expression {expressionIndex} in dialogue '{dialogueGO.name}'.");
                }
                else
                {
                    Debug.LogError("[AddExpressionSetInstructionToOnStart] Could not find Add method on InstructionList");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddExpressionSetInstructionToOnStart] Error creating instruction: {e.Message}");
            }
        }

        /// <summary>
        /// Processes text by replacing variable placeholders with actual values.
        /// Supports [GV:VariableName] for Global Variables and [SM:VariableName] for Save Manager variables.
        /// </summary>
        /// <param name="text">The text to process</param>
        /// <returns>The processed text with variable values substituted</returns>
        public static string ProcessTextWithVariables(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string result = text;

            // Process Global Variables [GV:VariableName]
            result = ProcessGlobalVariables(result);

            // Process Save Manager Variables [SM:VariableName]
            result = ProcessSaveManagerVariables(result);

            return result;
        }

        /// <summary>
        /// Processes Global Variables in the format [GV:VariableName]
        /// </summary>
        /// <param name="text">The text to process</param>
        /// <returns>The text with Global Variables replaced</returns>
        private static string ProcessGlobalVariables(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string result = text;
            var regex = new System.Text.RegularExpressions.Regex(@"\[GV:([^\]]+)\]");
            var matches = regex.Matches(result);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                string variableName = match.Groups[1].Value;
                string replacement = GetGlobalVariableValue(variableName);
                result = result.Replace(match.Value, replacement);
            }

            return result;
        }

        /// <summary>
        /// Processes Save Manager Variables in the format [SM:VariableName]
        /// </summary>
        /// <param name="text">The text to process</param>
        /// <returns>The text with Save Manager Variables replaced</returns>
        private static string ProcessSaveManagerVariables(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string result = text;
            var regex = new System.Text.RegularExpressions.Regex(@"\[SM:([^\]]+)\]");
            var matches = regex.Matches(result);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                string variableName = match.Groups[1].Value;
                string replacement = GetSaveManagerVariableValue(variableName);
                result = result.Replace(match.Value, replacement);
            }

            return result;
        }

        /// <summary>
        /// Gets the value of a Global Variable as a string
        /// </summary>
        /// <param name="variableName">The name of the Global Variable</param>
        /// <returns>The variable value as a string, or the variable name if not found</returns>
        private static string GetGlobalVariableValue(string variableName)
        {
            try
            {
                var manager = GlobalNameVariablesManager.Instance;
                if (manager == null)
                {
                    Debug.LogWarning($"[GetGlobalVariableValue] GlobalNameVariablesManager not initialized for variable: {variableName}");
                    return variableName;
                }

                // Access private Values dictionary
                PropertyInfo valuesProp = typeof(GlobalNameVariablesManager).GetProperty("Values", BindingFlags.NonPublic | BindingFlags.Instance);
                var values = valuesProp.GetValue(manager) as Dictionary<IdString, NameVariableRuntime>;
                if (values == null) return variableName;

                foreach (var pair in values)
                {
                    // Access the runtime's Variables dictionary
                    PropertyInfo varsProp = typeof(NameVariableRuntime).GetProperty("Variables", BindingFlags.NonPublic | BindingFlags.Instance);
                    var variables = varsProp.GetValue(pair.Value) as Dictionary<string, NameVariable>;
                    if (variables == null) continue;

                    if (variables.TryGetValue(variableName, out var nameVar))
                    {
                        object value = nameVar.Value;
                        return ConvertValueToString(value);
                    }
                }

                Debug.LogWarning($"[GetGlobalVariableValue] Variable '{variableName}' not found in any global variable set");
                return variableName;
            }
            catch (Exception e)
            {
                Debug.LogError($"[GetGlobalVariableValue] Error getting global variable '{variableName}': {e.Message}");
                return variableName;
            }
        }

        /// <summary>
        /// Gets the value of a Save Manager Variable as a string
        /// </summary>
        /// <param name="variableName">The name of the Save Manager Variable</param>
        /// <returns>The variable value as a string, or the variable name if not found</returns>
        private static string GetSaveManagerVariableValue(string variableName)
        {
            try
            {
                if (SaveManager.HasVariable(variableName))
                {
                    // Try to get the value based on its type
                    if (SaveManager.GetBool(variableName) != SaveManager.GetBool(variableName, !SaveManager.GetBool(variableName)))
                    {
                        // It's a bool variable
                        return SaveManager.GetBool(variableName).ToString();
                    }
                    else if (SaveManager.GetInt(variableName) != 0 || SaveManager.GetInt(variableName, 1) != 1)
                    {
                        // It's an int variable
                        return SaveManager.GetInt(variableName).ToString();
                    }
                    else if (SaveManager.GetFloat(variableName) != 0f || SaveManager.GetFloat(variableName, 1f) != 1f)
                    {
                        // It's a float variable
                        return SaveManager.GetFloat(variableName).ToString("F1");
                    }
                    else
                    {
                        // It's a string variable
                        return SaveManager.GetString(variableName);
                    }
                }

                Debug.LogWarning($"[GetSaveManagerVariableValue] Variable '{variableName}' not found in Save Manager");
                return variableName;
            }
            catch (Exception e)
            {
                Debug.LogError($"[GetSaveManagerVariableValue] Error getting Save Manager variable '{variableName}': {e.Message}");
                return variableName;
            }
        }

        /// <summary>
        /// Converts any value to a string representation
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>The string representation of the value</returns>
        private static string ConvertValueToString(object value)
        {
            if (value == null) return "null";

            try
            {
                if (value is bool boolValue)
                {
                    return boolValue.ToString();
                }
                else if (value is int intValue)
                {
                    return intValue.ToString();
                }
                else if (value is float floatValue)
                {
                    return floatValue.ToString("F1");
                }
                else if (value is double doubleValue)
                {
                    return doubleValue.ToString("F1");
                }
                else if (value is string stringValue)
                {
                    return stringValue;
                }
                else
                {
                    return value.ToString();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ConvertValueToString] Error converting value to string: {e.Message}");
                return value.ToString();
            }
        }

        private GameObject InitializeGiftUICanvas()
        {
            // Find or copy the UI_GiftItem_Canvas (disabled)
            Transform vanillaGiftCanvasTransform = Core.FindInActiveObjectByName("UI_GiftItem_Canvas");
            if (vanillaGiftCanvasTransform == null)
            {
                Debug.LogError("[InitializeGiftUICanvas] Could not find UI_GiftItem_Canvas");
                return null;
            }

            GameObject vanillaGiftCanvas = vanillaGiftCanvasTransform.gameObject;

            // Instantiate and set as root with no parent
            GameObject giftCanvasClone = GameObject.Instantiate(vanillaGiftCanvas);
            giftCanvasClone.transform.SetParent(null, worldPositionStays: false);
            giftCanvasClone.name = "UI_GiftItem_Androids_Canvas";
            Debug.Log("[InitializeGiftUICanvas] Created UI_GiftItem_Androids_Canvas");

            // Remove Trigger component
            Trigger triggerComponent = giftCanvasClone.GetComponent<Trigger>();
            if (triggerComponent != null)
            {
                GameObject.Destroy(triggerComponent);
                Debug.Log("[InitializeGiftUICanvas] Removed Trigger component from canvas");
            }

            // Define gift item mappings
            Dictionary<string, string> giftMappings = new Dictionary<string, string>
            {
                { "gift_chocolate", "Chocolate" },
                { "gift_wine", "Wine" },
                { "gift_EnergyDrink", "Inv-energydrink" },
                { "gift_Vape", "inv-vape" },
                { "gift_lovegum", "inv-lovegum" }
            };

            // Load audio clip
            AudioClip kissClip = Core.otherBundle.LoadAsset<AudioClip>("kiss1");

            // Process gift_list and gift_list (1)
            Transform giftList = giftCanvasClone.transform.Find("gift_list");
            Transform giftList2 = giftCanvasClone.transform.Find("gift_list (1)");

            if (giftList != null)
            {
                // Store template before deleting anything
                if (giftList.childCount > 0)
                {
                    giftItemTemplate = GameObject.Instantiate(giftList.GetChild(0).gameObject);
                    giftItemTemplate.SetActive(false);
                    giftItemTemplate.name = "GiftItemTemplate";
                    
                    // Remove Trigger component from template root (controls visibility based on vanilla GNVs)
                    Trigger rootTrigger = giftItemTemplate.GetComponent<Trigger>();
                    if (rootTrigger != null)
                    {
                        GameObject.DestroyImmediate(rootTrigger);
                        Debug.Log("[InitializeGiftUICanvas] Removed Trigger from template root");
                    }
                    
                    // Delete Text (TMP) (1) child from template
                    Transform textChild = giftItemTemplate.transform.Find("Text (TMP) (1)");
                    if (textChild != null)
                    {
                        GameObject.DestroyImmediate(textChild.gameObject);
                        Debug.Log("[InitializeGiftUICanvas] Deleted Text (TMP) (1) from template");
                    }
                    
                    // Remove Trigger from Image (1) child
                    Transform image1Child = giftItemTemplate.transform.Find("Image (1)");
                    if (image1Child != null)
                    {
                        Trigger image1Trigger = image1Child.GetComponent<Trigger>();
                        if (image1Trigger != null)
                        {
                            GameObject.DestroyImmediate(image1Trigger);
                            Debug.Log("[InitializeGiftUICanvas] Removed Trigger from template Image (1)");
                        }
                    }

                    // Find and configure Button child of template
                    Transform buttonChild = giftItemTemplate.transform.Find("Button");
                    if (buttonChild != null)
                    {
                        // Remove ButtonInstructions component
                        ButtonInstructions buttonInstructions = buttonChild.GetComponent<ButtonInstructions>();
                        if (buttonInstructions != null)
                        {
                            GameObject.DestroyImmediate(buttonInstructions);
                            Debug.Log("[InitializeGiftUICanvas] Destroyed ButtonInstructions from template Button");
                        }

                        // Remove all Trigger components from Button
                        Trigger[] triggers = buttonChild.GetComponents<Trigger>();
                        foreach (Trigger trigger in triggers)
                        {
                            GameObject.DestroyImmediate(trigger);
                        }
                        Debug.Log($"[InitializeGiftUICanvas] Removed {triggers.Length} Trigger components from template Button");

                        // Add Unity Button component if it doesn't exist
                        UnityEngine.UI.Button button = buttonChild.GetComponent<UnityEngine.UI.Button>();
                        if (button == null)
                        {
                            button = buttonChild.gameObject.AddComponent<UnityEngine.UI.Button>();
                        }

                        // Configure ColorBlock for red on hover
                        var colors = button.colors;
                        colors.normalColor = new Color(1f, 1f, 1f, 1f); // White for default
                        colors.highlightedColor = new Color(1f, 0f, 0f, 1f); // Bright red for hover
                        colors.pressedColor = new Color(0.8f, 0f, 0f, 1f); // Darker red for press
                        colors.selectedColor = new Color(1f, 1f, 1f, 1f); // White
                        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Dark grey
                        colors.colorMultiplier = 1f;
                        colors.fadeDuration = 0.1f;
                        button.colors = colors;
                        button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;

                        Debug.Log("[InitializeGiftUICanvas] Configured Button on template");
                    }
                    
                    // Add LayoutElement to template to enforce size
                    LayoutElement layoutElement = giftItemTemplate.GetComponent<LayoutElement>();
                    if (layoutElement == null)
                    {
                        layoutElement = giftItemTemplate.AddComponent<LayoutElement>();
                    }
                    layoutElement.preferredWidth = 175f;
                    layoutElement.preferredHeight = 175f;
                    Debug.Log("[InitializeGiftUICanvas] Added LayoutElement to template with size 175x175");
                    
                    Debug.Log("[InitializeGiftUICanvas] Stored gift item template");
                }

                // Create copies of gift_list for gift_list (1) and gift_list (2)
                if (giftList2 != null)
                {
                    // Delete the original gift_list (1)
                    GameObject.Destroy(giftList2.gameObject);
                    Debug.Log("[InitializeGiftUICanvas] Deleted original gift_list (1)");

                    // Create gift_list (1) as a copy of gift_list
                    GameObject giftList1Clone = GameObject.Instantiate(giftList.gameObject);
                    giftList1Clone.name = "gift_list (1)";
                    giftList1Clone.transform.SetParent(giftCanvasClone.transform, worldPositionStays: false);
                    giftList1Clone.transform.SetSiblingIndex(giftList.GetSiblingIndex() + 1);
                    
                    // Delete children from gift_list (1) copy
                    Transform giftList1Transform = giftList1Clone.transform;
                    while (giftList1Transform.childCount > 0)
                    {
                        GameObject.DestroyImmediate(giftList1Transform.GetChild(0).gameObject);
                    }
                    Debug.Log("[InitializeGiftUICanvas] Created gift_list (1) as copy of gift_list and deleted its children");

                    // Create gift_list (2) as a copy of gift_list
                    GameObject giftList2Clone = GameObject.Instantiate(giftList.gameObject);
                    giftList2Clone.name = "gift_list (2)";
                    giftList2Clone.transform.SetParent(giftCanvasClone.transform, worldPositionStays: false);
                    giftList2Clone.transform.SetSiblingIndex(giftList1Clone.transform.GetSiblingIndex() + 1);
                    
                    // Delete children from gift_list (2) copy
                    Transform giftList2Transform = giftList2Clone.transform;
                    while (giftList2Transform.childCount > 0)
                    {
                        GameObject.DestroyImmediate(giftList2Transform.GetChild(0).gameObject);
                    }
                    Debug.Log("[InitializeGiftUICanvas] Created gift_list (2) as copy of gift_list and deleted its children");
                }

                // Re-find the lists after creating the copies
                Transform giftList1 = giftCanvasClone.transform.Find("gift_list (1)");
                Transform giftList3 = giftCanvasClone.transform.Find("gift_list (2)");

                // Set RectTransform positions and scales
                RectTransform giftListRect = giftList.GetComponent<RectTransform>();
                if (giftListRect != null)
                {
                    giftListRect.anchoredPosition = new Vector2(giftListRect.anchoredPosition.x, 225f);
                    giftList.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    Debug.Log("[InitializeGiftUICanvas] Set gift_list anchoredPosition.y to 225 and scale to 0.8");
                }

                if (giftList1 != null)
                {
                    RectTransform giftList1Rect = giftList1.GetComponent<RectTransform>();
                    if (giftList1Rect != null)
                    {
                        giftList1.localPosition = new Vector3(0f, 0f, giftList1.localPosition.z);
                        giftList1.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                        Debug.Log("[InitializeGiftUICanvas] Set gift_list (1) localPosition to (0, 0) and scale to 0.8");
                    }
                }

                if (giftList3 != null)
                {
                    RectTransform giftList3Rect = giftList3.GetComponent<RectTransform>();
                    if (giftList3Rect != null)
                    {
                        giftList3Rect.anchoredPosition = new Vector2(giftList3Rect.anchoredPosition.x, -225f);
                        giftList3.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                        Debug.Log("[InitializeGiftUICanvas] Set gift_list (2) anchoredPosition.y to -225 and scale to 0.8");
                    }
                }

                // Delete all gift items from gift_list (template already stored)
                while (giftList.childCount > 0)
                {
                    GameObject.DestroyImmediate(giftList.GetChild(0).gameObject);
                }
                Debug.Log("[InitializeGiftUICanvas] Deleted all gift items from gift_list");
                
                // Remove EnableChildren components from all gift lists (they control visibility based on vanilla GNVs)
                RemoveEnableChildrenComponents(giftCanvasClone);
                
                // Configure HorizontalLayoutGroup components for all three lists
                ConfigureGiftListLayouts(giftCanvasClone);
            }

            // Delete Close_GiftUI and Gift_Dialogue GameObjects
            Transform closeGiftUI = giftCanvasClone.transform.Find("Close_GiftUI");
            if (closeGiftUI != null)
            {
                GameObject.Destroy(closeGiftUI.gameObject);
                Debug.Log("[InitializeGiftUICanvas] Deleted Close_GiftUI");
            }

            Transform giftDialogue = giftCanvasClone.transform.Find("Gift_Dialogue");
            if (giftDialogue != null)
            {
                GameObject.Destroy(giftDialogue.gameObject);
                Debug.Log("[InitializeGiftUICanvas] Deleted Gift_Dialogue");
            }

            // Setup the Button GO
            Transform buttonGO = giftCanvasClone.transform.Find("Button");
            if (buttonGO != null)
            {
                // Remove ButtonInstructions component
                ButtonInstructions buttonInstructions = buttonGO.GetComponent<ButtonInstructions>();
                if (buttonInstructions != null)
                {
                    GameObject.DestroyImmediate(buttonInstructions);
                    Debug.Log("[InitializeGiftUICanvas] Destroyed ButtonInstructions from Button");
                }

                // Remove all Trigger components from Button
                Trigger[] triggers = buttonGO.GetComponents<Trigger>();
                foreach (Trigger trigger in triggers)
                {
                    GameObject.Destroy(trigger);
                }
                Debug.Log($"[InitializeGiftUICanvas] Removed {triggers.Length} Trigger components from Button");

                // Add Unity Button component
                UnityEngine.UI.Button button = buttonGO.gameObject.AddComponent<UnityEngine.UI.Button>();

                // Set targetGraphic to the Text (TMP) child
                Transform textChild = buttonGO.Find("Text (TMP)");
                if (textChild != null)
                {
                    UnityEngine.UI.Image textImage = textChild.GetComponent<UnityEngine.UI.Image>();
                    if (textImage != null)
                    {
                        button.targetGraphic = textImage;
                        Debug.Log("[InitializeGiftUICanvas] Set Button targetGraphic to Text (TMP)");
                    }
                }

                // Configure ColorBlock for highlighted color (light pink on hover)
                var colors = button.colors;
                colors.normalColor = new Color(1f, 1f, 1f, 1f); // White for default
                colors.highlightedColor = new Color(1f, 0.75f, 0.8f, 1f); // Light pink for hover
                colors.pressedColor = new Color(0.9f, 0.5f, 0.6f, 1f); // Darker pink for press
                colors.selectedColor = new Color(1f, 1f, 1f, 1f); // White
                colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Dark grey
                colors.colorMultiplier = 1f;
                colors.fadeDuration = 0.1f;
                button.colors = colors;

                // Set transition to ColorTint
                button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;

                // Add onClick listener
                button.onClick.AddListener(() => {
                    Debug.Log("Click!");

                    // Play kiss1 SFX
                    if (kissClip != null)
                    {
                        Singleton<AudioManager>.Instance.UserInterface.Play(kissClip, AudioConfigSoundUI.Default, Args.EMPTY);
                    }

                    // Disable UI_GiftItem_Androids_Canvas
                    Signals.Emit(MainStory.fadeUISignal);
                    giftCanvasClone.SetActive(false);
                    Debug.Log("[GiftUI] Disabled UI_GiftItem_Androids_Canvas");
                });

                Debug.Log("[InitializeGiftUICanvas] Configured Button component");
            }

            Debug.Log("[InitializeGiftUICanvas] Gift UI canvas initialization complete");
            return giftCanvasClone;
        }

        /// <summary>
        /// Removes EnableChildren components from all gift lists.
        /// These components control child visibility based on vanilla GNVs, which breaks our custom gift items.
        /// </summary>
        private void RemoveEnableChildrenComponents(GameObject giftCanvasClone)
        {
            Transform[] giftLists = new Transform[]
            {
                giftCanvasClone.transform.Find("gift_list"),
                giftCanvasClone.transform.Find("gift_list (1)"),
                giftCanvasClone.transform.Find("gift_list (2)")
            };

            foreach (Transform giftList in giftLists)
            {
                if (giftList == null) continue;

                // Find and destroy EnableChildren component by name (GameCreator component)
                // Use GetComponents and check type name since we don't have direct reference
                var components = giftList.GetComponents<UnityEngine.Component>();
                foreach (var component in components)
                {
                    if (component != null && component.GetType().Name == "EnableChildren")
                    {
                        GameObject.DestroyImmediate(component);
                        Debug.Log($"[RemoveEnableChildrenComponents] Removed EnableChildren from {giftList.name}");
                    }
                }
            }
        }

        private void ConfigureGiftListLayouts(GameObject giftCanvasClone)
        {
            // The LayoutElement components on children will control their sizes
            // Just log that we've set up the layout
            Transform[] giftLists = new Transform[]
            {
                giftCanvasClone.transform.Find("gift_list"),
                giftCanvasClone.transform.Find("gift_list (1)"),
                giftCanvasClone.transform.Find("gift_list (2)")
            };

            foreach (Transform giftList in giftLists)
            {
                if (giftList != null)
                {
                    Debug.Log($"[ConfigureGiftListLayouts] {giftList.name} ready for gift items with LayoutElement sizing");
                }
            }
        }

        public static void AddGiftItem(string name, string displayName, bool isVanillaGNV, string textureFileName)
        {
            // Find UI_GiftItem_Androids_Canvas (may be disabled)
            Transform giftCanvasTransform = Core.FindInActiveObjectByName("UI_GiftItem_Androids_Canvas");
            if (giftCanvasTransform == null)
            {
                Debug.LogError("[AddGiftItem] UI_GiftItem_Androids_Canvas not found");
                return;
            }
            GameObject giftCanvas = giftCanvasTransform.gameObject;

            // Find a gift_list with space (max 7 children per list)
            Transform targetGiftList = null;
            Transform giftList = giftCanvas.transform.Find("gift_list");
            Transform giftList2 = giftCanvas.transform.Find("gift_list (1)");
            Transform giftList3 = giftCanvas.transform.Find("gift_list (2)");

            if (giftList != null && giftList.childCount < 7)
                targetGiftList = giftList;
            else if (giftList2 != null && giftList2.childCount < 7)
                targetGiftList = giftList2;
            else if (giftList3 != null && giftList3.childCount < 7)
                targetGiftList = giftList3;

            if (targetGiftList == null)
            {
                Debug.LogError("[AddGiftItem] All gift lists are full (7 items each)");
                return;
            }

            // Get template
            if (giftItemTemplate == null)
            {
                Debug.LogError("[AddGiftItem] Gift item template not initialized");
                return;
            }

            // Instantiate gift item
            GameObject newGiftItem = GameObject.Instantiate(giftItemTemplate, targetGiftList);
            newGiftItem.SetActive(true);
            newGiftItem.name = name;
            
            // Force size to 175x175
            RectTransform giftItemRect = newGiftItem.GetComponent<RectTransform>();
            if (giftItemRect != null)
            {
                giftItemRect.sizeDelta = new Vector2(175f, 175f);
                Debug.Log($"[AddGiftItem] Set {name} size to 175x175");
            }
            
            // Store vanilla/proxy mapping for visibility checks
            giftVanillaMap[name] = isVanillaGNV;
            
            Debug.Log($"[AddGiftItem] Created gift item: {name}");

            // Update Text (TMP) display name
            Transform textChild = newGiftItem.transform.Find("Text (TMP)");
            if (textChild != null)
            {
                TextMeshProUGUI textComponent = textChild.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = displayName;
                    Debug.Log($"[AddGiftItem] Set display name to: {displayName}");
                }
            }

            // Update Image (2) sprite
            Transform imageChild = newGiftItem.transform.Find("Image (2)");
            if (imageChild != null)
            {
                UnityEngine.UI.Image imageComponent = imageChild.GetComponent<UnityEngine.UI.Image>();
                if (imageComponent != null)
                {
                    // Load texture from Core.itemsPath using file I/O
                    string texturePath = Core.itemsPath + textureFileName;
                    if (System.IO.File.Exists(texturePath))
                    {
                        try
                        {
                            byte[] fileData = System.IO.File.ReadAllBytes(texturePath);
                            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                            texture.LoadImage(fileData);
                            
                            // Create sprite from texture
                            Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                            imageComponent.sprite = newSprite;
                            Debug.Log($"[AddGiftItem] Set image sprite to: {textureFileName}");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[AddGiftItem] Error loading texture: {e.Message}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[AddGiftItem] Texture file not found at: {texturePath}");
                    }
                }
            }

            // Configure Button
            Transform buttonChild = newGiftItem.transform.Find("Button");
            if (buttonChild != null)
            {
                UnityEngine.UI.Button button = buttonChild.GetComponent<UnityEngine.UI.Button>();
                if (button == null)
                {
                    button = buttonChild.gameObject.AddComponent<UnityEngine.UI.Button>();
                }

                // Clear existing listeners
                button.onClick.RemoveAllListeners();

                // Capture variables for the lambda
                string giftName = name;
                bool vanillaGNV = isVanillaGNV;
                GameObject giftCanvasRef = giftCanvas;

                // Load audio clip
                AudioClip kissClip = Core.otherBundle.LoadAsset<AudioClip>("kiss1");

                // Add onClick listener
                button.onClick.AddListener(() => {
                    Debug.Log("Click!");

                    // Play kiss1 SFX
                    if (kissClip != null)
                    {
                        Singleton<AudioManager>.Instance.UserInterface.Play(kissClip, AudioConfigSoundUI.Default, Args.EMPTY);
                    }

                    // Increment affection if character likes this gift
                    string giftingTarget = Core.GetProxyVariableString("Gifting_Target", "");
                    if (!string.IsNullOrEmpty(giftingTarget))
                    {
                        Core.IncrementAffectionForGiftIfLiked(giftName, giftingTarget);
                    }
                    else
                    {
                        Debug.LogWarning("[GiftUI] Gifting_Target proxy variable not found or empty");
                    }

                    // Set Gifting_Gift to this gift's name
                    Core.FindAndModifyProxyVariableString("Gifting_Gift", giftName);

                    // Set Gifting_Gifted to true
                    Core.FindAndModifyProxyVariableBool("Gifting_Gifted", true);

                    // Set DailyProc_Gifting_[CHARNAME] to true
                    string giftingTargetForDaily = Core.GetProxyVariableString("Gifting_Target", "");
                    if (!string.IsNullOrEmpty(giftingTargetForDaily))
                    {
                        string dailyProcVarName = $"DailyProc_Gifting_{giftingTargetForDaily}";
                        Core.FindAndModifyProxyVariableBool(dailyProcVarName, true);
                    }

                    // Set GNV to False (vanilla or proxy)
                    if (vanillaGNV)
                    {
                        Core.FindAndModifyVariableBool(giftName, false);
                        Debug.Log($"[GiftUI] Set vanilla GNV {giftName} to false");
                    }
                    else
                    {
                        if (Core.proxyVariables != null && Core.proxyVariables.Exists(giftName))
                        {
                            Core.SetAndSyncGiftVariable(giftName, false);
                            Debug.Log($"[GiftUI] Set and synced proxy GNV {giftName} to false");
                        }
                        else
                        {
                            Debug.LogWarning($"[GiftUI] Proxy variable '{giftName}' not found");
                        }
                    }

                    // Disable UI_GiftItem_Androids_Canvas
                    giftCanvasRef.SetActive(false);
                    Debug.Log("[GiftUI] Disabled UI_GiftItem_Androids_Canvas");
                });

                Debug.Log($"[AddGiftItem] Configured button for {name}");
            }

            Debug.Log($"[AddGiftItem] Successfully added gift: {name} to {targetGiftList.name}");
        }

        /// <summary>
        /// Registers a text pattern to trigger SFX playback.
        /// Will look for CLIPNAME and CLIPNAME_1, CLIPNAME_2, etc. in otherbundle.
        /// If multiple variants exist, one will be randomly selected each time.
        /// </summary>
        /// <param name="textPattern">The exact text pattern to detect (supports special characters)</param>
        /// <param name="clipName">The base name of the audio clip in otherbundle</param>
        /// <param name="volume">Volume level (0-1, default 2/3)</param>
        public static void CreateSFX(string textPattern, string clipName, float volume = 2f / 3f)
        {
            try
            {
                List<AudioClip> audioClips = new List<AudioClip>();

                // Try to load the base clip
                AudioClip baseClip = Core.otherBundle.LoadAsset<AudioClip>(clipName);
                if (baseClip != null)
                {
                    audioClips.Add(baseClip);
                    Debug.Log($"[CreateSFX] Loaded base clip: {clipName}");
                }

                // Try to load variants (CLIPNAME_1, CLIPNAME_2, etc.)
                int variantIndex = 1;
                while (true)
                {
                    string variantName = $"{clipName}_{variantIndex}";
                    AudioClip variantClip = Core.otherBundle.LoadAsset<AudioClip>(variantName);
                    if (variantClip == null)
                        break;

                    audioClips.Add(variantClip);
                    Debug.Log($"[CreateSFX] Loaded variant clip: {variantName}");
                    variantIndex++;
                }

                if (audioClips.Count == 0)
                {
                    Debug.LogWarning($"[CreateSFX] No audio clips found for '{clipName}' or its variants in otherbundle");
                    return;
                }

                // Register the mapping
                if (textToSFX.ContainsKey(textPattern))
                {
                    Debug.LogWarning($"[CreateSFX] Overwriting existing SFX mapping for text pattern: {textPattern}");
                }

                textToSFX[textPattern] = new SFXMapping(textPattern, audioClips, volume);
                Debug.Log($"[CreateSFX] Registered SFX mapping: '{textPattern}' -> {clipName} ({audioClips.Count} clip(s))");
            }
            catch (Exception e)
            {
                Debug.LogError($"[CreateSFX] Error registering SFX for pattern '{textPattern}': {e.Message}");
            }
        }

        /// <summary>
        /// Gets a random audio clip from the variants available for this mapping
        /// </summary>
        public static AudioClip GetRandomAudioClipForSFX(SFXMapping mapping)
        {
            if (mapping == null || mapping.AudioClips == null || mapping.AudioClips.Count == 0)
                return null;

            return mapping.AudioClips[UnityEngine.Random.Range(0, mapping.AudioClips.Count)];
        }

        /// <summary>
        /// Creates a new audio player GameObject by copying the Music object from 12_AudioPlayer.
        /// Sets the AudioSource's clip to an asset from OtherBundle and names the GameObject after the asset.
        /// </summary>
        /// <param name="assetName">The name of the audio asset to load from OtherBundle</param>
        /// <returns>The newly created GameObject with the configured AudioSource</returns>
        public static GameObject CreateMusicPlayer(string assetName)
        {
            try
            {
                // Find 12_AudioPlayer in the scene
                GameObject audioPlayerParent = GameObject.Find("12_AudioPlayer");
                if (audioPlayerParent == null)
                {
                    Debug.LogError("[CreateMusicPlayer] Could not find 12_AudioPlayer in the scene");
                    return null;
                }

                // Find the Music child object
                Transform musicTransform = audioPlayerParent.transform.Find("Beach");
                if (musicTransform == null)
                {
                    Debug.LogError("[CreateMusicPlayer] Could not find Music object under 12_AudioPlayer");
                    return null;
                }

                // Load the audio asset from OtherBundle
                AudioClip audioClip = Core.otherBundle.LoadAsset<AudioClip>(assetName);
                if (audioClip == null)
                {
                    Debug.LogError($"[CreateMusicPlayer] Could not load audio asset '{assetName}' from OtherBundle");
                    return null;
                }

                // Instantiate a copy of the Music object (disabled to prevent auto-play)
                GameObject newMusicPlayer = GameObject.Instantiate(musicTransform.gameObject, audioPlayerParent.transform);
                newMusicPlayer.SetActive(false);
                
                // Set the name to match the asset name
                newMusicPlayer.name = assetName;

                // Get the AudioSource component and set the clip
                AudioSource audioSource = newMusicPlayer.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.clip = audioClip;
                    Debug.Log($"[CreateMusicPlayer] Created music player '{assetName}' (disabled) with AudioClip successfully");
                }
                else
                {
                    Debug.LogWarning($"[CreateMusicPlayer] Created music player '{assetName}' but AudioSource component not found");
                }

                return newMusicPlayer;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CreateMusicPlayer] Error creating music player for '{assetName}': {e.Message}");
                return null;
            }
        }

        #region Harbor Home Talk Panel

        /// <summary>
        /// Initializes the Harbor Home Talk Panel UI that replaces the vanilla TalkButton
        /// when the player is in a Harbor Home room (excluding the entrance).
        /// The panel contains up to 2 character buttons for characters currently in that room.
        /// </summary>
        private void InitializeHHTalkPanel()
        {
            // Get reference to the vanilla TalkButton
            vanillaTalkButton = Core.mainCanvas.Find("TalkButton")?.gameObject;
            if (vanillaTalkButton == null)
            {
                Debug.LogError("[HHTalkPanel] Could not find TalkButton under mainCanvas");
                return;
            }

            // Generate rounded-corner sprites for panel, buttons, and labels (radius = 24)
            int cornerRadius = 24;
            hhPanelSprite = CreateRoundedRectSprite(128, 128, cornerRadius, true, true, true, true);
            hhButtonLeftSprite = CreateRoundedRectSprite(128, 128, cornerRadius, true, false, true, false);   // TL, BL rounded
            hhButtonRightSprite = CreateRoundedRectSprite(128, 128, cornerRadius, false, true, false, true);  // TR, BR rounded
            hhButtonFullSprite = CreateRoundedRectSprite(128, 128, cornerRadius, true, true, true, true);     // all corners rounded
            hhLabelLeftSprite = CreateRoundedRectSprite(128, 128, cornerRadius, false, false, true, false);   // BL rounded only
            hhLabelRightSprite = CreateRoundedRectSprite(128, 128, cornerRadius, false, false, false, true);  // BR rounded only
            hhLabelFullSprite = CreateRoundedRectSprite(128, 128, cornerRadius, false, false, true, true);    // both bottom corners

            // Create the parent panel as a sibling of TalkButton under mainCanvas
            hhTalkPanel = new GameObject("HHTalkPanel");
            hhTalkPanel.transform.SetParent(Core.mainCanvas, false);

            // Copy RectTransform position from TalkButton
            RectTransform talkButtonRect = vanillaTalkButton.GetComponent<RectTransform>();
            RectTransform panelRect = hhTalkPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = talkButtonRect.anchorMin;
            panelRect.anchorMax = talkButtonRect.anchorMax;
            panelRect.pivot = talkButtonRect.pivot;
            panelRect.anchoredPosition = talkButtonRect.anchoredPosition + new Vector2(0f, -20f);
            panelRect.sizeDelta = new Vector2(300f, 200f);

            // Add a rounded-corner background image to the panel
            UnityEngine.UI.Image panelBG = hhTalkPanel.AddComponent<UnityEngine.UI.Image>();
            panelBG.sprite = hhPanelSprite;
            panelBG.type = UnityEngine.UI.Image.Type.Sliced;
            panelBG.color = new Color(0.08f, 0.06f, 0.12f, 0.80f);
            panelBG.raycastTarget = false;

            // Add Shadow component (black 50% alpha, offset 5, -5)
            Shadow panelShadow = hhTalkPanel.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            panelShadow.effectDistance = new Vector2(5f, -5f);

            // Add a horizontal layout group to arrange buttons side by side
            UnityEngine.UI.HorizontalLayoutGroup hlg = hhTalkPanel.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            hlg.spacing = 0f;
            hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Place the panel right after TalkButton in sibling order
            hhTalkPanel.transform.SetSiblingIndex(vanillaTalkButton.transform.GetSiblingIndex() + 1);

            // Create the two character button slots (left and right)
            hhTalkButton1 = CreateHHTalkButton("HHTalkButton1", hhTalkPanel.transform, hhButtonLeftSprite, hhLabelLeftSprite);
            hhTalkButton2 = CreateHHTalkButton("HHTalkButton2", hhTalkPanel.transform, hhButtonRightSprite, hhLabelRightSprite);

            // Start hidden
            hhTalkPanel.SetActive(false);
            hhTalkPanelInitialized = true;

            Debug.Log("[HHTalkPanel] Initialized successfully");
        }

        /// <summary>
        /// Creates a single HH talk button with a background and semi-transparent character portrait.
        /// Uses the provided sprites for asymmetric rounded corners on button and label.
        /// </summary>
        private GameObject CreateHHTalkButton(string name, Transform parent, Sprite cornerSprite, Sprite labelSprite)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            RectTransform rect = buttonGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(130f, 180f);

            // LayoutElement so the HorizontalLayoutGroup gives each button equal flexible width
            UnityEngine.UI.LayoutElement layoutElem = buttonGO.AddComponent<UnityEngine.UI.LayoutElement>();
            layoutElem.flexibleWidth = 1f;

            // Background image with rounded corners
            UnityEngine.UI.Image bgImage = buttonGO.AddComponent<UnityEngine.UI.Image>();
            bgImage.sprite = cornerSprite;
            bgImage.type = UnityEngine.UI.Image.Type.Sliced;
            bgImage.color = new Color(0.15f, 0.12f, 0.2f, 0.85f);

            // Add Unity Button component
            UnityEngine.UI.Button button = buttonGO.AddComponent<UnityEngine.UI.Button>();
            var colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0.5f);
            colors.highlightedColor = new Color(0.85f, 0.75f, 1f, 1f);
            colors.pressedColor = new Color(0.7f, 0.55f, 0.9f, 1f);
            colors.selectedColor = new Color(1f, 1f, 1f, 0.8f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;
            button.targetGraphic = bgImage;

            // Character portrait (height = button height, width adjusts to aspect ratio, centered, 90% opaque)
            GameObject portraitGO = new GameObject("Portrait");
            portraitGO.transform.SetParent(buttonGO.transform, false);
            RectTransform portraitRect = portraitGO.AddComponent<RectTransform>();
            // Anchor to center, size controlled by AspectRatioFitter
            portraitRect.anchorMin = new Vector2(0.5f, 0f);
            portraitRect.anchorMax = new Vector2(0.5f, 1f);
            portraitRect.offsetMin = new Vector2(-65f, 0f); // fallback width before fitter kicks in
            portraitRect.offsetMax = new Vector2(65f, 0f);
            UnityEngine.UI.Image portraitImage = portraitGO.AddComponent<UnityEngine.UI.Image>();
            portraitImage.color = new Color(1f, 1f, 1f, 0.9f);
            portraitImage.raycastTarget = false;
            portraitImage.preserveAspect = false;

            // AspectRatioFitter ensures height always matches button, width scales proportionally
            UnityEngine.UI.AspectRatioFitter arf = portraitGO.AddComponent<UnityEngine.UI.AspectRatioFitter>();
            arf.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.HeightControlsWidth;
            arf.aspectRatio = 0.72f; // sensible default; updated when sprite is assigned in ConfigureHHTalkButton

            // Character name label at the bottom with matching rounded corners
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(buttonGO.transform, false);
            RectTransform labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0.2f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            // Label background with matching rounded bottom corners
            UnityEngine.UI.Image labelBG = labelGO.AddComponent<UnityEngine.UI.Image>();
            labelBG.sprite = labelSprite;
            labelBG.type = UnityEngine.UI.Image.Type.Sliced;
            labelBG.color = new Color(0f, 0f, 0f, 0.5f);
            labelBG.raycastTarget = false;

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(labelGO.transform, false);
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            TextMeshProUGUI tmpText = textGO.AddComponent<TextMeshProUGUI>();
            tmpText.text = "";
            tmpText.fontSize = 16;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;
            tmpText.raycastTarget = false;

            buttonGO.SetActive(false);
            return buttonGO;
        }

        /// <summary>
        /// Called every frame when dialogues are loaded. Manages visibility and content
        /// of the Harbor Home talk panel based on which HH room level is currently active.
        /// </summary>
        /// <param name="immediate">If true, bypasses throttle and forces immediate button rebuild</param>
        public static void UpdateHHTalkPanel(bool immediate = false)
        {
            if (!hhTalkPanelInitialized || hhTalkPanel == null || vanillaTalkButton == null)
                return;

            // Throttle updates unless immediate is true
            if (!immediate && Time.time - hhTalkLastUpdateTime < hhTalkUpdateInterval)
                return;
            hhTalkLastUpdateTime = Time.time;

            // Determine which HH room (non-entrance) is currently active
            string activeRoom = GetActiveHHRoom();

            if (activeRoom != null)
            {
                // We're in a Harbor Home room - hide vanilla talk button, show our panel
                if (vanillaTalkButton.activeSelf)
                {
                    vanillaTalkButton.SetActive(false);
                }

                if (!hhTalkPanel.activeSelf)
                {
                    hhTalkPanel.SetActive(true);
                }

                // Check if room changed or schedule updated
                bool roomChanged = activeRoom != hhTalkLastActiveRoom;
                bool scheduleChanged = Schedule.scheduleVersion != hhTalkLastScheduleVersion;
                
                if (roomChanged)
                {
                    hhTalkLastActiveRoom = activeRoom;
                }
                
                if (scheduleChanged)
                {
                    hhTalkLastScheduleVersion = Schedule.scheduleVersion;
                }
                
                // Rebuild buttons when room or schedule changes, or when immediate is requested
                if (roomChanged || scheduleChanged || immediate)
                {
                    PopulateHHTalkButtons(activeRoom);
                }

                // Update button interactability based on HarborHome_TalkSelected and Lock-Game
                bool canInteract = string.IsNullOrEmpty(SaveManager.GetString("HarborHome_TalkSelected")) && !Core.GetVariableBool("Lock-Game");
                if (hhTalkButton1 != null && hhTalkButton1.activeSelf)
                {
                    UnityEngine.UI.Button btn1 = hhTalkButton1.GetComponent<UnityEngine.UI.Button>();
                    if (btn1 != null) btn1.interactable = canInteract;
                }
                if (hhTalkButton2 != null && hhTalkButton2.activeSelf)
                {
                    UnityEngine.UI.Button btn2 = hhTalkButton2.GetComponent<UnityEngine.UI.Button>();
                    if (btn2 != null) btn2.interactable = canInteract;
                }
            }
            else
            {
                // Not in a Harbor Home room - restore vanilla talk button
                if (hhTalkPanel.activeSelf)
                {
                    hhTalkPanel.SetActive(false);
                }

                // Only re-enable the vanilla TalkButton if it was disabled by us
                // (it has its own Conditions component that controls visibility)
                if (hhTalkLastActiveRoom != null)
                {
                    vanillaTalkButton.SetActive(true);
                    hhTalkLastActiveRoom = null;
                }
            }
        }

        /// <summary>
        /// Returns the room name of the currently active HH level (excluding entrance), or null.
        /// </summary>
        private static string GetActiveHHRoom()
        {
            if (Places.harborHomeBathroomLevel != null && Places.harborHomeBathroomLevel.activeSelf)
                return "HarborHomeBathroom";
            if (Places.harborHomeBedroomLevel != null && Places.harborHomeBedroomLevel.activeSelf)
                return "HarborHomeBedroom";
            if (Places.harborHomeClosetLevel != null && Places.harborHomeClosetLevel.activeSelf)
                return "HarborHomeCloset";
            if (Places.harborHomeKitchenLevel != null && Places.harborHomeKitchenLevel.activeSelf)
                return "HarborHomeKitchen";
            if (Places.harborHomeLivingroomLevel != null && Places.harborHomeLivingroomLevel.activeSelf)
                return "HarborHomeLivingRoom";
            if (Places.harborHomePoolLevel != null && Places.harborHomePoolLevel.activeSelf)
                return "HarborHomePool";
            return null;
        }

        /// <summary>
        /// Finds characters whose HH location is in the given room and populates up to 2 buttons.
        /// </summary>
        private static void PopulateHHTalkButtons(string activeRoom)
        {
            List<string> charactersInRoom = new List<string>();

            foreach (string charName in hhTalkCharacterNames)
            {
                if (!SaveManager.GetBool("HarborHome_Visit_" + charName))
                    continue;

                // The main location variable is already set to the HH position by the roaming system
                string charLocation = Schedule.GetCharacterLocation(charName);
                if (string.IsNullOrEmpty(charLocation))
                    continue;

                // Extract the room name from the full location (e.g. "HarborHomeBathroomShower" -> "HarborHomeBathroom")
                string charRoom = GetHHTalkRoomNameFromLocation(charLocation);
                if (charRoom == activeRoom)
                {
                    charactersInRoom.Add(charName);
                    if (charactersInRoom.Count >= 2)
                        break; // Max 2 buttons
                }
            }

            // Configure button 1 (always left slot)
            if (charactersInRoom.Count >= 1)
            {
                ConfigureHHTalkButton(hhTalkButton1, charactersInRoom[0]);
                hhTalkButton1.SetActive(true);

                if (charactersInRoom.Count == 1)
                {
                    // Single button fills the whole panel - use fully rounded corners
                    hhTalkButton1.GetComponent<UnityEngine.UI.Image>().sprite = hhButtonFullSprite;
                    Transform label1 = hhTalkButton1.transform.Find("Label");
                    if (label1 != null) label1.GetComponent<UnityEngine.UI.Image>().sprite = hhLabelFullSprite;
                }
                else
                {
                    // Two buttons - left button gets left-rounded corners
                    hhTalkButton1.GetComponent<UnityEngine.UI.Image>().sprite = hhButtonLeftSprite;
                    Transform label1 = hhTalkButton1.transform.Find("Label");
                    if (label1 != null) label1.GetComponent<UnityEngine.UI.Image>().sprite = hhLabelLeftSprite;
                }
            }
            else
            {
                hhTalkButton1.SetActive(false);
            }

            // Configure button 2 (right slot)
            if (charactersInRoom.Count >= 2)
            {
                ConfigureHHTalkButton(hhTalkButton2, charactersInRoom[1]);
                hhTalkButton2.SetActive(true);
            }
            else
            {
                hhTalkButton2.SetActive(false);
            }

            // If no characters found, hide the panel entirely
            if (charactersInRoom.Count == 0)
            {
                hhTalkPanel.SetActive(false);
                // Restore vanilla talk button when no chars are present
                vanillaTalkButton.SetActive(true);
            }

            Debug.Log($"[HHTalkPanel] Room '{activeRoom}': {charactersInRoom.Count} character(s) found: {string.Join(", ", charactersInRoom)}");
        }

        /// <summary>
        /// Configures a single HH talk button with the given character's portrait and click handler.
        /// </summary>
        private static void ConfigureHHTalkButton(GameObject buttonGO, string characterName)
        {
            // Update the portrait image with the character's bust texture
            UnityEngine.UI.Image portraitImage = buttonGO.transform.Find("Portrait")?.GetComponent<UnityEngine.UI.Image>();
            if (portraitImage != null)
            {
                Sprite bustSprite = GetCharacterBustSprite(characterName);
                if (bustSprite != null)
                {
                    portraitImage.sprite = bustSprite;
                    portraitImage.color = new Color(1f, 1f, 1f, 0.9f);

                    // Update AspectRatioFitter with the actual sprite's aspect ratio
                    UnityEngine.UI.AspectRatioFitter arf = portraitImage.GetComponent<UnityEngine.UI.AspectRatioFitter>();
                    if (arf != null)
                    {
                        arf.aspectRatio = bustSprite.rect.width / bustSprite.rect.height;
                    }
                }
                else
                {
                    portraitImage.sprite = null;
                    portraitImage.color = new Color(1f, 1f, 1f, 0f);
                }
            }

            // Update the label text
            TextMeshProUGUI labelText = buttonGO.transform.Find("Label")?.Find("Text")?.GetComponent<TextMeshProUGUI>();
            if (labelText != null)
            {
                labelText.text = characterName;
            }

            // Set up click handler
            UnityEngine.UI.Button button = buttonGO.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                // Only allow interaction if no character is currently selected
                button.interactable = string.IsNullOrEmpty(SaveManager.GetString("HarborHome_TalkSelected"));

                button.onClick.RemoveAllListeners();
                string capturedName = characterName;
                button.onClick.AddListener(() =>
                {
                    Debug.Log($"[HHTalkPanel] Selected character: {capturedName}");
                    SaveManager.SetString("HarborHome_TalkSelected", capturedName);

                    // Play UI button sound
                    AudioClip buttonSound = Core.otherBundle?.LoadAsset<AudioClip>("Button-1");
                    if (buttonSound != null)
                    {
                        Singleton<AudioManager>.Instance.UserInterface.Play(buttonSound, AudioConfigSoundUI.Default, Args.EMPTY);
                    }
                });
            }
        }

        /// <summary>
        /// Gets the base bust sprite for a character (used for the portrait in HH talk buttons).
        /// Returns the sprite from the character's default bust GameObject's MBase1 SpriteRenderer.
        /// </summary>
        private static Sprite GetCharacterBustSprite(string characterName)
        {
            GameObject bust = GetDefaultBustForCharacter(characterName);
            if (bust == null)
                return null;

            Transform mBase = bust.transform.Find("MBase1");
            if (mBase == null)
                return null;

            SpriteRenderer sr = mBase.GetComponent<SpriteRenderer>();
            return sr?.sprite;
        }

        /// <summary>
        /// Returns the default bust GameObject for a character by name.
        /// </summary>
        private static GameObject GetDefaultBustForCharacter(string characterName)
        {
            switch (characterName)
            {
                case "Amber": return Characters.amber;
                case "Claire": return Characters.claire;
                case "Sarah": return Characters.sarah;
                case "Anis": return Characters.anis;
                case "Centi": return Characters.centi;
                case "Dorothy": return Characters.dorothy;
                case "Elegg": return Characters.elegg;
                case "Frima": return Characters.frima;
                case "Guilty": return Characters.guilty;
                case "Helm": return Characters.helm;
                case "Maiden": return Characters.maiden;
                case "Mary": return Characters.mary;
                case "Mast": return Characters.mast;
                case "Neon": return Characters.neon;
                case "Pepper": return Characters.pepper;
                case "Rapi": return Characters.rapi;
                case "Rosanna": return Characters.rosanna;
                case "Sakura": return Characters.sakura;
                case "Tove": return Characters.tove;
                case "Viper": return Characters.viper;
                case "Yan": return Characters.yan;
                default: return null;
            }
        }

        /// <summary>
        /// Extracts the room name from a full HH location string.
        /// E.g. "HarborHomeBathroomShower" -> "HarborHomeBathroom"
        /// </summary>
        private static string GetHHTalkRoomNameFromLocation(string location)
        {
            if (string.IsNullOrEmpty(location))
                return null;

            foreach (string roomName in hhTalkRoomNames)
            {
                if (location.StartsWith(roomName))
                    return roomName;
            }
            return location;
        }

        /// <summary>
        /// Creates a rounded-rect Sprite at runtime with selectable per-corner rounding.
        /// The sprite is set up with 9-slice borders so Unity's Sliced image mode stretches it properly.
        /// </summary>
        /// <param name="width">Texture width in pixels</param>
        /// <param name="height">Texture height in pixels</param>
        /// <param name="radius">Corner radius in pixels</param>
        /// <param name="roundTL">Round top-left corner</param>
        /// <param name="roundTR">Round top-right corner</param>
        /// <param name="roundBL">Round bottom-left corner</param>
        /// <param name="roundBR">Round bottom-right corner</param>
        private static Sprite CreateRoundedRectSprite(int width, int height, int radius, bool roundTL, bool roundTR, bool roundBL, bool roundBR)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color32 white = new Color32(255, 255, 255, 255);
            Color32 clear = new Color32(0, 0, 0, 0);

            Color32[] pixels = new Color32[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool inside = true;

                    // Bottom-left corner
                    if (roundBL && x < radius && y < radius)
                    {
                        float dist = Mathf.Sqrt((x - radius) * (x - radius) + (y - radius) * (y - radius));
                        if (dist > radius) inside = false;
                    }
                    // Bottom-right corner
                    if (roundBR && x >= width - radius && y < radius)
                    {
                        float dist = Mathf.Sqrt((x - (width - radius - 1)) * (x - (width - radius - 1)) + (y - radius) * (y - radius));
                        if (dist > radius) inside = false;
                    }
                    // Top-left corner
                    if (roundTL && x < radius && y >= height - radius)
                    {
                        float dist = Mathf.Sqrt((x - radius) * (x - radius) + (y - (height - radius - 1)) * (y - (height - radius - 1)));
                        if (dist > radius) inside = false;
                    }
                    // Top-right corner
                    if (roundTR && x >= width - radius && y >= height - radius)
                    {
                        float dist = Mathf.Sqrt((x - (width - radius - 1)) * (x - (width - radius - 1)) + (y - (height - radius - 1)) * (y - (height - radius - 1)));
                        if (dist > radius) inside = false;
                    }

                    pixels[y * width + x] = inside ? white : clear;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;

            // 9-slice borders: the corner radius determines the border size
            Vector4 border = new Vector4(radius, radius, radius, radius); // left, bottom, right, top
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            return sprite;
        }

        #endregion
    }
}
