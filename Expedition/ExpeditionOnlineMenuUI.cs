using Expedition;
using Menu;
using MoreSlugcats;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

using static Menu.SlugcatSelectMenu;

namespace RainMeadow
{
    public partial class ExpeditionOnlineMenu : ExpeditionMenu
    {
        private SlugcatStats.Name[] selectableSlugcats;
        public SlugcatStats.Name?[] playerSelectedSlugcats;
        public SlugcatStats.Name[] SelectableSlugcats
        {
            get
            {
                SetupSelectableSlugcats();
                return selectableSlugcats;
            }
        }
        public SlugcatStats.Name PlayerSelectedSlugcat
        {
            get
            {
                return playerSelectedSlugcats?[0] ?? ExpeditionGame.playableCharacters[currentSelection];//slugcatColorOrder[slugcatPageIndex];
            }
            set
            {
                SetSelectedSlugcat(0, value);
            }
        }
        //public List<SlugcatStats.Name> playableCharacters

        SimplerSymbolButton toggleChat;
        //
        private ButtonScroller? playerScrollBox;
        private Vector2 playerScrollBoxPos;
        public static int MaxVisibleOnList => 8 - 3;
        public static float ButtonSpacingOffset => 8;
        public static float ButtonSizeWithSpacing => ButtonSize + ButtonSpacingOffset;
        public static float ButtonSize => 30;
        int _currentPage = 1;
        //

        private StoryMenuSlugcatSelector? slugcatSelector;

        private Vector2 lobbylabelPos;
        private ChatTextBox chatTextBox;
        private Vector2 chatTextBoxPos;

        CheckBox customColorsCheckbox;

        bool _pagesMoving;

        private Vector2[] bodyButtonsPos;
        private Vector2[] bodyButtonsLastPos;
        private Vector2[] bodyColorBordersPos;
        private Vector2[] bodyColorBordersLastPos;
        private Vector2[] bodyColorsPos;
        private Vector2[] bodyColorsLastPos;

        void SetupOnlineMenuItens()
        {
            lobbylabelPos = new Vector2(manager.rainWorld.screenSize.x - 170, 553);
            lobbyLabel = new MenuLabel(this, pages[_currentPage], Translate("LOBBY"), lobbylabelPos, new(110, 30), true);
            pages[_currentPage].subObjects.Add(lobbyLabel);

            this.chatTextBoxPos = new Vector2(this.manager.rainWorld.options.ScreenSize.x * 0.001f + (1366f - this.manager.rainWorld.options.ScreenSize.x) / 2f, 0);
            toggleChat = new SimplerSymbolButton(this, pages[_currentPage], "Kill_Slugcat", "", this.chatTextBoxPos);
            toggleChat.OnClick += (_) =>
            {
                ToggleChat(!this.isChatToggled);
                if (input.controllerType == Options.ControlSetup.Preset.KeyboardSinglePlayer)
                {
                    selectedObject = null;
                }
            };
            pages[_currentPage].subObjects.Add(toggleChat);

            SetupColorMenu();
        }

        // update()
        //  - float num5 = ((this.manager.rainWorld.options.ScreenSize.x != 1024f) ? 695f : 728f);
        //  - this.manualButton.pos = new global::UnityEngine.Vector2(this.rightAnchor - (this.leftAnchor + 150f), num5) - this.manualButton.page.pos;
        //  - this.manualButton.lastPos = new global::UnityEngine.Vector2(this.rightAnchor - (this.leftAnchor + 150f), num5) - this.manualButton.page.lastPos;

        // updatePage()
        //  - this.manualButton.RemoveSprites();
        //  - this.manualButton.RemoveSubObject(this.manualButton);
        //  - this.manualButton = new global::Menu.SimpleButton(this, this.pages[this.currentPage], base.Translate("MANUAL"), "MANUAL", new global::UnityEngine.Vector2(this.rightAnchor - 150f, 695f), new global::UnityEngine.Vector2(100f, 30f));

        private void UpdatePlayerList()
        {
            playerScrollBox?.RemoveAllButtons(false);
            if (playerScrollBox == null)
            {
                playerScrollBoxPos = new(manager.rainWorld.screenSize.x - 170, 553 - 30 - ButtonScroller.CalculateHeightBasedOnAmtOfButtons(MaxVisibleOnList, ButtonSize, ButtonSpacingOffset));
                playerScrollBox = new(this, pages[_currentPage], playerScrollBoxPos, MaxVisibleOnList, 200, new(ButtonSize, ButtonSpacingOffset));
                pages[_currentPage].subObjects.Add(playerScrollBox);
            }
            foreach (OnlinePlayer player in OnlineManager.players)
            {
                StoryMenuPlayerButton playerButton = new(this, playerScrollBox, player, OnlineManager.lobby.isOwner && player != OnlineManager.lobby.owner);
                playerScrollBox.AddScrollObjects(playerButton);
            }
            playerScrollBox.ConstrainScroll();

        }

