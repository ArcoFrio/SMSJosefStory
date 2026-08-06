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
    [BepInPlugin(pluginGuid, Core.pluginName + " - Scenes", Core.pluginVersion)]
    internal class Scenes : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.scenes";
        #endregion

        public static GameObject amberEventHospitalhallwayScene01;
        public static GameObject amberEventHospitalhallwayScene02;
        public static GameObject amberStorySecretbeachScene01;

        public static GameObject anisAffection01Scene01;
        public static GameObject anisAffection01Scene02;
        public static GameObject anisAffection02Scene01;
        public static GameObject anisAffection02Scene02;
        public static GameObject anisAffection02Scene03;
        public static GameObject anisAffection02Scene04;
        public static GameObject anisAffection02Scene05;
        public static GameObject anisAffection03Scene01;
        public static GameObject anisAffection03Scene02;
        public static GameObject anisAffection03Scene03;
        public static GameObject anisAffection03Scene04;
        public static GameObject anisAffection03Scene05;
        public static GameObject anisChillToplessScene01;
        public static GameObject anisChillToplessScene02;
        public static GameObject anisChillToplessScene03;
        public static GameObject anisChillToplessScene04;
        public static GameObject anisChillToplessScene05;
        public static GameObject anisChillToplessScene06;
        public static GameObject anisEventMallScene01;

        public static GameObject centiEventKenshomeScene01;
        public static GameObject dorothyEventParkScene01;
        public static GameObject eleggEventDowntownScene01;
        public static GameObject frimaEventHotelScene01;
        public static GameObject guiltyEventParkinglotScene01;
        public static GameObject helmEventBeachScene01;
        public static GameObject maidenEventAlleyScene01;
        public static GameObject maryEventHospitalhallwayScene01;
        public static GameObject mastEventBeachScene01;
        public static GameObject neonEventTempleScene01;
        public static GameObject pepperEventHospitalScene01;
        public static GameObject rapiEventGasstationScene01;
        public static GameObject rosannaEventGabrielsmansionScene01;
        public static GameObject sakuraEventForestScene01;
        public static GameObject toveEventTrailScene01;
        public static GameObject viperEventVillaScene01;
        public static GameObject yanEventMallScene01;

        #region Voyeur Scene Variables
        public static GameObject anisVoyeurSecretbeachScene01;
        public static GameObject anisVoyeurSecretbeachScene02;
        public static GameObject anisVoyeurSecretbeachScene03;
        public static GameObject anisVoyeurSecretbeachScene04;

        public static GameObject centiVoyeurSecretbeachScene01;
        public static GameObject centiVoyeurSecretbeachScene02;
        public static GameObject centiVoyeurSecretbeachScene03;
        public static GameObject centiVoyeurSecretbeachScene04;

        public static GameObject dorothyVoyeurSecretbeachScene01;
        public static GameObject dorothyVoyeurSecretbeachScene02;
        public static GameObject dorothyVoyeurSecretbeachScene03;
        public static GameObject dorothyVoyeurSecretbeachScene04;

        public static GameObject eleggVoyeurSecretbeachScene01;
        public static GameObject eleggVoyeurSecretbeachScene02;
        public static GameObject eleggVoyeurSecretbeachScene03;
        public static GameObject eleggVoyeurSecretbeachScene04;

        public static GameObject frimaVoyeurSecretbeachScene01;
        public static GameObject frimaVoyeurSecretbeachScene02;
        public static GameObject frimaVoyeurSecretbeachScene03;
        public static GameObject frimaVoyeurSecretbeachScene04;

        public static GameObject guiltyVoyeurSecretbeachScene01;
        public static GameObject guiltyVoyeurSecretbeachScene02;
        public static GameObject guiltyVoyeurSecretbeachScene03;
        public static GameObject guiltyVoyeurSecretbeachScene04;

        public static GameObject helmVoyeurSecretbeachScene01;
        public static GameObject helmVoyeurSecretbeachScene02;
        public static GameObject helmVoyeurSecretbeachScene03;
        public static GameObject helmVoyeurSecretbeachScene04;

        public static GameObject maidenVoyeurSecretbeachScene01;
        public static GameObject maidenVoyeurSecretbeachScene02;
        public static GameObject maidenVoyeurSecretbeachScene03;
        public static GameObject maidenVoyeurSecretbeachScene04;

        public static GameObject maryVoyeurSecretbeachScene01;
        public static GameObject maryVoyeurSecretbeachScene02;
        public static GameObject maryVoyeurSecretbeachScene03;
        public static GameObject maryVoyeurSecretbeachScene04;

        public static GameObject mastVoyeurSecretbeachScene01;
        public static GameObject mastVoyeurSecretbeachScene02;
        public static GameObject mastVoyeurSecretbeachScene03;
        public static GameObject mastVoyeurSecretbeachScene04;

        public static GameObject neonVoyeurSecretbeachScene01;
        public static GameObject neonVoyeurSecretbeachScene02;
        public static GameObject neonVoyeurSecretbeachScene03;
        public static GameObject neonVoyeurSecretbeachScene04;

        public static GameObject pepperVoyeurSecretbeachScene01;
        public static GameObject pepperVoyeurSecretbeachScene02;
        public static GameObject pepperVoyeurSecretbeachScene03;
        public static GameObject pepperVoyeurSecretbeachScene04;

        public static GameObject rapiVoyeurSecretbeachScene01;
        public static GameObject rapiVoyeurSecretbeachScene02;
        public static GameObject rapiVoyeurSecretbeachScene03;
        public static GameObject rapiVoyeurSecretbeachScene04;

        public static GameObject rosannaVoyeurSecretbeachScene01;
        public static GameObject rosannaVoyeurSecretbeachScene02;
        public static GameObject rosannaVoyeurSecretbeachScene03;
        public static GameObject rosannaVoyeurSecretbeachScene04;

        public static GameObject sakuraVoyeurSecretbeachScene01;
        public static GameObject sakuraVoyeurSecretbeachScene02;
        public static GameObject sakuraVoyeurSecretbeachScene03;
        public static GameObject sakuraVoyeurSecretbeachScene04;

        public static GameObject toveVoyeurSecretbeachScene01;
        public static GameObject toveVoyeurSecretbeachScene02;
        public static GameObject toveVoyeurSecretbeachScene03;
        public static GameObject toveVoyeurSecretbeachScene04;

        public static GameObject viperVoyeurSecretbeachScene01;
        public static GameObject viperVoyeurSecretbeachScene02;
        public static GameObject viperVoyeurSecretbeachScene03;
        public static GameObject viperVoyeurSecretbeachScene04;

        public static GameObject yanVoyeurSecretbeachScene01;
        public static GameObject yanVoyeurSecretbeachScene02;
        public static GameObject yanVoyeurSecretbeachScene03;
        public static GameObject yanVoyeurSecretbeachScene04;
        #endregion

        public static bool loadedScenes = false;
        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                if (!loadedScenes)
                {
                    amberEventHospitalhallwayScene01 = CreateNewPicScene("AmberEventHospitalhallwayScene01", Core.scenePath + "Amber\\AmberEventHospitalhallwayScene01.PNG");
                    amberEventHospitalhallwayScene02 = CreateNewPicScene("AmberEventHospitalhallwayScene02", Core.scenePath + "Amber\\AmberEventHospitalhallwayScene02.PNG");
                    amberStorySecretbeachScene01 = CreateNewPicScene("AmberStorySecretbeachScene01", Core.scenePath + "Amber\\AmberStorySecretbeachScene01.PNG");

                    anisAffection01Scene01 = CreateNewPicScene("AnisAffection01Scene01", Core.scenePath + "Anis\\AnisAffection01Scene01.PNG");
                    anisAffection01Scene02 = CreateNewPicScene("AnisAffection01Scene02", Core.scenePath + "Anis\\AnisAffection01Scene02.PNG");
                    anisAffection02Scene01 = CreateNewPicScene("AnisAffection02Scene01", Core.scenePath + "Anis\\AnisAffection02Scene01.PNG");
                    anisAffection02Scene02 = CreateNewPicScene("AnisAffection02Scene02", Core.scenePath + "Anis\\AnisAffection02Scene02.PNG");
                    anisAffection02Scene03 = CreateNewPicScene("AnisAffection02Scene03", Core.scenePath + "Anis\\AnisAffection02Scene03.PNG");
                    anisAffection02Scene04 = CreateNewPicScene("AnisAffection02Scene04", Core.scenePath + "Anis\\AnisAffection02Scene04.PNG");
                    anisAffection02Scene05 = CreateNewPicScene("AnisAffection02Scene05", Core.scenePath + "Anis\\AnisAffection02Scene05.PNG");
                    anisAffection03Scene01 = CreateNewPicScene("AnisAffection03Scene01", Core.scenePath + "Anis\\AnisAffection03Scene01.PNG");
                    anisAffection03Scene02 = CreateNewPicScene("AnisAffection03Scene02", Core.scenePath + "Anis\\AnisAffection03Scene02.PNG");
                    anisAffection03Scene03 = CreateNewPicScene("AnisAffection03Scene03", Core.scenePath + "Anis\\AnisAffection03Scene03.PNG");
                    anisAffection03Scene04 = CreateNewPicScene("AnisAffection03Scene04", Core.scenePath + "Anis\\AnisAffection03Scene04.PNG");
                    anisAffection03Scene05 = CreateNewPicScene("AnisAffection03Scene05", Core.scenePath + "Anis\\AnisAffection03Scene05.PNG");
                    anisEventMallScene01 = CreateNewPicScene("AnisEventMallScene01", Core.scenePath + "Anis\\AnisEventMallScene01.PNG");
                    anisChillToplessScene01 = CreateNewPicScene("AnisChillToplessScene01", Core.scenePath + "Anis\\AnisChillToplessScene01.PNG");
                    anisChillToplessScene02 = CreateNewPicScene("AnisChillToplessScene02", Core.scenePath + "Anis\\AnisChillToplessScene02.PNG");
                    anisChillToplessScene03 = CreateNewPicScene("AnisChillToplessScene03", Core.scenePath + "Anis\\AnisChillToplessScene03.PNG");
                    anisChillToplessScene04 = CreateNewPicScene("AnisChillToplessScene04", Core.scenePath + "Anis\\AnisChillToplessScene04.PNG");
                    anisChillToplessScene05 = CreateNewPicScene("AnisChillToplessScene05", Core.scenePath + "Anis\\AnisChillToplessScene05.PNG");
                    anisChillToplessScene06 = CreateNewPicScene("AnisChillToplessScene06", Core.scenePath + "Anis\\AnisChillToplessScene06.PNG");
                    anisVoyeurSecretbeachScene01 = CreateNewPicScene("AnisVoyeurSecretbeachScene01", Core.scenePath + "Anis\\AnisVoyeurSecretbeachScene01.PNG");
                    anisVoyeurSecretbeachScene02 = CreateNewPicScene("AnisVoyeurSecretbeachScene02", Core.scenePath + "Anis\\AnisVoyeurSecretbeachScene02.PNG");
                    anisVoyeurSecretbeachScene03 = CreateNewPicScene("AnisVoyeurSecretbeachScene03", Core.scenePath + "Anis\\AnisVoyeurSecretbeachScene03.PNG");
                    anisVoyeurSecretbeachScene04 = CreateNewPicScene("AnisVoyeurSecretbeachScene04", Core.scenePath + "Anis\\AnisVoyeurSecretbeachScene04.PNG");

                    centiEventKenshomeScene01 = CreateNewPicScene("CentiEventKenshomeScene01", Core.scenePath + "Centi\\CentiEventKenshomeScene01.PNG");
                    centiVoyeurSecretbeachScene01 = CreateNewPicScene("CentiVoyeurSecretbeachScene01", Core.scenePath + "Centi\\CentiVoyeurSecretbeachScene01.PNG");
                    centiVoyeurSecretbeachScene02 = CreateNewPicScene("CentiVoyeurSecretbeachScene02", Core.scenePath + "Centi\\CentiVoyeurSecretbeachScene02.PNG");
                    centiVoyeurSecretbeachScene03 = CreateNewPicScene("CentiVoyeurSecretbeachScene03", Core.scenePath + "Centi\\CentiVoyeurSecretbeachScene03.PNG");
                    centiVoyeurSecretbeachScene04 = CreateNewPicScene("CentiVoyeurSecretbeachScene04", Core.scenePath + "Centi\\CentiVoyeurSecretbeachScene04.PNG");

                    dorothyEventParkScene01 = CreateNewPicScene("DorothyEventParkScene01", Core.scenePath + "Dorothy\\DorothyEventParkScene01.PNG");
                    dorothyVoyeurSecretbeachScene01 = CreateNewPicScene("DorothyVoyeurSecretbeachScene01", Core.scenePath + "Dorothy\\DorothyVoyeurSecretbeachScene01.PNG");
                    dorothyVoyeurSecretbeachScene02 = CreateNewPicScene("DorothyVoyeurSecretbeachScene02", Core.scenePath + "Dorothy\\DorothyVoyeurSecretbeachScene02.PNG");
                    dorothyVoyeurSecretbeachScene03 = CreateNewPicScene("DorothyVoyeurSecretbeachScene03", Core.scenePath + "Dorothy\\DorothyVoyeurSecretbeachScene03.PNG");
                    dorothyVoyeurSecretbeachScene04 = CreateNewPicScene("DorothyVoyeurSecretbeachScene04", Core.scenePath + "Dorothy\\DorothyVoyeurSecretbeachScene04.PNG");

                    eleggEventDowntownScene01 = CreateNewPicScene("EleggEventDowntownScene01", Core.scenePath + "Elegg\\EleggEventDowntownScene01.PNG");
                    eleggVoyeurSecretbeachScene01 = CreateNewPicScene("EleggVoyeurSecretbeachScene01", Core.scenePath + "Elegg\\EleggVoyeurSecretbeachScene01.PNG");
                    eleggVoyeurSecretbeachScene02 = CreateNewPicScene("EleggVoyeurSecretbeachScene02", Core.scenePath + "Elegg\\EleggVoyeurSecretbeachScene02.PNG");
                    eleggVoyeurSecretbeachScene03 = CreateNewPicScene("EleggVoyeurSecretbeachScene03", Core.scenePath + "Elegg\\EleggVoyeurSecretbeachScene03.PNG");
                    eleggVoyeurSecretbeachScene04 = CreateNewPicScene("EleggVoyeurSecretbeachScene04", Core.scenePath + "Elegg\\EleggVoyeurSecretbeachScene04.PNG");

                    frimaEventHotelScene01 = CreateNewPicScene("FrimaEventHotelScene01", Core.scenePath + "Frima\\FrimaEventHotelScene01.PNG");
                    frimaVoyeurSecretbeachScene01 = CreateNewPicScene("FrimaVoyeurSecretbeachScene01", Core.scenePath + "Frima\\FrimaVoyeurSecretbeachScene01.PNG");
                    frimaVoyeurSecretbeachScene02 = CreateNewPicScene("FrimaVoyeurSecretbeachScene02", Core.scenePath + "Frima\\FrimaVoyeurSecretbeachScene02.PNG");
                    frimaVoyeurSecretbeachScene03 = CreateNewPicScene("FrimaVoyeurSecretbeachScene03", Core.scenePath + "Frima\\FrimaVoyeurSecretbeachScene03.PNG");
                    frimaVoyeurSecretbeachScene04 = CreateNewPicScene("FrimaVoyeurSecretbeachScene04", Core.scenePath + "Frima\\FrimaVoyeurSecretbeachScene04.PNG");

                    guiltyEventParkinglotScene01 = CreateNewPicScene("GuiltyEventParkinglotScene01", Core.scenePath + "Guilty\\GuiltyEventParkinglotScene01.PNG");
                    guiltyVoyeurSecretbeachScene01 = CreateNewPicScene("GuiltyVoyeurSecretbeachScene01", Core.scenePath + "Guilty\\GuiltyVoyeurSecretbeachScene01.PNG");
                    guiltyVoyeurSecretbeachScene02 = CreateNewPicScene("GuiltyVoyeurSecretbeachScene02", Core.scenePath + "Guilty\\GuiltyVoyeurSecretbeachScene02.PNG");
                    guiltyVoyeurSecretbeachScene03 = CreateNewPicScene("GuiltyVoyeurSecretbeachScene03", Core.scenePath + "Guilty\\GuiltyVoyeurSecretbeachScene03.PNG");
                    guiltyVoyeurSecretbeachScene04 = CreateNewPicScene("GuiltyVoyeurSecretbeachScene04", Core.scenePath + "Guilty\\GuiltyVoyeurSecretbeachScene04.PNG");

                    helmEventBeachScene01 = CreateNewPicScene("HelmEventBeachScene01", Core.scenePath + "Helm\\HelmEventBeachScene01.PNG");
                    helmVoyeurSecretbeachScene01 = CreateNewPicScene("HelmVoyeurSecretbeachScene01", Core.scenePath + "Helm\\HelmVoyeurSecretbeachScene01.PNG");
                    helmVoyeurSecretbeachScene02 = CreateNewPicScene("HelmVoyeurSecretbeachScene02", Core.scenePath + "Helm\\HelmVoyeurSecretbeachScene02.PNG");
                    helmVoyeurSecretbeachScene03 = CreateNewPicScene("HelmVoyeurSecretbeachScene03", Core.scenePath + "Helm\\HelmVoyeurSecretbeachScene03.PNG");
                    helmVoyeurSecretbeachScene04 = CreateNewPicScene("HelmVoyeurSecretbeachScene04", Core.scenePath + "Helm\\HelmVoyeurSecretbeachScene04.PNG");

                    maidenEventAlleyScene01 = CreateNewPicScene("MaidenEventAlleyScene01", Core.scenePath + "Maiden\\MaidenEventAlleyScene01.PNG");
                    maidenVoyeurSecretbeachScene01 = CreateNewPicScene("MaidenVoyeurSecretbeachScene01", Core.scenePath + "Maiden\\MaidenVoyeurSecretbeachScene01.PNG");
                    maidenVoyeurSecretbeachScene02 = CreateNewPicScene("MaidenVoyeurSecretbeachScene02", Core.scenePath + "Maiden\\MaidenVoyeurSecretbeachScene02.PNG");
                    maidenVoyeurSecretbeachScene03 = CreateNewPicScene("MaidenVoyeurSecretbeachScene03", Core.scenePath + "Maiden\\MaidenVoyeurSecretbeachScene03.PNG");
                    maidenVoyeurSecretbeachScene04 = CreateNewPicScene("MaidenVoyeurSecretbeachScene04", Core.scenePath + "Maiden\\MaidenVoyeurSecretbeachScene04.PNG");

                    maryEventHospitalhallwayScene01 = CreateNewPicScene("MaryEventHospitalhallwayScene01", Core.scenePath + "Mary\\MaryEventHospitalhallwayScene01.PNG");
                    maryVoyeurSecretbeachScene01 = CreateNewPicScene("MaryVoyeurSecretbeachScene01", Core.scenePath + "Mary\\MaryVoyeurSecretbeachScene01.PNG");
                    maryVoyeurSecretbeachScene02 = CreateNewPicScene("MaryVoyeurSecretbeachScene02", Core.scenePath + "Mary\\MaryVoyeurSecretbeachScene02.PNG");
                    maryVoyeurSecretbeachScene03 = CreateNewPicScene("MaryVoyeurSecretbeachScene03", Core.scenePath + "Mary\\MaryVoyeurSecretbeachScene03.PNG");
                    maryVoyeurSecretbeachScene04 = CreateNewPicScene("MaryVoyeurSecretbeachScene04", Core.scenePath + "Mary\\MaryVoyeurSecretbeachScene04.PNG");

                    mastEventBeachScene01 = CreateNewPicScene("MastEventBeachScene01", Core.scenePath + "Mast\\MastEventBeachScene01.PNG");
                    mastVoyeurSecretbeachScene01 = CreateNewPicScene("MastVoyeurSecretbeachScene01", Core.scenePath + "Mast\\MastVoyeurSecretbeachScene01.PNG");
                    mastVoyeurSecretbeachScene02 = CreateNewPicScene("MastVoyeurSecretbeachScene02", Core.scenePath + "Mast\\MastVoyeurSecretbeachScene02.PNG");
                    mastVoyeurSecretbeachScene03 = CreateNewPicScene("MastVoyeurSecretbeachScene03", Core.scenePath + "Mast\\MastVoyeurSecretbeachScene03.PNG");
                    mastVoyeurSecretbeachScene04 = CreateNewPicScene("MastVoyeurSecretbeachScene04", Core.scenePath + "Mast\\MastVoyeurSecretbeachScene04.PNG");

                    neonEventTempleScene01 = CreateNewPicScene("NeonEventTempleScene01", Core.scenePath + "Neon\\NeonEventTempleScene01.PNG");
                    neonVoyeurSecretbeachScene01 = CreateNewPicScene("NeonVoyeurSecretbeachScene01", Core.scenePath + "Neon\\NeonVoyeurSecretbeachScene01.PNG");
                    neonVoyeurSecretbeachScene02 = CreateNewPicScene("NeonVoyeurSecretbeachScene02", Core.scenePath + "Neon\\NeonVoyeurSecretbeachScene02.PNG");
                    neonVoyeurSecretbeachScene03 = CreateNewPicScene("NeonVoyeurSecretbeachScene03", Core.scenePath + "Neon\\NeonVoyeurSecretbeachScene03.PNG");
                    neonVoyeurSecretbeachScene04 = CreateNewPicScene("NeonVoyeurSecretbeachScene04", Core.scenePath + "Neon\\NeonVoyeurSecretbeachScene04.PNG");

                    pepperEventHospitalScene01 = CreateNewPicScene("PepperEventHospitalScene01", Core.scenePath + "Pepper\\PepperEventHospitalScene01.PNG");
                    pepperVoyeurSecretbeachScene01 = CreateNewPicScene("PepperVoyeurSecretbeachScene01", Core.scenePath + "Pepper\\PepperVoyeurSecretbeachScene01.PNG");
                    pepperVoyeurSecretbeachScene02 = CreateNewPicScene("PepperVoyeurSecretbeachScene02", Core.scenePath + "Pepper\\PepperVoyeurSecretbeachScene02.PNG");
                    pepperVoyeurSecretbeachScene03 = CreateNewPicScene("PepperVoyeurSecretbeachScene03", Core.scenePath + "Pepper\\PepperVoyeurSecretbeachScene03.PNG");
                    pepperVoyeurSecretbeachScene04 = CreateNewPicScene("PepperVoyeurSecretbeachScene04", Core.scenePath + "Pepper\\PepperVoyeurSecretbeachScene04.PNG");

                    rapiEventGasstationScene01 = CreateNewPicScene("RapiEventGasstationScene01", Core.scenePath + "Rapi\\RapiEventGasstationScene01.PNG");
                    rapiVoyeurSecretbeachScene01 = CreateNewPicScene("RapiVoyeurSecretbeachScene01", Core.scenePath + "Rapi\\RapiVoyeurSecretbeachScene01.PNG");
                    rapiVoyeurSecretbeachScene02 = CreateNewPicScene("RapiVoyeurSecretbeachScene02", Core.scenePath + "Rapi\\RapiVoyeurSecretbeachScene02.PNG");
                    rapiVoyeurSecretbeachScene03 = CreateNewPicScene("RapiVoyeurSecretbeachScene03", Core.scenePath + "Rapi\\RapiVoyeurSecretbeachScene03.PNG");
                    rapiVoyeurSecretbeachScene04 = CreateNewPicScene("RapiVoyeurSecretbeachScene04", Core.scenePath + "Rapi\\RapiVoyeurSecretbeachScene04.PNG");

                    rosannaEventGabrielsmansionScene01 = CreateNewPicScene("RosannaEventGabrielsmansionScene01", Core.scenePath + "Rosanna\\RosannaEventGabrielsmansionScene01.PNG");
                    rosannaVoyeurSecretbeachScene01 = CreateNewPicScene("RosannaVoyeurSecretbeachScene01", Core.scenePath + "Rosanna\\RosannaVoyeurSecretbeachScene01.PNG");
                    rosannaVoyeurSecretbeachScene02 = CreateNewPicScene("RosannaVoyeurSecretbeachScene02", Core.scenePath + "Rosanna\\RosannaVoyeurSecretbeachScene02.PNG");
                    rosannaVoyeurSecretbeachScene03 = CreateNewPicScene("RosannaVoyeurSecretbeachScene03", Core.scenePath + "Rosanna\\RosannaVoyeurSecretbeachScene03.PNG");
                    rosannaVoyeurSecretbeachScene04 = CreateNewPicScene("RosannaVoyeurSecretbeachScene04", Core.scenePath + "Rosanna\\RosannaVoyeurSecretbeachScene04.PNG");

                    sakuraEventForestScene01 = CreateNewPicScene("SakuraEventForestScene01", Core.scenePath + "Sakura\\SakuraEventForestScene01.PNG");
                    sakuraVoyeurSecretbeachScene01 = CreateNewPicScene("SakuraVoyeurSecretbeachScene01", Core.scenePath + "Sakura\\SakuraVoyeurSecretbeachScene01.PNG");
                    sakuraVoyeurSecretbeachScene02 = CreateNewPicScene("SakuraVoyeurSecretbeachScene02", Core.scenePath + "Sakura\\SakuraVoyeurSecretbeachScene02.PNG");
                    sakuraVoyeurSecretbeachScene03 = CreateNewPicScene("SakuraVoyeurSecretbeachScene03", Core.scenePath + "Sakura\\SakuraVoyeurSecretbeachScene03.PNG");
                    sakuraVoyeurSecretbeachScene04 = CreateNewPicScene("SakuraVoyeurSecretbeachScene04", Core.scenePath + "Sakura\\SakuraVoyeurSecretbeachScene04.PNG");

                    toveEventTrailScene01 = CreateNewPicScene("ToveEventTrailScene01", Core.scenePath + "Tove\\ToveEventTrailScene01.PNG");
                    toveVoyeurSecretbeachScene01 = CreateNewPicScene("ToveVoyeurSecretbeachScene01", Core.scenePath + "Tove\\ToveVoyeurSecretbeachScene01.PNG");
                    toveVoyeurSecretbeachScene02 = CreateNewPicScene("ToveVoyeurSecretbeachScene02", Core.scenePath + "Tove\\ToveVoyeurSecretbeachScene02.PNG");
                    toveVoyeurSecretbeachScene03 = CreateNewPicScene("ToveVoyeurSecretbeachScene03", Core.scenePath + "Tove\\ToveVoyeurSecretbeachScene03.PNG");
                    toveVoyeurSecretbeachScene04 = CreateNewPicScene("ToveVoyeurSecretbeachScene04", Core.scenePath + "Tove\\ToveVoyeurSecretbeachScene04.PNG");

                    viperEventVillaScene01 = CreateNewPicScene("ViperEventVillaScene01", Core.scenePath + "Viper\\ViperEventVillaScene01.PNG");
                    viperVoyeurSecretbeachScene01 = CreateNewPicScene("ViperVoyeurSecretbeachScene01", Core.scenePath + "Viper\\ViperVoyeurSecretbeachScene01.PNG");
                    viperVoyeurSecretbeachScene02 = CreateNewPicScene("ViperVoyeurSecretbeachScene02", Core.scenePath + "Viper\\ViperVoyeurSecretbeachScene02.PNG");
                    viperVoyeurSecretbeachScene03 = CreateNewPicScene("ViperVoyeurSecretbeachScene03", Core.scenePath + "Viper\\ViperVoyeurSecretbeachScene03.PNG");
                    viperVoyeurSecretbeachScene04 = CreateNewPicScene("ViperVoyeurSecretbeachScene04", Core.scenePath + "Viper\\ViperVoyeurSecretbeachScene04.PNG");

                    yanEventMallScene01 = CreateNewPicScene("YanEventMallScene01", Core.scenePath + "Yan\\YanEventMallScene01.PNG");
                    yanVoyeurSecretbeachScene01 = CreateNewPicScene("YanVoyeurSecretbeachScene01", Core.scenePath + "Yan\\YanVoyeurSecretbeachScene01.PNG");
                    yanVoyeurSecretbeachScene02 = CreateNewPicScene("YanVoyeurSecretbeachScene02", Core.scenePath + "Yan\\YanVoyeurSecretbeachScene02.PNG");
                    yanVoyeurSecretbeachScene03 = CreateNewPicScene("YanVoyeurSecretbeachScene03", Core.scenePath + "Yan\\YanVoyeurSecretbeachScene03.PNG");
                    yanVoyeurSecretbeachScene04 = CreateNewPicScene("YanVoyeurSecretbeachScene04", Core.scenePath + "Yan\\YanVoyeurSecretbeachScene04.PNG");

                    Core.cGManagerSexy.GetComponent<SpriteManager>().RefreshCache();
                    Logger.LogInfo("----- SCENES LOADED -----");
                    loadedScenes = true;
                }
                if (Core.currentScene.name == "GameStart")
                {
                    if (loadedScenes)
                    {
                        Logger.LogInfo("----- SCENES UNLOADED -----");
                        loadedScenes = false;
                    }
                }
            }
        }

        public GameObject CreateNewPicScene(string name, string pathToCG)
        {
            GameObject newPicScene = GameObject.Instantiate(Core.cGManagerSexy.Find("Samanthabeach").gameObject, Core.cGManagerSexy);
            newPicScene.name = name;
            GameObject core = newPicScene.transform.Find("Core").gameObject;
            GameObject art = core.transform.Find("Art").gameObject;

            var rawData = System.IO.File.ReadAllBytes(pathToCG);
            Texture2D tex = new Texture2D(256, 256);
            tex.filterMode = FilterMode.Point;
            ImageConversion.LoadImage(tex, rawData);
            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
            art.GetComponent<SpriteRenderer>().sprite = newSprite;

            Core.cGManagerSexy.GetComponent<SpriteManager>().targetObjects.Add(newPicScene);

            newPicScene.SetActive(false);
            return newPicScene;
        }

        public static void DialogueScenePlayer(Transform cGManager, string baseSceneName, GameObject baseDialogue)
        {
            if (!cGManager.Find(baseSceneName + "Scene01").gameObject.activeSelf && baseDialogue.transform.Find("Scene1").gameObject.activeSelf)
            {
                cGManager.Find(baseSceneName + "Scene01").gameObject.SetActive(true);
            }
            if (!cGManager.Find(baseSceneName + "Scene02").gameObject.activeSelf && baseDialogue.transform.Find("Scene2").gameObject.activeSelf)
            {
                cGManager.Find(baseSceneName + "Scene02").gameObject.SetActive(true);
            }
            if (!cGManager.Find(baseSceneName + "Scene03").gameObject.activeSelf && baseDialogue.transform.Find("Scene3").gameObject.activeSelf)
            {
                cGManager.Find(baseSceneName + "Scene03").gameObject.SetActive(true);
            }
            if (!cGManager.Find(baseSceneName + "Scene04").gameObject.activeSelf && baseDialogue.transform.Find("Scene4").gameObject.activeSelf)
            {
                if (baseDialogue.transform.Find("Scene1").gameObject.activeSelf)
                {
                    baseDialogue.transform.Find("Scene1").gameObject.SetActive(false);
                    cGManager.Find(baseSceneName + "Scene01").gameObject.SetActive(false);
                }
                cGManager.Find(baseSceneName + "Scene04").gameObject.SetActive(true);
            }
        }
    }
}
