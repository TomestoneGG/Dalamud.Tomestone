using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalamud.Tomestone.Models
{
    internal class Gearset
    {
        public uint jobId { get; set; } = 0;
        public uint itemLevel { get; set; } = 0;
        public List<Item> items { get; set; } = new List<Item>();
    }

    // A equipped item
    internal class Item
    {
        public uint itemId { get; set; } = 0;
        public bool hq { get; set; } = false;
        public ItemSlot slot { get; set; } = 0;
        public List<Materia>? materia { get; set; } = null;
        public Glamour? glamour { get; set; } = null;
        public uint dye1 { get; set; } = 0;
        public uint dye2 { get; set; } = 0;
    }

    internal class Glamour
    {
        public uint itemId { get; set; } = 0;
        public bool hq { get; set; } = false;
    }

    internal class Materia
    {
        public uint materiaType { get; set; } = 0;
        public MateriaType type { get; set; } = 0;
        public uint grade { get; set; } = 0;
        public ushort slot { get; set; } = 0;
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
