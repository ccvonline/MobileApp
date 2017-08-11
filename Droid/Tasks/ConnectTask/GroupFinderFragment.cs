
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Webkit;

using MobileApp.Shared.Config;
using MobileApp.Shared.Strings;
using Android.Graphics;
using Android.Gms.Maps;
using MobileApp.Shared;
using MobileApp.Shared.Network;
using MobileApp.Shared.Analytics;
using Rock.Mobile.Animation;
using Android.Gms.Maps.Model;
using MobileApp.Shared.UI;
using Android.Views.InputMethods;
using MobileApp.Shared.PrivateConfig;
using MobileApp;

namespace Droid
{
    namespace Tasks
    {
        namespace Connect
        {
            public class GroupArrayAdapter : BaseAdapter
            {
                GroupFinderFragment ParentFragment { get; set; }
                public int SelectedIndex { get; set; }

                public GroupArrayAdapter( GroupFinderFragment parentFragment )
                {
                    ParentFragment = parentFragment;
                    SelectedIndex = -1;
                }

                public override int Count 
                {
                    get 
                    { 
                        // if we have any groups, return the count AND an additional, for the
                        // "get 10 more"
                        if( ParentFragment.GroupEntries.Count > 0 )
                        {
                            return ParentFragment.GroupEntries.Count + 1; 
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }

                public override Java.Lang.Object GetItem (int position) 
                {
                    // could wrap a Contact in a Java.Lang.Object
                    // to return it here if needed
                    return null;
                }

                public override long GetItemId (int position) 
                {
                    return 0;
                }

                public override View GetView(int position, View convertView, ViewGroup parent)
                {
                    GroupListItem messageItem = convertView as GroupListItem;
                    if ( messageItem == null )
                    {
                        messageItem = new GroupListItem( ParentFragment.Activity.BaseContext );
                    }

                    // the list is sorted, so we can safely assume the first entry is the closest group.
                    if ( SelectedIndex == position )
                    {
                        messageItem.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );
                    }
                    else
                    {
                        messageItem.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );
                    }

                    messageItem.ParentAdapter = this;
                    messageItem.Position = position;

                    // if the row should display a group
                    if( position < ParentFragment.GroupEntries.Count )
                    {
                        // ensure the join button is visible, in case this is a reused row
                        // that had it hidden for "tap for 10 more"

                        messageItem.JoinButton.Visibility = ViewStates.Visible;
                        messageItem.Title.Text = ParentFragment.GroupEntries[ position ].Name;

                        // if there's a meeting time set, display it. Otherwise we won't display that row
                        messageItem.MeetingTime.Visibility = ViewStates.Visible;
                        if ( string.IsNullOrEmpty( ParentFragment.GroupEntries[ position ].MeetingTime ) == false )
                        {
                            messageItem.MeetingTime.Text = ParentFragment.GroupEntries[ position ].MeetingTime;
                        }
                        else
                        {
                            messageItem.MeetingTime.Text = ConnectStrings.GroupFinder_ContactForTime;
                        }

                        // if this is the nearest group, add a label saying so
                        messageItem.Distance.Text = string.Format( "{0:##.0} {1}", ParentFragment.GroupEntries[ position ].DistanceFromSource, ConnectStrings.GroupFinder_MilesSuffix );
                        if ( position == 0 )
                        {
                            messageItem.Distance.Text += " " + ConnectStrings.GroupFinder_ClosestTag;
                        }

                        if( string.IsNullOrWhiteSpace( ParentFragment.GroupEntries[ position ].Filters ) == false && 
                            ParentFragment.GroupEntries[ position ].Filters.Contains( PrivateConnectConfig.GroupFinder_Childcare_Filter ) )
						{
							messageItem.Childcare.Text = ConnectStrings.GroupFinder_OffersChildcare;
						}
						else
						{
                            messageItem.Childcare.Text = string.Empty;
						}
                    }
                    // otherwise it's the "10 more" row
                    else
                    {
                        messageItem.Title.Text = ConnectStrings.GroupFinder_10More;
                        messageItem.Distance.Text = string.Empty;
                        messageItem.Childcare.Text = string.Empty;
                        messageItem.JoinButton.Visibility = ViewStates.Gone;
                        messageItem.MeetingTime.Text = string.Empty;
                    }

                    return messageItem;
                }

