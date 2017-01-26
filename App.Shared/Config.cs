using System;
using System.Drawing;
using Rock.Mobile.UI;
using MobileApp.Shared.Strings;
using MobileApp.Shared.PrivateConfig;
using MobileApp;

namespace MobileApp
{
    namespace Shared
    {
        /// <summary>
        /// Config contains values for customizing various aspects of App.
        /// </summary>
        namespace Config
        {
            // This class can be used to quickly test various things within the app.
            #if DEBUG
            public class DebugConfig
            {
                /// <summary>
                /// When this is true, all news items will go to the NewsItemURL.
                /// Useful for things like app invites, which only work in the app (can't be tested in a browser).
                /// This way you don't have to setup a news item in Rock.
                /// </summary>
                public static bool News_Override_Item = false;

                public const string News_Override_ReferenceURL = "http://ccv.church/soakedmobileinvite";
                public const bool News_Override_IncludeImpersonationToken = true;
                public const bool News_Override_ReferenceUrlLaunchesBrowser = false;
            }
            #endif
            
            public class GeneralConfig
            {
                public const int Version = 2190;

                /// <summary>
                /// The full name of your organization
                /// </summary>
                public const string OrganizationName = "Christ's Church of the Valley";

                /// <summary>
                /// The abbreviated name of your organization. (You can set this to OrganizationName if desired)
                /// </summary>
                public const string OrganizationShortName = "CCV";

                /// <summary>
                /// The name that Android should use for your app when launched.
                /// </summary>
                public const string AndroidAppName = "CCV Mobile";

                /// <summary>
                /// Used when creating new addresses that will be sent to Rock. If your app will run in another country,
                /// set CountryCode to the ISO country code.
                /// </summary>
                public const string CountryCode = "US";

                /// <summary>
                /// The Facebook app ID reprsenting the Facebook app that will get things on behalf of the user.
                /// </summary>
                public const string FBAppID = "495461873811179";

                /// <summary>
                /// iOS only, this controls what style of keyboard is used for PLATFORM textFields.
                /// Meaning the ones dynamically created in code via Rock.Mobile. 
                /// Any normal iOS text field needs to have its style explicitely set.
                /// Although this is an int, it should match the KeyboardAppearance enum.
                /// </summary>
                public const KeyboardAppearanceStyle iOSPlatformUIKeyboardAppearance = KeyboardAppearanceStyle.Dark;

                /// <summary>
                /// This should be the base URL for where your Rock instance is hosted.
                /// </summary>
                #if DEBUG
                public const string RockBaseUrl = "http://dev.ccv.church/";
                //public const string RockBaseUrl = "http://10.211.55.6:50345/";
                //public const string RockBaseUrl = "http://rock.ccv.church/";
                #else
                public const string RockBaseUrl = "http://rock.ccv.church/";
                #endif

                /// <summary>
                /// Defined in Rock, this should be generated ONCE at Rock's initial setup and then never changed.
                /// It is the key the mobile app uses so Rock knows who it's talking to.
                /// </summary>
                public const string RockMobileAppAuthorizationKey = "hWTaZ7buziBcJQH31KCm3Pzz";

                /// <summary>
                /// The base URL to look for Notes
                /// </summary>
                public const string NoteBaseURL = RockBaseUrl + "api/Podcast/Category/0/mobile_app/0?NumSeries=12";

                /// <summary>
                /// Set to true if you wish to use Analytics
                /// </summary>
                public const bool Use_Analytics = true;

                /// <summary>
                /// Defines the "app" keys for your Xamarin Insights analytics.
                /// </summary>
                public const string iOS_Xamarin_Insights_Key = "0ddb3228fc1eb8272392278ae7d73aa64bb535a5";
                public const string Droid_Xamarin_Insights_Key = "4d2a2e4245f141dad44ee3f8e89fb2370dfab628";

                public const string iOS_Localytics_Key = "ba64af94d379a0e732e4ffe-0841c882-67b7-11e5-bef4-0013a62af900";
                public const string Droid_Localytics_Key = "ba64af94d379a0e732e4ffe-0841c882-67b7-11e5-bef4-0013a62af900";
            }

