Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms

Public Class SettingsForm
    Inherits LauncherDialog

    Private txtName As TextBox
    Private txtPassword As TextBox
    Private txtMemory As TextBox
    Private txtJava As TextBox
    Private chkOnline As CheckBox
    Private chkDebug As CheckBox
    Private rbAutoMemory As RadioButton
    Private rbManualMemory As RadioButton
    Private rbRandomSkin As RadioButton
    Private rbDefaultSkin As RadioButton
    Private rbSaveMinecraft As RadioButton
    Private rbSaveVersion As RadioButton
    Private cmbGames As ComboBox
    Private btnSave As GlassButton

    Public Sub New()
        MyBase.New("游戏设置", New Size(875, 350))
        BuildUi()
        LoadCurrentSettings()
    End Sub

    Private Sub BuildUi()
        Dim groupColor As Color = Color.FromArgb(237, 244, 249)

        Dim grpLeft As New GroupBox With {
            .Text = "游戏设置",
            .Font = UiDraw.UIFont(9.0F),
            .ForeColor = Color.FromArgb(43, 61, 76),
            .BackColor = groupColor,
            .Location = New Point(18, 36),
            .Size = New Size(445, 244)
        }
        Controls.Add(grpLeft)

        AddLabel(grpLeft, "游戏名字", 15, 30)
        txtName = AddTextBox(grpLeft, "123", 80, 26, 120)
        AddLabel(grpLeft, "密码", 215, 30)
        txtPassword = AddTextBox(grpLeft, "", 248, 26, 84)
        txtPassword.UseSystemPasswordChar = True
        chkOnline = AddCheck(grpLeft, "正版登录", 340, 29)

        AddLabel(grpLeft, "最大内存", 15, 82)
        rbAutoMemory = AddRadio(grpLeft, "自动设置", 88, 79, True)
        rbManualMemory = AddRadio(grpLeft, "手动", 205, 79, False)
        txtMemory = AddTextBox(grpLeft, "1024", 276, 76, 62)
        AddLabel(grpLeft, "MB", 344, 82)

        AddLabel(grpLeft, "启动器皮肤", 15, 134)
        rbRandomSkin = AddRadio(grpLeft, "随机皮肤", 105, 131, True)
        rbDefaultSkin = AddRadio(grpLeft, "默认皮肤", 225, 131, False)

        AddLabel(grpLeft, "java路径", 15, 184)
        txtJava = AddTextBox(grpLeft, "C:\Program Files\Java\jre1.8.0_401\bin\java.exe", 80, 180, 268)
        Dim btnBrowse As GlassButton = AddButton(grpLeft, "浏览", 355, 178, 68)
        AddHandler btnBrowse.Click, AddressOf BrowseJava

        Dim grpRight As New GroupBox With {
            .Text = "游戏管理",
            .Font = UiDraw.UIFont(9.0F),
            .ForeColor = Color.FromArgb(43, 61, 76),
            .BackColor = groupColor,
            .Location = New Point(479, 36),
            .Size = New Size(378, 244)
        }
        Controls.Add(grpRight)

        AddLabel(grpRight, "游戏列表", 15, 30)
        cmbGames = New ComboBox With {
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .FlatStyle = FlatStyle.Flat,
            .Font = UiDraw.UIFont(9.0F),
            .Location = New Point(80, 26),
            .Size = New Size(265, 25)
        }
        grpRight.Controls.Add(cmbGames)

        Dim dirLabel As New Label With {
            .Text = "游戏目录（Forge mods, 材质包, 存档的目录）",
            .AutoSize = True,
            .Font = UiDraw.UIFont(8.3F),
            .ForeColor = Color.FromArgb(48, 66, 80),
            .BackColor = Color.Transparent,
            .Location = New Point(15, 82)
        }
        grpRight.Controls.Add(dirLabel)

        rbSaveMinecraft = AddRadio(grpRight, "保存到 .minecraft", 28, 112, True)
        rbSaveVersion = AddRadio(grpRight, "保存到 .minecraft\versions\版本名称\", 28, 140, False)
        Dim note As New Label With {
            .Text = "只在 1.6 以上的版本有效",
            .AutoSize = True,
            .Font = UiDraw.UIFont(8.0F),
            .ForeColor = Color.FromArgb(112, 123, 132),
            .BackColor = Color.Transparent,
            .Location = New Point(15, 184)
        }
        grpRight.Controls.Add(note)

        Dim btnRename As GlassButton = AddButton(grpRight, "重命名", 282, 178, 74)
        AddHandler btnRename.Click, Sub(sender, e)
                                        MessageBox.Show(Me, "重命名接口已预留，后续接入游戏配置管理。", "重命名",
                                                        MessageBoxButtons.OK, MessageBoxIcon.Information)
                                    End Sub

        chkDebug = AddCheck(Me, "调试模式", 18, 307)

        Dim btnFindJava As GlassButton = AddButton(Me, "查找java", 112, 300, 88)
        Dim btnOnlineTest As GlassButton = AddButton(Me, "正版登录测试", 208, 300, 112)
        Dim btnDeleteAuth As GlassButton = AddButton(Me, "删除正版信息", 328, 300, 116)
        Dim btnDeleteGame As GlassButton = AddButton(Me, "删除选中游戏", 588, 300, 112)
        btnSave = AddButton(Me, "保存设置", 710, 300, 122, True)

        AddHandler btnFindJava.Click, AddressOf FindJava
        AddHandler btnOnlineTest.Click, Sub(sender, e)
                                            MessageBox.Show(Me, "正版登录测试接口已预留，后续接入微软账户登录。",
                                                            "正版登录", MessageBoxButtons.OK, MessageBoxIcon.Information)
                                        End Sub
        AddHandler btnDeleteAuth.Click, AddressOf DeleteAuthInfo
        AddHandler btnDeleteGame.Click, AddressOf DeleteSelectedGame
        AddHandler btnSave.Click, AddressOf SaveCurrentSettings
    End Sub

    Private Function AddLabel(parent As Control, text As String, x As Integer, y As Integer) As Label
        Dim label As New Label With {
            .Text = text,
            .AutoSize = True,
            .Font = UiDraw.UIFont(8.5F),
            .ForeColor = Color.FromArgb(45, 61, 75),
            .BackColor = Color.Transparent,
            .Location = New Point(x, y)
        }
        parent.Controls.Add(label)
        Return label
    End Function

    Private Function AddTextBox(parent As Control, text As String, x As Integer, y As Integer, width As Integer) As TextBox
        Dim box As New TextBox With {
            .Text = text,
            .Font = UiDraw.UIFont(8.5F),
            .Location = New Point(x, y),
            .Size = New Size(width, 24),
            .BorderStyle = BorderStyle.FixedSingle
        }
        parent.Controls.Add(box)
        Return box
    End Function

    Private Function AddRadio(parent As Control, text As String, x As Integer, y As Integer, selected As Boolean) As RadioButton
        Dim radio As New RadioButton With {
            .Text = text,
            .AutoSize = True,
            .Font = UiDraw.UIFont(8.5F),
            .ForeColor = Color.FromArgb(42, 58, 72),
            .BackColor = Color.Transparent,
            .Location = New Point(x, y),
            .Checked = selected
        }
        parent.Controls.Add(radio)
        Return radio
    End Function

    Private Function AddCheck(parent As Control, text As String, x As Integer, y As Integer) As CheckBox
        Dim check As New CheckBox With {
            .Text = text,
            .AutoSize = True,
            .Font = UiDraw.UIFont(8.5F),
            .ForeColor = Color.FromArgb(42, 58, 72),
            .BackColor = Color.Transparent,
            .Location = New Point(x, y)
        }
        parent.Controls.Add(check)
        Return check
    End Function

    Private Function AddButton(parent As Control, text As String, x As Integer, y As Integer, width As Integer,
                               Optional accent As Boolean = False) As GlassButton
        Dim button As New GlassButton With {
            .Text = text,
            .Font = UiDraw.UIFont(8.5F),
            .ForeColor = Color.FromArgb(34, 50, 64),
            .Location = New Point(x, y),
            .Size = New Size(width, 32),
            .FillColor = If(accent, Color.FromArgb(255, 247, 250, 252), Color.FromArgb(255, 228, 236, 243)),
            .HoverFillColor = Color.FromArgb(255, 249, 252, 254),
            .PressedFillColor = Color.FromArgb(255, 213, 226, 235),
            .BorderColor = Color.FromArgb(255, 149, 169, 184),
            .CornerRadius = 4
        }
        parent.Controls.Add(button)
        Return button
    End Function

    Private Sub LoadCurrentSettings()
        Dim s As LauncherSettings = LauncherApi.LoadSettings()
        txtName.Text = s.GameName
        txtPassword.Text = ""
        txtMemory.Text = s.MemoryMb.ToString()
        txtJava.Text = s.JavaPath
        chkOnline.Checked = s.OnlineLogin
        chkDebug.Checked = s.DebugMode
        rbAutoMemory.Checked = s.AutoMemory
        rbManualMemory.Checked = Not s.AutoMemory
        rbRandomSkin.Checked = s.RandomSkin
        rbDefaultSkin.Checked = Not s.RandomSkin
        rbSaveMinecraft.Checked = Not s.SaveToVersion
        rbSaveVersion.Checked = s.SaveToVersion

        cmbGames.Items.Clear()
        If Not String.IsNullOrWhiteSpace(s.GameName) Then
            cmbGames.Items.Add(s.GameName)
            cmbGames.SelectedIndex = 0
        End If
    End Sub

    Private Sub BrowseJava(sender As Object, e As EventArgs)
        Using dialog As New OpenFileDialog()
            dialog.Title = "选择 java.exe"
            dialog.Filter = "Java 可执行文件 (java.exe)|java.exe|所有文件 (*.*)|*.*"
            dialog.CheckFileExists = True
            If dialog.ShowDialog(Me) = DialogResult.OK Then
                txtJava.Text = dialog.FileName
            End If
        End Using
    End Sub

    Private Sub FindJava(sender As Object, e As EventArgs)
        Dim path As String = LauncherApi.DetectJava()
        If String.IsNullOrWhiteSpace(path) Then
            MessageBox.Show(Me, "未在常见目录中找到 Java，请手动选择 java.exe。", "查找 Java",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            txtJava.Text = path
        End If
    End Sub

    Private Sub DeleteAuthInfo(sender As Object, e As EventArgs)
        txtPassword.Clear()
        chkOnline.Checked = False
        MessageBox.Show(Me, "已清除当前窗口中的正版登录信息。", "删除正版信息",
                        MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub DeleteSelectedGame(sender As Object, e As EventArgs)
        If cmbGames.SelectedItem Is Nothing Then Return
        Dim answer = MessageBox.Show(Me, "确定删除选中的游戏“" & cmbGames.SelectedItem.ToString() & "”吗？",
                                     "删除选中游戏", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
        If answer = DialogResult.Yes Then
            MessageBox.Show(Me, "删除接口已预留，后续接入游戏配置管理。", "删除选中游戏",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub SaveCurrentSettings(sender As Object, e As EventArgs)
        If String.IsNullOrWhiteSpace(txtName.Text) Then
            MessageBox.Show(Me, "游戏名字不能为空。", "保存设置", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtName.Focus()
            Return
        End If

        Dim memory As Integer
        If Not Integer.TryParse(txtMemory.Text.Trim(), memory) OrElse memory < 512 OrElse memory > 65536 Then
            MessageBox.Show(Me, "最大内存请输入 512 - 65536 之间的整数（MB）。", "保存设置",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtMemory.Focus()
            Return
        End If

        Dim s As New LauncherSettings With {
            .GameName = txtName.Text.Trim(),
            .Password = "",
            .OnlineLogin = chkOnline.Checked,
            .AutoMemory = rbAutoMemory.Checked,
            .MemoryMb = memory,
            .RandomSkin = rbRandomSkin.Checked,
            .JavaPath = txtJava.Text.Trim(),
            .SaveToVersion = rbSaveVersion.Checked,
            .DebugMode = chkDebug.Checked
        }
        If LauncherApi.SaveSettings(s) Then
            DialogResult = DialogResult.OK
            Close()
        End If
    End Sub
End Class
