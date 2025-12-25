using Dalamud.Game.Gui.ContextMenu;

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
                    return menuTargetDefault.TargetName != string.Empty && DalamudUtils.IsWorldValid(menuTargetDefault.TargetHomeWorld.RowId);

                case "BlackList":
                case "MuteList":
                    return menuTargetDefault.TargetName != string.Empty;
            }

            return false;
        }

        public static void OnOpenContextMenu(IMenuOpenedArgs menuOpenedArgs)
        {
            if (!Service.PluginInterface.UiBuilder.ShouldModifyUi || !Tomestone.T.Configuration.ModifyUI || !Tomestone.T.Configuration.RemoteConfig.uiModify || !IsMenuValid(menuOpenedArgs)) return;

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
            var world = DalamudUtils.GetWorld(menuTargetDefault.TargetHomeWorld.RowId);

            // Ensure the world is valid
            if (!DalamudUtils.IsWorldValid(world))
            {
                return;
            }

            Utils.OpenTomestoneLink(world!.Name.ExtractText(), playerName);
        }
    }
}
