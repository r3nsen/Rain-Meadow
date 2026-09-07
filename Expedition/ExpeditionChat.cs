using Menu;
using Menu.Remix.MixedUI;

using RainMeadow.UI.Components;

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RainMeadow
{
    // basically copied from story menu chat
    public partial class ExpeditionOnlineMenu : ExpeditionMenu
    {        
        private int currentLogIndex = 0;
        private bool isChatToggled = false;

        public NullLobbyError nullLobbyError;
        private ButtonScroller.TextAnchor textAnchor;

        internal void ResetChatInput()
        {
            if(chatMenuBox is not null)
                ChatLogManager.MessageLogged -= chatMenuBox.OnMessageLogged;

            pages[_currentPage].ClearMenuObject(ref this.chatMenuBox);

            if (this.isChatToggled && this.chatMenuBox is null)
            {
                this.chatMenuBox = new ChatMenuBox(this, pages[_currentPage], new Vector2(this.chatTextBoxPos.x + 24, 0), new(280, 300));
                ChatLogManager.MessageLogged += chatMenuBox.OnMessageLogged;
                
                pages[_currentPage].subObjects.Add(this.chatMenuBox);
            }
        }

        public void ToggleChat(bool toggled)
        {
            this.isChatToggled = toggled;
            this.ResetChatInput();
        }
    }
}
