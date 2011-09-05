﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NuclearWinter.Animation;

namespace NuclearWinter.UI
{
    public class DropDownBox: Widget
    {
        List<string>                    mlValues;
        public int                      SelectedValueIndex      { get; private set; }

        bool                            mbIsHovered;
        int                             miHoveredValueIndex;

        AnimatedValue                   mPressedAnim;
        bool                            mbIsOpen;
        bool                            mbIsPressed;

        public Texture2D                ButtonFrame             { get; set; }
        public Texture2D                ButtonFrameDown         { get; set; }
        public Texture2D                ButtonFrameHover        { get; set; }
        public Texture2D                ButtonFramePressed      { get; set; }

        public Action<DropDownBox>      ChangeHandler;

        Rectangle                       mDropDownHitBox;
        const int                       siLineHeight = 50;
        const int                       siMaxLineDisplayed = 3;
        int                             miScrollOffset;

        //----------------------------------------------------------------------
        public DropDownBox( Screen _screen, List<string> _lValues, int _iInitialValueIndex )
        : base( _screen )
        {
            mlValues = _lValues;
            SelectedValueIndex = _iInitialValueIndex;
            miScrollOffset = Math.Max( 0, Math.Min( SelectedValueIndex, mlValues.Count - siMaxLineDisplayed ) );

            Padding = new Box( 20 );

            mPressedAnim    = new SmoothValue( 1f, 0f, 0.2f );
            mPressedAnim.SetTime( mPressedAnim.Duration );

            ButtonFrame         = Screen.Style.ButtonFrame;
            ButtonFrameDown     = Screen.Style.ButtonFrameDown;
            ButtonFrameHover    = Screen.Style.ButtonFrameHover;
            ButtonFramePressed  = Screen.Style.ButtonFramePressed;

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
            UIFont uiFont = Screen.Style.MediumFont;

            int iMaxWidth = 0;
            foreach( string _strValue in mlValues )
            {
                iMaxWidth = Math.Max( iMaxWidth, (int)uiFont.MeasureString( _strValue ).X );
            }

            ContentWidth    = iMaxWidth + Padding.Left + Padding.Right + Screen.Style.DropDownArrow.Width;
            ContentHeight   = (int)( uiFont.LineSpacing * 0.9f ) + Padding.Top + Padding.Bottom;

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );

            Point pCenter = new Point( Position.X + Size.X / 2, Position.Y + Size.Y / 2 );

            HitBox = new Rectangle( Position.X, Position.Y, Size.X, Size.Y );
            /*
            HitBox = new Rectangle(
                pCenter.X - ContentWidth / 2,
                pCenter.Y - ContentHeight / 2,
                ContentWidth,
                ContentHeight
            );
            */

            mDropDownHitBox = new Rectangle(
                HitBox.Left, HitBox.Bottom,
                HitBox.Width, Padding.Vertical + Math.Min( siMaxLineDisplayed, mlValues.Count ) * siLineHeight );
        }

        //----------------------------------------------------------------------
        internal override bool Update( float _fElapsedTime )
        {
            mPressedAnim.Update( _fElapsedTime );
            return ! mPressedAnim.IsOver;
        }

        //----------------------------------------------------------------------
        internal override void OnMouseEnter( Point _hitPoint )
        {
            base.OnMouseEnter( _hitPoint );
            mbIsHovered = true;
        }

        internal override void OnMouseOut( Point _hitPoint )
        {
            base.OnMouseOut( _hitPoint );
            mbIsHovered = false;
        }

        //----------------------------------------------------------------------
        internal override void OnMouseDown( Point _hitPoint )
        {
            base.OnMouseDown( _hitPoint );
            Screen.Focus( this );
            
            if( mbIsOpen && mDropDownHitBox.Contains( _hitPoint ) )
            {
            }
            else
            {
                miHoveredValueIndex = SelectedValueIndex;

                if( miHoveredValueIndex < miScrollOffset )
                {
                    miScrollOffset = miHoveredValueIndex;
                }
                else
                if( miHoveredValueIndex >= miScrollOffset + siMaxLineDisplayed )
                {
                    miScrollOffset = Math.Min( miHoveredValueIndex - siMaxLineDisplayed + 1, mlValues.Count - siMaxLineDisplayed );
                }

                mbIsOpen = ! mbIsOpen;
                mPressedAnim.SetTime( 0f );
            }
        }

        internal override void OnMouseUp( Point _hitPoint )
        {
            if( mbIsOpen && mDropDownHitBox.Contains( _hitPoint ) )
            {
                SelectedValueIndex = (int)( ( _hitPoint.Y - ( Position.Y + Size.Y + Padding.Top ) ) / siLineHeight ) + miScrollOffset;
                mPressedAnim.SetTime( 1f );
                mbIsOpen = false;
                mbIsPressed = false;

                if( ChangeHandler != null ) ChangeHandler( this );
            }
            else
            if( HitTest( _hitPoint ) == this )
            {
                OnClick();
            }
            else
            {
                mPressedAnim.SetTime( 1f );
                mbIsOpen = false;
                mbIsPressed = false;
            }
        }

        internal override void OnMouseMove(Point _hitPoint)
        {
            if( mbIsOpen && mDropDownHitBox.Contains( _hitPoint ) )
            {
                miHoveredValueIndex = (int)( ( _hitPoint.Y - ( Position.Y + Size.Y + Padding.Top ) ) / siLineHeight ) + miScrollOffset;
            }
            else
            {
                base.OnMouseMove( _hitPoint );
            }
        }

        internal override void OnMouseWheel( int _iDelta )
        {
            int iNewScrollOffset = (int)MathHelper.Clamp( miScrollOffset - _iDelta / 120, 0, Math.Max( 0, mlValues.Count - siMaxLineDisplayed ) );
            miHoveredValueIndex += iNewScrollOffset - miScrollOffset;
            miScrollOffset = iNewScrollOffset;
        }