                public void OnClick( int position, int buttonIndex )
                {
                    ParentFragment.OnClick( position, buttonIndex );
                }

                public void SetSelectedRow( int position )
                {
                    // set the selection index IF it's a row with a group.
                    // don't allow selecting the "10 more" entry.
                    if( position < ParentFragment.GroupEntries.Count )
                    {
                        SelectedIndex = position;

                        // and, inefficiently, force the whole dumb list to redraw.
                        // It's either this or manage all the list view items myself. Just..no.
                        NotifyDataSetChanged( );
                    }
                }
            }

            public class GroupListItem : LinearLayout
            {
                public LinearLayout TitleLayout { get; set; }
                public TextView Title { get; set; }
                public TextView MeetingTime { get; set; }
                public TextView Distance { get; set; }
                public TextView Childcare { get; set; }

                public Button JoinButton { get; set; }

                public GroupArrayAdapter ParentAdapter { get; set; }
                public int Position { get; set; }

                public GroupListItem( Context context ) : base( context )
                {
                    SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );
                    LayoutParameters = new AbsListView.LayoutParams( LayoutParams.MatchParent, LayoutParams.MatchParent );

                    Orientation = Orientation.Vertical;

                    LinearLayout contentLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    contentLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.MatchParent, LayoutParams.MatchParent );
                    contentLayout.Orientation = Orientation.Horizontal;
                    AddView( contentLayout );

