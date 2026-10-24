Imports System.Diagnostics
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Linq

Public Class Form1
    Private Const WindowWidth As Integer = 720
    Private Const WindowHeight As Integer = 450

    Private WithEvents btnLaunch As GlassButton
    Private WithEvents btnUpdate As GlassButton
    Private WithEvents btnSettings As GlassButton
    Private WithEvents btnOpenDir As Label
    Private WithEvents btnClose As Label
    Private cmbVersion As ComboBox
    Private panelMain As GlassPanel
    Private lblFooter As Label

    Private dragging As Boolean
    Private dragOffset As Point

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "MineCraft Launcher 5.31 by 忘劫的旋律"
        Me.ClientSize = New Size(WindowWidth, WindowHeight)
        Me.FormBorderStyle = FormBorderStyle.None
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.AutoScaleMode = AutoScaleMode.None
        Me.BackColor = Color.FromArgb(34, 104, 160)
        Me.DoubleBuffered = True
        Me.KeyPreview = True

        SetRandomBackground()
        BuildUi()
        ApplyRoundedRegion()
    End Sub

    Private Sub SetRandomBackground()
        Dim imageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images")
        If Not Directory.Exists(imageFolder) Then Return
        Dim images = Directory.GetFiles(imageFolder).
            Where(Function(f) f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) OrElse
                                f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)).ToArray()
        If images.Length = 0 Then Return
        Try
            Me.BackgroundImage = Image.FromFile(images(New Random().Next(images.Length)))
            Me.BackgroundImageLayout = ImageLayout.Stretch
        Catch
            ' 背景加载失败时保留纯色背景。
        End Try
    End Sub

    Private Sub BuildUi()
        btnClose = New Label With {
            .Text = "×",
            .Font = UiDraw.UIFont(13.0F, FontStyle.Bold),
            .ForeColor = Color.FromArgb(244, 249, 252),
            .BackColor = Color.Transparent,
            .TextAlign = ContentAlignment.MiddleCenter,
            .Location = New Point(682, 4),
            .Size = New Size(30, 28),
            .Cursor = Cursors.Hand,
            .UseMnemonic = False
        }
        btnClose.AccessibleRole = AccessibleRole.PushButton
        Me.Controls.Add(btnClose)
        AddHandler btnClose.MouseEnter, Sub(sender, e) btnClose.ForeColor = Color.FromArgb(255, 175, 160)
        AddHandler btnClose.MouseLeave, Sub(sender, e) btnClose.ForeColor = Color.FromArgb(244, 249, 252)

        panelMain = New GlassPanel With {
            .Location = New Point(424, 300),
            .Size = New Size(278, 136),
            .FillColor = Color.Transparent,
            .BorderColor = Color.Transparent,
            .CornerRadius = 5
        }
        Me.Controls.Add(panelMain)

        btnLaunch = New GlassButton With {
            .Text = "启动游戏",
            .Font = UiDraw.UIFont(18.0F, FontStyle.Bold),
            .ForeColor = Color.FromArgb(28, 39, 48),
            .FillColor = Color.FromArgb(255, 247, 250, 252),
            .HoverFillColor = Color.FromArgb(255, 255, 255, 255),
            .PressedFillColor = Color.FromArgb(255, 225, 234, 240),
            .BorderColor = Color.FromArgb(255, 130, 151, 166),
            .CornerRadius = 5,
            .Location = New Point(8, 7),
            .Size = New Size(148, 80)
        }
        panelMain.Controls.Add(btnLaunch)

        btnUpdate = New GlassButton With {
            .Text = "更新游戏",
            .Font = UiDraw.UIFont(9.5F),
            .ForeColor = Color.FromArgb(35, 48, 58),
            .FillColor = Color.FromArgb(255, 232, 240, 246),
            .HoverFillColor = Color.FromArgb(255, 249, 252, 254),
            .PressedFillColor = Color.FromArgb(255, 214, 226, 235),
            .BorderColor = Color.FromArgb(255, 126, 148, 164),
            .CornerRadius = 3,
            .Location = New Point(164, 7),
            .Size = New Size(105, 36)
        }
        panelMain.Controls.Add(btnUpdate)

        btnSettings = New GlassButton With {
            .Text = "设置",
            .Font = UiDraw.UIFont(9.5F),
            .ForeColor = Color.FromArgb(35, 48, 58),
            .FillColor = Color.FromArgb(255, 232, 240, 246),
            .HoverFillColor = Color.FromArgb(255, 249, 252, 254),
            .PressedFillColor = Color.FromArgb(255, 214, 226, 235),
            .BorderColor = Color.FromArgb(255, 126, 148, 164),
            .CornerRadius = 3,
            .Location = New Point(164, 49),
            .Size = New Size(105, 36)
        }
        panelMain.Controls.Add(btnSettings)

        cmbVersion = New ComboBox With {
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .FlatStyle = FlatStyle.Flat,
            .Font = UiDraw.UIFont(9.5F),
            .BackColor = Color.FromArgb(250, 252, 253),
            .ForeColor = Color.FromArgb(35, 48, 58),
            .Location = New Point(8, 94),
            .Size = New Size(178, 27)
        }
        cmbVersion.Items.AddRange(LauncherApi.Backend.GetVersions().ToArray())
        If cmbVersion.Items.Count > 0 Then cmbVersion.SelectedIndex = 0
        panelMain.Controls.Add(cmbVersion)

        btnOpenDir = New Label With {
            .Text = "打开目录",
            .Font = UiDraw.UIFont(9.0F),
            .ForeColor = Color.FromArgb(30, 45, 57),
            .BackColor = Color.Transparent,
            .TextAlign = ContentAlignment.MiddleCenter,
            .Location = New Point(186, 91),
            .Size = New Size(84, 32),
            .Cursor = Cursors.Hand,
            .UseMnemonic = False
        }
        panelMain.Controls.Add(btnOpenDir)
        AddHandler btnOpenDir.MouseEnter, Sub(sender, e) btnOpenDir.ForeColor = Color.FromArgb(18, 96, 152)
        AddHandler btnOpenDir.MouseLeave, Sub(sender, e) btnOpenDir.ForeColor = Color.FromArgb(30, 45, 57)

        lblFooter = New Label With {
            .Text = "MineCraft Launcher 5.31 by 忘劫的旋律",
            .Font = UiDraw.UIFont(8.0F),
            .ForeColor = Color.FromArgb(245, 250, 253),
            .BackColor = Color.Transparent,
            .AutoSize = True,
            .Location = New Point(10, 432),
            .UseMnemonic = False
        }
        Me.Controls.Add(lblFooter)

        AddHandler Me.MouseDown, AddressOf StartDrag
        AddHandler Me.MouseMove, AddressOf MoveDrag
        AddHandler Me.MouseUp, AddressOf EndDrag
        AddHandler Me.KeyDown, Sub(sender, e)
                                    If e.KeyCode = Keys.Escape Then Close()
                                End Sub
    End Sub

    Private Sub ApplyRoundedRegion()
        Using path As GraphicsPath = UiDraw.RoundRect(New RectangleF(0, 0, Width, Height), 8)
            Region = New Region(path)
        End Using
    End Sub

    Private Sub StartDrag(sender As Object, e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then
            dragging = True
            dragOffset = New Point(Cursor.Position.X - Left, Cursor.Position.Y - Top)
        End If
    End Sub

    Private Sub MoveDrag(sender As Object, e As MouseEventArgs)
        If dragging Then
            Location = New Point(Cursor.Position.X - dragOffset.X, Cursor.Position.Y - dragOffset.Y)
        End If
    End Sub

    Private Sub EndDrag(sender As Object, e As MouseEventArgs)
        dragging = False
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Close()
    End Sub

    Private Sub btnLaunch_Click(sender As Object, e As EventArgs) Handles btnLaunch.Click
        If cmbVersion.SelectedItem Is Nothing Then Return
        LauncherApi.Backend.Launch(cmbVersion.SelectedItem.ToString())
    End Sub

    Private Sub btnUpdate_Click(sender As Object, e As EventArgs) Handles btnUpdate.Click
        Using dlg As New UpdateForm()
            dlg.ShowDialog(Me)
        End Using
    End Sub

    Private Sub btnSettings_Click(sender As Object, e As EventArgs) Handles btnSettings.Click
        Using dlg As New SettingsForm()
            dlg.ShowDialog(Me)
        End Using
    End Sub

    Private Sub btnOpenDir_Click(sender As Object, e As EventArgs) Handles btnOpenDir.Click
        If Directory.Exists(LauncherApi.MinecraftDirectory) Then
            Process.Start("explorer.exe", LauncherApi.MinecraftDirectory)
        Else
            MessageBox.Show("未找到 .minecraft 文件夹！" & vbCrLf & LauncherApi.MinecraftDirectory,
                            "打开目录", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub
End Class
