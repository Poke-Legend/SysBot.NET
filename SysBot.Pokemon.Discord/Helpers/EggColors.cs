using System;
using System.Collections.Generic;
using static PKHeX.Core.AutoMod.Aesthetics;

namespace SysBot.Pokemon.Discord.Helpers
{
    internal class EggColors
    {
        // Define a dictionary to map PersonalColor enums to their corresponding
        // URLs
        public static Dictionary<PersonalColor, string> ColorToImageUrl = new Dictionary<PersonalColor, string>
        {
            { PersonalColor.Red, "https://sysbots.net/wp-content/uploads/2024/01/bYKR3QR.png" },
            { PersonalColor.Blue, "https://sysbots.net/wp-content/uploads/2024/01/L74uJf6.png" },
            { PersonalColor.Yellow, "https://sysbots.net/wp-content/uploads/2024/01/As5tZfq.png" },
            { PersonalColor.Green, "https://sysbots.net/wp-content/uploads/2024/01/f61zCSc.png" },
            { PersonalColor.Black, "https://sysbots.net/wp-content/uploads/2024/01/FxAtz42.png" },
            { PersonalColor.Brown, "https://sysbots.net/wp-content/uploads/2024/01/2uZABue.png" },
            { PersonalColor.Purple, "https://sysbots.net/wp-content/uploads/2024/01/MFNvS13.png" },
            { PersonalColor.Gray, "https://sysbots.net/wp-content/uploads/2024/01/ZTgB7wQ.png" },
            { PersonalColor.White, "https://sysbots.net/wp-content/uploads/2024/01/TBrao98.png" },
            { PersonalColor.Pink, "https://sysbots.net/wp-content/uploads/2024/01/HvO8MMQ.png" }
        };

        // Define the EggColor enum
        public enum EggColor : byte
        {
            Red,
            Blue,
            Yellow,
            Green,
            Black,
            Brown,
            Purple,
            Gray,
            White,
            Pink
        }

        // Define the mapping from PersonalColor to EggColor
        public static Dictionary<PersonalColor, EggColor> ColorToEggColor = new Dictionary<PersonalColor, EggColor>
{
    { PersonalColor.Red, EggColor.Red },
    { PersonalColor.Blue, EggColor.Blue },
    { PersonalColor.Yellow, EggColor.Yellow },
    { PersonalColor.Green, EggColor.Green },
    { PersonalColor.Black, EggColor.Black },
    { PersonalColor.Brown, EggColor.Brown },
    { PersonalColor.Purple, EggColor.Purple },
    { PersonalColor.Gray, EggColor.Gray },
    { PersonalColor.White, EggColor.White },
    { PersonalColor.Pink, EggColor.Pink }
};
    }
}



