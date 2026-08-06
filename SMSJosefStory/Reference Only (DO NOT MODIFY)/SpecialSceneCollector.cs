using System.Collections.Generic;
using UnityEngine;

namespace SMSGallery
{
    /// <summary>
    /// Collects special scene gallery items from EventUI.
    /// Special scenes behave like NPCs (no button, show in gallery) but display with regular scenes.
    /// </summary>
    public class SpecialSceneCollector
    {
        public class SpecialSceneData
        {
            public GameObject sceneGO;
            public Sprite sprite;
        }

        private SaveManager saveManager;

        public SpecialSceneCollector(SaveManager manager)
        {
            saveManager = manager;
        }

        public Dictionary<string, SpecialSceneData> CollectAll()
        {
            Dictionary<string, SpecialSceneData> allSpecialScenes = new Dictionary<string, SpecialSceneData>();

            // Find 9_MainCanvas for new special scenes
            GameObject mainCanvasGO = GameObject.Find("9_MainCanvas");
            if (mainCanvasGO != null)
            {
                CollectPayoutScenes(mainCanvasGO, allSpecialScenes);
                CollectDesktopScenes(mainCanvasGO, allSpecialScenes);
                CollectDesktopSpamAdsScenes(mainCanvasGO, allSpecialScenes);
                CollectStarmakerCreatorScenes(mainCanvasGO, allSpecialScenes);
                CollectWallpaperScenes(mainCanvasGO, allSpecialScenes);
            }
            else
            {
                Debug.Log("[Gallery] 9_MainCanvas not found for new special scenes");
            }

            // Find 5_Levels for Cinema movie frames (may be inactive)
            GameObject levelsGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "5_Levels" && go.scene.name != null);
            if (levelsGO != null)
            {
                CollectCinemaMovieFrames(levelsGO, allSpecialScenes);
            }
            else
            {
                Debug.Log("[Gallery] 5_Levels not found for Cinema movie frames");
            }

            // Find 15_EventUI via Resources.FindObjectsOfTypeAll (may be inactive)
            GameObject eventUIGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "15_EventUI" && go.scene.name != null);

            if (eventUIGO == null)
            {
                Debug.Log("[Gallery] 15_EventUI not found (may be inactive)");
                return allSpecialScenes;
            }

            Debug.Log("[Gallery] Found 15_EventUI GO for special scenes");