                    TitleLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    TitleLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.WrapContent, LayoutParams.WrapContent );
                    TitleLayout.Orientation = Orientation.Vertical;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).Weight = 1;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).LeftMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 15 );
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).TopMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 5 );
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).BottomMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 5 );
                    contentLayout.AddView( TitleLayout );

                    Title = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Bold ), TypefaceStyle.Normal );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Medium_FontSize );
                    Title.SetSingleLine( );
                    Title.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
                    Title.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    TitleLayout.AddView( Title );

                    Typeface buttonFontFace = Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary );

                    JoinButton = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    JoinButton.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)JoinButton.LayoutParameters ).Weight = 0;
                    ( (LinearLayout.LayoutParams)JoinButton.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    JoinButton.SetTypeface( buttonFontFace, TypefaceStyle.Normal );
                    JoinButton.SetTextSize( Android.Util.ComplexUnitType.Dip, PrivateConnectConfig.GroupFinder_Join_IconSize );
                    JoinButton.Text = PrivateConnectConfig.GroupFinder_JoinIcon;
                    JoinButton.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    JoinButton.Background = null;
                    JoinButton.FocusableInTouchMode = false;
                    JoinButton.Focusable = false;
                    contentLayout.AddView( JoinButton );

                    JoinButton.Click += (object sender, EventArgs e ) =>
                        {
                            ParentAdapter.OnClick( Position, 1 );
                        };
                    
                    MeetingTime = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    MeetingTime.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    MeetingTime.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Light ), TypefaceStyle.Normal );
                    MeetingTime.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    MeetingTime.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    TitleLayout.AddView( MeetingTime );

                    Distance  = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Distance.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Distance.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Light ), TypefaceStyle.Normal );
                    Distance.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Distance.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    TitleLayout.AddView( Distance );

					Childcare = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
					Childcare.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
					Childcare.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Light ), TypefaceStyle.Normal );
					Childcare.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
					Childcare.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
					TitleLayout.AddView( Childcare );

                    // add our own custom seperator at the bottom
                    View seperator = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    seperator.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, 0 );
                    seperator.LayoutParameters.Height = 2;
                    seperator.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );
                    AddView( seperator );
                }
            }

            public class GroupFinderFragment : TaskFragment, IOnMapReadyCallback, TextView.IOnEditorActionListener, Android.Gms.Maps.GoogleMap.IOnMarkerClickListener
            {
                ListView ListView { get; set; }

                public Android.Gms.Maps.MapView MapView { get; set; }
                public GoogleMap Map { get; set; }
                public Button SearchAddressButton { get; set; }

                UIBlockerView BlockerView { get; set; }

                /// <summary>
                /// If true, OnCreateView will automatically show the search.
                /// </summary>
                public bool ShowSearchOnAppear { get; set; }

                LinearLayout SearchLayout { get; set; }
                public TextView SearchResultNeighborhood { get; set; }
                public TextView SearchResultPrefix { get; set; }

                View Seperator { get; set; }
                public List<MobileAppApi.GroupSearchResult> GroupEntries { get; set; }
                public List<Android.Gms.Maps.Model.Marker> MarkerList { get; set; }
                public MobileAppApi.GroupSearchResult SourceLocation { get; set; }

                // store the values they type in so that if they leave the page and return, we can re-populate them.
                string StreetValue { get; set; }
                string CityValue { get; set; }
                string StateValue { get; set; }
                string ZipValue { get; set; }

                UIGroupFinderSearch SearchPage { get; set; }

                public bool OnEditorAction(TextView v, Android.Views.InputMethods.ImeAction actionId, KeyEvent keyEvent)
                {
                    // don't allow searching until the map is valid (which it should be by now)
                    if ( Map != null )
                    {
                        SearchPage.ShouldReturn( );
                    }
                    return true;
                }

                public override bool OnTouch(View v, MotionEvent e)
                {
                    return base.OnTouch(v, e);
                }

                public GroupFinderFragment( ) : base( )
                {
                }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    if (container == null)
                    {
                        // Currently in a layout without a container, so no reason to create our view.
                        return null;
                    }

                    GroupEntries = new List<MobileAppApi.GroupSearchResult>();
                    MarkerList = new List<Android.Gms.Maps.Model.Marker>();
                    SourceLocation = new MobileAppApi.GroupSearchResult();

                    // limit the address to 90% of the screen so it doesn't conflict with the progress bar.
                    Point displaySize = new Point( );
                    Activity.WindowManager.DefaultDisplay.GetSize( displaySize );
                    //float fixedWidth = displaySize.X / 4.0f;

                    // catch any exceptions thrown, as they'll be related to no map API key
                    try
                    {
                        MapView = new Android.Gms.Maps.MapView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                        MapView.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
                        MapView.LayoutParameters.Height = (int) (displaySize.Y * .50f);
                        MapView.GetMapAsync( this );
                        MapView.SetBackgroundColor( Color.Black );

                        MapView.OnCreate( savedInstanceState );
                    }
                    catch
                    {
                        MapView = null;
                        Rock.Mobile.Util.Debug.WriteLine( "GOOGLE MAPS: Unable to create. Verify you have a valid API KEY." );
                    }

                    NumRequestedGroups = 10;


                    SearchAddressButton = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    SearchAddressButton.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    ControlStyling.StyleButton( SearchAddressButton, ConnectStrings.GroupFinder_SearchButtonLabel, ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
                    SearchAddressButton.Click += (object sender, EventArgs e ) =>
                    {
                            SearchPage.Show( );
                    };


                    // setup the linear layout containing the "Your Neighborhood is: Horizon" text
                    SearchLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    SearchLayout.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    SearchLayout.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );
                    SearchLayout.SetGravity( GravityFlags.Center );

                    SearchResultPrefix = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    SearchResultPrefix.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    SearchResultPrefix.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Regular ), TypefaceStyle.Normal );
                    SearchResultPrefix.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    SearchResultPrefix.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    SearchResultPrefix.Text = ConnectStrings.GroupFinder_NoGroupsFound;

                    SearchResultNeighborhood = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    SearchResultNeighborhood.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    SearchResultNeighborhood.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Regular ), TypefaceStyle.Normal );
                    SearchResultNeighborhood.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    SearchResultNeighborhood.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                    SearchResultNeighborhood.Text = "";


                    Seperator = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Seperator.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, 0 );
                    Seperator.LayoutParameters.Height = 2;
                    Seperator.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

                    ListView = new ListView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    ListView.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
                    ListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e ) =>
                        {
                            OnClick( e.Position, 0 );
                        };
                    ListView.SetOnTouchListener( this );
                    ListView.Adapter = new GroupArrayAdapter( this );

                    View view = inflater.Inflate(Resource.Layout.Connect_GroupFinder, container, false);
                    view.SetOnTouchListener( this );

                    view.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

                    LinearLayout groupLayout = view.FindViewById<LinearLayout>( Resource.Id.groupFrame ) as LinearLayout;

                    // setup the address layout, which has the address text, padding, and finally the progress bar.
                    if ( MapView != null )
                    {
                        ( (LinearLayout)groupLayout ).AddView( MapView );
                    }

                    ((LinearLayout)groupLayout).AddView( SearchAddressButton );

                    ((LinearLayout)groupLayout).AddView( SearchLayout );
                    ((LinearLayout)SearchLayout).AddView( SearchResultPrefix );
                    ((LinearLayout)SearchLayout).AddView( SearchResultNeighborhood );

                    ((LinearLayout)groupLayout).AddView( Seperator );
                    ((LinearLayout)groupLayout).AddView( ListView );

                    BlockerView = new UIBlockerView( view, new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetCurrentContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );

                    SearchPage = new UIGroupFinderSearch();
                    SearchPage.Create( view, new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetCurrentContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ), 
                        // Search Neighborhood Groups
                        delegate
                        {
                            SearchPage.Hide( true );
                            GetInitialGroups( PrivateGeneralConfig.GroupType_Neighborhood_GroupId, SearchPage.Street.Text, SearchPage.City.Text, SearchPage.State.Text, SearchPage.ZipCode.Text );
                        },

                        // Search Next Gen Groups
                        delegate
                        {
                            SearchPage.Hide( true );
                            GetInitialGroups( PrivateGeneralConfig.GroupType_NextGenGroupId, SearchPage.Street.Text, SearchPage.City.Text, SearchPage.State.Text, SearchPage.ZipCode.Text );
                        },
                                     
                        // Search Young Adult Groups
                        delegate
                        {
                            SearchPage.Hide( true );
                            GetInitialGroups( PrivateGeneralConfig.GroupType_YoungAdultsGroupId, SearchPage.Street.Text, SearchPage.City.Text, SearchPage.State.Text, SearchPage.ZipCode.Text );
                        });
                    SearchPage.SetTitle( ConnectStrings.GroupFinder_SearchPageHeader, ConnectStrings.GroupFinder_SearchPageDetails );
                    SearchPage.LayoutChanged( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetCurrentContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );
                    SearchPage.Hide( false );

                    // if we should automatically show the search page...
                    if ( ShowSearchOnAppear == true )
                    {
                        // don't allow them to tap the address button until we reveal the search page.
                        SearchAddressButton.Enabled = false;

                        // wait a couple seconds before revealing the search page.
                        System.Timers.Timer timer = new System.Timers.Timer();
                        timer.AutoReset = false;
                        timer.Interval = 1000;
                        timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                        {
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                {
                                    SearchAddressButton.Enabled = true;
                                    SearchPage.Show( );
                                } );
                        };
                        timer.Start( );
                    }
                    else
                    {
                        // otherwise, just allow the seach button
                        SearchAddressButton.Enabled = true;
                    }

                    // hook into the search page as its listener
                    ((View)SearchPage.View.PlatformNativeObject).SetOnTouchListener( this );
                    ((EditText)SearchPage.Street.PlatformNativeObject).SetOnEditorActionListener( this );
                    ((EditText)SearchPage.City.PlatformNativeObject).SetOnEditorActionListener( this );
                    ((EditText)SearchPage.State.PlatformNativeObject).SetOnEditorActionListener( this );
                    ((EditText)SearchPage.ZipCode.PlatformNativeObject).SetOnEditorActionListener( this );
                    return view;
                }

                public void OnMapReady( GoogleMap map )
                {
                    // sanity checks in case loading the map fails (Google Play can error)
                    if ( map != null )
                    {
                        Map = map;

                        // because google can actually give me a valid map that isn't actually ready.
                        try
                        {
                            // For privacy, turn off the map toolbar, which prevents people from getting specific driving directions
                            // to neighborhood group houses.
                            Map.UiSettings.MapToolbarEnabled = false;
                            Map.SetOnMarkerClickListener( this );

                            // set the map to a default position
                            // additionally, set the default position for the map to whatever specified area.
                            Android.Gms.Maps.Model.LatLng defaultPos = new Android.Gms.Maps.Model.LatLng( ConnectConfig.GroupFinder_DefaultLatitude, ConnectConfig.GroupFinder_DefaultLongitude );

                            CameraUpdate camPos = CameraUpdateFactory.NewLatLngZoom( defaultPos, ConnectConfig.GroupFinder_DefaultScale_Android );
                            map.MoveCamera( camPos );

                            // see if there's an address for this person that we can automatically use.
                            if ( RockMobileUser.Instance.HasFullAddress( ) == true )
                            {
                                // if they don't already have any value, use their address
                                if ( string.IsNullOrEmpty( StreetValue ) == true &&
                                     string.IsNullOrEmpty( CityValue ) == true &&
                                     string.IsNullOrEmpty( StateValue ) == true &&
                                     string.IsNullOrEmpty( ZipValue ) == true )
                                {
                                    SearchPage.SetAddress( RockMobileUser.Instance.Street1( ), RockMobileUser.Instance.City( ), RockMobileUser.Instance.State( ), RockMobileUser.Instance.Zip( ) );    
                                }
                                else
                                {
                                    // otherwise use what they last had.
                                    SearchPage.SetAddress( StreetValue, CityValue, StateValue, ZipValue );
                                }
                            }
                            else
                            {
                                // otherwise, if there are values from a previous session, use those.
                                SearchPage.SetAddress( StreetValue, CityValue, string.IsNullOrEmpty( StateValue ) ? ConnectStrings.GroupFinder_DefaultState : StateValue, ZipValue );
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                public bool OnMarkerClick( Android.Gms.Maps.Model.Marker marker )
                {
                    // select the appropriate row
                    int position = MapMarkerToRow( marker );

                    ListView.SmoothScrollToPosition( position );
                    ( ListView.Adapter as GroupArrayAdapter ).SetSelectedRow( position );

                    return false;
                }

                int MapMarkerToRow( Android.Gms.Maps.Model.Marker marker )
                {
                    // given a map marker, get the index of it in the row list
                    for ( int i = 0; i < GroupEntries.Count; i++ )
                    {
                        double currLatitude = GroupEntries[ i ].Latitude;
                        double currLongitude = GroupEntries[ i ].Longitude;

                        if ( marker.Position.Latitude == currLatitude &&
                             marker.Position.Longitude == currLongitude )
                        {
                            return i;
                        }
                    }

                    return -1;
                }

                Android.Gms.Maps.Model.Marker RowToMapMarker( int row )
                {
                    // setup the row marker coordinates
                    double rowLatitude = GroupEntries[ row ].Latitude;
                    double rowLongitude = GroupEntries[ row ].Longitude;

                    // go thru each marker and find the match, and then return it
                    foreach ( Android.Gms.Maps.Model.Marker marker in MarkerList )
                    {
                        if ( marker.Position.Latitude == rowLatitude &&
                             marker.Position.Longitude == rowLongitude )
                        {
                            return marker;
                        }
                    }

                    return null;
                }

                public override void OnSaveInstanceState(Bundle outState)
                {
                    base.OnSaveInstanceState(outState);

                    if ( MapView != null )
                    {
                        MapView.OnSaveInstanceState( outState );
                    }
                }

                public override void OnLowMemory()
                {
                    base.OnLowMemory();

                    if ( MapView != null )
                    {
                        MapView.OnLowMemory( );
                    }
                }

                public void SetSearchAddress( string street, string city, string state, string zip )
                {
                    StreetValue = street;
                    CityValue = city;
                    StateValue = state;
                    ZipValue = zip;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    if ( MapView != null )
                    {
                        MapView.OnResume( );
                    }

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    // restore saved values
                    SearchPage.SetAddress( StreetValue, CityValue, StateValue, ZipValue );
                }

                public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
                {
                    base.OnConfigurationChanged(newConfig);

                    Point displaySize = new Point( );
                    Activity.WindowManager.DefaultDisplay.GetSize( displaySize );

                    if ( MapView != null )
                    {
                        MapView.LayoutParameters.Height = (int)( displaySize.Y * .50f );
                    }

                    BlockerView.SetBounds( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetCurrentContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );

                    SearchPage.LayoutChanged( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetCurrentContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );
                }

                public override void OnDestroy()
                {
                    base.OnDestroy();

                    if ( MapView != null )
                    {
                        MapView.OnDestroy( );
                    }
                }

                public void OnClick( int position, int buttonIndex )
                {
                    if( position < GroupEntries.Count )
                    {
                        if ( buttonIndex == 0 )
                        {
                            // select the row
                            ( ListView.Adapter as GroupArrayAdapter ).SetSelectedRow( position );

                            // scroll it into view
                            ListView.SmoothScrollToPosition( position );

                            // hide all other marker windows (if showing)
                            // go thru each marker and find the match, and then return it
                            foreach ( Android.Gms.Maps.Model.Marker currMarker in MarkerList )
                            {
                                currMarker.HideInfoWindow( );
                            }

                            // validate the map because Google Play can error
                            if ( Map != null )
                            {
                                // center that map marker
                                Android.Gms.Maps.Model.Marker marker = RowToMapMarker( position );
                                marker.ShowInfoWindow( );

                                Android.Gms.Maps.Model.LatLng centerMarker = new Android.Gms.Maps.Model.LatLng( marker.Position.Latitude, marker.Position.Longitude );

                                CameraUpdate camPos = CameraUpdateFactory.NewLatLngZoom( centerMarker, Map.CameraPosition.Zoom );
                                Map.AnimateCamera( camPos, 250, null );
                            }
                        }
                        else if ( buttonIndex == 1 )
                        {
                            // Ok! notify the parent they tapped Join, and it will launch the
                            // join group fragment! It's MARCH, FRIDAY THE 13th!!!! OH NOOOO!!!!
                            ParentTask.OnClick( this, position, GroupEntries[ position ] );
                            Rock.Mobile.Util.Debug.WriteLine( string.Format( "Join neighborhood group in row {0}", position ) );
                        }
                    }
                    else
                    {
                        GetAdditionalGroups( );
                    }
                }

                public override void OnPause()
                {
                    base.OnPause();

                    if ( MapView != null )
                    {
                        MapView.OnPause( );
                    }

                    StreetValue = SearchPage.Street.Text;
                    CityValue = SearchPage.City.Text;
                    StateValue = SearchPage.State.Text;
                    ZipValue = SearchPage.ZipCode.Text;
                }

                void UpdateMap( bool result )
                {
                    if ( GroupEntries.Count > 0 )
                    {
                        // update our list and display
                        SearchResultPrefix.Text = ConnectStrings.GroupFinder_Neighborhood;

                        ( ListView.Adapter as GroupArrayAdapter ).SetSelectedRow( 0 );

                        // for the map, ensure it's valid, because Google Play can fail
                        if( Map != null )
                        {
                            Map.Clear( );
                            MarkerList.Clear( );
                        
                            Android.Gms.Maps.Model.LatLngBounds.Builder builder = new Android.Gms.Maps.Model.LatLngBounds.Builder();

                            // add the source position
                            Android.Gms.Maps.Model.MarkerOptions markerOptions = new Android.Gms.Maps.Model.MarkerOptions();
                            Android.Gms.Maps.Model.LatLng pos = new Android.Gms.Maps.Model.LatLng( SourceLocation.Latitude, SourceLocation.Longitude );
                            markerOptions.SetPosition( pos );
                            markerOptions.InvokeIcon( BitmapDescriptorFactory.DefaultMarker( BitmapDescriptorFactory.HueGreen ) );
                            builder.Include( pos );

                            Android.Gms.Maps.Model.Marker marker = Map.AddMarker( markerOptions );
                            MarkerList.Add( marker );

                            for ( int i = 0; i < GroupEntries.Count; i++ )
                            {
                                // add the positions to the map
                                markerOptions = new Android.Gms.Maps.Model.MarkerOptions();
                                pos = new Android.Gms.Maps.Model.LatLng( GroupEntries[ i ].Latitude, GroupEntries[ i ].Longitude );
                                markerOptions.SetPosition( pos );
                                markerOptions.SetTitle( GroupEntries[ i ].Name );
                                markerOptions.SetSnippet( string.Format( "{0:##.0} {1}", GroupEntries[ i ].DistanceFromSource, ConnectStrings.GroupFinder_MilesSuffix ) );

                                builder.Include( pos );

                                marker = Map.AddMarker( markerOptions );
                                MarkerList.Add( marker );
                            }

                            Android.Gms.Maps.Model.LatLngBounds bounds = builder.Build( );

                            int paddingInPixels = Math.Min( View.Width, (int)(View.Height * .1f) );

                            CameraUpdate camPos = CameraUpdateFactory.NewLatLngBounds( bounds, paddingInPixels );
                            Map.AnimateCamera( camPos );

                            // show the info window for the first (closest) group
                            MarkerList[ 1 ].ShowInfoWindow( );
                        }
                    }
                    else
                    {
                        if ( result == true )
                        {
                            // send the analytic and update our list
                            SearchResultPrefix.Text = ConnectStrings.GroupFinder_NoGroupsFound;
                            SearchResultNeighborhood.Text = string.Empty;

                            ( ListView.Adapter as GroupArrayAdapter ).SetSelectedRow( -1 );

                            // validate the map before using. Google Play can error
                            if ( Map != null )
                            {
                                // no groups found, so move the camera to the default position
                                Android.Gms.Maps.Model.LatLng defaultPos = new Android.Gms.Maps.Model.LatLng( ConnectConfig.GroupFinder_DefaultLatitude, ConnectConfig.GroupFinder_DefaultLongitude );
                                CameraUpdate camPos = CameraUpdateFactory.NewLatLngZoom( defaultPos, ConnectConfig.GroupFinder_DefaultScale_Android );
                                Map.AnimateCamera( camPos );
                            }
                        }
                        else
                        {
                            // there was actually an error. Let them know.
                            SearchResultPrefix.Text = ConnectStrings.GroupFinder_NetworkError;
                            SearchResultNeighborhood.Text = string.Empty;
                        }
                    }
                }

                bool RetrievingGroups { get; set; }
                int GroupTypeId { get; set; }
                int CurrGroupIndex { get; set; }
                int NumRequestedGroups { get; set; }

                void GetInitialGroups( int groupTypeId, string streetValue, string cityValue, string stateValue, string zipValue )
                {
                    if ( RetrievingGroups == false )
                    {
                        // since this is the first search for the new address, get initial values
                        // so that if they leave the page and return, we can re-populate them.
                        StreetValue = SearchPage.Street.Text;
                        CityValue = SearchPage.City.Text;
                        StateValue = SearchPage.State.Text;
                        ZipValue = SearchPage.ZipCode.Text;

                        RetrievingGroups = true;

                        BlockerView.Show( delegate
                            {
                                GroupTypeId = groupTypeId;
                                CurrGroupIndex = 0;

                                GroupFinder.GetGroups( groupTypeId, streetValue, cityValue, stateValue, zipValue, CurrGroupIndex, NumRequestedGroups,
                                    delegate( MobileAppApi.GroupSearchResult sourceLocation, List<MobileAppApi.GroupSearchResult> groupEntries, bool result )
                                    {
                                        BlockerView.Hide( delegate
                                            {
                                                RetrievingGroups = false;

                                                SourceLocation = sourceLocation;

                                                GroupEntries = groupEntries;

                                                UpdateMap( result );

                                                // if the result was valid
                                                string address = StreetValue + " " + CityValue + ", " + StateValue + ", " + ZipValue;
                                                if ( result )
                                                {
                                                    // take the lesser of the two. The number we requested, or the amount returned, because 
                                                    // it's possible there weren't as many as we requested.
                                                    CurrGroupIndex = Math.Min( NumRequestedGroups, groupEntries.Count );
                                                    
                                                    // record an analytic that they searched
                                                    GroupFinderAnalytic.Instance.Trigger( GroupFinderAnalytic.Location, address );
                                                    //GroupFinderAnalytic.Instance.Trigger( GroupFinderAnalytic.Neighborhood, GroupEntries[ 0 ].NeighborhoodArea );
                                                }
                                                else
                                                {
                                                    // record an analytic that this address failed
                                                    GroupFinderAnalytic.Instance.Trigger( GroupFinderAnalytic.OutOfBounds, address );
                                                }
                                            });
                                    } );
                            } );
                    }
                }

                void GetAdditionalGroups( )
                {
                    if ( RetrievingGroups == false )
                    {
                        RetrievingGroups = true;

                        BlockerView.Show( delegate
                            {
                                GroupFinder.GetGroups( GroupTypeId, StreetValue, CityValue, StateValue, ZipValue, CurrGroupIndex, NumRequestedGroups,
                                    delegate( MobileAppApi.GroupSearchResult sourceLocation, List<MobileAppApi.GroupSearchResult> groupEntries, bool result )
                                    {
                                        BlockerView.Hide( delegate
                                            {
                                                RetrievingGroups = false;

                                                // for additional groups, only take action if we got something back.
                                                if( result )
                                                {
                                                    // increment our index to the next set, or the end of the list, whichever is less
                                                    // this will ensure when we hit the end of the list, CurrGroupIndex reflects that.
                                                    CurrGroupIndex += Math.Min( groupEntries.Count, NumRequestedGroups );

                                                    GroupEntries.AddRange( groupEntries );

                                                    UpdateMap( true );
                                                }
                                            });
                                    });
                            });
                    }
                }
            }
        }
    }
}

