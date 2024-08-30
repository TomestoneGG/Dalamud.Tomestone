using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System.Collections.Generic;

namespace Dalamud.Tomestone.Features
{
    internal static class TripleTriad
    {
        internal static unsafe List<uint> GetTripleTriadCards(UIState* uiState, List<uint> cache)
        {
            var unlockedTripleTriadCards = new List<uint>();
            foreach (var tripleTriadCardId in cache)
            {
                if (uiState->IsTripleTriadCardUnlocked((ushort)tripleTriadCardId))
                {
                    unlockedTripleTriadCards.Add(tripleTriadCardId);
                }
            }

            return unlockedTripleTriadCards;
        }
    }
}
