using System;
using System.Xml;
using Rock.Mobile.UI;
using MobileApp.Shared.Notes.Model;
using System.Collections.Generic;
using System.Drawing;

namespace MobileApp
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// A text label that is hidden until a user taps on it.
            /// </summary>
            public class RevealBox : NoteText
            {
                /// <summary>
                /// True when the text is revealed, false when it isn't.
                /// We can't just rely on the fade value because it's a bit different across android / ios.
                /// </summary>
                /// <value><c>true</c> if revealed; otherwise, <c>false</c>.</value>
                public bool Revealed { get; protected set; }

                public string Text { get { return PlatformLabel != null ? PlatformLabel.Text : ""; } }

                public RevealBox( CreateParams parentParams, string revealText ) : base( )
                {
                    // don't call the base constructor that reads. we'll do the reading here.
                    base.Initialize( );

                    Revealed = false;

                    PlatformLabel = PlatformLabel.CreateRevealLabel( );
                    PlatformLabel.SetFade( 0.0f );

                    // take our parent's style, and for anything not set by them use the default.
                    mStyle = parentParams.Style;
                    Styles.Style.MergeStyleAttributesWithDefaults( ref mStyle, ref ControlStyles.mText );
                    
                    // create the font that either we or our parent defined
                    PlatformLabel.SetFont( mStyle.mFont.mName, mStyle.mFont.mSize.Value );
                    PlatformLabel.TextColor = mStyle.mFont.mColor.Value;

                    // check for border styling
                    if ( mStyle.mBorderColor.HasValue )
                    {
                        PlatformLabel.BorderColor = mStyle.mBorderColor.Value;
                    }

                    if( mStyle.mBorderRadius.HasValue )
                    {
                        PlatformLabel.CornerRadius = mStyle.mBorderRadius.Value;
                    }

                    if( mStyle.mBorderWidth.HasValue )
                    {
                        PlatformLabel.BorderWidth = mStyle.mBorderWidth.Value;
                    }

                    if( mStyle.mBackgroundColor.HasValue )
                    {
                        PlatformLabel.BackgroundColor = mStyle.mBackgroundColor.Value;
                    }

                    // set the dimensions and position
                    RectangleF bounds = new RectangleF( );
                    
                    if( bounds.Width == 0 )
                    {
                        // always take the available width, in case this control
                        // is specified to be offset relative to its parent
                        bounds.Width = parentParams.Width - bounds.X;
                    }
                    PlatformLabel.Bounds = bounds;
                    
                    // ensure that there's something IN the reveal text. We cannot render blank, because the bitmap mask can't be 0 width / height
                    if( string.IsNullOrWhiteSpace( revealText ) ) throw new Exception( "RevealBox text cannot be blank." );
                    
                    // adjust the text
                    switch( mStyle.mTextCase )
                    {
                        case Styles.TextCase.Upper:
                        {
                            revealText = revealText.ToUpper( );
                            break;
                        }

                        case Styles.TextCase.Lower:
                        {
                            revealText = revealText.ToLower( );
                            break;
                        }
                    }

                    SetText( revealText );

                    PlatformLabel.Position = new PointF( bounds.X, bounds.Y );
                }

                public RevealBox( CreateParams parentParams, XmlReader reader ) : base( )
                {
                    // don't call the base constructor that reads. we'll do the reading here.
                    base.Initialize( );

                    Revealed = false;

                    PlatformLabel = PlatformLabel.CreateRevealLabel( );
                    PlatformLabel.SetFade( 0.0f );

                    // Always get our style first
                    mStyle = parentParams.Style;
                    Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mRevealBox );

                    // check for attributes we support
                    RectangleF bounds = new RectangleF( );
                    SizeF parentSize = new SizeF( parentParams.Width, parentParams.Height );
                    ParseCommonAttribs( reader, ref parentSize, ref bounds );

                    // Get margins and padding
                    RectangleF padding;
                    RectangleF margin;
                    GetMarginsAndPadding( ref mStyle, ref parentSize, ref bounds, out margin, out padding );

                    // apply margins to as much of the bounds as we can (bottom must be done by our parent container)
                    ApplyImmediateMargins( ref bounds, ref margin, ref parentSize );
                    Margin = margin;

                    // create the font that either we or our parent defined
                    PlatformLabel.SetFont( mStyle.mFont.mName, mStyle.mFont.mSize.Value );
                    PlatformLabel.TextColor = mStyle.mFont.mColor.Value;

                    // check for border styling
                    if ( mStyle.mBorderColor.HasValue )
                    {
                        PlatformLabel.BorderColor = mStyle.mBorderColor.Value;
                    }

                    if( mStyle.mBorderRadius.HasValue )
                    {
                        PlatformLabel.CornerRadius = mStyle.mBorderRadius.Value;
                    }

                    if( mStyle.mBorderWidth.HasValue )
                    {
                        PlatformLabel.BorderWidth = mStyle.mBorderWidth.Value;
                    }

                    if( mStyle.mBackgroundColor.HasValue )
                    {
                        PlatformLabel.BackgroundColor = mStyle.mBackgroundColor.Value;
                    }

                    // parse the stream
                    string revealText = "";
                    if( reader.IsEmptyElement == false )
                    {
                        bool finishedLabel = false;
                        while( finishedLabel == false && reader.Read( ) )
                        {
                            switch( reader.NodeType )
                            {
                                case XmlNodeType.Text:
                                {
                                    // support text as embedded in the element
                                    revealText = reader.Value.Replace( System.Environment.NewLine, "" );

                                    break;
                                }

                                case XmlNodeType.EndElement:
                                {
                                    // if we hit the end of our label, we're done.
                                    //if( reader.Name == "RevealBox" || reader.Name == "RB" )
                                    if( ElementTagMatches( reader.Name ) )
                                    {
                                        finishedLabel = true;
                                    }

                                    break;
                                }
                            }
                        }
                    }

                    // ensure that there's something IN the reveal text. We cannot render blank, because the bitmap mask can't be 0 width / height
                    if( string.IsNullOrWhiteSpace( revealText ) ) throw new Exception( "RevealBox text cannot be blank." );

                    // adjust the text
                    switch( mStyle.mTextCase )
                    {
                        case Styles.TextCase.Upper:
                        {
                            revealText = revealText.ToUpper( );
                            break;
                        }

                        case Styles.TextCase.Lower:
                        {
                            revealText = revealText.ToLower( );
                            break;
                        }
                    }

                    SetText( revealText );

                    PlatformLabel.Position = new PointF( bounds.X, bounds.Y );
                }

                public RectangleF GetHitTarget( )
                {
                    // create an expanded bounding box and see if the touch fell within that.
                    // we expand the height by 50% in both directions
                    return new RectangleF( PlatformLabel.Frame.Left, 
                                           PlatformLabel.Frame.Top - (PlatformLabel.Bounds.Height / 2), 
                                           PlatformLabel.Bounds.Right, 
                                           PlatformLabel.Bounds.Bottom + PlatformLabel.Bounds.Height );
                }

                public override IUIControl TouchesEnded( PointF touch )
                {
                    RectangleF boundingBox = GetHitTarget( );

                    if( boundingBox.Contains( touch ) )
                    {
                        Revealed = !Revealed;

                        float targetFade = 1.0f - PlatformLabel.GetFade( );

                        PlatformLabel.AnimateToFade( targetFade );
                        return this;
                    }

                    return null;
                }

                public NoteState.RevealBoxState GetState( )
                {
                    return new NoteState.RevealBoxState( Revealed );
                }

                public override PlatformBaseUI GetPlatformControl()
                {
                    return PlatformLabel;
                }

                public override void BuildHTMLContent( ref string htmlStream, ref string textStream, List<IUIControl> userNotes )
                {
                    htmlStream += string.Format( "<U>{0}</U>", PlatformLabel.Text );
                    textStream += PlatformLabel.Text;
                }

                public static new bool ElementTagMatches(string elementTag)
                {
                    if ( elementTag == "RB" || elementTag == "RevealBox" )
                    {
                        return true;
                    }
                    return false;
                }

                public void SetRevealed( bool revealed )
                {
                    Revealed = revealed;
                    PlatformLabel.SetFade( revealed == true ? 1.0f : 0.0f );
                }
            }
        }
    }
}