            // BPC_New > OtherPC_Messenger > OtherPC_Core > OtherPC_MSGList > Image (child 11 and child 5 with Image (5))
            Transform bpcNew = eventUIGO.transform.Find("BPC_New");
            if (bpcNew != null)
            {
                Transform otherPCMessenger = bpcNew.Find("OtherPC_Messenger");
                if (otherPCMessenger != null)
                {
                    Transform otherPCCore = otherPCMessenger.Find("OtherPC_Core");
                    if (otherPCCore != null)
                    {
                        Transform otherPCMsgList = otherPCCore.Find("OtherPC_MSGList");
                        if (otherPCMsgList != null)
                        {
                            // Special Scene 1: Child 11 > OtherPC_Pic
                            if (otherPCMsgList.childCount > 11)
                            {
                                Transform child11 = otherPCMsgList.GetChild(11);
                                if (child11 != null)
                                {
                                    Transform otherPCPic = child11.Find("OtherPC_Pic");
                                    if (otherPCPic != null)
                                    {
                                        AddSpecialScene(otherPCPic, "15_EventUI_BPC_Messenger_MSGList_Child11", allSpecialScenes);
                                    }
                                }
                            }

                            // Special Scene 2: Image (5) > OtherPC_Pic
                            Transform image5 = otherPCMsgList.Find("Image (5)");
                            if (image5 != null)
                            {
                                Transform otherPCPic = image5.Find("OtherPC_Pic");
                                if (otherPCPic != null)
                                {
                                    AddSpecialScene(otherPCPic, "15_EventUI_BPC_Messenger_MSGList_Image5", allSpecialScenes);
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("[Gallery] OtherPC_MSGList not found in BPC");
                        }
                    }
                    else
                    {
                        Debug.Log("[Gallery] OtherPC_Core not found in BPC_Messenger");
                    }
                }
                else
                {
                    Debug.Log("[Gallery] OtherPC_Messenger not found in BPC_New");
                }
            }
            else
            {
                Debug.Log("[Gallery] BPC_New not found for special scenes");
            }

            // DPC_New > MImages > List > Image variants
            Transform dpcNew = eventUIGO.transform.Find("DPC_New");
            if (dpcNew != null)
            {
                Transform mImages = dpcNew.Find("MImages");
                if (mImages != null)
                {
                    Transform list = mImages.Find("List");
                    if (list != null)
                    {
                        // Special Scenes: Image, Image (3)
                        string[] imageNames = new string[] { "Image", "Image (3)" };
                        foreach (string imageName in imageNames)
                        {
                            Transform imageTransform = list.Find(imageName);
                            if (imageTransform != null)
                            {
                                AddSpecialScene(imageTransform, $"15_EventUI_DPC_MImages_{imageName}", allSpecialScenes);
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("[Gallery] List not found in DPC MImages");
                    }
                }
                else
                {
                    Debug.Log("[Gallery] MImages not found in DPC_New");
                }

                // DPC_New > MImages (1) > List > Image variants
                Transform mImages1 = dpcNew.Find("MImages (1)");
                if (mImages1 != null)
                {
                    Transform list1 = mImages1.Find("List");
                    if (list1 != null)
                    {
                        // Special Scenes: Image, Image (1), Image (2), Image (3), Image (4), Image (5)
                        string[] imageNames = new string[] { "Image", "Image (1)", "Image (2)", "Image (3)", "Image (4)", "Image (5)" };
                        foreach (string imageName in imageNames)
                        {
                            Transform imageTransform = list1.Find(imageName);
                            if (imageTransform != null)
                            {
                                AddSpecialScene(imageTransform, $"15_EventUI_DPC_MImages1_{imageName}", allSpecialScenes);
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("[Gallery] List not found in DPC MImages (1)");
                    }
                }
                else
                {
                    Debug.Log("[Gallery] MImages (1) not found in DPC_New");
                }

                // DPC_New > OtherPC_Messenger > OtherPC_Core > OtherPC_MSGList > Image > OtherPC_Pic
                Transform otherPCMessengerDPC = dpcNew.Find("OtherPC_Messenger");
                if (otherPCMessengerDPC != null)
                {
                    Transform otherPCCoreDPC = otherPCMessengerDPC.Find("OtherPC_Core");
                    if (otherPCCoreDPC != null)
                    {
                        Transform otherPCMsgListDPC = otherPCCoreDPC.Find("OtherPC_MSGList");
                        if (otherPCMsgListDPC != null)
                        {
                            // Special Scene: Image > OtherPC_Pic
                            Transform imageDPC = otherPCMsgListDPC.Find("Image");
                            if (imageDPC != null)
                            {
                                Transform otherPCPicDPC = imageDPC.Find("OtherPC_Pic");
                                if (otherPCPicDPC != null)
                                {
                                    AddSpecialScene(otherPCPicDPC, "15_EventUI_DPC_Messenger_Image", allSpecialScenes);
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("[Gallery] OtherPC_MSGList not found in DPC_Messenger");
                        }
                    }
                    else
                    {
                        Debug.Log("[Gallery] OtherPC_Core not found in DPC_Messenger");
                    }
                }
                else
                {
                    Debug.Log("[Gallery] OtherPC_Messenger not found in DPC_New");
                }
            }
            else
            {
                Debug.Log("[Gallery] DPC_New not found for special scenes");
            }

            // Collect direct children of 15_EventUI (like PrismeMagazine1)
            CollectDirectEventUIScenes(eventUIGO, allSpecialScenes);

            return allSpecialScenes;
        }

        private void AddSpecialScene(Transform specialSceneTransform, string specialSceneId, Dictionary<string, SpecialSceneData> allSpecialScenes)
        {
            UnityEngine.UI.Image imageComponent = specialSceneTransform.GetComponent<UnityEngine.UI.Image>();
            if (imageComponent == null)
            {
                Debug.LogWarning($"[Gallery] Special Scene '{specialSceneId}' has no Image component");
                return;
            }

            if (imageComponent.sprite == null)
            {
                Debug.LogWarning($"[Gallery] Special Scene '{specialSceneId}' Image has no sprite");
                return;
            }

            if (!allSpecialScenes.ContainsKey(specialSceneId))
            {
                SpecialSceneData data = new SpecialSceneData
                {
                    sceneGO = specialSceneTransform.gameObject,
                    sprite = imageComponent.sprite
                };

                allSpecialScenes[specialSceneId] = data;
                //Debug.Log($"[Gallery] Added special scene: {specialSceneId}");
            }
        }

        private void AddSpecialSceneFromSpriteRenderer(Transform specialSceneTransform, string specialSceneId, Dictionary<string, SpecialSceneData> allSpecialScenes)
        {
            SpriteRenderer spriteRenderer = specialSceneTransform.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"[Gallery] Special Scene '{specialSceneId}' has no SpriteRenderer component");
                return;
            }

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"[Gallery] Special Scene '{specialSceneId}' SpriteRenderer has no sprite");
                return;
            }

            if (!allSpecialScenes.ContainsKey(specialSceneId))
            {
                SpecialSceneData data = new SpecialSceneData
                {
                    sceneGO = specialSceneTransform.gameObject,
                    sprite = spriteRenderer.sprite
                };

                allSpecialScenes[specialSceneId] = data;
                //Debug.Log($"[Gallery] Added special scene from SpriteRenderer: {specialSceneId}");
            }
        }

        private void CollectDirectEventUIScenes(GameObject eventUIGO, Dictionary<string, SpecialSceneData> allSpecialScenes)
        {
            // Collect direct children of 15_EventUI that have Image components
            // Example: PrismeMagazine1
            Transform prismeMagazine1 = eventUIGO.transform.Find("PrismeMagazine1");
            if (prismeMagazine1 != null)
            {
                AddSpecialScene(prismeMagazine1, "15_EventUI_PrismeMagazine1", allSpecialScenes);
            }
        }

        private void CollectPayoutScenes(GameObject mainCanvas, Dictionary<string, SpecialSceneData> allSpecialScenes)
        {
            // 9_MainCanvas > Payout > CelesteStoleFollowers > CelesteStoleFollowers (1)
            Transform payout = mainCanvas.transform.Find("Payout");
            if (payout != null)
            {
                Transform celesteStoleFollowers = payout.Find("CelesteStoleFollowers");
                if (celesteStoleFollowers != null)
                {
                    Transform celesteStoleFollowers1 = celesteStoleFollowers.Find("CelesteStoleFollowers (1)");
                    if (celesteStoleFollowers1 != null)
                    {
                        AddSpecialScene(celesteStoleFollowers1, "9_MainCanvas_Payout_CelesteStoleFollowers1", allSpecialScenes);
                    }
                }
            }
        }

        private void CollectDesktopScenes(GameObject mainCanvas, Dictionary<string, SpecialSceneData> allSpecialScenes)
        {
            // 9_MainCanvas > Desktop > Cats > Image
            Transform desktop = mainCanvas.transform.Find("Desktop");
            if (desktop != null)
            {
                Transform cats = desktop.Find("Cats");
                if (cats != null)
                {
                    Transform image = cats.Find("Image");
                    if (image != null)
                    {
                        AddSpecialScene(image, "9_MainCanvas_Desktop_Cats_Image", allSpecialScenes);
                    }
                }
            }
        }

        private void CollectDesktopSpamAdsScenes(GameObject mainCanvas, Dictionary<string, SpecialSceneData> allSpecialScenes)
        {
            // 9_MainCanvas > Desktop > Desktop_SpamAds > DesAds_List > DesAd (0-24) > DesWin > Image(s)
            Transform desktop = mainCanvas.transform.Find("Desktop");
            if (desktop != null)
            {
                Transform desktopSpamAds = desktop.Find("Desktop_SpamAds");
                if (desktopSpamAds != null)
                {
                    Transform desAdsList = desktopSpamAds.Find("DesAds_List");
                    if (desAdsList != null)
                    {
                        // Collect DesAd (0) > DesWin > Image (1)
                        Transform desAd0 = desAdsList.Find("DesAd (0)");
                        if (desAd0 != null)
                        {
                            Transform desWin0 = desAd0.Find("DesWin");
                            if (desWin0 != null)
                            {
                                Transform image1 = desWin0.Find("Image (1)");
                                if (image1 != null)
                                {
                                    AddSpecialScene(image1, "9_MainCanvas_Desktop_SpamAds_DesAd0_Image1", allSpecialScenes);
                                }
                            }
                        }

                        // Collect DesAd (1-24) > DesWin > img
                        for (int i = 1; i <= 24; i++)
                        {
                            Transform desAd = desAdsList.Find($"DesAd ({i})");
                            if (desAd != null)
                            {
                                Transform desWin = desAd.Find("DesWin");
                                if (desWin != null)
                                {
                                    Transform img = desWin.Find("img");
                                    if (img != null)
                                    {
                                        AddSpecialScene(img, $"9_MainCanvas_Desktop_SpamAds_DesAd{i}_img", allSpecialScenes);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CollectWallpaperScenes(GameObject mainCanvas, Dictionary<string, SpecialSceneData> allSpecialScenes)
        {
            // 9_MainCanvas > Wallpaperselection > UI_Core > List > WallpaperButton (0-13)
            Transform wallpaperselection = mainCanvas.transform.Find("Wallpaperselection");
            if (wallpaperselection != null)
            {
                Transform uiCore = wallpaperselection.Find("UI_Core");
                if (uiCore != null)
                {
                    Transform list = uiCore.Find("List");
                    if (list != null)
                    {
                        // Collect all direct children of List
                        for (int i = 0; i < list.childCount; i++)
                        {
                            Transform wallpaperButton = list.GetChild(i);
                            if (wallpaperButton != null)
                            {
                                string buttonName = wallpaperButton.gameObject.name.Replace(" ", "_").Replace("(", "").Replace(")", "");
                                string sceneId = $"9_MainCanvas_Wallpaper_{buttonName}";
                                AddSpecialScene(wallpaperButton, sceneId, allSpecialScenes);
                            }
                        }
                    }
                }
            }
        }

        private void CollectStarmakerCreatorScenes(GameObject mainCanvas, Dictionary<string, SpecialSceneData> allSpecialScenes)
        {
            // 9_MainCanvas > Starmaker > Browse_Creators > Creator_Windows > CW_[Creator] > Unlocked > Creator_Pictures > Scrollarea > Creator_PicsContent > Image(s)
            Transform starmaker = mainCanvas.transform.Find("Starmaker");
            if (starmaker != null)
            {
                Transform browseCreators = starmaker.Find("Browse_Creators");
                if (browseCreators != null)
                {
                    Transform creatorWindows = browseCreators.Find("Creator_Windows");
                    if (creatorWindows != null)
                    {
                        // Celeste creator (5 images)
                        CollectCreatorPictures(creatorWindows, "CW_Celeste", "9_MainCanvas_Starmaker_Celeste", new string[] { "Image", "Image (1)", "Image (2)", "Image (3)", "Image (4)" }, allSpecialScenes);

                        // Ms Umbra creator (4 images)
                        CollectCreatorPictures(creatorWindows, "CW_MsUmbra", "9_MainCanvas_Starmaker_MsUmbra", new string[] { "Image", "Image (1)", "Image (2)", "Image (3)" }, allSpecialScenes);

                        // Dreamy Wisp creator (9 images)
                        CollectCreatorPictures(creatorWindows, "CW_DreamyWisp", "9_MainCanvas_Starmaker_DreamyWisp", new string[] { "Image", "Image (1)", "Image (2)", "Image (3)", "Image (4)", "Image (5)", "Image (6)", "Image (7)", "Image (8)" }, allSpecialScenes);

                        // Rose creator (5 images - specific indices)
                        CollectCreatorPicturesSpecific(creatorWindows, "CW_Rose", "9_MainCanvas_Starmaker_Rose", new string[] { "Image (2)", "Image (5)", "Image (6)", "Image (7)", "Image (8)" }, allSpecialScenes);
                    }
                }
            }
        }

        private void CollectCreatorPictures(Transform creatorWindows, string creatorName, string idPrefix, string[] imageNames, Dictionary<string, SpecialSceneData> allSpecialScenes)
        {
            Transform creatorWindow = creatorWindows.Find(creatorName);
            if (creatorWindow != null)
            {
                Transform unlocked = creatorWindow.Find("Unlocked");
                if (unlocked != null)
                {
                    Transform creatorPictures = unlocked.Find("Creator_Pictures");
                    if (creatorPictures != null)
                    {
                        Transform scrollArea = creatorPictures.Find("Scrollarea");
                        if (scrollArea != null)
                        {
                            Transform picContent = scrollArea.Find("Creator_PicsContent");
                            if (picContent != null)
                            {
                                foreach (string imageName in imageNames)
                                {
                                    Transform image = picContent.Find(imageName);
                                    if (image != null)
                                    {
                                        AddSpecialScene(image, $"{idPrefix}_{imageName.Replace(" ", "_").Replace("(", "").Replace(")", "")}", allSpecialScenes);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CollectCreatorPicturesSpecific(Transform creatorWindows, string creatorName, string idPrefix, string[] imageNames, Dictionary<string, SpecialSceneData> allSpecialScenes)
        {
            Transform creatorWindow = creatorWindows.Find(creatorName);
            if (creatorWindow != null)
            {
                Transform unlocked = creatorWindow.Find("Unlocked");
                if (unlocked != null)
                {
                    Transform creatorPictures = unlocked.Find("Creator_Pictures");
                    if (creatorPictures != null)
                    {
                        Transform scrollArea = creatorPictures.Find("Scrollarea");
                        if (scrollArea != null)
                        {
                            Transform picContent = scrollArea.Find("Creator_PicsContent");
                            if (picContent != null)
                            {
                                foreach (string imageName in imageNames)
                                {
                                    Transform image = picContent.Find(imageName);
                                    if (image != null)
                                    {
                                        AddSpecialScene(image, $"{idPrefix}_{imageName.Replace(" ", "_").Replace("(", "").Replace(")", "")}", allSpecialScenes);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CollectCinemaMovieFrames(GameObject levelsGO, Dictionary<string, SpecialSceneData> allSpecialScenes)
        {
            // 5_Levels > 122_Cinema > Dark_Cinema > Cinema_MovieFrames > CinemaMovie_1 (0-16)
            Transform cinema = levelsGO.transform.Find("122_Cinema");
            if (cinema != null)
            {
                Transform darkCinema = cinema.Find("Dark_Cinema");
                if (darkCinema != null)
                {
                    Transform cinemaMovieFrames = darkCinema.Find("Cinema_MovieFrames");
                    if (cinemaMovieFrames != null)
                    {
                        // Collect all children of Cinema_MovieFrames
                        for (int i = 0; i < cinemaMovieFrames.childCount; i++)
                        {
                            Transform movieFrame = cinemaMovieFrames.GetChild(i);
                            if (movieFrame != null)
                            {
                                string sceneId = $"5_Levels_Cinema_MovieFrame_{movieFrame.name.Replace(" ", "_").Replace("(", "").Replace(")", "")}";
                                AddSpecialSceneFromSpriteRenderer(movieFrame, sceneId, allSpecialScenes);
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("[Gallery] Cinema_MovieFrames not found in Dark_Cinema");
                    }
                }
                else
                {
                    Debug.Log("[Gallery] Dark_Cinema not found in 122_Cinema");
                }
            }
            else
            {
                Debug.Log("[Gallery] 122_Cinema not found in 5_Levels");
            }
        }
    }
}
