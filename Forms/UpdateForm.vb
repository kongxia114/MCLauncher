Imports System.Drawing
Imports System.Windows.Forms

Public Class UpdateForm
    Inherits LauncherDialog

    Private rbSimple As RadioButton
    Private rbVanilla As RadioButton
    Private rbForge As RadioButton
    Private rbForgeOptiFine As RadioButton
    Private cmbVersion As ComboBox
    Private lstVersions As ListView

    Public Sub New()
        MyBase.New("版本更新", New Size(820, 430))
        BuildUi()
    End Sub

    Private Sub BuildUi()
        Dim listTitle As New Label With {
            .Text = "版本列表",
            .AutoSize = True,
            .Font = UiDraw.UIFont(10.0F, FontStyle.Bold),
            .ForeColor = Color.FromArgb(42, 59, 74),
            .BackColor = Color.Transparent,
            .Location = New Point(20, 41)
        }
        Controls.Add(listTitle)

        Dim listHint As New Label With {
            .Text = "选择需要更新的版本，再选择下方的更新方式。",
            .AutoSize = True,
            .Font = UiDraw.UIFont(8.0F),
            .ForeColor = Color.FromArgb(99, 114, 127),
            .BackColor = Color.Transparent,
            .Location = New Point(20, 65)
        }
        Controls.Add(listHint)

        lstVersions = New ListView With {
            .View = View.Details,
            .FullRowSelect = True,
            .MultiSelect = False,
            .HideSelection = False,
            .HeaderStyle = ColumnHeaderStyle.Nonclickable,
            .Font = UiDraw.UIFont(9.0F),
            .Location = New Point(18, 88),
            .Size = New Size(784, 220),
            .BorderStyle = BorderStyle.FixedSingle,
            .BackColor = Color.FromArgb(250, 252, 253),
            .ForeColor = Color.FromArgb(44, 58, 70)
        }
        lstVersions.Columns.Add("版本", 90)
        lstVersions.Columns.Add("名称", 230)
        lstVersions.Columns.Add("简介", 430)
        Controls.Add(lstVersions)
        AddHandler lstVersions.SelectedIndexChanged, AddressOf VersionSelected

        Dim panel As New GlassPanel With {
            .Location = New Point(18, 324),
            .Size = New Size(784, 58),
            .FillColor = Color.FromArgb(214, 255, 255, 255),
            .BorderColor = Color.FromArgb(120, 255, 255, 255),
            .CornerRadius = 5
        }
        Controls.Add(panel)

        rbSimple = AddRadio(panel, "简单整合", 14, 18, True)
        rbVanilla = AddRadio(panel, "官方原版", 112, 18, False)
        rbForge = AddRadio(panel, "Forge", 210, 18, False)
        rbForgeOptiFine = AddRadio(panel, "Forge+OptiFine", 286, 18, False)

        cmbVersion = New ComboBox With {
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .FlatStyle = FlatStyle.Flat,
            .Font = UiDraw.UIFont(9.0F),
            .Location = New Point(430, 13),
            .Size = New Size(100, 25)
        }
        cmbVersion.Items.AddRange(LauncherApi.Backend.GetVersions().ToArray())
        If cmbVersion.Items.Count > 0 Then cmbVersion.SelectedIndex = 0
        panel.Controls.Add(cmbVersion)

        Dim btnSounds As GlassButton = AddButton(panel, "更新声音文件", 548, 11, 116)
        Dim btnUpdate As GlassButton = AddButton(panel, "更新游戏", 676, 11, 94, True)
        AddHandler btnSounds.Click, Sub(sender, e) LauncherApi.Backend.UpdateSounds()
        AddHandler btnUpdate.Click, AddressOf UpdateGame

        Dim status As New Label With {
            .Text = "更新接口已预留，后续接入下载、校验与进度显示。",
            .AutoSize = True,
            .Font = UiDraw.UIFont(8.0F),
            .ForeColor = Color.FromArgb(104, 119, 132),
            .BackColor = Color.Transparent,
            .Location = New Point(20, 398)
        }
        Controls.Add(status)

        PopulateVersions()
    End Sub

    Private Sub PopulateVersions()
        lstVersions.BeginUpdate()
        lstVersions.Items.Clear()
        For Each version In LauncherApi.Backend.GetVersions()
            Dim row As New ListViewItem(version)
            row.SubItems.Add("Minecraft " & version)
            row.SubItems.Add(If(version = "1.6.4", "已安装 · 可检查更新", "未安装 · 可选择更新方式下载"))
            lstVersions.Items.Add(row)
        Next
        lstVersions.EndUpdate()
        If lstVersions.Items.Count > 0 Then
            lstVersions.Items(0).Selected = True
            lstVersions.Items(0).Focused = True
        End If
    End Sub

    Private Sub VersionSelected(sender As Object, e As EventArgs)
        If lstVersions.SelectedItems.Count = 0 Then Return
        Dim version As String = lstVersions.SelectedItems(0).Text
        If cmbVersion.Items.Contains(version) Then
            cmbVersion.SelectedItem = version
        End If
    End Sub

    Private Function AddRadio(parent As Control, text As String, x As Integer, y As Integer, selected As Boolean) As RadioButton
        Dim radio As New RadioButton With {
            .Text = text,
            .AutoSize = True,
            .Font = UiDraw.UIFont(8.5F),
            .ForeColor = Color.FromArgb(38, 55, 70),
            .BackColor = Color.Transparent,
            .Location = New Point(x, y),
            .Checked = selected
        }
        parent.Controls.Add(radio)
        Return radio
    End Function

    Private Function AddButton(parent As Control, text As String, x As Integer, y As Integer, width As Integer,
                               Optional accent As Boolean = False) As GlassButton
        Dim button As New GlassButton With {
            .Text = text,
            .Font = UiDraw.UIFont(8.5F),
            .ForeColor = Color.FromArgb(33, 50, 64),
            .Location = New Point(x, y),
            .Size = New Size(width, 34),
            .FillColor = If(accent, Color.FromArgb(255, 247, 250, 252), Color.FromArgb(255, 228, 236, 243)),
            .HoverFillColor = Color.FromArgb(255, 249, 252, 254),
            .PressedFillColor = Color.FromArgb(255, 213, 226, 235),
            .BorderColor = Color.FromArgb(255, 149, 169, 184),
            .CornerRadius = 4
        }
        parent.Controls.Add(button)
        Return button
    End Function

    Private Sub UpdateGame(sender As Object, e As EventArgs)
        Dim kind As String
        If rbForgeOptiFine.Checked Then
            kind = "Forge+OptiFine"
        ElseIf rbForge.Checked Then
            kind = "Forge"
        ElseIf rbVanilla.Checked Then
            kind = "官方原版"
        Else
            kind = "简单整合"
        End If

        Dim version As String = If(cmbVersion.SelectedItem IsNot Nothing, cmbVersion.SelectedItem.ToString(), "")
        LauncherApi.Backend.UpdateGame(kind, version)
    End Sub
End Class