        private void OnlineManager_OnPlayerListReceived(PlayerInfo[] players)
        {
            if (RainMeadow.isExpeditionMode(out var _))
            {
                UpdatePlayerList();
            }
        }

        private void UpdateUI()
        {
            if (this.pagesMoving || _pagesMoving)
            {
                int usePagePos = (pagesMoving || !_pagesMoving) ? 1 : 0;

                float offset = 2.9f;

                Vector2 pagePos = new Vector2(pages[_currentPage].pos.x + offset, 0) * usePagePos;
                Vector2 pageLastPos = new Vector2(pages[_currentPage].lastPos.x + offset, 0) * usePagePos;

                lobbyLabel.pos = lobbylabelPos - pagePos;
                lobbyLabel.lastPos = lobbylabelPos - pageLastPos;

                chatTextBox?.pos = chatTextBoxPos + new Vector2(24, 0) - pagePos;
                chatTextBox?.lastPos = chatTextBoxPos + new Vector2(24, 0) - pageLastPos;

                chatTextBox?.roundedRect.pos = new Vector2(24 + 4.4f, 0) - pagePos;
                chatTextBox?.roundedRect.lastPos = new Vector2(24 + 4.4f, 0) - pageLastPos;

                toggleChat.pos = /*chatTextBoxPos +*/ new Vector2(4.4f, 0) - pagePos;
                toggleChat.lastPos = /*chatTextBoxPos +*/ new Vector2(4.4f, 0) - pageLastPos;

                playerScrollBox.pos = playerScrollBoxPos - pagePos;
                playerScrollBox.lastPos = playerScrollBoxPos - pageLastPos;

                Vector2 slugcatLabelpos = new(70, 553);
                Vector2 slugcatSelectorpos = new(slugcatLabelpos.x, slugcatLabelpos.y - (ButtonSize * 2));

                slugcatLabel.pos = slugcatLabelpos - pagePos;
                slugcatLabel.lastPos = slugcatLabelpos - pageLastPos;
                slugcatSelector.pos = slugcatSelectorpos - pagePos;
                slugcatSelector.lastPos = slugcatSelectorpos - pageLastPos;


                float restartTextWidth = GetRestartTextWidth(base.CurrLang);
                //float restartTextOffset = GetRestartTextOffset(base.CurrLang);

                Vector2 pos = new(70 + restartTextWidth, 553 + 60);

                customColorsCheckbox.pos = pos - pagePos;
                customColorsCheckbox.lastPos = pos - pageLastPos;

                // why...
                if (colorInterface != null)
                {
                    for (int i = 0; i < colorInterface.bodyColors.Length; i++)
                    {
                        colorInterface.bodyButtons[i].pos = bodyButtonsPos[i] - pagePos;
                        colorInterface.bodyButtons[i].lastPos = bodyButtonsLastPos[i] - pageLastPos;
                        colorInterface.bodyColorBorders[i].pos = bodyColorBordersPos[i] - pagePos;
                        colorInterface.bodyColorBorders[i].lastPos = bodyColorBordersLastPos[i] - pageLastPos;
                        colorInterface.bodyColors[i].pos = bodyColorsPos[i] - pagePos;
                        colorInterface.bodyColors[i].lastPos = bodyColorsLastPos[i] - pageLastPos;
                    }
                }
                _pagesMoving = pagesMoving;
            }
        }
        void UpdateOnlinePage(int pageIndex)
        {
            _currentPage = pageIndex;

            pages[currentPage].ClearMenuObject(ref lobbyLabel);

            pages[currentPage].ClearMenuObject(ref toggleChat);

            playerScrollBox?.RemoveAllButtons(false);
            pages[currentPage].ClearMenuObject(ref playerScrollBox);

            this.chatTextBox?.DelayedUnload(0.1f);
            pages[currentPage].ClearMenuObject(ref chatTextBox);

            pages[currentPage].ClearMenuObject(ref customColorsCheckbox);

            RemoveColorInterface();
            RemoveSlugcatList();


            RemoveColorButtons();

            if (colorChecked)
                AddColorButtons();

            SetupSlugcatList();

            SetupOnlineMenuItens();
            UpdatePlayerList();
            ResetChatInput();

            //AddColorInterface();
        }

