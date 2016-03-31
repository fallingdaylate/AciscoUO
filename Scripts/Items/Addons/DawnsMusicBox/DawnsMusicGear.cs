﻿using System;
using Server.Targeting;

namespace Server.Items
{
    [Flipable(0x1053, 0x1054)]
    public class DawnsMusicGear : Item
    {
        private MusicName m_Music;
        [Constructable]
        public DawnsMusicGear()
            : this(DawnsMusicBox.RandomTrack(DawnsMusicRarity.Common))
        {
        }

        [Constructable]
        public DawnsMusicGear(MusicName music)
            : base(0x1053)
        {
            this.m_Music = music;

            this.Weight = 1.0;
        }

        public DawnsMusicGear(Serial serial)
            : base(serial)
        {
        }

        public static DawnsMusicGear RandomCommon
        {
            get
            {
                return new DawnsMusicGear(DawnsMusicBox.RandomTrack(DawnsMusicRarity.Common));
            }
        }
        public static DawnsMusicGear RandomUncommon
        {
            get
            {
                return new DawnsMusicGear(DawnsMusicBox.RandomTrack(DawnsMusicRarity.Uncommon));
            }
        }
        public static DawnsMusicGear RandomRare
        {
            get
            {
                return new DawnsMusicGear(DawnsMusicBox.RandomTrack(DawnsMusicRarity.Rare));
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public MusicName Music
        {
            get
            {
                return this.m_Music;
            }
            set
            {
                this.m_Music = value;
            }
        }
        public override void AddNameProperty(ObjectPropertyList list)
        {
            DawnsMusicInfo info = DawnsMusicBox.GetInfo(this.m_Music);

            if (info != null)
            {
                if (info.Rarity == DawnsMusicRarity.Common)
                    list.Add(1075204); // Gear for Dawn's Music Box (Common)
                else if (info.Rarity == DawnsMusicRarity.Uncommon)
                    list.Add(1075205); // Gear for Dawn's Music Box (Uncommon)
                else if (info.Rarity == DawnsMusicRarity.Rare)
                    list.Add(1075206); // Gear for Dawn's Music Box (Rare)

                list.Add(info.Name);
            }
            else
                base.AddNameProperty(list);
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.Target = new InternalTarget(this);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt((int)1); // version
			
            writer.Write((int)this.m_Music);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
			
            switch ( version )
            {
                case 1:
                    {
                        this.m_Music = (MusicName)reader.ReadInt();
                        break;
                    }
            }
			
            if (version == 0) // Music wasn't serialized in version 0, pick a new track of random rarity
            {
                DawnsMusicRarity rarity;
                double rand = Utility.RandomDouble();
				
                if (rand < 0.025)
                    rarity = DawnsMusicRarity.Rare;
                else if (rand < 0.225)
                    rarity = DawnsMusicRarity.Uncommon;
                else
                    rarity = DawnsMusicRarity.Common;
				
                this.m_Music = DawnsMusicBox.RandomTrack(rarity);
            }
        }

        public class InternalTarget : Target
        {
            private readonly DawnsMusicGear m_Gear;
            public InternalTarget(DawnsMusicGear gear)
                : base(2, false, TargetFlags.None)
            {
                this.m_Gear = gear;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (this.m_Gear == null || this.m_Gear.Deleted)
                    return;

                DawnsMusicBox box = targeted as DawnsMusicBox;

                if (box != null)
                {
                    if (!box.Tracks.Contains(this.m_Gear.Music))
                    {
                        box.Tracks.Add(this.m_Gear.Music);
                        box.InvalidateProperties();

                        this.m_Gear.Delete();

                        from.SendLocalizedMessage(1071961); // This song has been added to the musicbox.
                    }
                    else
                        from.SendLocalizedMessage(1071962); // This song track is already in the musicbox.
                }
                else
                    from.SendLocalizedMessage(1071964); // Gears can only be put into a musicbox.
            }
        }
    }
}