        //----------------------------------------------------------------------
        void OnClick()
        {
            mPressedAnim.SetTime( 0f );
            Screen.AddWidgetToUpdateList( this );
        }

        //----------------------------------------------------------------------
        internal override void OnActivateDown()
        {
            if( mbIsOpen )
            {
            }
            else
            {
                miHoveredValueIndex = SelectedValueIndex;

                if( miHoveredValueIndex < miScrollOffset )
                {
                    miScrollOffset = miHoveredValueIndex;
                }
                else
                if( miHoveredValueIndex >= miScrollOffset + siMaxLineDisplayed )
                {
                    miScrollOffset = Math.Min( miHoveredValueIndex - siMaxLineDisplayed + 1, mlValues.Count - siMaxLineDisplayed );
                }

                mbIsPressed = true;
                mPressedAnim.SetTime( 0f );
            }
        }

        internal override void OnActivateUp()
        {
            if( mbIsOpen )
            {
                SelectedValueIndex = miHoveredValueIndex;
                if( ChangeHandler != null ) ChangeHandler( this );

                mPressedAnim.SetTime( 1f );
                mbIsOpen = false;
                mbIsPressed = false;
            }
            else
            {
                mbIsOpen = true;
            }
        }

        internal override bool OnCancel( bool _bPressed )
        {
            if( mbIsOpen )
            {
                if( ! _bPressed ) OnBlur();
                return true;
            }
            else
            {
                return false;
            }
        }

        //----------------------------------------------------------------------
        internal override void OnBlur()
        {
            mPressedAnim.SetTime( 1f );
            mbIsOpen = false;
            mbIsPressed = false;
        }

        //----------------------------------------------------------------------
        internal override void OnPadMove( Direction _direction )
        {
            if( ! mbIsOpen )
            {
                base.OnPadMove( _direction );
                return;
            }

            if( _direction == Direction.Up )
            {
                miHoveredValueIndex = Math.Max( 0, miHoveredValueIndex - 1 );

                if( miHoveredValueIndex < miScrollOffset )
                {
                    miScrollOffset = miHoveredValueIndex;
                }
            }
            else
            if( _direction == Direction.Down )
            {
                miHoveredValueIndex = Math.Min( mlValues.Count - 1, miHoveredValueIndex + 1 );

                if( miHoveredValueIndex >= miScrollOffset + siMaxLineDisplayed )
                {
                    miScrollOffset = Math.Min( miHoveredValueIndex - siMaxLineDisplayed + 1, mlValues.Count - siMaxLineDisplayed );
                }
            }
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            Screen.DrawBox( (!mbIsOpen && !mbIsPressed) ? ButtonFrame  : ButtonFrameDown, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), 30, Color.White );

            if( mbIsHovered && ! mbIsOpen && mPressedAnim.IsOver )
            {
                if( Screen.IsActive )
                {
                    Screen.DrawBox( ButtonFrameHover,      new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), 30, Color.White );
                }
            }
            else
            {
                Screen.DrawBox( ButtonFramePressed,    new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), 30, Color.White * mPressedAnim.CurrentValue );
            }

            if( Screen.IsActive && HasFocus && ! mbIsOpen )
            {
                Screen.DrawBox( Screen.Style.ButtonFrameFocus, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), 30, Color.White );
            }

            Screen.Game.SpriteBatch.Draw( Screen.Style.DropDownArrow,
                new Vector2( Position.X + Size.X - Padding.Right - Screen.Style.DropDownArrow.Width, Position.Y + Size.Y / 2 - Screen.Style.DropDownArrow.Height / 2 ),
                Color.White
            );

            Screen.Game.DrawBlurredText( Screen.Style.BlurRadius, Screen.Style.MediumFont, mlValues[SelectedValueIndex], new Vector2( Position.X + Padding.Left, Position.Y + Size.Y / 2 - ContentHeight / 2 + Padding.Top ) );
        }

        //----------------------------------------------------------------------
        public override Widget HitTest( Point _point )
        {
            if( HasFocus && mbIsOpen )
            {
                return this;
            }

            if( HitBox.Contains( _point ) )
            {
                return this;
            }
            else
            {
                return null;
            }
        }

        //----------------------------------------------------------------------
        internal override void DrawFocused()
        {
            if( mbIsOpen )
            {
                int iLinesDisplayed = Math.Min( siMaxLineDisplayed, mlValues.Count );

                Screen.DrawBox( Screen.Style.GridFrame, new Rectangle( Position.X, Position.Y + Size.Y, Size.X, Padding.Vertical + iLinesDisplayed * siLineHeight ), 30, Color.White );

                int iMaxIndex = Math.Min( mlValues.Count - 1, miScrollOffset + iLinesDisplayed - 1 );
                for( int iIndex = miScrollOffset; iIndex <= iMaxIndex; iIndex++ )
                {
                    if( Screen.IsActive && miHoveredValueIndex == iIndex )
                    {
                        Screen.DrawBox( Screen.Style.GridBoxFrameHover, new Rectangle( Position.X + Padding.Left, Position.Y + Size.Y + siLineHeight * ( iIndex - miScrollOffset ) + Padding.Top, Size.X - Padding.Horizontal, siLineHeight ), 10, Color.White );
                    }

                    Screen.Game.DrawBlurredText( Screen.Style.BlurRadius, Screen.Style.MediumFont, mlValues[iIndex], new Vector2( Position.X + Padding.Left, Position.Y + Size.Y + siLineHeight * ( iIndex - miScrollOffset ) + Padding.Top + Screen.Style.MediumFont.YOffset ) );
                }
            }
        }

    }
}