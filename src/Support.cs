
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public sealed class SaveSession
{
    public string Path;
    public ulong AccountId;
    public ulong SteamId64;
    public byte[] Plain;
    public CCSaveCrypto Crypto;
    public DateTime Modified;
}

public sealed class CharacterData
{
    public int Index;
    public string Name;
    public bool Unlocked;
    public int Level;
    public int XP;
    public int Gold;
    public int Strength;
    public int Magic;
    public int Defense;
    public int Agility;
    public int Potions;
    public int Bombs;
    public int Sandwiches;
}

public sealed class ComboItem
{
    public string Text;
    public int Value;
    public ComboItem(string text, int value) { Text = text; Value = value; }
    public override string ToString() { return Text; }
}

public sealed class ThemedTabControl : TabControl
{
    public Color ThemeCanvas = Color.FromArgb(17, 20, 24);
    public Color ThemeSelected = Color.FromArgb(27, 32, 38);
    public Color ThemeInactive = Color.FromArgb(23, 27, 32);
    public Color ThemeBorder = Color.FromArgb(52, 61, 71);
    public Color ThemePrimary = Color.White;
    public Color ThemeMuted = Color.Silver;
    public Color ThemeAccent = Color.SteelBlue;

    public ThemedTabControl()
    {
        SetStyle(ControlStyles.UserPaint |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw, true);
        UpdateStyles();
    }

    public void SetTheme(Color canvas, Color selected, Color inactive, Color border, Color primary, Color muted, Color accent)
    {
        ThemeCanvas = canvas;
        ThemeSelected = selected;
        ThemeInactive = inactive;
        ThemeBorder = border;
        ThemePrimary = primary;
        ThemeMuted = muted;
        ThemeAccent = accent;
        BackColor = canvas;
        Invalidate();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(ThemeCanvas);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(ThemeCanvas);

        Rectangle pageRect = DisplayRectangle;
        if (pageRect.Width > 0 && pageRect.Height > 0)
        {
            using (SolidBrush pageBrush = new SolidBrush(ThemeCanvas))
                e.Graphics.FillRectangle(pageBrush, pageRect);
        }

        for (int i = 0; i < TabCount; i++)
        {
            Rectangle r = GetTabRect(i);
            bool selected = i == SelectedIndex;
            Color fill = selected ? ThemeSelected : ThemeInactive;

            using (SolidBrush brush = new SolidBrush(fill))
                e.Graphics.FillRectangle(brush, r);
            using (Pen pen = new Pen(ThemeBorder))
                e.Graphics.DrawRectangle(pen, r.X, r.Y, Math.Max(0, r.Width - 1), Math.Max(0, r.Height - 1));

            if (selected)
            {
                using (SolidBrush accent = new SolidBrush(ThemeAccent))
                    e.Graphics.FillRectangle(accent, r.X + 1, r.Bottom - 3, Math.Max(0, r.Width - 2), 3);
            }

            Rectangle textRect = new Rectangle(r.X + 8, r.Y + 1, Math.Max(1, r.Width - 16), Math.Max(1, r.Height - 3));
            TextRenderer.DrawText(
                e.Graphics,
                TabPages[i].Text.Replace("&&", "&"),
                Font,
                textRect,
                selected ? ThemePrimary : ThemeMuted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }
    }

    protected override void OnSelectedIndexChanged(EventArgs e)
    {
        base.OnSelectedIndexChanged(e);
        Invalidate();
    }
}
