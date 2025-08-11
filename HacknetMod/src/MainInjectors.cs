using Hacknet;
using Hacknet.Misc;
using LoomModLib;
using LoomModLib.Attributes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HacknetMod;

public class MainInjectors
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

    [HoistType]
    private class OS
    {
        [Hoist]
        public int totalRam;
    }
    
    [Inject("switchTheme", typeof(ThemeManager))]
    [InjectHead]
    private void OverrideTheme(object osObject, OSTheme theme)
    {
        theme = OSTheme.HackerGreen;
        var os = (OS)osObject;
        os.totalRam += 1000;
    }
    
    [HoistType]
    private class MainMenu
    {
        [Hoist]
        private Color buttonColor;
        
        [Hoist]
        public static string OSVersion;

        
        [Inject("LoadContent")]
        [InjectIlIndex(9)]
        private void ChangeMenuButtonColor()
        {
            buttonColor = Color.Magenta;
        }
        
        [Inject(".cctor")]
        [InjectIlIndex(3)]
        private void ModifyOsVersion()
        {
            OSVersion += '*';
        }
    }
    
    [HoistType(name:"Game1")]
    private class Game
    {
        [Insert]
        public static TimeSpan GameTime = TimeSpan.Zero;
        
        [Inject("Update")]
        [InjectHead]
        private static unsafe void StealGameTime(GameTime gameTime)
        {
            var p = (byte*)&gameTime;
            var v0 = *(long*)(p + 60); // do I know why the elapsed game time is 60 bytes after the object start? no
            var d = new TimeSpan(v0); // do I care? ...yes but there seems to be no better way
            
            GameTime = d;
        }
    }
    
    [HoistType(ns: "Hacknet.Gui")]
    private class Button
    {
        [Insert]
        private static Color CalculateDynamicHue()
        {
            const float period = 60;
            
            // for god knows what reason, hue is in [0,6] range, not [0, tau] or [0, 360]
            var h = (float)Game.GameTime.TotalMilliseconds * 6 / (period * 1000f) % 6;
            var c = new HSLColor(h, 1f, 0.5f).ToRGB();
            
            return c;
        }
        
        [Inject("drawModernButton")]
        [InjectHead]
        private void OverrideButtonColor(int myID, int x, int y, int width, int height, string text, Color? selectedColor, Texture2D tex)
        {
            selectedColor = CalculateDynamicHue();
        }
    }
    
    [HoistType]
    private class Utils
    {
        [Inject("SendRealWorldEmail")]
        [InjectHead]
        public static void DisableCrashEmail(string subject, string to, string body)
        {
            Injector.Return();
        }
    }
}