        // custom scugs

        private void SetupSlugcatList()
        {
            Vector2 pos = new(70, 553);
            if (slugcatLabel == null)
            {
                slugcatLabel = new(this, pages[_currentPage], Translate("Selected Slugcat").Replace("<LINE>", "\n"), pos, new(110, 30), true);
                pages[_currentPage].subObjects.Add(slugcatLabel);
            }
            if (slugcatSelector == null)
            {
                //first player button is 30 pos below size of list. and list top part is 30 below the title. Plus
                slugcatSelector = new(this, pages[_currentPage], new(pos.x, pos.y - (ButtonSize * 2)), MaxVisibleOnList, ButtonSpacingOffset, PlayerSelectedSlugcat, GetSlugcatSelectionButtons);
                pages[_currentPage].subObjects.Add(slugcatSelector);
            }
            if (expeditionGameMode.preferredSlug != null)
            {
                SetSelectedSlugcat(0, expeditionGameMode.preferredSlug);
            }
        }

        private void RemoveSlugcatList()
        {
            pages[currentPage].ClearMenuObject(ref slugcatLabel);
            pages[currentPage].ClearMenuObject(ref slugcatSelector);
        }

        public void SetupSelectableSlugcats()
        {
            if (selectableSlugcats == null)
            {
                var SelectableSlugcatsEnumerable = ExpeditionGame.playableCharacters.AsEnumerable();//slugcatColorOrder.AsEnumerable();
                if (ModManager.MSC)
                {
                    if (!SelectableSlugcatsEnumerable.Contains(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel))
                    {
                        SelectableSlugcatsEnumerable = SelectableSlugcatsEnumerable.Append(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel);
                    }
                    //if (!SelectableSlugcatsEnumerable.Contains(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Slugpup))
                    //{
                    //    SelectableSlugcatsEnumerable = SelectableSlugcatsEnumerable.Append(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Slugpup);
                    //}
                }
                if (ModManager.Watcher)
                {
                    if (!SelectableSlugcatsEnumerable.Contains(Watcher.WatcherEnums.SlugcatStatsName.Watcher))
                    {
                        SelectableSlugcatsEnumerable = SelectableSlugcatsEnumerable.Append(Watcher.WatcherEnums.SlugcatStatsName.Watcher);
                    }
                }
                selectableSlugcats = SelectableSlugcatsEnumerable.ToArray();
            }
        }

        public void SetSelectedSlugcat(int player, SlugcatStats.Name slugcat)
        {
            if ((playerSelectedSlugcats[player] != slugcat && playerSelectedSlugcats[player] != null) || (playerSelectedSlugcats[player] == null && ExpeditionGame.playableCharacters[currentSelection] != slugcat))//slugcatColorOrder[slugcatPageIndex] != slugcat))
            {
                if (ModManager.JollyCoop)
                {
                    manager.rainWorld.options.jollyPlayerOptionsArray[player].playerClass = slugcat;
                }
                playerSelectedSlugcats[player] = slugcat == ExpeditionGame.playableCharacters[currentSelection] ? null : slugcat; // slugcatColorOrder[slugcatPageIndex] ? null : slugcat;
                expeditionGameMode.preferredSlug = slugcat;

                if (player == 0)
                {
                    if (colorInterface is not null)
                    {
                        RemoveColorButtons();
                        AddColorButtons();
                    }
                }
            }
        }

        public StoryMenuSlugcatButton[] GetSlugcatSelectionButtons(StoryMenuSlugcatSelector slugcatSelector, ButtonScroller buttonScroller)
        {
            List<StoryMenuSlugcatButton> slugcatButtons = [];
            for (int i = 0; i < SelectableSlugcats.Length; i++)
            {
                if (SelectableSlugcats[i] != slugcatSelector.Slug)
                {
                    StoryMenuSlugcatButton storyMenuSlugcatButton = new(this, buttonScroller, SelectableSlugcats[i], (scug) =>
                    {
                        PlayerSelectedSlugcat = scug;
                        slugcatSelector.OpenCloseList(false, true, true);
                    });
                    slugcatButtons.Add(storyMenuSlugcatButton);
                }
            }
            return [.. slugcatButtons];
        }

