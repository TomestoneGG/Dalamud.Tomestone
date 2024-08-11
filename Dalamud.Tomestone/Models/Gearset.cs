using NetStone.Model.Parseables.Character;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Dalamud.Tomestone.Models
{
    internal class Gearset
    {
        public uint jobId { get; set; } = 0;
        public uint itemLevel { get; set; } = 0;
        public List<Attribute> attributes { get; set; } = null;
        public List<Item> items { get; set; } = new List<Item>();
    }

    internal class Attribute
    {
        public string name { get; set; } = string.Empty;
        public int value { get; set; } = 0;
    }

    // A equipped item
    internal class Item
    {
        public uint itemId { get; set; } = 0;
        public bool hq { get; set; } = false;
        [JsonIgnore]
        public uint itemLevel { get; set; } = 0;
        public ItemSlot slot { get; set; } = 0;
        public List<Materia>? materia { get; set; } = null;
        public Glamour? glamour { get; set; } = null;
        public string dye1 { get; set; } = string.Empty;
        public string dye2 { get; set; } = string.Empty;
    }

    internal class Glamour
    {
        public uint itemId { get; set; } = 0;
        public bool hq { get; set; } = false;
    }

    internal class Materia
    {
        public string name { get; set; } = string.Empty;
        public string stat { get; set; } = string.Empty;
        public short value { get; set; } = 0;
        public short slot { get; set; } = 0;
    }

    internal class MateriaIDTranslationEntry
    {
        public MateriaType Type { get; set; }
        public uint grade { get; set; }
        public uint materiaId { get; set; }
    }

    enum ItemSlot
    {
        MainHand = 0,
        OffHand = 1,
        Head = 2,
        Body = 3,
        Hands = 4,
        Waist = 5,
        Legs = 6,
        Feet = 7,
        Ears = 8,
        Neck = 9,
        Wrists = 10,
        LeftRing = 11,
        RightRing = 12,
        Crystals = 13,
    }

    internal enum MateriaType
    {
        // TODO: Do we even need resistance materia?
        Unknown = 0,
        Unknown1 = 1,
        Strength = 2,
        Vitality = 3,
        Dexterity = 4,
        Intelligence = 5,
        Mind = 6,
        Piety = 7,
        FireResistance = 8,
        IceResistance = 9,
        WindResistance = 10,
        EarthResistance = 11,
        LightningResistance = 12,
        WaterResistance = 13,
        DirectHit = 14,
        CriticalHit = 15,       
        Determination = 16,
        Tenacity = 17,
        Gathering = 18,
        Perception = 19,
        GP = 20,
        Craftsmanship = 21,
        CP = 22,
        Control = 23,
        SkillSpeed = 24,
        SpellSpeed = 25,
        SlowResistance = 26,
        SilenceResistance = 27,
        BlindResistance = 28,
        PoisonResistance = 29,
        StunResistance = 30,
        SleepResistance = 31,
        BindResistance = 32,
        HeavyResistance = 33,
        // 34+ are "unknown" / there's some mainstat materia, we'll see if we need them
    }
}
