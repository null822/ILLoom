using Hacknet;
using Hacknet.Misc;
using LoomModLib;
using LoomModLib.Attributes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestMod;

public class Test
{
    [Inject(".cctor", typeof(Settings))]
    [InjectIlIndex(82)]
    private void ModifySettings()
    {
        // menu buttons
        Settings.testingMenuItemsEnabled = true;
        Settings.HasLabyrinthsDemoStartMainMenuButton = true;
        Settings.isServerMode = true;
        
        // debug
        Settings.debugDrawEnabled = true;
        Settings.debugCommandsEnabled = true;
        
        // testing
        Settings.isSpecialTestBuild = true;
        Settings.isAlphaDemoMode = true;
        
        // disable tutorial
        Settings.osStartsWithTutorial = false;
        Settings.initShowsTutorial = false;
        Settings.slowOSStartup = false;
        
        // show all crashes
        Settings.recoverFromErrorsSilently = false;
    }
    
    [HoistType("Hacknet", "1.0.0.0", "Hacknet.Game1")]
    private class HacknetGame
    {
        [Insert("GameTime", typeof(HacknetGame))]
        public static TimeSpan GameTime = TimeSpan.Zero;
        
        [Inject("Update", typeof(HacknetGame))]
        [InjectHead]
        private static unsafe void StealGameTime(GameTime gameTime)
        {
            var p = (byte*)&gameTime;
            var v0 = *(long*)(p + 60); // do I know why the elapsed game time is 60 bytes after the object start? no
            var d = new TimeSpan(v0); // do I care? ...yes but there seems to be no better way
            
            GameTime = d;
        }
    }
    
    [Inject("switchTheme", typeof(ThemeManager))]
    [InjectHead]
    private void OverrideTheme(object osObject, OSTheme theme)
    {
        theme = OSTheme.HackerGreen;
    }
    
    [HoistType("Hacknet", "1.0.0.0", "Hacknet.MainMenu")]
    private class MainMenu
    {
        [Hoist("buttonColor")]
        private Color buttonColor;
        
        [Hoist("OSVersion")]
        public static string OSVersion;

        
        [Inject("LoadContent", typeof(MainMenu))]
        [InjectIlIndex(9)]
        private void ChangeMenuButtonColor()
        {
            buttonColor = Color.Magenta;
        }
        
        [Inject(".cctor", typeof(MainMenu))]
        [InjectIlIndex(3)]
        private void ModifyOsVersion()
        {
            OSVersion += '*';
        }
    }
    
    [HoistType("Hacknet", "1.0.0.0", "Hacknet.Gui.Button")]
    private class Button
    {
        [Insert("CalculateDynamicHue", typeof(Button))]
        private static Color CalculateDynamicHue()
        {
            const float period = 60;
            
            // for god knows what reason, hue is in [0,6] range, not [0, tau] or [0, 360]
            var h = (float)HacknetGame.GameTime.TotalMilliseconds * 6 / (period * 1000f) % 6;
            var c = new HSLColor(h, 1f, 0.5f).ToRGB();
            
            return c;
        }
        
        [Inject("drawModernButton", typeof(Button))]
        [InjectHead]
        private void OverrideButtonColor(int myID, int x, int y, int width, int height, string text, Color? selectedColor, Texture2D tex)
        {
            selectedColor = CalculateDynamicHue();
        }
    }

    [HoistType("Hacknet", "1.0.0.0", "Hacknet.Utils")]
    private class Utils
    {
        [Inject("SendRealWorldEmail", typeof(Utils))]
        [InjectHead]
        public static void DisableCrashEmail(string subject, string to, string body)
        {
            Injector.Return();
        }
    }
}
