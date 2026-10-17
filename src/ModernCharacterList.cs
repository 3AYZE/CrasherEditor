using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public sealed class ModernCharacterList : Control
{
    public sealed class Entry
    {
        public int Slot;
        public string Name;
        public string Detail;
        public bool Locked;
        public Entry(int slot,string name,string detail,bool locked)
        { Slot=slot;Name=name;Detail=detail;Locked=locked; }
    }
    readonly List<Entry> rows=new List<Entry>();
    const int RowHeight=55;
    int topRow, selectedSlot=-1, hoverRow=-1;
    bool dragging;
    Color canvas=Color.FromArgb(16,23,33),active=Color.FromArgb(31,42,58);
    Color hover=Color.FromArgb(25,33,45),primary=Color.White,muted=Color.Silver;
    Color accent=Color.FromArgb(106,167,255),rail=Color.FromArgb(44,57,77);
    public event EventHandler SelectionChanged;
    public int SelectedSlot { get { return selectedSlot<0?0:selectedSlot; } }
    int VisibleRows { get { return Math.Max(1,(Height-8)/RowHeight); } }
    int MaxTop { get { return Math.Max(0,rows.Count-VisibleRows); } }

    public ModernCharacterList()
    {
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw|ControlStyles.Selectable,true);
        DoubleBuffered=true;TabStop=true;Font=new Font("Segoe UI",9.1f);
    }
    public void SetPalette(Color background,Color selected,Color hovering,Color foreground,Color subtext,Color selectedAccent,Color track)
    {
        canvas=background;active=selected;hover=hovering;primary=foreground;muted=subtext;accent=selectedAccent;rail=track;
        BackColor=canvas;Invalidate();
    }
    public void SetEntries(List<Entry> entries,int preferred)
    {
        int prior=selectedSlot;
        rows.Clear();if(entries!=null)rows.AddRange(entries);
        selectedSlot=-1;
        foreach(Entry e in rows)if(e.Slot==preferred){selectedSlot=preferred;break;}
        if(selectedSlot<0 && rows.Count>0)selectedSlot=rows[0].Slot;
        if(rows.Count==0)selectedSlot=prior;
        topRow=Math.Max(0,Math.Min(MaxTop,topRow));
        EnsureVisible();Invalidate();
        if(selectedSlot!=prior && SelectionChanged!=null)SelectionChanged(this,EventArgs.Empty);
    }
    void EnsureVisible()
    {
        int i=rows.FindIndex(delegate(Entry e){return e.Slot==selectedSlot;});
        if(i<0)return;
        if(i<topRow)topRow=i;
        if(i>=topRow+VisibleRows)topRow=i-VisibleRows+1;
        topRow=Math.Max(0,Math.Min(MaxTop,topRow));
    }
    public bool SelectSlot(int slot)
    {
        if(!rows.Exists(delegate(Entry e){return e.Slot==slot;}))return false;
        bool changed=selectedSlot!=slot;selectedSlot=slot;EnsureVisible();Invalidate();
        if(changed&&SelectionChanged!=null)SelectionChanged(this,EventArgs.Empty);
        return true;
    }
    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        topRow=Math.Max(0,Math.Min(MaxTop,topRow+(e.Delta>0?-3:3)));
        Invalidate();
    }
    void DragScroll(int y)
    {
        double fraction=Math.Max(0,Math.Min(1,(double)y/Math.Max(1,Height)));
        topRow=(int)Math.Round(MaxTop*fraction);Invalidate();
    }
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);Focus();
        if(e.Button!=MouseButtons.Left)return;
        if(e.X>Width-16 && rows.Count>VisibleRows){dragging=true;Capture=true;DragScroll(e.Y);return;}
        int i=topRow+(e.Y-4)/RowHeight;
        if(e.Y>=4 && i>=0 && i<rows.Count)SelectSlot(rows[i].Slot);
    }
    protected override void OnMouseUp(MouseEventArgs e){dragging=false;Capture=false;base.OnMouseUp(e);}
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if(dragging){DragScroll(e.Y);return;}
        int next=e.Y>=4?topRow+(e.Y-4)/RowHeight:-1;
        if(next>=rows.Count||e.X>Width-16)next=-1;
        if(next!=hoverRow){hoverRow=next;Invalidate();}
    }
    protected override void OnMouseLeave(EventArgs e){hoverRow=-1;Invalidate();base.OnMouseLeave(e);}
    protected override bool IsInputKey(Keys data)
    {
        Keys k=data&Keys.KeyCode;
        if(k==Keys.Down||k==Keys.Up||k==Keys.Home||k==Keys.End||k==Keys.PageDown||k==Keys.PageUp)return true;
        return base.IsInputKey(data);
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if(rows.Count==0)return;
        int i=rows.FindIndex(delegate(Entry x){return x.Slot==selectedSlot;});
        i=Math.Max(0,i);
        switch(e.KeyCode)
        {
            case Keys.Down:i++;break;
            case Keys.Up:i--;break;
            case Keys.Home:i=0;break;
            case Keys.End:i=rows.Count-1;break;
            case Keys.PageDown:i+=VisibleRows;break;
            case Keys.PageUp:i-=VisibleRows;break;
            default:return;
        }
        SelectSlot(rows[Math.Max(0,Math.Min(rows.Count-1,i))].Slot);e.Handled=true;
    }
    static GraphicsPath Round(Rectangle r,int radius)
    {
        GraphicsPath p=new GraphicsPath();
        int d=Math.Min(Math.Min(r.Width,r.Height),radius*2);
        if(d<=0)return p;
        p.AddArc(r.Left,r.Top,d,d,180,90);p.AddArc(r.Right-d,r.Top,d,d,270,90);
        p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.Left,r.Bottom-d,d,d,90,90);
        p.CloseFigure();return p;
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g=e.Graphics;g.Clear(canvas);g.SmoothingMode=SmoothingMode.AntiAlias;
        int count=Math.Min(rows.Count,topRow+VisibleRows+1);
        for(int i=topRow;i<count;i++)
        {
            Entry row=rows[i];
            int y=4+(i-topRow)*RowHeight;
            Rectangle rect=new Rectangle(4,y,Math.Max(1,Width-22),RowHeight-4);
            bool selected=row.Slot==selectedSlot;
            if(selected||hoverRow==i)
                using(GraphicsPath shape=Round(rect,8))
                using(SolidBrush brush=new SolidBrush(selected?active:hover))g.FillPath(brush,shape);
            if(selected)using(SolidBrush stripe=new SolidBrush(accent))g.FillRectangle(stripe,rect.Left+2,rect.Top+12,3,rect.Height-24);
            int left=rect.Left+16,width=Math.Max(1,rect.Width-28);
            TextRenderer.DrawText(g,row.Name,Font,new Rectangle(left,y+7,width,22),row.Locked&&!selected?muted:primary,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
            using(Font small=new Font("Segoe UI",8f))
                TextRenderer.DrawText(g,row.Detail,small,new Rectangle(left,y+29,width,18),muted,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
        }
        if(rows.Count>VisibleRows)
        {
            int height=Math.Max(8,Height-20);
            int thumb=Math.Max(24,height*VisibleRows/rows.Count);
            int offset=MaxTop==0?0:(height-thumb)*topRow/MaxTop;
            using(SolidBrush b=new SolidBrush(rail))g.FillRectangle(b,Width-8,10,3,height);
            using(SolidBrush b=new SolidBrush(accent))g.FillRectangle(b,Width-9,10+offset,5,thumb);
        }
    }
}
