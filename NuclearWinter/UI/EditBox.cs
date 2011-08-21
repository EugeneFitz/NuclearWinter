﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace NuclearWinter.UI
{
    /*
     * An EditBox to enter some text
     */
    public class EditBox: Widget
    {
        SpriteFont mFont;
        public SpriteFont       Font
        {
            get { return mFont; }
            
            set
            {
                mFont = value;
                UpdateContentSize();
            }
        }

        string  mstrText;
        Point   mpTextPosition;

        int     miCaretOffset;
        int     CaretOffset {
            get { return miCaretOffset; }

            set {
                miCaretOffset = (int)MathHelper.Clamp( value, 0, mstrText.Length );
                miCaretX = miCaretOffset > 0 ? (int)mFont.MeasureString( mstrDisplayedText.Substring( 0, miCaretOffset ) ).X : 0;
            }
        }
        int     miCaretX;

        bool    mbIsHovered;
        float   mfTimer;

        bool    mbHideContent;

        public bool HideContent {
            get { return mbHideContent; }
            set {
                mbHideContent = value;
                UpdateContentSize();
            }
        }

        public string Text
        {
            get { return mstrText; }
            set
            {
                mstrText = value;
                CaretOffset = Math.Min( miCaretOffset, mstrText.Length );

                UpdateContentSize();
            }
        }

        string                  mstrDisplayedText;

        public override bool CanFocus { get { return true; } }

        public Action<EditBox>  ValidateHandler;

        //----------------------------------------------------------------------
        public EditBox( Screen _screen, string _strText )
        : base( _screen )
        {
            mstrText    = _strText;
            mFont       = _screen.Style.MediumFont;
            mPadding    = new Box( 10 );

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public EditBox( Screen _screen )
        : this( _screen, "" )
        {
        }

        //----------------------------------------------------------------------
        protected override void UpdateContentSize()
        {
            mstrDisplayedText = (!mbHideContent) ? Text : "".PadLeft( Text.Length, '●' );

            ContentWidth = (int)Font.MeasureString( mstrDisplayedText ).X + Padding.Left + Padding.Right;
            ContentHeight = (int)( Font.LineSpacing * 0.9f ) + Padding.Top + Padding.Bottom;
        }

        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle? _rect )
        {
            if( _rect.HasValue )
            {
                Position = _rect.Value.Location;
                Size = new Point( _rect.Value.Width, _rect.Value.Height );
            }

            HitBox = new Rectangle( Position.X, Position.Y, Size.X, Size.Y );

            Point pCenter = new Point( Position.X + Size.X / 2, Position.Y + Size.Y / 2 );

            mpTextPosition = new Point(
                Position.X + Padding.Left,
                pCenter.Y - ContentHeight / 2 + Padding.Top
            );
        }

        //----------------------------------------------------------------------
        public override void OnMouseEnter( Point _hitPoint )
        {
            base.OnMouseEnter( _hitPoint );
            mbIsHovered = true;
        }

        public override void OnMouseOut( Point _hitPoint )
        {
            base.OnMouseOut( _hitPoint );
            mbIsHovered = false;
        }

        //----------------------------------------------------------------------
        public override void OnMouseDown( Point _hitPoint )
        {
            base.OnMouseDown( _hitPoint );

            Screen.Focus( this );
            OnActivateDown();
        }

        public override void OnMouseUp( Point _hitPoint )
        {
            if( HitTest( _hitPoint ) == this )
            {
                OnActivateUp();
            }
            else
            {
            }
        }

        //----------------------------------------------------------------------
        public override void OnKeyPress( Keys _key )
        {
            switch( _key )
            {
                case Keys.Enter:
                    if( ValidateHandler != null ) ValidateHandler( this );
                    break;
                case Keys.Left:
                    CaretOffset--;
                    break;
                case Keys.Right:
                    CaretOffset++;
                    break;
                case Keys.Back:
                    if( Text.Length > 0 && CaretOffset > 0 )
                    {
                        Text = Text.Substring( 0, CaretOffset - 1 ) + Text.Substring( CaretOffset, Text.Length - CaretOffset );
                        //CaretOffset--;
                    }
                    break;
                case Keys.Space:
                    Text = Text.Substring( 0, CaretOffset ) + " " + Text.Substring( CaretOffset, Text.Length - CaretOffset );
                    CaretOffset++;
                    break;
                default:
                    if( _key >= Keys.A && _key <= Keys.Z )
                    {
                        string key = _key.ToString();
                        bool bShift = Screen.Game.GamePadMgr.KeyboardState.Native.IsKeyDown( Keys.LeftShift ) || Screen.Game.GamePadMgr.KeyboardState.Native.IsKeyDown( Keys.RightShift );

                        Text = Text.Substring( 0, CaretOffset ) + ( bShift ? key : key.ToLower() ) + Text.Substring( CaretOffset, Text.Length - CaretOffset );
                        CaretOffset++;
                    }
                    break;
            }
        }

        //----------------------------------------------------------------------
        public override void OnFocus()
        {
            Screen.AddWidgetToUpdateList( this );
        }

        public override bool Update( float _fElapsedTime )
        {
            if( ! HasFocus )
            {
                mfTimer = 0f;
                return false;
            }

            mfTimer += _fElapsedTime;

            return true;
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            Screen.DrawBox( Screen.Style.ButtonFrameDown, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), 30, Color.White );

            if( mbIsHovered )
            {
                Screen.DrawBox( Screen.Style.ButtonFramePressed, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), 30, Color.White );
            }

            Screen.Game.DrawBlurredText( Screen.Style.BlurRadius, mFont, mstrDisplayedText, new Vector2( mpTextPosition.X, mpTextPosition.Y ), Color.White );

            const float fBlinkInterval = 0.3f;

            if( HasFocus && mfTimer % (fBlinkInterval * 2) < fBlinkInterval )
            {
                Screen.Game.SpriteBatch.Draw( Screen.Game.WhitePixelTex, new Rectangle( mpTextPosition.X + miCaretX, Position.Y + Padding.Top, 3, Size.Y - Padding.Vertical ), Color.White );
            }
        }
    }
}
