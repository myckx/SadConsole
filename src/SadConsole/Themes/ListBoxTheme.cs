﻿#if XNA
using Microsoft.Xna.Framework;
#endif

namespace SadConsole.Themes
{
    using SadConsole.Controls;
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The theme for a ListBox control.
    /// </summary>
    [DataContract]
    public class ListBoxTheme : ThemeBase
    {
        private bool _updatedColors;

        /// <summary>
        /// The drawing theme for the boarder when <see cref="DrawBorder"/> is true.
        /// </summary>
        [DataMember]
        public ThemeStates BorderTheme;

        /// <summary>
        /// The line style for the border when <see cref="DrawBorder"/> is true.
        /// </summary>
        [DataMember]
        public int[] BorderLineStyle;

        /// <summary>
        /// If false the border will not be drawn.
        /// </summary>
        [DataMember]
        public bool DrawBorder;

        /// <summary>
        /// The appearance of the scrollbar used by the listbox control.
        /// </summary>
        [DataMember]
        public ScrollBarTheme ScrollBarTheme;

        /// <summary>
        /// Creates a new theme used by the <see cref="ListBox"/>.
        /// </summary>
        /// <param name="scrollBarTheme">The theme to use to draw the scroll bar.</param>
        public ListBoxTheme(ScrollBarTheme scrollBarTheme)
        {
            ScrollBarTheme = scrollBarTheme;
        }

        /// <inheritdoc />
        public override void Attached(ControlBase control)
        {
            control.Surface = new CellSurface(control.Width, control.Height);
            control.Surface.DefaultBackground = Color.Transparent;
            control.Surface.Clear();

            ((ListBox) control).ScrollBar.Theme = ScrollBarTheme;

            base.Attached(control);
        }

        /// <inheritdoc />
        public override void RefreshTheme(Colors themeColors)
        {
            base.RefreshTheme(themeColors);
            _updatedColors = true;

            SetForeground(Normal.Foreground);
            SetBackground(Normal.Background);

            ScrollBarTheme?.RefreshTheme(themeColors);
            BorderTheme = new ThemeStates(themeColors);
            BorderTheme.SetForeground(Normal.Foreground);
            BorderTheme.SetBackground(Normal.Background);
            BorderLineStyle = (int[])CellSurface.ConnectedLineThick.Clone();
        }

        /// <inheritdoc />
        public override void UpdateAndDraw(ControlBase control, TimeSpan time)
        {
            if (!(control is ListBox listbox)) return;

            if (!listbox.IsDirty) return;

            if (_updatedColors)
            {
                listbox.ItemTheme.RefreshTheme(Colors ?? control.Parent?.Theme.Colors ?? Library.Default.Colors);
                _updatedColors = false;
            }

            int columnOffset;
            int columnEnd;
            int startingRow;
            int endingRow;
            
            Cell appearance = GetStateAppearance(listbox.State);
            Cell scrollBarAppearance = ScrollBarTheme.GetStateAppearance(listbox.State);
            Cell borderAppearance = BorderTheme.GetStateAppearance(listbox.State);

            // Redraw the control
            listbox.Surface.Fill(
                appearance.Foreground,
                appearance.Background,
                appearance.Glyph);

            if (DrawBorder)
            {
                endingRow = listbox.Height - 2;
                startingRow = 1;
                columnOffset = 1;
                columnEnd = listbox.Width - 2;
                listbox.Surface.DrawBox(new Rectangle(0, 0, listbox.Width, listbox.Height), new Cell(borderAppearance.Foreground, borderAppearance.Background, 0), null, BorderLineStyle);
            }
            else
            {
                endingRow = listbox.Height;
                startingRow = 0;
                columnOffset = 0;
                columnEnd = listbox.Width;
                listbox.Surface.Fill(borderAppearance.Foreground, borderAppearance.Background, 0, null);
            }

            ShowHideScrollBar(listbox);

            int offset = listbox.IsScrollBarVisible ? listbox.ScrollBar.Value : 0;
            for (int i = 0; i < endingRow; i++)
            {
                var itemIndexRelative = i + offset;
                if (itemIndexRelative < listbox.Items.Count)
                {
                    ControlStates state = 0;

                    if (Helpers.HasFlag(listbox.State, ControlStates.MouseOver) && listbox.RelativeIndexMouseOver == itemIndexRelative)
                        Helpers.SetFlag(ref state, ControlStates.MouseOver);

                    if (listbox.State.HasFlag(ControlStates.MouseLeftButtonDown))
                        Helpers.SetFlag(ref state, ControlStates.MouseLeftButtonDown);

                    if (listbox.State.HasFlag(ControlStates.MouseRightButtonDown))
                        Helpers.SetFlag(ref state, ControlStates.MouseRightButtonDown);

                    if (listbox.State.HasFlag(ControlStates.Disabled))
                        Helpers.SetFlag(ref state, ControlStates.Disabled);

                    if (itemIndexRelative == listbox.SelectedIndex)
                        Helpers.SetFlag(ref state, ControlStates.Selected);

                    listbox.ItemTheme.Draw(listbox.Surface, new Rectangle(columnOffset, i + startingRow, columnEnd, 1), listbox.Items[itemIndexRelative], state);
                }
            }

            if (listbox.IsScrollBarVisible)
            {
                listbox.ScrollBar.IsDirty = true;
                listbox.ScrollBar.Update(time);
                var y = listbox.ScrollBarRenderLocation.Y;

                for (var ycell = 0; ycell < listbox.ScrollBar.Height; ycell++)
                {
                    listbox.Surface.SetGlyph(listbox.ScrollBarRenderLocation.X, y, listbox.ScrollBar.Surface[0, ycell].Glyph);
                    listbox.Surface.SetCellAppearance(listbox.ScrollBarRenderLocation.X, y, listbox.ScrollBar.Surface[0, ycell]);
                    y++;
                }
            }


            listbox.IsDirty = Helpers.HasFlag(listbox.State, ControlStates.MouseOver);
        }

