using System.Collections.Generic;

namespace PikunikuAPMod;

public class SlotData
{
    public readonly bool DeathLink;
    public readonly int DeathLinkAmnesty;
    public readonly string PikuColor;
    public readonly bool Coinsanity;
    public readonly bool CoopLevels;

    public SlotData(Dictionary<string, object> slotDict)
    {
        if (slotDict.TryGetValue("coinsanity", out var coinsanity))
            Coinsanity = (int)(long)coinsanity == 1;
        if (slotDict.TryGetValue("coop_levels", out var coopLevels))
            CoopLevels = (int)(long)coopLevels == 1;
        if (slotDict.TryGetValue("death_link_amnesty", out var amnesty))
            DeathLinkAmnesty = (int)(long)amnesty;
        if (slotDict.TryGetValue("death_link", out var deathLink))
            DeathLink = (int)(long)deathLink == 1;
        if (slotDict.TryGetValue("piku_color", out var pikuColor))
            PikuColor = (string)pikuColor;
        
        PikunikuAPMod.ItemHandler.FlushQueue();
    }
}
