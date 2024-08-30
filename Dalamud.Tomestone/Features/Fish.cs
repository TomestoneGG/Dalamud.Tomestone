using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System.Collections.Generic;

namespace Dalamud.Tomestone.Features
{
    internal static class Fish
    {
        internal static unsafe List<uint> GetFish(PlayerState* playerState)
        {
            List<uint> fish = new List<uint>();
            // Ensure PlayerState is initialized
            if (playerState == null)
            {
                return fish;
            }

            var caughtFishBitmaskArray = playerState->CaughtFishBitmask;

            // Calculate the fish ID and add it to the list of fish caught
            for (int i = 0; i < caughtFishBitmaskArray.Length; i++)
            {
                // Get the current byte
                byte currentByte = caughtFishBitmaskArray[i];

                // Reverse the bits in the current byte
                byte reversedByte = Utils.ReverseBits(currentByte);

                // Iterate over the bits in the byte
                for (int j = 0; j < 8; j++)
                {
                    // Check if the bit is set
                    if ((currentByte & (1 << j)) != 0)
                    {
                        // Calculate the fish ID
                        uint fishId = (uint)(i * 8 + j);

                        // Add the fish ID to the list of fish caught
                        fish.Add(fishId);
                    }
                }
            }

            return fish;
        }
    }
}
