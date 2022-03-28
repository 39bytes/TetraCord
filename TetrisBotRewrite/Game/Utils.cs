using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrisBotRewrite
{
    public static class Utils
    {
        public static string TetrominoTypeToEmoji(int type)
        {
            switch (type)
            {
                case 1:
                    return "🍎";
                case 2:
                    return "🍓";
                case 3:
                    return "🍐";
                case 4:
                    return "🍊";
                case 5:
                    return "🍑";
                case 6:
                    return "🍋";
                case 7:
                    return "🍏";
                default:
                    return "◽";
            }
        }
    }
}
