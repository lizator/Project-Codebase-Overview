using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Project_Codebase_Overview.ContributorManagement
{
    public class ColorPicker
    {
        private List<Color> HardcodedColors = new List<Color>
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

        private static ColorPicker Instance;

        public static ColorPicker GetInstance()
        {
            if (Instance == null)
            {
                Instance = new ColorPicker();
            }
            return Instance;
        }

        private ColorPicker()
        {

        }

        public Color AssignAuthorColor()
        {
            if (AuthorCounter >= HardcodedColors.Count)
            {
                return Color.FromArgb(255, 150, 150, 150); //Return grey as default
            }
            return HardcodedColors[AuthorCounter++];
        }

    }
}