        public CustomColorInterface GetColorInterfaceForSlugcat(SlugcatStats.Name slugcatID, Vector2 pos)
        {
            List<string> names = PlayerGraphics.ColoredBodyPartList(slugcatID);
            List<string> list = PlayerGraphics.DefaultBodyPartColorHex(slugcatID);
            for (int i = 0; i < list.Count; i++)
            {
                Vector3 vector = RWCustom.Custom.RGB2HSL(RWCustom.Custom.hexToColor(list[i]));
                list[i] = vector[0].ToString(CultureInfo.InvariantCulture) + "," + vector[1].ToString(CultureInfo.InvariantCulture) + "," + vector[2].ToString(CultureInfo.InvariantCulture);
            }

            //RainMeadow.Debug($"this: {this}, pages[{_currentPage}]: {pages[_currentPage]}, pos: {pos}, slugcatID: {slugcatID}, names: {names}, list: {list}");
            return new CustomColorInterface(this, pages[_currentPage], pos, slugcatID, names, list);
        }

    }
    public partial class ExpeditionOnlineMenu : CheckBox.IOwnCheckBox
    {
        public bool colorChecked;
        public bool restartChecked;
        public CustomColorInterface colorInterface;

        public HorizontalSlider hueSlider;
        public HorizontalSlider satSlider;
        public HorizontalSlider litSlider;

        public SimpleButton defaultColorButton;

        public int activeColorChooser;

        public void SetupColorMenu()
        {
            if (ModManager.MMF)
            {
                float restartTextWidth = GetRestartTextWidth(base.CurrLang);
                float restartTextOffset = GetRestartTextOffset(base.CurrLang);

                Vector2 pos = new(70, 553);

                customColorsCheckbox = new CheckBox(this, pages[_currentPage], this, pos + new Vector2(restartTextWidth, 60), restartTextWidth, Translate("Custom colors"), "COLORS");
                customColorsCheckbox.label.pos.x += restartTextWidth - customColorsCheckbox.label.label.textRect.width - 5f;
                customColorsCheckbox.selectable = true;
                pages[_currentPage].subObjects.Add(customColorsCheckbox);
            }
        }

        public void AddColorButtons()
        {
            if (colorInterface == null)
            {
                float w = ButtonScroller.CalculateHeightBasedOnAmtOfButtons(MaxVisibleOnList + 2, ButtonSize, ButtonSpacingOffset);
                Vector2 vector = new(70, 553 - w - 15);
                colorInterface = GetColorInterfaceForSlugcat(pos: vector, slugcatID: PlayerSelectedSlugcat);
                pages[_currentPage].subObjects.Add(colorInterface);

                // why...
                if (bodyButtonsPos == null)
                {
                    int len = colorInterface.bodyColors.Length;
                    bodyButtonsPos = new Vector2[len];
                    bodyButtonsLastPos = new Vector2[len];
                    bodyColorBordersPos = new Vector2[len];
                    bodyColorBordersLastPos = new Vector2[len];
                    bodyColorsPos = new Vector2[len];
                    bodyColorsLastPos = new Vector2[len];

                    for (int i = 0; i < colorInterface.bodyColors.Length; i++)
                    {
                        bodyButtonsPos[i] = colorInterface.bodyButtons[i].pos;
                        bodyButtonsLastPos[i] = colorInterface.bodyButtons[i].lastPos;
                        bodyColorBordersPos[i] = colorInterface.bodyColorBorders[i].pos;
                        bodyColorBordersLastPos[i] = colorInterface.bodyColorBorders[i].lastPos;
                        bodyColorsPos[i] = colorInterface.bodyColors[i].pos;
                        bodyColorsLastPos[i] = colorInterface.bodyColors[i].lastPos;
                    }

                }
            }
        }

        public void RemoveColorButtons()
        {
            if (colorInterface != null)
            {
                colorInterface.RemoveSprites();
                pages[currentPage].RemoveSubObject(colorInterface);
                colorInterface = null;

                bodyButtonsPos = null;
                bodyButtonsLastPos = null;
                bodyColorBordersPos = null;
                bodyColorBordersLastPos = null;
                bodyColorsPos = null;
                bodyColorsLastPos = null;
            }

            RemoveColorInterface();
        }

