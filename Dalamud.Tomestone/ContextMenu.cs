using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalamud.Tomestone
{
    internal class ContextMenu
    {
        public static void Enable()
        {
            Service.ContextMenu.OnMenuOpened += OnOpenContextMenu;
        }

        public static void Disable() {
            Service.ContextMenu.OnMenuOpened -= OnOpenContextMenu;
        }

        private static bool IsMenuValid(IMenuArgs menuOpenedArgs)
        {
            if (menuOpenedArgs.Target is not MenuTargetDefault menuTargetDefault)
            {
                return false;
            }

            switch (menuOpenedArgs.AddonName)
            {
                case null: // Nameplate/Model menu
                case "LookingForGroup":
                case "PartyMemberList":
                case "FriendList":
                case "FreeCompany":
                case "SocialList":
                case "ContactList":
                case "ChatLog":
                case "_PartyList":
                case "LinkShell":
                case "CrossWorldLinkshell":
                case "ContentMemberList": // Eureka/Bozja/...
                case "BeginnerChatList":
                    return menuTargetDefault.TargetName != string.Empty && DalamudUtils.IsWorldValid(menuTargetDefault.TargetHomeWorld.Id);

                case "BlackList":
                case "MuteList":
                    return menuTargetDefault.TargetName != string.Empty;
            }

            return false;
        }

        public static void OnOpenContextMenu(IMenuOpenedArgs menuOpenedArgs)
        {
            if (!Service.PluginInterface.UiBuilder.ShouldModifyUi || !IsMenuValid(menuOpenedArgs)) return;

            menuOpenedArgs.AddMenuItem(new MenuItem
            {
                PrefixChar = 'T',
                Name = Tomestone.T.Configuration.ContextMenuButtonName,
                OnClicked = OpenTomestone
            });
        }

        private static void OpenTomestone(IMenuItemClickedArgs menuItemClickedArgs)
        {
            if (!IsMenuValid(menuItemClickedArgs))
            {
                return;
            }

            if (menuItemClickedArgs.Target is not MenuTargetDefault menuTargetDefault)
            {
                return;
            }

            string playerName = menuTargetDefault.TargetName;
            var world = DalamudUtils.GetWorld(menuTargetDefault.TargetHomeWorld.Id);

            // Ensure the world is valid
            if (!DalamudUtils.IsWorldValid(world))
            {
                return;
            }

            Utils.OpenTomestoneLink(world.Name, playerName);
        }
    }
}
