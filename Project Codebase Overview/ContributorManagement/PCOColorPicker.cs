using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.Media;
using System.Drawing;
using Color = Windows.UI.Color;

namespace Project_Codebase_Overview.ContributorManagement
{
    public class PCOColorPicker
    {
        public static List<Color> HardcodedColors = new List<Color>
        {
            Color.FromArgb(255,255,0,0), //red
            Color.FromArgb(255,0,230,0), //green
            Color.FromArgb(255,0,0,230), //blue
            Color.FromArgb(255,255,255,0), //yellow
            Color.FromArgb(255,0,255,255), //cyan
            Color.FromArgb(255,255,0,255), //magenta
            Color.FromArgb(255,255,128,0), //orange
            Color.FromArgb(255,153,0,153), //purple
            Color.FromArgb(255,102,178,255) //lightblue
        };

        private int AuthorCounter = 0;
        private int TeamCounter = 0;

        public List<Color> ColorPalette;

        private static PCOColorPicker Instance;
        public static void ResetInstance()
        {
            Instance = null;
        }

        public static Color Black { get => Color.FromArgb(255, 0, 0, 0); }
        public static Color Tranparent { get => Color.FromArgb(0, 0, 0, 0); }

        public static PCOColorPicker GetInstance()
        {
            if (Instance == null)
            {
                Instance = new PCOColorPicker();
            }
            return Instance;
        }

        private PCOColorPicker()
        {
            GenerateColorPalette(50);
        }

        public Color AssignAuthorColor()
        {
            if (AuthorCounter >= ColorPalette.Count)
            {
                AuthorCounter = 0; //Return black as default
            }
            var col = ColorPalette[AuthorCounter];
            AuthorCounter++;
            return col;
        }

        public Color AssignTeamColor()
        {
            if (TeamCounter >= ColorPalette.Count)
            {
                TeamCounter = 0;
            }
            var col = ColorPalette[TeamCounter];
            TeamCounter++;
            return col;
        }



        private void GenerateColorPalette(int n)
        {
            List<Color> colors = new List<Color>();
            List<string> palette;
            if(n <= 20)
            {
                palette = Pallette20;
            }
            else
            {
                palette = Pallette50;
            }
            foreach(var paletteColor in palette)
            {
                var temp = ColorTranslator.FromHtml(paletteColor);
                colors.Add(Color.FromArgb(temp.A, temp.R, temp.G, temp.B));
            }
            ColorPalette = colors;
        }

        private List<string> Pallette20 = new List<string>
        {
            "#2f4f4f",
            "#2e8b57",
            "#800000",
            "#808000",
            "#000080",
            "#ff0000",
            "#ff8c00",
            "#ffff00",
            "#00ff00",
            "#ba55d3",
            "#00ffff",
            "#0000ff",
            "#ff00ff",
            "#1e90ff",
            "#fa8072",
            "#dda0dd",
            "#ff1493",
            "#98fb98",
            "#87cefa",
            "#ffdead"
        };
        private List<string> Pallette50 = new List<string>
        {
            "#808080",
            "#2f4f4f",
            "#556b2f",
            "#8b4513",
            "#a52a2a",
            "#2e8b57",
            "#228b22",
            "#808000",
            "#483d8b",
            "#008b8b",
            "#cd853f",
            "#4682b4",
            "#000080",
            "#9acd32",
            "#32cd32",
            "#daa520",
            "#8fbc8f",
            "#8b008b",
            "#b03060",
            "#d2b48c",
            "#48d1cc",
            "#9932cc",
            "#ff4500",
            "#ff8c00",
            "#ffd700",
            "#ffff00",
            "#00ff00",
            "#00ff7f",
            "#dc143c",
            "#00bfff",
            "#9370db",
            "#0000ff",
            "#a020f0",
            "#f08080",
            "#adff2f",
            "#ff6347",
            "#da70d6",
            "#b0c4de",
            "#ff00ff",
            "#f0e68c",
            "#6495ed",
            "#dda0dd",
            "#ff1493",
            "#afeeee",
            "#98fb98",
            "#7fffd4",
            "#ff69b4",
            "#fffacd",
            "#ffe4e1",
            "#ffb6c1"   
        };


    }
}
