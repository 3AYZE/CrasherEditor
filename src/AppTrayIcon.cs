using System;
using System.Drawing;
using System.Windows.Forms;

public sealed partial class MainForm : Form
{
    NotifyIcon trayIcon;
    ContextMenuStrip trayMenu;

    void InitializeApplicationIcon()
    {
        try { Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
        catch { Icon=SystemIcons.Application; }
        ShowIcon=true;
        ShowInTaskbar=true;

        trayMenu=new ContextMenuStrip();
        ToolStripMenuItem open=new ToolStripMenuItem("Open Crasher Editor");
        open.Click+=delegate{RestoreWindow();};
        ToolStripMenuItem exit=new ToolStripMenuItem("Exit Crasher Editor");
        exit.Click+=delegate{Close();};
        trayMenu.Items.Add(open);
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add(exit);

        trayIcon=new NotifyIcon();
        trayIcon.Icon=Icon??SystemIcons.Application;
        trayIcon.Text="Crasher Editor V1.4";
        trayIcon.ContextMenuStrip=trayMenu;
        trayIcon.DoubleClick+=delegate{RestoreWindow();};
        trayIcon.Visible=true;

        FormClosed+=delegate{
            trayIcon.Visible=false;
            trayIcon.Dispose();
            trayMenu.Dispose();
        };
    }
    void RestoreWindow()
    {
        if(WindowState==FormWindowState.Minimized)WindowState=FormWindowState.Normal;
        Show();Activate();
    }
}