        /// <inheritdoc />
        public override ThemeBase Clone()
        {
            return new ListBoxTheme((ScrollBarTheme)ScrollBarTheme.Clone())
            {
                Colors = Colors?.Clone(),
                Normal = Normal.Clone(),
                Disabled = Disabled.Clone(),
                MouseOver = MouseOver.Clone(),
                MouseDown = MouseDown.Clone(),
                Selected = Selected.Clone(),
                Focused = Focused.Clone(),
                BorderTheme = BorderTheme.Clone(),
                BorderLineStyle = (int[])BorderLineStyle.Clone(),
                DrawBorder = DrawBorder,
            };
        }

        public void ShowHideScrollBar(ListBox control)
        {
            var heightOffset = DrawBorder ? 2 : 0;

            // process the scroll bar
            var scrollbarItems = control.Items.Count - (control.Height - heightOffset);

            if (scrollbarItems > 0)
            {
                control.ScrollBar.Maximum = scrollbarItems;
                control.IsScrollBarVisible = true;
            }
            else
            {
                control.ScrollBar.Maximum = 0;
                control.IsScrollBarVisible = false;
            }
        }
    }

    public class ListBoxItemTheme: ThemeStates
    {
        public ListBoxItemTheme(Colors themeColors): base(themeColors)
        {
            
        }

        public ListBoxItemTheme(): base(Library.Default.Colors) { }

        /// <inheritdoc />
        public override void RefreshTheme(Colors themeColors)
        {
            base.RefreshTheme(themeColors);

            SetForeground(Normal.Foreground);
            SetBackground(Normal.Background);

            Selected.Foreground = themeColors.Appearance_ControlSelected.Foreground;
            MouseOver = themeColors.Appearance_ControlOver.Clone();
        }
        
        public virtual void Draw(CellSurface surface, Rectangle area, object item, ControlStates itemState)
        {
            string value = item.ToString();
            if (value.Length < area.Width)
                value += new string(' ', area.Width - value.Length);
            else if (value.Length > area.Width)
                value = value.Substring(0, area.Width);
            if (Helpers.HasFlag(itemState, ControlStates.Selected) && !Helpers.HasFlag(itemState, ControlStates.MouseOver))
                surface.Print(area.Left, area.Top, value, Selected);
            else
                surface.Print(area.Left, area.Top, value, GetStateAppearance(itemState));
        }

        public virtual object Clone()
        {
            return new ListBoxItemTheme()
            {
                Normal = Normal.Clone(),
                Disabled = Disabled.Clone(),
                MouseOver = MouseOver.Clone(),
                MouseDown = MouseDown.Clone(),
                Selected = Selected.Clone(),
                Focused = Focused.Clone(),
            };
        }
    }

    public class ListBoxItemColorTheme : ListBoxItemTheme
    {
        public ListBoxItemColorTheme(Colors themeColors) : base(themeColors)
        {
            
        }

        public ListBoxItemColorTheme():base() { }
        
        public override void Draw(CellSurface surface, Rectangle area, object item, ControlStates itemState)
        {
            if (item is Color || item is Tuple<Color, Color, string>)
            {
                string value = new string(' ', area.Width - 2);

                Cell cellLook = GetStateAppearance(itemState).Clone();

                surface.Print(area.Left + 1, area.Top, value, cellLook);

                surface.Print(area.Left, area.Top, " ", cellLook);
                surface.Print(area.Left + area.Width - 1, area.Top, " ", cellLook);


                if (item is Color color)
                {
                    cellLook.Background = color;
                    surface.Print(area.Left + 1, area.Top, value, cellLook);
                }
                else
                {
                    cellLook.Foreground = ((Tuple<Color, Color, string>)item).Item2;
                    cellLook.Background = ((Tuple<Color, Color, string>)item).Item1;
                    value = ((Tuple<Color, Color, string>)item).Item3.Align(HorizontalAlignment.Left, area.Width - 2);
                    surface.Print(area.Left + 1, area.Top, value, cellLook);
                }
                
                if (itemState.HasFlag(ControlStates.Selected))
                {
                    surface.SetGlyph(area.Left, area.Top, 16);
                    surface.SetGlyph(area.Left + area.Width - 1, area.Top, 17);
                }
            }
            else
                base.Draw(surface, area, item, itemState);
        }

        public override object Clone()
        {
            return new ListBoxItemColorTheme()
            {
                Normal = Normal.Clone(),
                Disabled = Disabled.Clone(),
                MouseOver = MouseOver.Clone(),
                MouseDown = MouseDown.Clone(),
                Selected = Selected.Clone(),
                Focused = Focused.Clone(),
            };
        }
    }
}