            public class SpringboardConfig
            {
                /// <summary>
                /// The icon to use representing the News element
                /// </summary>
                public const string Element_News_Icon = "";

                /// <summary>
                /// The icon to use representing the Connect element
                /// </summary>
                public const string Element_Connect_Icon = "";

                /// <summary>
                /// The icon to use representing the Messages element
                /// </summary>
                public const string Element_Messages_Icon = "";

                /// <summary>
                /// The icon to use representing the Prayer element
                /// </summary>
                public const string Element_Prayer_Icon = "";

                /// <summary>
                /// The icon to use representing the Give element
                /// </summary>
                public const string Element_Give_Icon = "";

                /// <summary>
                /// The icon to use representing the More element
                /// </summary>
                public const string Element_More_Icon = "";
            }

            public class NewsConfig
            {
                /// <summary>
                ///  This will show if the user needs to update the app.
                /// </summary>
                public static string [] UpgradeNews =
                    {
                        //Title
                        "Time to Upgrade", 

                        // Description
                        string.Empty,

                        // Reference URL
                        "http://ccv.church/apps",

                        // Image Names
                        "upgrade_news_main",

                        "upgrade_news_header",
                    };
            }

            public class NoteConfig
            {
                /// <summary>
                /// The color of the font/icon when displaying the citation icon.
                /// </summary>
                public const uint CitationUrl_IconColor = 0x878686BB;

                /// <summary>
                /// The color of the user note anchor (which is what the user interacts with to move, open, close and delete the note)
                /// </summary>
                public const uint UserNote_AnchorColor = 0x77777777;

                /// <summary>
                /// The color of the icon in the details table row.
                /// </summary>
                public const uint Details_Table_IconColor = 0xc43535FF;
            }

            public class PrayerConfig
            {
                /// <summary>
                /// The color to fill the "Pray" circle when a user prays for a request.
                /// </summary>
                public const uint PrayedForColor = 0x26b8e2FF;
            }

            public class ConnectConfig
            {
                // Allocate each Engagement object in the order we want them displayed.
                // Engagement objects are defined in Engagement.cs
                public static IEngagement[] GetEngagedList = 
                {
                    new StartingPoint_Engagement( ),
                    new Baptism_Engagement( ),
                    new Worship_Engagement( ),
                    new Connect_Engagement( ),
                    new Serve_Engagement( ),
                    new Give_Engagement( ),
                    new Share_Engagement( ),
                    new Coach_Engagement( )
                };
                
                /// <summary>
                /// The default latitude the group finder map will be positioned to before a search is performed.
                /// </summary>
                public const double GroupFinder_DefaultLatitude = 33.6054149;

                /// <summary>
                /// The default longitude the group finder map will be positioned to before a search is performed. 
                /// </summary>
                public const double GroupFinder_DefaultLongitude = -112.125051;

                /// <summary>
                /// The default latitude scale (how far out) the map will be scaled to before a search is performed.
                /// </summary>
                public const int GroupFinder_DefaultScale_iOS = 100000;

                /// <summary>
                /// The default longitude scale (how far out) the map will be scaled to before a search is performed. 
                /// </summary>
                public const float GroupFinder_DefaultScale_Android = 9.25f;
            }

            public class GiveConfig
            {
                /// <summary>
                /// The url to take a user to for giving.
                /// </summary>
                public const string GiveUrl = "http://ccv.church/ma-give";
            }

            public class ControlStylingConfig
            {
                /// <summary>
                /// The color of text for buttons
                /// </summary>
                public const uint Button_TextColor = 0xCCCCCCFF;

                /// <summary>
                /// The color for text that is not placeholder (what the user types in, control labels, etc.)
                /// </summary>
                public const uint TextField_ActiveTextColor = 0xCCCCCCFF;