        public void RemoveColorInterface()
        {
            if (hueSlider != null)
            {
                pages[currentPage].RemoveSubObject(hueSlider);
                hueSlider.RemoveSprites();
                hueSlider = null;
            }

            if (satSlider != null)
            {
                pages[currentPage].RemoveSubObject(satSlider);
                satSlider.RemoveSprites();
                satSlider = null;
            }

            if (litSlider != null)
            {
                pages[currentPage].RemoveSubObject(litSlider);
                litSlider.RemoveSprites();
                litSlider = null;
            }

            if (defaultColorButton != null)
            {
                pages[currentPage].RemoveSubObject(defaultColorButton);
                defaultColorButton.RemoveSprites();
                defaultColorButton = null;
            }

            activeColorChooser = -1;
        }

        public void AddColorInterface()
        {
            float w = ButtonScroller.CalculateHeightBasedOnAmtOfButtons(MaxVisibleOnList + 3, ButtonSize, ButtonSpacingOffset);
            Vector2 vector = new(70, 553 - w);
            if (ModManager.JollyCoop)
            {
                vector[1] -= 40f;
            }

            if (colorInterface != null)
            {
                vector[1] -= (float)colorInterface.bodyColors.Length * 40f;
            }

            if (hueSlider == null)
            {
                hueSlider = new HorizontalSlider(this, pages[_currentPage], Translate("HUE"), vector, new Vector2(200f, 30f), MMFEnums.SliderID.Hue, subtleSlider: false);
                pages[_currentPage].subObjects.Add(hueSlider);
            }

            if (satSlider == null)
            {
                satSlider = new HorizontalSlider(this, pages[_currentPage], Translate("SAT"), vector + new Vector2(0f, -40f), new Vector2(200f, 30f), MMFEnums.SliderID.Saturation, subtleSlider: false);
                pages[_currentPage].subObjects.Add(satSlider);
            }

            if (litSlider == null)
            {
                litSlider = new HorizontalSlider(this, pages[_currentPage], Translate("LIT"), vector + new Vector2(0f, -80f), new Vector2(200f, 30f), MMFEnums.SliderID.Lightness, subtleSlider: false);
                pages[_currentPage].subObjects.Add(litSlider);
            }

            float x = 110f;
            if (base.CurrLang == InGameTranslator.LanguageID.Japanese || base.CurrLang == InGameTranslator.LanguageID.French)
            {
                x = 140f;
            }
            else if (base.CurrLang == InGameTranslator.LanguageID.Italian || base.CurrLang == InGameTranslator.LanguageID.Spanish)
            {
                x = 180f;
            }

            if (defaultColorButton == null)
            {
                defaultColorButton = new SimpleButton(this, pages[_currentPage], Translate("Restore Default"), "DEFAULTCOL", vector + new Vector2(0f, -120f), new Vector2(x, 30f));
                pages[_currentPage].subObjects.Add(defaultColorButton);
            }

            MutualVerticalButtonBind(hueSlider, colorInterface.bodyButtons[colorInterface.bodyButtons.Length - 1]);
            MutualVerticalButtonBind(satSlider, hueSlider);
            MutualVerticalButtonBind(litSlider, satSlider);
            MutualVerticalButtonBind(defaultColorButton, litSlider);
        }

        public bool GetChecked(CheckBox box)
        {
            if (box.IDString == "COLORS")
            {
                return colorChecked;
            }
            else
            {
                RainMeadow.Debug($"GetChecked {box.IDString} not implemented");
                return false;
            }
        }

        public void SetChecked(CheckBox box, bool c)
        {
            if (box.IDString == "COLORS")
            {
                colorChecked = c;
                if (colorChecked)// && !CheckJollyCoopAvailable(colorFromIndex(slugcatPageIndex)))
                {
                    AddColorButtons();
                    manager.rainWorld.progression.miscProgressionData.colorsEnabled[PlayerSelectedSlugcat.value] = true;
                }
                else
                {
                    RemoveColorButtons();
                    manager.rainWorld.progression.miscProgressionData.colorsEnabled[PlayerSelectedSlugcat.value] = false;
                }
            }
        }

        public static float GetRestartTextWidth(InGameTranslator.LanguageID lang)
        {
            float result = 85f;
            if (lang == InGameTranslator.LanguageID.Chinese || lang == InGameTranslator.LanguageID.TraditionalChinese)
            {
                result = 110f;
            }
            else if (lang == InGameTranslator.LanguageID.French || lang == InGameTranslator.LanguageID.German)
            {
                result = 155f;
            }
            else if (lang == InGameTranslator.LanguageID.Spanish || lang == InGameTranslator.LanguageID.Portuguese)
            {
                result = 140f;
            }
            else if (lang == InGameTranslator.LanguageID.Japanese)
            {
                result = 180f;
            }

            return result;
        }

