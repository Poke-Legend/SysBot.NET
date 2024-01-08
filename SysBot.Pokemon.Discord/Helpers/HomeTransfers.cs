// Decompiled with JetBrains decompiler
// Type: SysBot.Pokemon.Discord.Helpers.HomeTransfers
// Assembly: SysBot.Pokemon.Discord, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 74F5F4C6-45FB-4005-92E3-65C9CBB53001
// Assembly location: SysBot.Pokemon.Discord.dll inside C:\Users\bdkoh\OneDrive\Desktop\MergeBot.exe)
// Compiler-generated code is shown

using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SysBot.Pokemon.Discord.Helpers
{
    internal class HomeTransfers
    {
      
        public static readonly HashSet<ValueTuple<Species, byte>> TransferMapSV;
        public static readonly HashSet<Species> MightyMap;

        public static bool IsHomeTransferOnlySV(Species species, byte form)
        {
            return HomeTransfers.TransferMapSV.Contains(new ValueTuple<Species, byte>(species, form));
        }

        public static bool NoSportSafari(Species species)
        {
            return HomeTransfers.MightyMap.Contains(species);
        }

        public HomeTransfers()
        {
            base.GetHashCode();
        }

        static HomeTransfers()
        {
            HashSet<ValueTuple<Species, byte>> valueTupleSet = new HashSet<ValueTuple<Species, byte>>();
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Pikachu, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Pikachu, (byte)2));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Pikachu, (byte)3));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Pikachu, (byte)4));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Pikachu, (byte)5));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Pikachu, (byte)6));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Pikachu, (byte)7));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Pikachu, (byte)8));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Pikachu, (byte)9));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Raichu, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Weezing, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Articuno, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Zapdos, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Moltres, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Zapdos, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Moltres, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Jirachi, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Deoxys, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Uxie, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Mesprit, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Azelf, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Heatran, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Regigigas, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Giratina, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Giratina, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Cresselia, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Manaphy, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Shaymin, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Shaymin, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Arceus, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Arceus, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Arceus, (byte)2));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Arceus, (byte)3));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Arceus, (byte)4));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Arceus, (byte)5));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Arceus, (byte)6));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Arceus, (byte)7));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Arceus, (byte)8));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Arceus, (byte)9));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Arceus, (byte)10));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Arceus, (byte)11));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Arceus, (byte)12));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Arceus, (byte)13));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Arceus, (byte)14));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Arceus, (byte)15));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Arceus, (byte)16));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Arceus, (byte)17));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Lilligant, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Braviary, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Tornadus, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Tornadus, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Thundurus, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Thundurus, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Landorus, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Landorus, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Keldeo, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Vivillon, (byte)19));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Sliggoo, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Goodra, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Avalugg, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Diancie, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Hoopa, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Hoopa, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Volcanion, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Magearna, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Magearna, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Zacian, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Zamazenta, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Eternatus, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Zarude, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Zarude, (byte)1));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Regieleki, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Regidrago, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Calyrex, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Wyrdeer, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Ursaluna, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Enamorus, (byte)0));
            valueTupleSet.Add(new ValueTuple<Species, byte>(Species.Enamorus, (byte)1));
            HomeTransfers.TransferMapSV = valueTupleSet;
            HashSet<Species> speciesSet = new HashSet<Species>();
            speciesSet.Add(Species.Eevee);
            speciesSet.Add(Species.Charizard);
            speciesSet.Add(Species.Delibird);
            speciesSet.Add(Species.Cinderace);
            speciesSet.Add(Species.Greninja);
            speciesSet.Add(Species.Pikachu);
            speciesSet.Add(Species.Decidueye);
            speciesSet.Add(Species.Samurott);
            speciesSet.Add(Species.Typhlosion);
            speciesSet.Add(Species.Inteleon);
            speciesSet.Add(Species.Chesnaught);
            speciesSet.Add(Species.Delphox);
            speciesSet.Add(Species.Rillaboom);
            speciesSet.Add(Species.Mewtwo);
            speciesSet.Add(Species.GreatTusk);
            speciesSet.Add(Species.IronTreads);
            speciesSet.Add(Species.Gimmighoul);
            speciesSet.Add(Species.SlitherWing);
            speciesSet.Add(Species.IronMoth);
            HomeTransfers.MightyMap = speciesSet;
        }
    }
}
