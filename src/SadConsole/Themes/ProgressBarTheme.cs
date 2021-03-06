﻿#if XNA
using Microsoft.Xna.Framework;
#endif

namespace SadConsole.Themes
{
    using SadConsole.Controls;
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The theme of a radio button control.
    /// </summary>
    [DataContract]
    public class ProgressBarTheme: ThemeBase
    {
        /// <summary>
        /// The theme of the unprogressed part of the bar.
        /// </summary>
        [DataMember]
        public ThemeStates Background;

        /// <summary>
        /// The theme of the progressed part of the bar.
        /// </summary>
        [DataMember]
        public ThemeStates Foreground;

        /// <summary>
        /// Creates a new theme used by the <see cref="ProgressBar"/>.
        /// </summary>
        public ProgressBarTheme()
        {

        }

        /// <inheritdoc />
        public override void Attached(ControlBase control)
        {
            control.Surface = new CellSurface(control.Width, control.Height);
            control.Surface.DefaultBackground = Color.Transparent;
            control.Surface.Clear();

            base.Attached(control);
        }

        /// <inheritdoc />
        public override void RefreshTheme(Colors themeColors)
        {
            base.RefreshTheme(themeColors);

            Background = new ThemeStates(themeColors);
            Background.SetForeground(Normal.Foreground);
            Background.SetBackground(Normal.Background);
            Background.SetGlyph(176);
            Background.Disabled = new Cell(Color.Gray, Color.Black, 176);
            Foreground = new ThemeStates(themeColors);
            Foreground.SetForeground(Normal.Foreground);
            Foreground.SetBackground(Normal.Background);
            Foreground.SetGlyph(219);
            Foreground.Disabled = new Cell(Color.Gray, Color.Black, 219);
        }

        /// <inheritdoc />
        public override void UpdateAndDraw(ControlBase control, TimeSpan time)
        {
            if (!(control is ProgressBar progressbar)) return;
            
            if (!control.IsDirty) return;

            Cell foregroundAppearance;
            Cell backgroundAppearance;

            if (Helpers.HasFlag(progressbar.State, ControlStates.Disabled))
            {
                foregroundAppearance = Foreground.Disabled;
                backgroundAppearance = Background.Disabled;
            }

            else if (Helpers.HasFlag(progressbar.State, ControlStates.MouseLeftButtonDown) ||
                     Helpers.HasFlag(progressbar.State, ControlStates.MouseRightButtonDown))
            {
                foregroundAppearance = Foreground.MouseDown;
                backgroundAppearance = Background.MouseDown;
            }

            else if (Helpers.HasFlag(progressbar.State, ControlStates.MouseOver))
            {
                foregroundAppearance = Foreground.MouseOver;
                backgroundAppearance = Background.MouseOver;
            }

            else if (Helpers.HasFlag(progressbar.State, ControlStates.Focused))
            {
                foregroundAppearance = Foreground.Focused;
                backgroundAppearance = Background.Focused;
            }

            else
            {
                foregroundAppearance = Foreground.Normal;
                backgroundAppearance = Background.Normal;
            }

            progressbar.Surface.Clear();

            progressbar.Surface.Fill(backgroundAppearance.Foreground, backgroundAppearance.Background, backgroundAppearance.Glyph);

            if (progressbar.IsHorizontal)
            {
                Rectangle fillRect;

                if (progressbar.HorizontalAlignment == HorizontalAlignment.Left)
                    fillRect = new Rectangle(0, 0, progressbar.fillSize, progressbar.Height);
                else
                    fillRect = new Rectangle(progressbar.Width - progressbar.fillSize, 0, progressbar.fillSize, progressbar.Height);

                progressbar.Surface.Fill(fillRect, foregroundAppearance.Foreground, foregroundAppearance.Background, foregroundAppearance.Glyph);
            }

            else
            {
                Rectangle fillRect;

                if (progressbar.VerticalAlignment == VerticalAlignment.Top)
                    fillRect = new Rectangle(0, 0, progressbar.Width, progressbar.fillSize);
                else
                    fillRect = new Rectangle(0, progressbar.Height - progressbar.fillSize, progressbar.Width, progressbar.fillSize);

                progressbar.Surface.Fill(fillRect, foregroundAppearance.Foreground, foregroundAppearance.Background, foregroundAppearance.Glyph);
            }

            progressbar.IsDirty = false;
        }

        /// <inheritdoc />
        public override ThemeBase Clone()
        {
            return new ProgressBarTheme()
            {
                Normal = Normal.Clone(),
                Disabled = Disabled.Clone(),
                MouseOver = MouseOver.Clone(),
                MouseDown = MouseDown.Clone(),
                Selected = Selected.Clone(),
                Focused = Focused.Clone(),
                Foreground = Foreground.Clone(),
                Background = Background.Clone()
            };
        }
    }
}