        public static float GetRestartTextOffset(InGameTranslator.LanguageID lang)
        {
            float result = 0f;
            if (lang == InGameTranslator.LanguageID.French)
            {
                result = 35f;
            }
            else if (lang == InGameTranslator.LanguageID.Japanese || lang == InGameTranslator.LanguageID.Italian || lang == InGameTranslator.LanguageID.Spanish || lang == InGameTranslator.LanguageID.Portuguese)
            {
                result = 25f;
            }
            else if (lang == InGameTranslator.LanguageID.German)
            {
                result = 50f;
            }

            return result;
        }

        public override void SliderSetValue(Slider slider, float f)
        {
            if (slider.ID == ExpeditionEnums.SliderID.ChallengeDifficulty)
            {
                base.SliderSetValue(slider, f);
                return;
            }

            // slugcatSelectMenu SliderSetValue

            SlugcatStats.Name name = PlayerSelectedSlugcat;
            int num = activeColorChooser;
            Vector3 vector = new Vector3(1f, 1f, 1f);
            if (manager.rainWorld.progression.miscProgressionData.colorChoices[name.value][num].Contains(","))
            {
                string[] array = manager.rainWorld.progression.miscProgressionData.colorChoices[name.value][num].Split(',');
                vector = new Vector3(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture));
            }

            if (slider.ID == MMFEnums.SliderID.Hue)
            {
                vector[0] = Mathf.Clamp(f, 0f, 0.99f);
                manager.rainWorld.progression.miscProgressionData.colorChoices[name.value][num] = vector[0].ToString(CultureInfo.InvariantCulture) + "," + vector[1].ToString(CultureInfo.InvariantCulture) + "," + vector[2].ToString(CultureInfo.InvariantCulture);
            }
            else if (slider.ID == MMFEnums.SliderID.Saturation)
            {
                vector[1] = Mathf.Clamp(f, 0f, 1f);
                RWCustom.Custom.colorToHex(RWCustom.Custom.HSL2RGB(vector[0], vector[1], vector[2]));
                manager.rainWorld.progression.miscProgressionData.colorChoices[name.value][num] = vector[0].ToString(CultureInfo.InvariantCulture) + "," + vector[1].ToString(CultureInfo.InvariantCulture) + "," + vector[2].ToString(CultureInfo.InvariantCulture);
            }
            else if (slider.ID == MMFEnums.SliderID.Lightness)
            {
                vector[2] = Mathf.Clamp(f, 0.01f, 1f);
                RWCustom.Custom.colorToHex(RWCustom.Custom.HSL2RGB(vector[0], vector[1], vector[2]));
                manager.rainWorld.progression.miscProgressionData.colorChoices[name.value][num] = vector[0].ToString(CultureInfo.InvariantCulture) + "," + vector[1].ToString(CultureInfo.InvariantCulture) + "," + vector[2].ToString(CultureInfo.InvariantCulture);
            }

            if (colorInterface != null)
            {
                colorInterface.bodyColors[num].color = RWCustom.Custom.HSL2RGB(vector[0], vector[1], vector[2]);
            }

            selectedObject = slider;
        }

        public override float ValueOfSlider(Slider slider)
        {
            try
            {
                if (slider.ID == ExpeditionEnums.SliderID.ChallengeDifficulty)
                {
                    return ExpeditionData.challengeDifficulty;
                }

                SlugcatStats.Name name = PlayerSelectedSlugcat;
                int index = activeColorChooser;
                Vector3 vector = new Vector3(1f, 1f, 1f);
                if (manager.rainWorld.progression.miscProgressionData.colorChoices[name.value][index].Contains(","))
                {
                    string[] array = manager.rainWorld.progression.miscProgressionData.colorChoices[name.value][index].Split(',');
                    if (array.Length == 3)
                    {
                        vector = new Vector3(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture));
                    }
                }

                if (slider.ID == MMFEnums.SliderID.Hue)
                {
                    return vector[0];
                }

                if (slider.ID == MMFEnums.SliderID.Saturation)
                {
                    return vector[1];
                }

                if (slider.ID == MMFEnums.SliderID.Lightness)
                {
                    return vector[2];
                }

                return 0f;
            }
            catch (System.Exception e)
            {
                RainMeadow.Error(e);
                return 0f;
            }
        }
    }
}
