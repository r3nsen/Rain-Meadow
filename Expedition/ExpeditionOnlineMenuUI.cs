using Menu;
using Menu.Remix.MixedUI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RainMeadow
{
    public partial class ExpeditionOnlineMenu : ExpeditionMenu
    {
        private SlugcatStats.Name[] selectableSlugcats;
        public SlugcatStats.Name?[] playerSelectedSlugcats;

        void SetupOnlineMenuItens()
        {
           // lobbyLabel = new MenuLabel(this, pages[1], Translate("LOBBY"), new Vector2(194, 553), new(110, 30), true);
           // pages[0].subObjects.Add(lobbyLabel);

            this.chatTextBoxPos = new Vector2(this.manager.rainWorld.options.ScreenSize.x * 0.001f + (1366f - this.manager.rainWorld.options.ScreenSize.x) / 2f, 0);
            var toggleChat = new SimplerSymbolButton(this, pages[1], "Kill_Slugcat", "", this.chatTextBoxPos);
            toggleChat.OnClick += (_) =>
            {
                ToggleChat(!this.isChatToggled);
                if (input.controllerType == Options.ControlSetup.Preset.KeyboardSinglePlayer)
                {
                    selectedObject = null;
                }
            };
            pages[0].subObjects.Add(toggleChat);
        }
    }
}