                /// <summary>
                /// The color for placeholder text in fields the user can type into.
                /// </summary>
                public const uint TextField_PlaceholderTextColor = 0x878686FF;

                /// <summary>
                /// The color of text in standard labels
                /// </summary>
                public const uint Label_TextColor = 0xCCCCCCFF;

                /// <summary>
                /// The color of the active springboard element.
                /// </summary>
                public const uint Springboard_ActiveElementTextColor = 0xFFFFFFFF;

                /// <summary>
                /// The color for inactive springboard elements.
                /// </summary>
                public const uint Springboard_InActiveElementTextColor = 0xA7A7A7FF;




                /// <summary>
                /// The background color of the springboard area.
                /// </summary>
                public const uint Springboard_BackgroundColor = 0x2D2D2DFF;

                /// <summary>
                /// The background color of a selected element.
                /// </summary>
                public const int Springboard_Element_SelectedColor = 0x7a1315FF;

                /// <summary>
                /// The color of the top nav toolbar background.
                /// </summary>
                public const uint TopNavToolbar_BackgroundColor = 0x191919FF;

                /// <summary>
                /// The background color for the pages (basically the darkest area)
                /// </summary>
                public const uint BackgroundColor = 0x212121FF;

                /// <summary>
                /// The background color for buttons
                /// </summary>
                public const uint Button_BGColor = 0x7E7E7EFF;

                /// <summary>
                /// The background color for the layer that backs elements (like the strip behind First Name)
                /// </summary>
                public const uint BG_Layer_Color = 0x3E3E3EFF;

                /// <summary>
                /// The background color for a text layer that has invalid input. This lets the user know
                /// there is something wrong with the particular field.
                /// </summary>
                public const uint BadInput_BG_Layer_Color = 0x5B1013FF;

                /// <summary>
                /// The border color for the layer that backs elements (like the strip behind First Name)
                /// This can also be used to highlight certain elements, as it is a bright color.
                /// </summary>
                public const uint BG_Layer_BorderColor = 0x595959FF;

                /// <summary>
                /// The color for the footers of primary table cells. (Like the footer in the Messages->Series primary cell that says "Previous Messages")
                /// </summary>
                public const uint Table_Footer_Color = 0x262626FF;

                /// <summary>
                /// The color of a UI Switch when turned 'on'
                /// </summary>
                public const uint Switch_OnColor = 0x26b8e2FF;

                /// <summary>
                /// The color when transitioning from the android splash screen to the splash / oobe intro.
                /// You probably want it to be the primary color in your splash screen.
                /// </summary>
                public const uint OOBE_Splash_BG_Color = 0x5b0f10FF;




                /// <summary>
                /// The corner roundedness for buttons (0 is no curvature)
                /// </summary>
                public const uint Button_CornerRadius = 3;

                /// <summary>
                /// The border thickness for the layer that backs elements (like the strip behind First Name)
                /// </summary>
                public const float BG_Layer_BorderWidth = .5f;




                /// <summary>
                /// The font to use representing a bold font throughout the app.
                /// </summary>
                public const string Font_Bold = "DINOffcCondBlack-Regular";

                /// <summary>
                /// The font to use representing a regular font throughout the app.
                /// </summary>
                public const string Font_Regular = "OpenSans-Regular";

                /// <summary>
                /// The font to use representing a light font throughout the app.
                /// </summary>
                public const string Font_Light = "OpenSansLight-Regular";

                /// <summary>
                /// The size of to use for the large font throughout the app.
                /// </summary>
                public const uint Large_FontSize = 23;

                /// <summary>
                /// The size of to use for the medium font throughout the app.
                /// </summary>
                public const uint Medium_FontSize = 19;

                /// <summary>
                /// The size of to use for the small font throughout the app.
                /// </summary>
                public const uint Small_FontSize = 16;
            }

            public class AboutConfig
            {
                /// <summary>
                /// The page to navigate to in the About's embedded webview.
                /// </summary>
                public const string Url = Config.GeneralConfig.RockBaseUrl + "ma-more";
            }
        }
    }
}
