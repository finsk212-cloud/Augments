using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace Augments
{
    public class AugmentDebugCommand : ModCommand
    {
        public override string Command => "augment";
        public override CommandType Type => CommandType.Chat;
        public override string Usage => "/augment add <id or number> | /augment remove <id or number> | /augment addall | /augment clear | /augment list";
        public override string Description => "Debug: add, remove, clear, or list augments.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var augmentPlayer = caller.Player.GetModPlayer<AugmentPlayer>();

            if (args.Length == 0)
            {
                caller.Reply(Usage, Color.Yellow);
                return;
            }

            string sub = args[0].ToLower();

            if (sub == "list")
            {
                const int AugmentsPerPage = 8;
                int maxPage = System.Math.Max(1, (AugmentDatabase.All.Count + AugmentsPerPage - 1) / AugmentsPerPage);
                int page = 1;

                if (args.Length >= 2 && int.TryParse(args[1], out int requestedPage))
                    page = System.Math.Clamp(requestedPage, 1, maxPage);

                caller.Reply($"Augments page {page}/{maxPage}", Color.Yellow);

                int startIndex = (page - 1) * AugmentsPerPage;
                int endIndex = System.Math.Min(startIndex + AugmentsPerPage, AugmentDatabase.All.Count);

                for (int i = startIndex; i < endIndex; i++)
                {
                    var augment = AugmentDatabase.All[i];
                    caller.Reply($"{i + 1}. {augment.Id} - {augment.DisplayName} ({augment.Rarity})", Color.White);
                }

                return;
            }

            if (sub == "addall")
            {
                int addedCount = 0;

                foreach (var augment in AugmentDatabase.All)
                {
                    if (!augmentPlayer.HasAugment(augment.Id))
                    {
                        augmentPlayer.GrantAugment(augment);
                        addedCount++;
                    }
                }

                caller.Reply($"Added all augments. Total added: {addedCount}.", Color.LightGreen);
                return;
            }

            if (sub == "clear" || sub == "removeall" || sub == "reset")
            {
                int removedCount = 0;

                foreach (var augment in AugmentDatabase.All)
                {
                    if (augmentPlayer.HasAugment(augment.Id))
                    {
                        augmentPlayer.RemoveAugment(augment);
                        removedCount++;
                    }
                }

                caller.Reply($"Removed all augments. Total removed: {removedCount}.", Color.Orange);
                return;
            }

            if (args.Length < 2)
            {
                caller.Reply(Usage, Color.Yellow);
                return;
            }

            var selectedAugment = GetAugmentFromArg(args[1]);

            if (selectedAugment == null)
            {
                caller.Reply("No augment found. Use /augment list to see valid numbers and ids.", Color.Red);
                return;
            }

            switch (sub)
            {
                case "add":
                    augmentPlayer.GrantAugment(selectedAugment);
                    break;

                case "remove":
                    augmentPlayer.RemoveAugment(selectedAugment);
                    caller.Reply($"Removed {selectedAugment.DisplayName}.", Color.Orange);
                    break;

                default:
                    caller.Reply(Usage, Color.Yellow);
                    break;
            }
        }

        private Augment GetAugmentFromArg(string arg)
        {
            if (int.TryParse(arg, out int number))
            {
                int index = number - 1;

                if (index >= 0 && index < AugmentDatabase.All.Count)
                    return AugmentDatabase.All[index];

                return null;
            }

            return AugmentDatabase.GetById(arg);
        }
    }
}
