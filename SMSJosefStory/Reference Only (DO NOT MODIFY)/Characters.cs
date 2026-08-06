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
    [BepInPlugin(pluginGuid, Core.pluginName + " - Characters", Core.pluginVersion)]
    internal class Characters : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.characters";
        #endregion

        public static GameObject amber;
        public static GameObject amberCoatless;
        public static GameObject amberTopless;
        public static GameObject amberPanties;
        public static GameObject amberSwim;
        public static GameObject amberSwimCoatless;
        public static GameObject amberSwimSlip;
        public static GameObject amberNaked;

        public static GameObject claire;

        public static GameObject sarah;



        public static GameObject anis;
        public static GameObject anisCoatless;
        public static GameObject anisTopless;
        public static GameObject anisSwim;
        public static GameObject anisSwimWet;
        public static GameObject anisSwimSlip;
        public static GameObject anisNaked;
        public static Transform anisNPCHHBedleft;
        public static GameObject anisNPCHHBedleftDefault;
        public static GameObject anisNPCHHBedleftSwim;
        public static Transform anisNPCHHBedright;
        public static GameObject anisNPCHHBedrightDefault;
        public static GameObject anisNPCHHBedrightSwim;
        public static Transform anisNPCHHChangingleft;
        public static GameObject anisNPCHHChangingleftDefault;
        public static GameObject anisNPCHHChangingleftSwim;
        public static Transform anisNPCHHChangingright;
        public static GameObject anisNPCHHChangingrightDefault;
        public static GameObject anisNPCHHChangingrightSwim;
        public static Transform anisNPCHHCouchleft;
        public static GameObject anisNPCHHCouchleftDefault;
        public static GameObject anisNPCHHCouchleftSwim;
        public static Transform anisNPCHHCouchright;
        public static GameObject anisNPCHHCouchrightDefault;
        public static GameObject anisNPCHHCouchrightSwim;
        public static Transform anisNPCHHFridge;
        public static GameObject anisNPCHHFridgeDefault;
        public static GameObject anisNPCHHFridgeSwim;
        public static Transform anisNPCHHShower;
        public static GameObject anisNPCHHShowerNaked;
        public static Transform anisNPCHHSink;
        public static GameObject anisNPCHHSinkDefault;
        public static GameObject anisNPCHHSinkSwim;
        public static Transform anisNPCHHTanningleft;
        public static GameObject anisNPCHHTanningleftSwim;
        public static Transform anisNPCHHTanningright;
        public static GameObject anisNPCHHTanningrightSwim;

        public static GameObject centi;
        public static GameObject centiTopless;
        public static GameObject centiSwim;
        public static GameObject centiSwimShirtless;
        public static GameObject centiSwimSlip;
        public static GameObject centiNaked;

        public static GameObject dorothy;
        public static GameObject dorothyUnderwear;
        public static GameObject dorothySwim;
        public static GameObject dorothySwimWet;
        public static GameObject dorothySwimSlip;
        public static GameObject dorothyNaked;

        public static GameObject elegg;
        public static GameObject eleggUnderwear;
        public static GameObject eleggSwim;
        public static GameObject eleggSwimSlip;
        public static GameObject eleggNaked;

        public static GameObject frima;
        public static GameObject frimaUnderwear;
        public static GameObject frimaSwim;
        public static GameObject frimaSwimShirtless;
        public static GameObject frimaSwimSlip;
        public static GameObject frimaNaked;

        public static GameObject guilty;
        public static GameObject guiltyUnderwear;
        public static GameObject guiltySwim;
        public static GameObject guiltySwimSlip;
        public static GameObject guiltyNaked;

        public static GameObject helm;
        public static GameObject helmUnderwear;
        public static GameObject helmSwim;
        public static GameObject helmSwimWet;
        public static GameObject helmSwimShirtless;
        public static GameObject helmSwimSlip;
        public static GameObject helmNaked;

        public static GameObject maiden;
        public static GameObject maidenUnderwear;
        public static GameObject maidenSwim;
        public static GameObject maidenSwimSlip;
        public static GameObject maidenNaked;

        public static GameObject mary;
        public static GameObject maryUnderwear;
        public static GameObject marySwim;
        public static GameObject marySwimSlip;
        public static GameObject maryNaked;

        public static GameObject mast;
        public static GameObject mastShirtless;
        public static GameObject mastUnderwear;
        public static GameObject mastSwim;
        public static GameObject mastSwimSlip;
        public static GameObject mastNaked;

        public static GameObject neon;
        public static GameObject neonUnderwear;
        public static GameObject neonSwim;
        public static GameObject neonSwimWet;
        public static GameObject neonSwimSlip;
        public static GameObject neonNaked;

        public static GameObject pepper;
        public static GameObject pepperUnderwear;
        public static GameObject pepperSwim;
        public static GameObject pepperSwimSlip;
        public static GameObject pepperNaked;

        public static GameObject rapi;
        public static GameObject rapiCoatless;
        public static GameObject rapiUnderwear;
        public static GameObject rapiSwim;
        public static GameObject rapiSwimSlip;
        public static GameObject rapiNaked;

        public static GameObject rosanna;
        public static GameObject rosannaCoatless;
        public static GameObject rosannaUnderwear;
        public static GameObject rosannaSwim;
        public static GameObject rosannaSwimSlip;
        public static GameObject rosannaNaked;

        public static GameObject sakura;
        public static GameObject sakuraCoatless;
        public static GameObject sakuraUnderwear;
        public static GameObject sakuraSwim;
        public static GameObject sakuraSwimSlip;
        public static GameObject sakuraNaked;

        public static GameObject tove;
        public static GameObject toveCoatless;
        public static GameObject toveSwim;
        public static GameObject toveSwimSlip;
        public static GameObject toveNaked;

        public static GameObject viper;
        public static GameObject viperUnderwear;
        public static GameObject viperSwim;
        public static GameObject viperSwimShirtless;
        public static GameObject viperSwimWet;
        public static GameObject viperSwimSlip;
        public static GameObject viperNaked;

        public static GameObject yan;
        public static GameObject yanUnderwear;
        public static GameObject yanSwim;
        public static GameObject yanSwimSlip;
        public static GameObject yanNaked;

        #region Vanilla Variables
        public static GameObject adrian;
        public static GameObject ameliaSwim;
        public static GameObject anna;
        public static GameObject doctorFrost;
        public static GameObject emmaSwim;
        public static GameObject gabriel;
        public static GameObject isabella;
        public static GameObject katarina;
        public static GameObject kate;
        public static GameObject masterZhen;
        public static GameObject mobsterBlonde;
        public static GameObject river;
        public static GameObject samSwim;
        public static GameObject sofia;
        public static GameObject toni;
        #endregion


        public static GameObject solidSnake;

        // Mapping of character names to lists of gift names they like
        public static Dictionary<string, List<string>> characterGiftLikesMap = new Dictionary<string, List<string>>
        {
            { "Anis", new List<string> { "Beer", "Body-Oil", "Chocolate", "Gift_Sunglasses" , "Wine" } },
        };

        public static bool loadedBusts = false;

        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedBusts && Core.loadedCore)
                {
                    amber = CreateNewBust("Amber", Core.bustPath, "Amber\\Amber00.PNG", "Amber\\AmberBlink.PNG", "Amber\\Amber00Mask.PNG", "Amber\\Mouth", "Amber\\Expression", true, true);
                    amberCoatless = CreateNewBust("AmberCoatless", Core.bustPath, "Amber\\Amber01.PNG", "Amber\\AmberBlink.PNG", "Amber\\Amber01Mask.PNG", "Amber\\Mouth", "Amber\\Expression", true, true);
                    //amberTopless = CreateNewBust("AmberTopless", Core.bustPath, "Amber\\Amber02.PNG", "Amber\\AmberBlink.PNG", "Amber\\Amber02Mask.PNG", "Amber\\Mouth", "Amber\\Expression", true, true);
                    //amberPanties = CreateNewBust("AmberPanties", Core.bustPath, "Amber\\Amber03.PNG", "Amber\\AmberBlink.PNG", "Amber\\Amber03Mask.PNG", "Amber\\Mouth", "Amber\\Expression", true, true);
                    amberSwim = CreateNewBust("AmberSwim", Core.bustPath, "Amber\\AmberSwim00.PNG", "Amber\\AmberBlink.PNG", "Amber\\AmberSwim00Mask.PNG", "Amber\\Mouth", "Amber\\Expression", true, true);
                    //amberSwimCoatless = CreateNewBust("AmberSwimCoatless", Core.bustPath, "Amber\\AmberSwim01.PNG", "Amber\\AmberBlink.PNG", "Amber\\AmberSwim01Mask.PNG", "Amber\\Mouth", "Amber\\Expression", true, true);
                    //amberSwimSlip = CreateNewBust("AmberSwimSlip", Core.bustPath, "Amber\\AmberSwim02.PNG", "Amber\\AmberBlink.PNG", "Amber\\AmberSwim02Mask.PNG", "Amber\\Mouth", "Amber\\Expression", true, true);
                    //amberNaked = CreateNewBust("AmberNaked", Core.bustPath, "Amber\\AmberNaked00.PNG", "Amber\\AmberBlink.PNG", "Amber\\AmberNaked00Mask.PNG", "Amber\\Mouth", "Amber\\Expression", true, true);
                    
                    claire = CreateNewBust("Claire", Core.bustPath, "Claire\\Claire00.PNG", "Claire\\ClaireBlink.PNG", "Claire\\Claire00Mask.PNG", "Claire\\Mouth", "Claire\\Expression", true, true);

                    sarah = CreateNewBust("Sarah", Core.bustPath, "Sarah\\Sarah00.PNG", "Sarah\\SarahBlink.PNG", "Sarah\\Sarah00Mask.PNG", "Sarah\\Mouth", "Sarah\\Expression", true, true);



                    anisNPCHHBedleft = GameObject.Instantiate(new GameObject(), Places.harborHomeBedroomNPCBedleft).transform; anisNPCHHBedleft.name = "Anis";
                    anisNPCHHBedright = GameObject.Instantiate(new GameObject(), Places.harborHomeBedroomNPCBedright).transform; anisNPCHHBedright.name = "Anis";
                    anisNPCHHChangingleft = GameObject.Instantiate(new GameObject(), Places.harborHomeClosetNPCChangingleft).transform; anisNPCHHChangingleft.name = "Anis";
                    anisNPCHHChangingright = GameObject.Instantiate(new GameObject(), Places.harborHomeClosetNPCChangingright).transform; anisNPCHHChangingright.name = "Anis";
                    anisNPCHHCouchleft = GameObject.Instantiate(new GameObject(), Places.harborHomeLivingroomNPCCouchleft).transform; anisNPCHHCouchleft.name = "Anis";
                    anisNPCHHCouchright = GameObject.Instantiate(new GameObject(), Places.harborHomeLivingroomNPCCouchright).transform; anisNPCHHCouchright.name = "Anis";
                    anisNPCHHFridge = GameObject.Instantiate(new GameObject(), Places.harborHomeKitchenNPCFridge).transform; anisNPCHHFridge.name = "Anis";
                    anisNPCHHShower = GameObject.Instantiate(new GameObject(), Places.harborHomeBathroomNPCShower).transform; anisNPCHHShower.name = "Anis";
                    anisNPCHHSink = GameObject.Instantiate(new GameObject(), Places.harborHomeKitchenNPCSink).transform; anisNPCHHSink.name = "Anis";
                    anisNPCHHTanningleft = GameObject.Instantiate(new GameObject(), Places.harborHomePoolNPCTanningleft).transform; anisNPCHHTanningleft.name = "Anis";
                    anisNPCHHTanningright = GameObject.Instantiate(new GameObject(), Places.harborHomePoolNPCTanningright).transform; anisNPCHHTanningright.name = "Anis";

                    anis = CreateNewBust("AnisBase", Core.bustNikkePath, "Anis\\Anis00.PNG", "Anis\\AnisBlink.PNG", "Anis\\Anis00Mask.PNG", "Anis\\Mouth", "Anis\\Expression", true, true);
                    anisCoatless = CreateNewBust("AnisCoatless", Core.bustNikkePath, "Anis\\Anis01.PNG", "Anis\\AnisBlink.PNG", "Anis\\Anis01Mask.PNG", "Anis\\Mouth", "Anis\\Expression", true, true);
                    anisTopless = CreateNewBust("AnisTopless", Core.bustNikkePath, "Anis\\Anis02.PNG", "Anis\\AnisBlink.PNG", "Anis\\Anis02Mask.PNG", "Anis\\Mouth", "Anis\\Expression", true, true);
                    anisSwim = CreateNewBust("AnisSwim", Core.bustNikkePath, "Anis\\AnisSwim00.PNG", "Anis\\AnisBlink.PNG", "Anis\\AnisSwim00Mask.PNG", "Anis\\Mouth", "Anis\\Expression", true, true);
                    anisSwimWet = CreateNewBust("AnisSwimWet", Core.bustNikkePath, "Anis\\AnisSwim01.PNG", "Anis\\AnisBlink.PNG", "Anis\\AnisSwim01Mask.PNG", "Anis\\Mouth", "Anis\\Expression", true, true);
                    anisSwimWet.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    anisSwimSlip = CreateNewBust("AnisSwimSlip", Core.bustNikkePath, "Anis\\AnisSwim02.PNG", "Anis\\AnisBlink.PNG", "Anis\\AnisSwim02Mask.PNG", "Anis\\Mouth", "Anis\\Expression", true, true);
                    anisSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    anisNaked = CreateNewBust("AnisNaked", Core.bustNikkePath, "Anis\\AnisNaked00.PNG", "Anis\\AnisBlink.PNG", "Anis\\AnisNaked00Mask.PNG", "Anis\\Mouth", "Anis\\Expression", true, true);

                    anisNPCHHBedleftDefault = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCBedleftDefault"), anisNPCHHBedleft); anisNPCHHBedleftDefault.name = "Default";
                    anisNPCHHBedleftDefault.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHBedleftDefault);
                    anisNPCHHBedrightDefault = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCBedrightDefault"), anisNPCHHBedright); anisNPCHHBedrightDefault.name = "Default";
                    anisNPCHHBedrightDefault.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHBedrightDefault);
                    anisNPCHHChangingleftDefault = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCChangingleftDefault"), anisNPCHHChangingleft); anisNPCHHChangingleftDefault.name = "Default";
                    anisNPCHHChangingleftDefault.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHChangingleftDefault);
                    anisNPCHHChangingrightDefault = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCChangingrightDefault"), anisNPCHHChangingright); anisNPCHHChangingrightDefault.name = "Default";
                    anisNPCHHChangingrightDefault.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHChangingrightDefault);
                    anisNPCHHCouchleftDefault = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCCouchleftDefault"), anisNPCHHCouchleft); anisNPCHHCouchleftDefault.name = "Default";
                    anisNPCHHCouchleftDefault.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHCouchleftDefault);
                    anisNPCHHCouchrightDefault = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCCouchrightDefault"), anisNPCHHCouchright); anisNPCHHCouchrightDefault.name = "Default";
                    anisNPCHHCouchrightDefault.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHCouchrightDefault);
                    anisNPCHHFridgeDefault = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCFridgeDefault"), anisNPCHHFridge); anisNPCHHFridgeDefault.name = "Default";
                    anisNPCHHFridgeDefault.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHFridgeDefault);
                    anisNPCHHSinkDefault = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCSinkDefault"), anisNPCHHSink); anisNPCHHSinkDefault.name = "Default";
                    anisNPCHHSinkDefault.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHSinkDefault);

                    anisNPCHHBedleftSwim = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCBedleftSwim"), anisNPCHHBedleft); anisNPCHHBedleftSwim.name = "Swim";
                    anisNPCHHBedleftSwim.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHBedleftSwim);
                    anisNPCHHBedrightSwim = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCBedrightSwim"), anisNPCHHBedright); anisNPCHHBedrightSwim.name = "Swim";
                    anisNPCHHBedrightSwim.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHBedrightSwim);
                    anisNPCHHChangingleftSwim = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCChangingleftSwim"), anisNPCHHChangingleft); anisNPCHHChangingleftSwim.name = "Swim";
                    anisNPCHHChangingleftSwim.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHChangingleftSwim);
                    anisNPCHHChangingrightSwim = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCChangingrightSwim"), anisNPCHHChangingright); anisNPCHHChangingrightSwim.name = "Swim";
                    anisNPCHHChangingrightSwim.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHChangingrightSwim);
                    anisNPCHHCouchleftSwim = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCCouchleftSwim"), anisNPCHHCouchleft); anisNPCHHCouchleftSwim.name = "Swim";
                    anisNPCHHCouchleftSwim.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHCouchleftSwim);
                    anisNPCHHCouchrightSwim = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCCouchrightSwim"), anisNPCHHCouchright); anisNPCHHCouchrightSwim.name = "Swim";
                    anisNPCHHCouchrightSwim.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHCouchrightSwim);
                    anisNPCHHFridgeSwim = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCFridgeSwim"), anisNPCHHFridge); anisNPCHHFridgeSwim.name = "Swim";
                    anisNPCHHFridgeSwim.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHFridgeSwim);
                    anisNPCHHSinkSwim = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCSinkSwim"), anisNPCHHSink); anisNPCHHSinkSwim.name = "Swim";
                    anisNPCHHSinkSwim.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHSinkSwim);
                    anisNPCHHTanningleftSwim = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCTanningleftSwim"), anisNPCHHTanningleft); anisNPCHHTanningleftSwim.name = "Swim";
                    anisNPCHHTanningleftSwim.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHTanningleftSwim);
                    anisNPCHHTanningrightSwim = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCTanningrightSwim"), anisNPCHHTanningright); anisNPCHHTanningrightSwim.name = "Swim";
                    anisNPCHHTanningrightSwim.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHTanningrightSwim);

                    anisNPCHHShowerNaked = GameObject.Instantiate(Core.characterBundle.LoadAsset<GameObject>("AnisNPCShowerNaked"), anisNPCHHShower); anisNPCHHShowerNaked.name = "Naked";
                    anisNPCHHShowerNaked.gameObject.AddComponent<RandomChildActivator>();
                    AddBlinkingSpriteToBlinkObjects(anisNPCHHShowerNaked);
                    CopyParticleSystemToParticleObjects(anisNPCHHShowerNaked);



                    centi = CreateNewBust("CentiBase", Core.bustNikkePath, "Centi\\Centi00.PNG", "Centi\\CentiBlink.PNG", "Centi\\Centi00Mask.PNG", "Centi\\Mouth", "Centi\\Expression", true, true);
                    //centiTopless = CreateNewBust("CentiTopless", Core.bustNikkePath, "Centi\\Centi01.PNG", "Centi\\CentiBlink.PNG", "Centi\\Centi01Mask.PNG", "Centi\\Mouth", "Centi\\Expression", true, true);
                    centiSwim = CreateNewBust("CentiSwim", Core.bustNikkePath, "Centi\\CentiSwim00.PNG", "Centi\\CentiBlink.PNG", "Centi\\CentiSwim00Mask.PNG", "Centi\\Mouth", "Centi\\Expression", true, true);
                    centiSwimShirtless = CreateNewBust("centiSwimShirtless", Core.bustNikkePath, "Centi\\CentiSwim01.PNG", "Centi\\CentiBlink.PNG", "Centi\\CentiSwim01Mask.PNG", "Centi\\Mouth", "Centi\\Expression", true, true);
                    centiSwimSlip = CreateNewBust("CentiSwimSlip", Core.bustNikkePath, "Centi\\CentiSwim02.PNG", "Centi\\CentiBlink.PNG", "Centi\\CentiSwim02Mask.PNG", "Centi\\Mouth", "Centi\\Expression", true, true);
                    centiSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    //centiNaked = CreateNewBust("CentiNaked", Core.bustNikkePath, "Centi\\CentiNaked00.PNG", "Centi\\CentiBlink.PNG", "Centi\\CentiNaked00Mask.PNG", "Centi\\Mouth", "Centi\\Expression", true, true);

                    dorothy = CreateNewBust("DorothyBase", Core.bustNikkePath, "Dorothy\\Dorothy00.PNG", "Dorothy\\DorothyBlink.PNG", "Dorothy\\Dorothy00Mask.PNG", "Dorothy\\Mouth", "Dorothy\\Expression", true, true);
                    //dorothyUnderwear = CreateNewBust("DorothyUnderwear", Core.bustNikkePath, "Dorothy\\Dorothy01.PNG", "Dorothy\\DorothyBlink.PNG", "Dorothy\\Dorothy01Mask.PNG", "Dorothy\\Mouth", "Dorothy\\Expression", true, true);
                    dorothySwim = CreateNewBust("DorothySwim", Core.bustNikkePath, "Dorothy\\DorothySwim00.PNG", "Dorothy\\DorothyBlink.PNG", "Dorothy\\DorothySwim00Mask.PNG", "Dorothy\\Mouth", "Dorothy\\Expression", true, true);
                    dorothySwimWet = CreateNewBust("DorothySwimWet", Core.bustNikkePath, "Dorothy\\DorothySwim01.PNG", "Dorothy\\DorothyBlink.PNG", "Dorothy\\DorothySwim01Mask.PNG", "Dorothy\\Mouth", "Dorothy\\Expression", true, true);
                    dorothySwimWet.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    dorothySwimSlip = CreateNewBust("DorothySwimSlip", Core.bustNikkePath, "Dorothy\\DorothySwim02.PNG", "Dorothy\\DorothyBlink.PNG", "Dorothy\\DorothySwim02Mask.PNG", "Dorothy\\Mouth", "Dorothy\\Expression", true, true);
                    dorothySwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    //dorothyNaked = CreateNewBust("DorothyNaked", Core.bustNikkePath, "Dorothy\\DorothyNaked00.PNG", "Dorothy\\DorothyBlink.PNG", "Dorothy\\DorothyNaked00Mask.PNG", "Dorothy\\Mouth", "Dorothy\\Expression", true, true);

                    elegg = CreateNewBust("EleggBase", Core.bustNikkePath, "Elegg\\Elegg00.PNG", "Elegg\\EleggBlink.PNG", "Elegg\\Elegg00Mask.PNG", "Elegg\\Mouth", "Elegg\\Expression", true, true);
                    //eleggUnderwear = CreateNewBust("EleggUnderwear", Core.bustNikkePath, "Elegg\\Elegg01.PNG", "Elegg\\EleggBlink.PNG", "Elegg\\Elegg01Mask.PNG", "Elegg\\Mouth", "Elegg\\Expression", true, true);
                    eleggSwim = CreateNewBust("EleggSwim", Core.bustNikkePath, "Elegg\\EleggSwim00.PNG", "Elegg\\EleggBlink.PNG", "Elegg\\EleggSwim00Mask.PNG", "Elegg\\Mouth", "Elegg\\Expression", true, true);
                    eleggSwimSlip = CreateNewBust("EleggSwimSlip", Core.bustNikkePath, "Elegg\\EleggSwim01.PNG", "Elegg\\EleggBlink.PNG", "Elegg\\EleggSwim01Mask.PNG", "Elegg\\Mouth", "Elegg\\Expression", true, true);
                    //eleggNaked = CreateNewBust("EleggNaked", Core.bustNikkePath, "Elegg\\EleggNaked00.PNG", "Elegg\\EleggBlink.PNG", "Elegg\\EleggNaked00Mask.PNG", "Elegg\\Mouth", "Elegg\\Expression", true, true);

                    frima = CreateNewBust("FrimaBase", Core.bustNikkePath, "Frima\\Frima00.PNG", "Frima\\FrimaBlink.PNG", "Frima\\Frima00Mask.PNG", "Frima\\Mouth", "Frima\\Expression", true, true);
                    //frimaUnderwear = CreateNewBust("FrimaUnderwear", Core.bustNikkePath, "Frima\\Frima01.PNG", "Frima\\FrimaBlink.PNG", "Frima\\Frima01Mask.PNG", "Frima\\Mouth", "Frima\\Expression", true, true);
                    frimaSwim = CreateNewBust("FrimaSwim", Core.bustNikkePath, "Frima\\FrimaSwim00.PNG", "Frima\\FrimaBlink.PNG", "Frima\\FrimaSwim00Mask.PNG", "Frima\\Mouth", "Frima\\Expression", true, true);
                    frimaSwimShirtless = CreateNewBust("FrimaSwimShirtless", Core.bustNikkePath, "Frima\\FrimaSwim01.PNG", "Frima\\FrimaBlink.PNG", "Frima\\FrimaSwim01Mask.PNG", "Frima\\Mouth", "Frima\\Expression", true, true);
                    frimaSwimSlip = CreateNewBust("FrimaSwimSlip", Core.bustNikkePath, "Frima\\FrimaSwim02.PNG", "Frima\\FrimaBlink.PNG", "Frima\\FrimaSwim02Mask.PNG", "Frima\\Mouth", "Frima\\Expression", true, true);
                    frimaSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    //frimaNaked = CreateNewBust("FrimaNaked", Core.bustNikkePath, "Frima\\FrimaNaked00.PNG", "Frima\\FrimaBlink.PNG", "Frima\\FrimaNaked00Mask.PNG", "Frima\\Mouth", "Frima\\Expression", true, true);

                    guilty = CreateNewBust("GuiltyBase", Core.bustNikkePath, "Guilty\\Guilty00.PNG", "Guilty\\GuiltyBlink.PNG", "Guilty\\Guilty00Mask.PNG", "Guilty\\Mouth", "Guilty\\Expression", true, true);
                    //guiltyUnderwear = CreateNewBust("GuiltyUnderwear", Core.bustNikkePath, "Guilty\\Guilty01.PNG", "Guilty\\GuiltyBlink.PNG", "Guilty\\Guilty01Mask.PNG", "Guilty\\Mouth", "Guilty\\Expression", true, true);
                    guiltySwim = CreateNewBust("GuiltySwim", Core.bustNikkePath, "Guilty\\GuiltySwim00.PNG", "Guilty\\GuiltyBlink.PNG", "Guilty\\GuiltySwim00Mask.PNG", "Guilty\\Mouth", "Guilty\\Expression", true, true);
                    guiltySwimSlip = CreateNewBust("GuiltySwimSlip", Core.bustNikkePath, "Guilty\\GuiltySwim01.PNG", "Guilty\\GuiltyBlink.PNG", "Guilty\\GuiltySwim01Mask.PNG", "Guilty\\Mouth", "Guilty\\Expression", true, true);
                    guiltySwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    //guiltyNaked = CreateNewBust("GuiltyNaked", Core.bustNikkePath, "Guilty\\GuiltyNaked00.PNG", "Guilty\\GuiltyBlink.PNG", "Guilty\\GuiltyNaked00Mask.PNG", "Guilty\\Mouth", "Guilty\\Expression", true, true);

                    helm = CreateNewBust("HelmBase", Core.bustNikkePath, "Helm\\Helm00.PNG", "Helm\\HelmBlink.PNG", "Helm\\Helm00Mask.PNG", "Helm\\Mouth", "Helm\\Expression", true, true);
                    //helmUnderwear = CreateNewBust("HelmUnderwear", Core.bustNikkePath, "Helm\\Helm01.PNG", "Helm\\HelmBlink.PNG", "Helm\\Helm01Mask.PNG", "Helm\\Mouth", "Helm\\Expression", true, true);
                    helmSwim = CreateNewBust("HelmSwim", Core.bustNikkePath, "Helm\\HelmSwim00.PNG", "Helm\\HelmBlink.PNG", "Helm\\HelmSwim00Mask.PNG", "Helm\\Mouth", "Helm\\Expression", true, true);
                    helmSwimWet = CreateNewBust("HelmSwimWet", Core.bustNikkePath, "Helm\\HelmSwim01.PNG", "Helm\\HelmBlink.PNG", "Helm\\HelmSwim01Mask.PNG", "Helm\\Mouth", "Helm\\Expression", true, true);
                    helmSwimWet.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    helmSwimShirtless = CreateNewBust("HelmSwimShirtless", Core.bustNikkePath, "Helm\\HelmSwim02.PNG", "Helm\\HelmBlink.PNG", "Helm\\HelmSwim02Mask.PNG", "Helm\\Mouth", "Helm\\Expression", true, true);
                    helmSwimShirtless.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    helmSwimSlip = CreateNewBust("HelmSwimSlip", Core.bustNikkePath, "Helm\\HelmSwim03.PNG", "Helm\\HelmBlink.PNG", "Helm\\HelmSwim03Mask.PNG", "Helm\\Mouth", "Helm\\Expression", true, true);
                    helmSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    //helmNaked = CreateNewBust("HelmNaked", Core.bustNikkePath, "Helm\\HelmNaked00.PNG", "Helm\\HelmBlink.PNG", "Helm\\HelmNaked00Mask.PNG", "Helm\\Mouth", "Helm\\Expression", true, true);

                    maiden = CreateNewBust("MaidenBase", Core.bustNikkePath, "Maiden\\Maiden00.PNG", "Maiden\\MaidenBlink.PNG", "Maiden\\Maiden00Mask.PNG", "Maiden\\Mouth", "Maiden\\Expression", true, true);
                    //maidenUnderwear = CreateNewBust("MaidenUnderwear", Core.bustNikkePath, "Maiden\\Maiden01.PNG", "Maiden\\MaidenBlink.PNG", "Maiden\\Maiden01Mask.PNG", "Maiden\\Mouth", "Maiden\\Expression", true, true);
                    maidenSwim = CreateNewBust("MaidenSwim", Core.bustNikkePath, "Maiden\\MaidenSwim00.PNG", "Maiden\\MaidenBlink.PNG", "Maiden\\MaidenSwim00Mask.PNG", "Maiden\\MouthB", "Maiden\\Expression", true, true);
                    maidenSwimSlip = CreateNewBust("MaidenSwimSlip", Core.bustNikkePath, "Maiden\\MaidenSwim01.PNG", "Maiden\\MaidenBlink.PNG", "Maiden\\MaidenSwim01Mask.PNG", "Maiden\\MouthB", "Maiden\\Expression", true, true);
                    maidenSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    //maidenNaked = CreateNewBust("MaidenNaked", Core.bustNikkePath, "Maiden\\MaidenNaked00.PNG", "Maiden\\MaidenBlink.PNG", "Maiden\\MaidenNaked00Mask.PNG", "Maiden\\Mouth", "Maiden\\Expression", true, true);

                    mary = CreateNewBust("MaryBase", Core.bustNikkePath, "Mary\\Mary00.PNG", "Mary\\MaryBlink.PNG", "Mary\\Mary00Mask.PNG", "Mary\\Mouth", "Mary\\Expression", true, true);
                    //maryUnderwear = CreateNewBust("MaryUnderwear", Core.bustNikkePath, "Mary\\Mary01.PNG", "Mary\\MaryBlink.PNG", "Mary\\Mary01Mask.PNG", "Mary\\Mouth", "Mary\\Expression", true, true);
                    marySwim = CreateNewBust("MarySwim", Core.bustNikkePath, "Mary\\MarySwim00.PNG", "Mary\\MaryBlink.PNG", "Mary\\MarySwim00Mask.PNG", "Mary\\Mouth", "Mary\\Expression", true, true);
                    marySwimSlip = CreateNewBust("MarySwimSlip", Core.bustNikkePath, "Mary\\MarySwim01.PNG", "Mary\\MaryBlink.PNG", "Mary\\MarySwim01Mask.PNG", "Mary\\Mouth", "Mary\\Expression", true, true);
                    //maryNaked = CreateNewBust("MaryNaked", Core.bustNikkePath, "Mary\\MaryNaked00.PNG", "Mary\\MaryBlink.PNG", "Mary\\MaryNaked00Mask.PNG", "Mary\\Mouth", "Mary\\Expression", true, true);

                    mast = CreateNewBust("MastBase", Core.bustNikkePath, "Mast\\Mast00.PNG", "Mast\\MastBlink.PNG", "Mast\\Mast00Mask.PNG", "Mast\\Mouth", "Mast\\Expression", true, true);
                    mastShirtless = CreateNewBust("MastShirtless", Core.bustNikkePath, "Mast\\Mast01.PNG", "Mast\\MastBlink.PNG", "Mast\\Mast01Mask.PNG", "Mast\\Mouth", "Mast\\Expression", true, true);
                    //mastUnderwear = CreateNewBust("MastUnderwear", Core.bustNikkePath, "Mast\\Mast02.PNG", "Mast\\MastBlink.PNG", "Mast\\Mast02Mask.PNG", "Mast\\Mouth", "Mast\\Expression", true, true);
                    mastSwim = CreateNewBust("MastSwim", Core.bustNikkePath, "Mast\\MastSwim00.PNG", "Mast\\MastBlink.PNG", "Mast\\MastSwim00Mask.PNG", "Mast\\Mouth", "Mast\\Expression", true, true);
                    mastSwimSlip = CreateNewBust("MastSwimSlip", Core.bustNikkePath, "Mast\\MastSwim01.PNG", "Mast\\MastBlink.PNG", "Mast\\MastSwim01Mask.PNG", "Mast\\Mouth", "Mast\\Expression", true, true);
                    //mastNaked = CreateNewBust("MastNaked", Core.bustNikkePath, "Mast\\MastNaked00.PNG", "Mast\\MastBlink.PNG", "Mast\\MastNaked00Mask.PNG", "Mast\\Mouth", "Mast\\Expression", true, true);

                    neon = CreateNewBust("NeonBase", Core.bustNikkePath, "Neon\\Neon00.PNG", "Neon\\NeonBlink.PNG", "Neon\\Neon00Mask.PNG", "Neon\\Mouth", "Neon\\Expression", true, true);
                    //neonUnderwear = CreateNewBust("NeonUnderwear", Core.bustNikkePath, "Neon\\Neon01.PNG", "Neon\\NeonBlink.PNG", "Neon\\Neon01Mask.PNG", "Neon\\Mouth", "Neon\\Expression", true, true);
                    neonSwim = CreateNewBust("NeonSwim", Core.bustNikkePath, "Neon\\NeonSwim00.PNG", "Neon\\NeonBlink.PNG", "Neon\\NeonSwim00Mask.PNG", "Neon\\Mouth", "Neon\\Expression", true, true);
                    neonSwimWet = CreateNewBust("NeonSwimWet", Core.bustNikkePath, "Neon\\NeonSwim01.PNG", "Neon\\NeonBlink.PNG", "Neon\\NeonSwim01Mask.PNG", "Neon\\Mouth", "Neon\\Expression", true, true);
                    neonSwimWet.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    neonSwimSlip = CreateNewBust("NeonSwimSlip", Core.bustNikkePath, "Neon\\NeonSwim02.PNG", "Neon\\NeonBlink.PNG", "Neon\\NeonSwim02Mask.PNG", "Neon\\Mouth", "Neon\\Expression", true, true);
                    neonSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    //neonNaked = CreateNewBust("NeonNaked", Core.bustNikkePath, "Neon\\NeonNaked00.PNG", "Neon\\NeonBlink.PNG", "Neon\\NeonNaked00Mask.PNG", "Neon\\Mouth", "Neon\\Expression", true, true);

                    pepper = CreateNewBust("PepperBase", Core.bustNikkePath, "Pepper\\Pepper00.PNG", "Pepper\\PepperBlink.PNG", "Pepper\\Pepper00Mask.PNG", "Pepper\\Mouth", "Pepper\\Expression", true, true);
                    //pepperUnderwear = CreateNewBust("PepperUnderwear", Core.bustNikkePath, "Pepper\\Pepper01.PNG", "Pepper\\PepperBlink.PNG", "Pepper\\Pepper01Mask.PNG", "Pepper\\Mouth", "Pepper\\Expression", true, true);
                    pepperSwim = CreateNewBust("PepperSwim", Core.bustNikkePath, "Pepper\\PepperSwim00.PNG", "Pepper\\PepperBlink.PNG", "Pepper\\PepperSwim00Mask.PNG", "Pepper\\Mouth", "Pepper\\Expression", true, true);
                    pepperSwimSlip = CreateNewBust("PepperSwimSlip", Core.bustNikkePath, "Pepper\\PepperSwim01.PNG", "Pepper\\PepperBlink.PNG", "Pepper\\PepperSwim01Mask.PNG", "Pepper\\Mouth", "Pepper\\Expression", true, true);
                    //pepperNaked = CreateNewBust("PepperNaked", Core.bustNikkePath, "Pepper\\PepperNaked00.PNG", "Pepper\\PepperBlink.PNG", "Pepper\\PepperNaked00Mask.PNG", "Pepper\\Mouth", "Pepper\\Expression", true, true);

                    rapi = CreateNewBust("RapiBase", Core.bustNikkePath, "Rapi\\Rapi00.PNG", "Rapi\\RapiBlink.PNG", "Rapi\\Rapi00Mask.PNG", "Rapi\\Mouth", "Rapi\\Expression", true, true);
                    //rapiCoatless = CreateNewBust("RapiCoatless", Core.bustNikkePath, "Rapi\\Rapi01.PNG", "Rapi\\RapiBlink.PNG", "Rapi\\Rapi01Mask.PNG", "Rapi\\Mouth", "Rapi\\Expression", true, true);
                    //rapiUnderwear = CreateNewBust("RapiUnderwear", Core.bustNikkePath, "Rapi\\Rapi02.PNG", "Rapi\\RapiBlink.PNG", "Rapi\\Rapi02Mask.PNG", "Rapi\\Mouth", "Rapi\\Expression", true, true);
                    rapiSwim = CreateNewBust("RapiSwim", Core.bustNikkePath, "Rapi\\RapiSwim00.PNG", "Rapi\\RapiBlink.PNG", "Rapi\\RapiSwim00Mask.PNG", "Rapi\\Mouth", "Rapi\\Expression", true, true);
                    rapiSwimSlip = CreateNewBust("RapiSwimSlip", Core.bustNikkePath, "Rapi\\RapiSwim01.PNG", "Rapi\\RapiBlink.PNG", "Rapi\\RapiSwim01Mask.PNG", "Rapi\\Mouth", "Rapi\\Expression", true, true);
                    rapiSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    //rapiNaked = CreateNewBust("RapiNaked", Core.bustNikkePath, "Rapi\\RapiNaked00.PNG", "Rapi\\RapiBlink.PNG", "Rapi\\RapiNaked00Mask.PNG", "Rapi\\Mouth", "Rapi\\Expression", true, true);

                    rosanna = CreateNewBust("RosannaBase", Core.bustNikkePath, "Rosanna\\Rosanna00.PNG", "Rosanna\\RosannaBlink.PNG", "Rosanna\\Rosanna00Mask.PNG", "Rosanna\\Mouth", "Rosanna\\Expression", true, true);
                    //rosannaCoatless = CreateNewBust("RosannaCoatless", Core.bustNikkePath, "Rosanna\\Rosanna01.PNG", "Rosanna\\RosannaBlink.PNG", "Rosanna\\Rosanna01Mask.PNG", "Rosanna\\Mouth", "Rosanna\\Expression", true, true);
                    //rosannaUnderwear = CreateNewBust("RosannaUnderwear", Core.bustNikkePath, "Rosanna\\Rosanna02.PNG", "Rosanna\\RosannaBlink.PNG", "Rosanna\\Rosanna02Mask.PNG", "Rosanna\\Mouth", "Rosanna\\Expression", true, true);
                    rosannaSwim = CreateNewBust("RosannaSwim", Core.bustNikkePath, "Rosanna\\RosannaSwim00.PNG", "Rosanna\\RosannaBlink.PNG", "Rosanna\\RosannaSwim00Mask.PNG", "Rosanna\\Mouth", "Rosanna\\Expression", true, true);
                    rosannaSwimSlip = CreateNewBust("RosannaSwimSlip", Core.bustNikkePath, "Rosanna\\RosannaSwim01.PNG", "Rosanna\\RosannaBlink.PNG", "Rosanna\\RosannaSwim01Mask.PNG", "Rosanna\\Mouth", "Rosanna\\Expression", true, true);
                    //rosannaNaked = CreateNewBust("RosannaNaked", Core.bustNikkePath, "Rosanna\\RosannaNaked00.PNG", "Rosanna\\RosannaBlink.PNG", "Rosanna\\RosannaNaked00Mask.PNG", "Rosanna\\Mouth", "Rosanna\\Expression", true, true);

                    sakura = CreateNewBust("SakuraBase", Core.bustNikkePath, "Sakura\\Sakura00.PNG", "Sakura\\SakuraBlink.PNG", "Sakura\\Sakura00Mask.PNG", "Sakura\\Mouth", "Sakura\\Expression", true, true);
                    //sakuraCoatless = CreateNewBust("SakuraCoatless", Core.bustNikkePath, "Sakura\\Sakura01.PNG", "Sakura\\SakuraBlink.PNG", "Sakura\\Sakura01Mask.PNG", "Sakura\\Mouth", "Sakura\\Expression", true, true);
                    sakuraUnderwear = CreateNewBust("SakuraShirtless", Core.bustNikkePath, "Sakura\\Sakura02.PNG", "Sakura\\SakuraBlink.PNG", "Sakura\\Sakura02Mask.PNG", "Sakura\\Mouth", "Sakura\\Expression", true, true);
                    sakuraSwim = CreateNewBust("SakuraSwim", Core.bustNikkePath, "Sakura\\SakuraSwim00.PNG", "Sakura\\SakuraBlink.PNG", "Sakura\\SakuraSwim00Mask.PNG", "Sakura\\Mouth", "Sakura\\Expression", true, true);
                    sakuraSwimSlip = CreateNewBust("SakuraSwimSlip", Core.bustNikkePath, "Sakura\\SakuraSwim01.PNG", "Sakura\\SakuraBlink.PNG", "Sakura\\SakuraSwim01Mask.PNG", "Sakura\\Mouth", "Sakura\\Expression", true, true);
                    //sakuraNaked = CreateNewBust("SakuraNaked", Core.bustNikkePath, "Sakura\\SakuraNaked00.PNG", "Sakura\\SakuraBlink.PNG", "Sakura\\SakuraNaked00Mask.PNG", "Sakura\\Mouth", "Sakura\\Expression", true, true);
                    
                    tove = CreateNewBust("ToveBase", Core.bustNikkePath, "Tove\\Tove00.PNG", "Tove\\ToveBlink.PNG", "Tove\\Tove00Mask.PNG", "Tove\\Mouth", "Tove\\Expression", true, true);
                    //toveCoatless = CreateNewBust("ToveCoatless", Core.bustNikkePath, "Tove\\Tove01.PNG", "Tove\\ToveBlink.PNG", "Tove\\Tove01Mask.PNG", "Tove\\Mouth", "Tove\\Expression", true, true);
                    toveSwim = CreateNewBust("ToveSwim", Core.bustNikkePath, "Tove\\ToveSwim00.PNG", "Tove\\ToveBlink.PNG", "Tove\\ToveSwim00Mask.PNG", "Tove\\Mouth", "Tove\\Expression", true, true);
                    toveSwimSlip = CreateNewBust("ToveSwimSlip", Core.bustNikkePath, "Tove\\ToveSwim01.PNG", "Tove\\ToveBlink.PNG", "Tove\\ToveSwim01Mask.PNG", "Tove\\Mouth", "Tove\\Expression", true, true);
                    //toveNaked = CreateNewBust("ToveNaked", Core.bustNikkePath, "Tove\\ToveNaked00.PNG", "Tove\\ToveBlink.PNG", "Tove\\ToveNaked00Mask.PNG", "Tove\\Mouth", "Tove\\Expression", true, true);

                    viper = CreateNewBust("ViperBase", Core.bustNikkePath, "Viper\\Viper00.PNG", "Viper\\ViperBlink.PNG", "Viper\\Viper00Mask.PNG", "Viper\\Mouth", "Viper\\Expression", true, true);
                    //viperUnderwear = CreateNewBust("ViperUnderwear", Core.bustNikkePath, "Viper\\Viper01.PNG", "Viper\\ViperBlink.PNG", "Viper\\Viper01Mask.PNG", "Viper\\Mouth", "Viper\\Expression", true, true);
                    viperSwim = CreateNewBust("ViperSwim", Core.bustNikkePath, "Viper\\ViperSwim00.PNG", "Viper\\ViperBlink.PNG", "Viper\\ViperSwim00Mask.PNG", "Viper\\Mouth", "Viper\\Expression", true, true);
                    viperSwimShirtless = CreateNewBust("ViperSwimShirtless", Core.bustNikkePath, "Viper\\ViperSwim01.PNG", "Viper\\ViperBlink.PNG", "Viper\\ViperSwim01Mask.PNG", "Viper\\Mouth", "Viper\\Expression", true, true);
                    viperSwimWet = CreateNewBust("ViperSwimWet", Core.bustNikkePath, "Viper\\ViperSwim02.PNG", "Viper\\ViperBlink.PNG", "Viper\\ViperSwim02Mask.PNG", "Viper\\Mouth", "Viper\\Expression", true, true);
                    viperSwimWet.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    viperSwimSlip = CreateNewBust("ViperSwimSlip", Core.bustNikkePath, "Viper\\ViperSwim03.PNG", "Viper\\ViperBlink.PNG", "Viper\\ViperSwim03Mask.PNG", "Viper\\Mouth", "Viper\\Expression", true, true);
                    viperSwimSlip.transform.Find("MBase1").Find("Wet").gameObject.SetActive(true);
                    //viperNaked = CreateNewBust("ViperNaked", Core.bustNikkePath, "Viper\\ViperNaked00.PNG", "Viper\\ViperBlink.PNG", "Viper\\ViperNaked00Mask.PNG", "Viper\\Mouth", "Viper\\Expression", true, true);

                    yan = CreateNewBust("YanBase", Core.bustNikkePath, "Yan\\Yan00.PNG", "Yan\\YanBlink.PNG", "Yan\\Yan00Mask.PNG", "Yan\\Mouth", "Yan\\Expression", true, true);
                    //yanUnderwear = CreateNewBust("YanUnderwear", Core.bustNikkePath, "Yan\\Yan01.PNG", "Yan\\YanBlink.PNG", "Yan\\Yan01Mask.PNG", "Yan\\Mouth", "Yan\\Expression", true, true);
                    yanSwim = CreateNewBust("YanSwim", Core.bustNikkePath, "Yan\\YanSwim00.PNG", "Yan\\YanBlink.PNG", "Yan\\YanSwim00Mask.PNG", "Yan\\Mouth", "Yan\\Expression", true, true);
                    yanSwimSlip = CreateNewBust("YanSwimSlip", Core.bustNikkePath, "Yan\\YanSwim01.PNG", "Yan\\YanBlink.PNG", "Yan\\YanSwim01Mask.PNG", "Yan\\Mouth", "Yan\\Expression", true, true);
                    //yanNaked = CreateNewBust("YanNaked", Core.bustNikkePath, "Yan\\YanNaked00.PNG", "Yan\\YanBlink.PNG", "Yan\\YanNaked00Mask.PNG", "Yan\\Mouth", "Yan\\Expression", true, true);



                    #region Vanilla
                    adrian = Core.bustManager.Find("Adrian_bust").gameObject;
                    ameliaSwim = Core.bustManager.Find("Amelia_Beach").gameObject;
                    anna = Core.bustManager.Find("Anna_Bust").gameObject;
                    doctorFrost = Core.bustManager.Find("doctorfrost_default").gameObject;
                    emmaSwim = Core.bustManager.Find("Emma_Swimwear").gameObject;
                    gabriel = Core.bustManager.Find("Gabriel_Bust").gameObject;
                    isabella = Core.bustManager.Find("Isabella").gameObject;
                    katarina = Core.bustManager.Find("Katarina_Normal").gameObject;
                    kate = Core.bustManager.Find("Kate").gameObject;
                    masterZhen = Core.bustManager.Find("Master_Default").gameObject;
                    mobsterBlonde = Core.bustManager.Find("S_Mobster1").gameObject;
                    river = Core.bustManager.Find("River_Base").gameObject;
                    samSwim = Core.bustManager.Find("Samantha_Swimsuit").gameObject;
                    sofia = Core.bustManager.Find("Sofia_Police").gameObject;
                    toni = Core.bustManager.Find("TomboyToni_BustDefault").gameObject;
                    #endregion


                    solidSnake = CreateNewBust("Snek", Core.bustPath, "Solid\\Snek00.PNG", "Solid\\SnekBlink.PNG", "Solid\\Snek00Mask.PNG", "Solid\\Mouth", "Solid\\Expression", false, false);


                    Core.bustManager.GetComponent<SpriteManager>().RefreshCache();
                    Logger.LogInfo("----- BUSTS LOADED -----");
                    loadedBusts = true;
                }

                if (Core.loadedBases)
                {
                }
            }
            if (Core.currentScene.name == "GameStart")
            {
                if (loadedBusts)
                {
                    Logger.LogInfo("----- BUSTS UNLOADED -----");
                    loadedBusts = false;
                }
            }
        }


        public GameObject CreateNewBust(string name, string pathToCG, string baseSprite, string blinkSprite, string maskSprite, string mouthSprite, string expressionSprite, bool hasMouth, bool hasExpression)
        {
            GameObject newBust = GameObject.Instantiate(Core.baseBust, Core.bustManager);
            newBust.name = name;
            GameObject mBase = newBust.transform.Find("MBase1").gameObject;
            GameObject blink = mBase.transform.Find("Blink").gameObject;
            GameObject mouth = mBase.transform.Find("Mouth").gameObject;
            GameObject expressions = mBase.transform.Find("Expressions").gameObject;
            Material mat = new Material(mBase.GetComponent<SpriteRenderer>().material);
            string[] expressionNames = { "Happy", "Angry", "Sad", "Flirty" };

            Texture2D tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            var rawData = System.IO.File.ReadAllBytes(pathToCG + baseSprite);
            tex.LoadImage(rawData);
            tex.filterMode = FilterMode.Point;
            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
            mBase.GetComponent<SpriteRenderer>().sprite = newSprite;

            tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            rawData = System.IO.File.ReadAllBytes(pathToCG + blinkSprite);
            tex.LoadImage(rawData);
            tex.filterMode = FilterMode.Point;
            newSprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
            blink.GetComponent<SpriteRenderer>().sprite = newSprite;

            tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            rawData = System.IO.File.ReadAllBytes(pathToCG + maskSprite);
            tex.LoadImage(rawData);
            tex.filterMode = FilterMode.Point;
            mat.SetTexture("_MaskTex", tex);
            mBase.GetComponent<SpriteRenderer>().material = mat;
            mBase.GetComponent<SpriteRenderer>().material.SetTexture("_MaskTex", tex);

            if (hasMouth)
            {
                for (int i = 1; i <= 4; i++)
                {
                    tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                    rawData = System.IO.File.ReadAllBytes(pathToCG + mouthSprite + i + ".PNG");
                    tex.LoadImage(rawData);
                    tex.filterMode = FilterMode.Point;
                    newSprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
                    mouth.transform.Find(i.ToString()).GetComponent<SpriteRenderer>().sprite = newSprite;
                }
            }
            else
            {
                for (int i = 1; i <= 4; i++)
                {
                    Destroy(mouth.transform.Find(i.ToString()).GetComponent<SpriteRenderer>());
                }
            }
            if (hasExpression)
            {
                foreach (string expressionName in expressionNames)
                {
                    tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                    rawData = System.IO.File.ReadAllBytes(pathToCG + expressionSprite + expressionName + ".PNG");
                    tex.LoadImage(rawData);
                    tex.filterMode = FilterMode.Point;
                    newSprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
                    expressions.transform.Find(expressionName).GetComponent<SpriteRenderer>().sprite = newSprite;
                }
            }
            else 
            {
                foreach (string expressionName in expressionNames)
                {
                    Destroy(expressions.transform.Find(expressionName).GetComponent<SpriteRenderer>());
                }
            }

            Destroy(expressions.GetComponent<Conditions>());
            Destroy(expressions.GetComponent<Trigger>());

            GameObject wetParticles = GameObject.Instantiate(Core.bustManager.Find("Anna_Towel").Find("MBase1").Find("Particle System").gameObject, mBase.transform);
            wetParticles.name = "Wet";
            wetParticles.SetActive(false);

            if (hasMouth)
            {
            }

            Core.bustManager.GetComponent<SpriteManager>().targetObjects.Add(newBust);

            newBust.SetActive(false);
            return newBust;
        }

        /// <summary>
        /// Creates a new NPC by copying the AnnaInShower GameObject from 5_Levels > 4_Bath,
        /// reparenting it, swapping its sprite and mask texture, disabling its particle system,
        /// and positioning it at the specified local position.
        /// </summary>
        public static GameObject CreateNewNPC(string name, string pathToCG, string spritePath, string maskSpritePath, Vector3 localPosition, Vector3 localScale, Vector3 particlePosition, Transform parent)
        {
            // Find the source AnnaInShower object under 5_Levels > 4_Bath
            GameObject source = Core.level.Find("4_Bath").Find("AnnaInShower").gameObject;
            GameObject newNPC = GameObject.Instantiate(source, parent);
            newNPC.name = name;

            // Change the SpriteRenderer's sprite
            SpriteRenderer sr = newNPC.GetComponent<SpriteRenderer>();
            Texture2D tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            var rawData = System.IO.File.ReadAllBytes(pathToCG + spritePath);
            tex.LoadImage(rawData);
            tex.filterMode = FilterMode.Point;
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            // Change the mask texture (same approach as CreateNewBust)
            Material mat = new Material(sr.material);
            Texture2D maskTex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            var maskData = System.IO.File.ReadAllBytes(pathToCG + maskSpritePath);
            maskTex.LoadImage(maskData);
            maskTex.filterMode = FilterMode.Point;
            mat.SetTexture("_MaskTex", maskTex);
            sr.material = mat;

            // Reposition and disable the "Particle System (1)" child
            Transform particleChild = newNPC.transform.Find("Particle System (1)");
            if (particleChild != null)
            {
                particleChild.localPosition = particlePosition;
                //particleChild.gameObject.SetActive(false);
            }

            // Add Blink GameObject if blink texture exists
            string blinkPath = System.IO.Path.Combine(pathToCG, System.IO.Path.GetDirectoryName(spritePath), 
                System.IO.Path.GetFileNameWithoutExtension(spritePath) + "Blink" + System.IO.Path.GetExtension(spritePath));
            
            if (System.IO.File.Exists(blinkPath))
            {
                GameObject blinkGO = new GameObject("Blink");
                blinkGO.transform.SetParent(newNPC.transform);
                blinkGO.transform.localPosition = Vector3.zero;
                blinkGO.transform.localScale = Vector3.one;
                
                SpriteRenderer blinkSR = blinkGO.AddComponent<SpriteRenderer>();
                Texture2D blinkTex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                var blinkData = System.IO.File.ReadAllBytes(blinkPath);
                blinkTex.LoadImage(blinkData);
                blinkTex.filterMode = FilterMode.Point;
                blinkSR.sprite = Sprite.Create(blinkTex, new Rect(0, 0, blinkTex.width, blinkTex.height), new Vector2(0.5f, 0.5f));
                blinkSR.sortingLayerName = sr.sortingLayerName;
                blinkSR.sortingOrder = sr.sortingOrder + 1;
                var newBlinking = blinkGO.AddComponent<BlinkingSprite>();
            }

            // Set the local position and scale
            newNPC.transform.localPosition = localPosition;
            newNPC.transform.localScale = localScale;

            newNPC.SetActive(false);
            return newNPC;
        }

        /// <summary>
        /// Recursively searches through all children of the specified GameObject for GameObjects named "Blink"
        /// and adds a BlinkingSprite component to them if they don't already have one.
        /// </summary>
        /// <param name="parent">The parent GameObject to search within</param>
        public static void AddBlinkingSpriteToBlinkObjects(GameObject parent)
        {
            if (parent == null)
            {
                Debug.LogWarning("AddBlinkingSpriteToBlinkObjects: parent GameObject is null");
                return;
            }

            // Check all children recursively
            foreach (Transform child in parent.transform)
            {
                // Check if this child is named "Blink"
                if (child.gameObject.name == "Blink")
                {
                    // Add BlinkingSprite component if it doesn't already exist
                    if (child.gameObject.GetComponent<BlinkingSprite>() == null)
                    {
                        child.gameObject.AddComponent<BlinkingSprite>();
                        Debug.Log($"Added BlinkingSprite component to {child.gameObject.name} at path: {GetGameObjectPath(child.gameObject)}");
                    }
                }

                // Recursively search this child's children
                AddBlinkingSpriteToBlinkObjects(child.gameObject);
            }
        }

        /// <summary>
        /// Helper method to get the full hierarchy path of a GameObject for debugging purposes.
        /// </summary>
        /// <param name="obj">The GameObject to get the path for</param>
        /// <returns>The full hierarchy path</returns>
        private static string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform current = obj.transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }

        /// <summary>
        /// Recursively searches through all children of the specified GameObject for GameObjects named "Particle System (1)"
        /// and copies the ParticleSystem component configuration from the reference object to them.
        /// </summary>
        /// <param name="parent">The parent GameObject to search within</param>
        public static void CopyParticleSystemToParticleObjects(GameObject parent)
        {
            if (parent == null)
            {
                Debug.LogWarning("CopyParticleSystemToParticleObjects: parent GameObject is null");
                return;
            }

            // Get the source ParticleSystem from AnnaInShower
            GameObject sourceGO = Core.level.Find("4_Bath").Find("AnnaInShower").Find("Particle System (1)").gameObject;
            ParticleSystem sourcePS = sourceGO.GetComponent<ParticleSystem>();
            
            if (sourcePS == null)
            {
                Debug.LogError("CopyParticleSystemToParticleObjects: Source ParticleSystem not found!");
                return;
            }

            // Check all children recursively
            foreach (Transform child in parent.transform)
            {
                // Check if this child is named "Particle System (1)"
                if (child.gameObject.name == "Particle System (1)")
                {
                    ParticleSystem targetPS = child.gameObject.GetComponent<ParticleSystem>();
                    if (targetPS != null)
                    {
                        CopyParticleSystemSettings(sourcePS, targetPS);
                        Debug.Log($"Copied ParticleSystem configuration to {child.gameObject.name} at path: {GetGameObjectPath(child.gameObject)}");
                    }
                }

                // Recursively search this child's children
                CopyParticleSystemToParticleObjects(child.gameObject);
            }
        }

        /// <summary>
        /// Copies all ParticleSystem settings from source to target.
        /// </summary>
        private static void CopyParticleSystemSettings(ParticleSystem source, ParticleSystem target)
        {
            // Main module
            var sourceMain = source.main;
            var targetMain = target.main;
            targetMain.duration = sourceMain.duration;
            targetMain.loop = sourceMain.loop;
            targetMain.prewarm = sourceMain.prewarm;
            targetMain.startDelay = sourceMain.startDelay;
            targetMain.startDelayMultiplier = sourceMain.startDelayMultiplier;
            targetMain.startLifetime = sourceMain.startLifetime;
            targetMain.startLifetimeMultiplier = sourceMain.startLifetimeMultiplier;
            targetMain.startSpeed = sourceMain.startSpeed;
            targetMain.startSpeedMultiplier = sourceMain.startSpeedMultiplier;
            targetMain.startSize3D = sourceMain.startSize3D;
            targetMain.startSize = sourceMain.startSize;
            targetMain.startSizeMultiplier = sourceMain.startSizeMultiplier;
            targetMain.startSizeX = sourceMain.startSizeX;
            targetMain.startSizeXMultiplier = sourceMain.startSizeXMultiplier;
            targetMain.startSizeY = sourceMain.startSizeY;
            targetMain.startSizeYMultiplier = sourceMain.startSizeYMultiplier;
            targetMain.startSizeZ = sourceMain.startSizeZ;
            targetMain.startSizeZMultiplier = sourceMain.startSizeZMultiplier;
            targetMain.startRotation3D = sourceMain.startRotation3D;
            targetMain.startRotation = sourceMain.startRotation;
            targetMain.startRotationMultiplier = sourceMain.startRotationMultiplier;
            targetMain.startRotationX = sourceMain.startRotationX;
            targetMain.startRotationXMultiplier = sourceMain.startRotationXMultiplier;
            targetMain.startRotationY = sourceMain.startRotationY;
            targetMain.startRotationYMultiplier = sourceMain.startRotationYMultiplier;
            targetMain.startRotationZ = sourceMain.startRotationZ;
            targetMain.startRotationZMultiplier = sourceMain.startRotationZMultiplier;
            targetMain.flipRotation = sourceMain.flipRotation;
            targetMain.startColor = sourceMain.startColor;
            targetMain.gravityModifier = sourceMain.gravityModifier;
            targetMain.gravityModifierMultiplier = sourceMain.gravityModifierMultiplier;
            targetMain.simulationSpace = sourceMain.simulationSpace;
            targetMain.simulationSpeed = sourceMain.simulationSpeed;
            targetMain.useUnscaledTime = sourceMain.useUnscaledTime;
            targetMain.scalingMode = sourceMain.scalingMode;
            targetMain.playOnAwake = sourceMain.playOnAwake;
            targetMain.emitterVelocityMode = sourceMain.emitterVelocityMode;
            targetMain.maxParticles = sourceMain.maxParticles;
            targetMain.stopAction = sourceMain.stopAction;
            targetMain.cullingMode = sourceMain.cullingMode;
            targetMain.ringBufferMode = sourceMain.ringBufferMode;
            targetMain.ringBufferLoopRange = sourceMain.ringBufferLoopRange;

            // Emission module
            var sourceEmission = source.emission;
            var targetEmission = target.emission;
            targetEmission.enabled = sourceEmission.enabled;
            targetEmission.rateOverTime = sourceEmission.rateOverTime;
            targetEmission.rateOverTimeMultiplier = sourceEmission.rateOverTimeMultiplier;
            targetEmission.rateOverDistance = sourceEmission.rateOverDistance;
            targetEmission.rateOverDistanceMultiplier = sourceEmission.rateOverDistanceMultiplier;

            // Shape module
            var sourceShape = source.shape;
            var targetShape = target.shape;
            targetShape.enabled = sourceShape.enabled;
            targetShape.shapeType = sourceShape.shapeType;
            targetShape.angle = sourceShape.angle;
            targetShape.radius = sourceShape.radius;
            targetShape.radiusThickness = sourceShape.radiusThickness;
            targetShape.arc = sourceShape.arc;
            targetShape.arcMode = sourceShape.arcMode;
            targetShape.arcSpread = sourceShape.arcSpread;
            targetShape.arcSpeed = sourceShape.arcSpeed;
            targetShape.arcSpeedMultiplier = sourceShape.arcSpeedMultiplier;
            targetShape.length = sourceShape.length;
            targetShape.boxThickness = sourceShape.boxThickness;
            targetShape.meshShapeType = sourceShape.meshShapeType;
            targetShape.mesh = sourceShape.mesh;
            targetShape.meshRenderer = sourceShape.meshRenderer;
            targetShape.skinnedMeshRenderer = sourceShape.skinnedMeshRenderer;
            targetShape.useMeshMaterialIndex = sourceShape.useMeshMaterialIndex;
            targetShape.meshMaterialIndex = sourceShape.meshMaterialIndex;
            targetShape.useMeshColors = sourceShape.useMeshColors;
            targetShape.normalOffset = sourceShape.normalOffset;
            targetShape.meshSpawnMode = sourceShape.meshSpawnMode;
            targetShape.meshSpawnSpread = sourceShape.meshSpawnSpread;
            targetShape.meshSpawnSpeed = sourceShape.meshSpawnSpeed;
            targetShape.meshSpawnSpeedMultiplier = sourceShape.meshSpawnSpeedMultiplier;
            targetShape.alignToDirection = sourceShape.alignToDirection;
            targetShape.randomDirectionAmount = sourceShape.randomDirectionAmount;
            targetShape.sphericalDirectionAmount = sourceShape.sphericalDirectionAmount;
            targetShape.randomPositionAmount = sourceShape.randomPositionAmount;
            targetShape.position = sourceShape.position;
            targetShape.rotation = sourceShape.rotation;
            targetShape.scale = sourceShape.scale;

            // Velocity over Lifetime module
            var sourceVelocity = source.velocityOverLifetime;
            var targetVelocity = target.velocityOverLifetime;
            targetVelocity.enabled = sourceVelocity.enabled;
            targetVelocity.x = sourceVelocity.x;
            targetVelocity.y = sourceVelocity.y;
            targetVelocity.z = sourceVelocity.z;
            targetVelocity.xMultiplier = sourceVelocity.xMultiplier;
            targetVelocity.yMultiplier = sourceVelocity.yMultiplier;
            targetVelocity.zMultiplier = sourceVelocity.zMultiplier;
            targetVelocity.space = sourceVelocity.space;
            targetVelocity.orbitalX = sourceVelocity.orbitalX;
            targetVelocity.orbitalY = sourceVelocity.orbitalY;
            targetVelocity.orbitalZ = sourceVelocity.orbitalZ;
            targetVelocity.orbitalXMultiplier = sourceVelocity.orbitalXMultiplier;
            targetVelocity.orbitalYMultiplier = sourceVelocity.orbitalYMultiplier;
            targetVelocity.orbitalZMultiplier = sourceVelocity.orbitalZMultiplier;
            targetVelocity.orbitalOffsetX = sourceVelocity.orbitalOffsetX;
            targetVelocity.orbitalOffsetY = sourceVelocity.orbitalOffsetY;
            targetVelocity.orbitalOffsetZ = sourceVelocity.orbitalOffsetZ;
            targetVelocity.orbitalOffsetXMultiplier = sourceVelocity.orbitalOffsetXMultiplier;
            targetVelocity.orbitalOffsetYMultiplier = sourceVelocity.orbitalOffsetYMultiplier;
            targetVelocity.orbitalOffsetZMultiplier = sourceVelocity.orbitalOffsetZMultiplier;
            targetVelocity.radial = sourceVelocity.radial;
            targetVelocity.radialMultiplier = sourceVelocity.radialMultiplier;
            targetVelocity.speedModifier = sourceVelocity.speedModifier;
            targetVelocity.speedModifierMultiplier = sourceVelocity.speedModifierMultiplier;

            // Limit Velocity over Lifetime module
            var sourceLimitVelocity = source.limitVelocityOverLifetime;
            var targetLimitVelocity = target.limitVelocityOverLifetime;
            targetLimitVelocity.enabled = sourceLimitVelocity.enabled;
            targetLimitVelocity.limitX = sourceLimitVelocity.limitX;
            targetLimitVelocity.limitY = sourceLimitVelocity.limitY;
            targetLimitVelocity.limitZ = sourceLimitVelocity.limitZ;
            targetLimitVelocity.limitXMultiplier = sourceLimitVelocity.limitXMultiplier;
            targetLimitVelocity.limitYMultiplier = sourceLimitVelocity.limitYMultiplier;
            targetLimitVelocity.limitZMultiplier = sourceLimitVelocity.limitZMultiplier;
            targetLimitVelocity.limit = sourceLimitVelocity.limit;
            targetLimitVelocity.limitMultiplier = sourceLimitVelocity.limitMultiplier;
            targetLimitVelocity.dampen = sourceLimitVelocity.dampen;
            targetLimitVelocity.separateAxes = sourceLimitVelocity.separateAxes;
            targetLimitVelocity.space = sourceLimitVelocity.space;
            targetLimitVelocity.drag = sourceLimitVelocity.drag;
            targetLimitVelocity.dragMultiplier = sourceLimitVelocity.dragMultiplier;
            targetLimitVelocity.multiplyDragByParticleSize = sourceLimitVelocity.multiplyDragByParticleSize;
            targetLimitVelocity.multiplyDragByParticleVelocity = sourceLimitVelocity.multiplyDragByParticleVelocity;

            // Color over Lifetime module
            var sourceColor = source.colorOverLifetime;
            var targetColor = target.colorOverLifetime;
            targetColor.enabled = sourceColor.enabled;
            targetColor.color = sourceColor.color;

            // Color by Speed module
            var sourceColorBySpeed = source.colorBySpeed;
            var targetColorBySpeed = target.colorBySpeed;
            targetColorBySpeed.enabled = sourceColorBySpeed.enabled;
            targetColorBySpeed.color = sourceColorBySpeed.color;
            targetColorBySpeed.range = sourceColorBySpeed.range;

            // Size over Lifetime module
            var sourceSize = source.sizeOverLifetime;
            var targetSize = target.sizeOverLifetime;
            targetSize.enabled = sourceSize.enabled;
            targetSize.separateAxes = sourceSize.separateAxes;
            targetSize.size = sourceSize.size;
            targetSize.sizeMultiplier = sourceSize.sizeMultiplier;
            targetSize.x = sourceSize.x;
            targetSize.xMultiplier = sourceSize.xMultiplier;
            targetSize.y = sourceSize.y;
            targetSize.yMultiplier = sourceSize.yMultiplier;
            targetSize.z = sourceSize.z;
            targetSize.zMultiplier = sourceSize.zMultiplier;

            // Size by Speed module
            var sourceSizeBySpeed = source.sizeBySpeed;
            var targetSizeBySpeed = target.sizeBySpeed;
            targetSizeBySpeed.enabled = sourceSizeBySpeed.enabled;
            targetSizeBySpeed.separateAxes = sourceSizeBySpeed.separateAxes;
            targetSizeBySpeed.size = sourceSizeBySpeed.size;
            targetSizeBySpeed.sizeMultiplier = sourceSizeBySpeed.sizeMultiplier;
            targetSizeBySpeed.x = sourceSizeBySpeed.x;
            targetSizeBySpeed.xMultiplier = sourceSizeBySpeed.xMultiplier;
            targetSizeBySpeed.y = sourceSizeBySpeed.y;
            targetSizeBySpeed.yMultiplier = sourceSizeBySpeed.yMultiplier;
            targetSizeBySpeed.z = sourceSizeBySpeed.z;
            targetSizeBySpeed.zMultiplier = sourceSizeBySpeed.zMultiplier;
            targetSizeBySpeed.range = sourceSizeBySpeed.range;

            // Rotation over Lifetime module
            var sourceRotation = source.rotationOverLifetime;
            var targetRotation = target.rotationOverLifetime;
            targetRotation.enabled = sourceRotation.enabled;
            targetRotation.x = sourceRotation.x;
            targetRotation.xMultiplier = sourceRotation.xMultiplier;
            targetRotation.y = sourceRotation.y;
            targetRotation.yMultiplier = sourceRotation.yMultiplier;
            targetRotation.z = sourceRotation.z;
            targetRotation.zMultiplier = sourceRotation.zMultiplier;
            targetRotation.separateAxes = sourceRotation.separateAxes;

            // Rotation by Speed module
            var sourceRotationBySpeed = source.rotationBySpeed;
            var targetRotationBySpeed = target.rotationBySpeed;
            targetRotationBySpeed.enabled = sourceRotationBySpeed.enabled;
            targetRotationBySpeed.x = sourceRotationBySpeed.x;
            targetRotationBySpeed.xMultiplier = sourceRotationBySpeed.xMultiplier;
            targetRotationBySpeed.y = sourceRotationBySpeed.y;
            targetRotationBySpeed.yMultiplier = sourceRotationBySpeed.yMultiplier;
            targetRotationBySpeed.z = sourceRotationBySpeed.z;
            targetRotationBySpeed.zMultiplier = sourceRotationBySpeed.zMultiplier;
            targetRotationBySpeed.separateAxes = sourceRotationBySpeed.separateAxes;
            targetRotationBySpeed.range = sourceRotationBySpeed.range;

            // Noise module
            var sourceNoise = source.noise;
            var targetNoise = target.noise;
            targetNoise.enabled = sourceNoise.enabled;
            targetNoise.separateAxes = sourceNoise.separateAxes;
            targetNoise.strength = sourceNoise.strength;
            targetNoise.strengthMultiplier = sourceNoise.strengthMultiplier;
            targetNoise.strengthX = sourceNoise.strengthX;
            targetNoise.strengthXMultiplier = sourceNoise.strengthXMultiplier;
            targetNoise.strengthY = sourceNoise.strengthY;
            targetNoise.strengthYMultiplier = sourceNoise.strengthYMultiplier;
            targetNoise.strengthZ = sourceNoise.strengthZ;
            targetNoise.strengthZMultiplier = sourceNoise.strengthZMultiplier;
            targetNoise.frequency = sourceNoise.frequency;
            targetNoise.scrollSpeed = sourceNoise.scrollSpeed;
            targetNoise.scrollSpeedMultiplier = sourceNoise.scrollSpeedMultiplier;
            targetNoise.damping = sourceNoise.damping;
            targetNoise.octaveCount = sourceNoise.octaveCount;
            targetNoise.octaveMultiplier = sourceNoise.octaveMultiplier;
            targetNoise.octaveScale = sourceNoise.octaveScale;
            targetNoise.quality = sourceNoise.quality;
            targetNoise.remapEnabled = sourceNoise.remapEnabled;
            targetNoise.remap = sourceNoise.remap;
            targetNoise.remapMultiplier = sourceNoise.remapMultiplier;
            targetNoise.remapX = sourceNoise.remapX;
            targetNoise.remapXMultiplier = sourceNoise.remapXMultiplier;
            targetNoise.remapY = sourceNoise.remapY;
            targetNoise.remapYMultiplier = sourceNoise.remapYMultiplier;
            targetNoise.remapZ = sourceNoise.remapZ;
            targetNoise.remapZMultiplier = sourceNoise.remapZMultiplier;
            targetNoise.positionAmount = sourceNoise.positionAmount;
            targetNoise.rotationAmount = sourceNoise.rotationAmount;
            targetNoise.sizeAmount = sourceNoise.sizeAmount;

            // Renderer module
            var sourceRenderer = source.GetComponent<ParticleSystemRenderer>();
            var targetRenderer = target.GetComponent<ParticleSystemRenderer>();
            if (sourceRenderer != null && targetRenderer != null)
            {
                targetRenderer.renderMode = sourceRenderer.renderMode;
                targetRenderer.sortMode = sourceRenderer.sortMode;
                targetRenderer.sortingFudge = sourceRenderer.sortingFudge;
                targetRenderer.normalDirection = sourceRenderer.normalDirection;
                targetRenderer.material = sourceRenderer.material;
                targetRenderer.trailMaterial = sourceRenderer.trailMaterial;
                targetRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
                targetRenderer.sortingOrder = sourceRenderer.sortingOrder;
                targetRenderer.minParticleSize = sourceRenderer.minParticleSize;
                targetRenderer.maxParticleSize = sourceRenderer.maxParticleSize;
                targetRenderer.alignment = sourceRenderer.alignment;
                targetRenderer.flip = sourceRenderer.flip;
                targetRenderer.allowRoll = sourceRenderer.allowRoll;
                targetRenderer.pivot = sourceRenderer.pivot;
                targetRenderer.maskInteraction = sourceRenderer.maskInteraction;
                targetRenderer.enableGPUInstancing = sourceRenderer.enableGPUInstancing;
                targetRenderer.shadowCastingMode = sourceRenderer.shadowCastingMode;
                targetRenderer.receiveShadows = sourceRenderer.receiveShadows;
                targetRenderer.shadowBias = sourceRenderer.shadowBias;
                targetRenderer.motionVectorGenerationMode = sourceRenderer.motionVectorGenerationMode;
                targetRenderer.forceRenderingOff = sourceRenderer.forceRenderingOff;
                targetRenderer.lightProbeUsage = sourceRenderer.lightProbeUsage;
                targetRenderer.reflectionProbeUsage = sourceRenderer.reflectionProbeUsage;
                targetRenderer.probeAnchor = sourceRenderer.probeAnchor;
            }
        }


    }
}
