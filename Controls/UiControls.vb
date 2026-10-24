Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

Friend Module UiDraw
    Public Function RoundRect(bounds As RectangleF, radius As Single) As GraphicsPath
        Dim path As New GraphicsPath()
        If radius <= 0 Then
            path.AddRectangle(bounds)
            Return path
        End If
        Dim d As Single = Math.Min(Math.Min(radius * 2.0F, bounds.Width), bounds.Height)
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90)
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90)
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90)
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90)
        path.CloseFigure()
        Return path
    End Function

    Public Function UIFont(size As Single, Optional style As FontStyle = FontStyle.Regular) As Font
        Return New Font("Microsoft YaHei UI", size, style, GraphicsUnit.Point)
    End Function
End Module

''' <summary>扁平按钮。交给系统 Button 绘制，避免自绘透明圆角产生白框和黑边。</summary>
Public Class GlassButton
    Inherits Button

    Private _fillColor As Color = Color.FromArgb(255, 246, 249, 251)
    Private _hoverFillColor As Color = Color.FromArgb(255, 255, 255, 255)
    Private _pressedFillColor As Color = Color.FromArgb(255, 235, 240, 245)
    Private _borderColor As Color = Color.FromArgb(255, 145, 160, 174)

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property FillColor As Color
        Get
            Return _fillColor
        End Get
        Set(value As Color)
            _fillColor = Opaque(value)
            BackColor = _fillColor
        End Set
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property HoverFillColor As Color
        Get
            Return _hoverFillColor
        End Get
        Set(value As Color)
            _hoverFillColor = Opaque(value)
            FlatAppearance.MouseOverBackColor = _hoverFillColor
        End Set
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property PressedFillColor As Color
        Get
            Return _pressedFillColor
        End Get
        Set(value As Color)
            _pressedFillColor = Opaque(value)
            FlatAppearance.MouseDownBackColor = _pressedFillColor
        End Set
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property BorderColor As Color
        Get
            Return _borderColor
        End Get
        Set(value As Color)
            _borderColor = Opaque(value)
            FlatAppearance.BorderColor = _borderColor
            FlatAppearance.BorderSize = If(value.A = 0, 0, 1)
        End Set
    End Property

    ' 保留该属性以兼容现有调用；系统扁平按钮不绘制圆角。
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property CornerRadius As Integer

    Public Sub New()
        FlatStyle = FlatStyle.Flat
        UseVisualStyleBackColor = False
        Cursor = Cursors.Hand
        TextAlign = ContentAlignment.MiddleCenter
        AutoEllipsis = False
        BackColor = _fillColor
        FlatAppearance.BorderColor = _borderColor
        FlatAppearance.BorderSize = 1
        FlatAppearance.MouseOverBackColor = _hoverFillColor
        FlatAppearance.MouseDownBackColor = _pressedFillColor
    End Sub

    Private Shared Function Opaque(color As Color) As Color
        If color.A = 255 Then Return color
        Return Color.FromArgb(255, color.R, color.G, color.B)
    End Function
End Class

''' <summary>透明玻璃卡片。背景绘制放在 OnPaintBackground 中，子控件透明时也能正确显示。</summary>
Public Class GlassPanel
    Inherits Panel

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property FillColor As Color = Color.FromArgb(196, 255, 255, 255)
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property BorderColor As Color = Color.FromArgb(120, 255, 255, 255)
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property CornerRadius As Integer = 6

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor, True)
        BackColor = Color.Transparent
    End Sub

    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
        MyBase.OnPaintBackground(e)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        Dim rect As New RectangleF(0.5F, 0.5F, Width - 1.0F, Height - 1.0F)
        Using path As GraphicsPath = UiDraw.RoundRect(rect, CornerRadius)
            If FillColor.A > 0 Then
                Using br As New SolidBrush(FillColor)
                    g.FillPath(br, path)
                End Using
            End If
        End Using
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        Dim rect As New RectangleF(0.5F, 0.5F, Width - 1.0F, Height - 1.0F)
        Using path As GraphicsPath = UiDraw.RoundRect(rect, CornerRadius)
            If BorderColor.A > 0 Then
                Using pen As New Pen(BorderColor, 1.0F)
                    g.DrawPath(pen, path)
                End Using
            End If
        End Using
    End Sub
End Class

''' <summary>无边框对话框基类：统一标题栏、关闭按钮、拖动与浅蓝玻璃背景。</summary>
Public MustInherit Class LauncherDialog
    Inherits Form

    Protected ReadOnly TitleLabel As Label
    Protected ReadOnly CloseButton As Label

    Private _dragging As Boolean
    Private _dragOffset As Point

    Protected Sub New(title As String, formSize As Size)
        SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True)
        DoubleBuffered = True
        FormBorderStyle = FormBorderStyle.None
        StartPosition = FormStartPosition.CenterParent
        ShowInTaskbar = False
        MinimizeBox = False
        MaximizeBox = False
        KeyPreview = True
        Text = title
        Font = UiDraw.UIFont(9.0F)
        BackColor = Color.FromArgb(230, 239, 246)
        Me.ClientSize = formSize

        TitleLabel = New Label With {
            .Text = title,
            .Font = UiDraw.UIFont(10.0F, FontStyle.Regular),
            .ForeColor = Color.FromArgb(35, 53, 69),
            .BackColor = Color.Transparent,
            .AutoSize = True,
            .Location = New Point(14, 8),
            .UseMnemonic = False
        }
        Controls.Add(TitleLabel)

        CloseButton = New Label With {
            .Text = "×",
            .Font = UiDraw.UIFont(13.0F, FontStyle.Bold),
            .ForeColor = Color.FromArgb(60, 76, 90),
            .BackColor = Color.Transparent,
            .TextAlign = ContentAlignment.MiddleCenter,
            .Size = New Size(26, 24),
            .Location = New Point(ClientSize.Width - 32, 2),
            .Cursor = Cursors.Hand,
            .UseMnemonic = False
        }
        CloseButton.AccessibleRole = AccessibleRole.PushButton
        Controls.Add(CloseButton)

        AddHandler CloseButton.Click, Sub(sender, e) Close()
        AddHandler CloseButton.MouseEnter, Sub(sender, e) CloseButton.ForeColor = Color.FromArgb(190, 55, 45)
        AddHandler CloseButton.MouseLeave, Sub(sender, e) CloseButton.ForeColor = Color.FromArgb(60, 76, 90)
        AttachDrag(Me)
        AttachDrag(TitleLabel)
        AddHandler KeyDown, Sub(sender, e)
                                If e.KeyCode = Keys.Escape Then Close()
                            End Sub
    End Sub

    Private Sub AttachDrag(control As Control)
        AddHandler control.MouseDown, AddressOf DragStart
        AddHandler control.MouseMove, AddressOf DragMove
        AddHandler control.MouseUp, AddressOf DragEnd
    End Sub

    Private Sub DragStart(sender As Object, e As MouseEventArgs)
        If e.Button <> MouseButtons.Left Then Return
        _dragging = True
        _dragOffset = New Point(Cursor.Position.X - Left, Cursor.Position.Y - Top)
    End Sub

    Private Sub DragMove(sender As Object, e As MouseEventArgs)
        If _dragging Then
            Location = New Point(Cursor.Position.X - _dragOffset.X, Cursor.Position.Y - _dragOffset.Y)
        End If
    End Sub

    Private Sub DragEnd(sender As Object, e As MouseEventArgs)
        _dragging = False
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        If Width > 20 AndAlso Height > 20 Then
            Using path As GraphicsPath = UiDraw.RoundRect(New RectangleF(0, 0, Width, Height), 8)
                Dim oldRegion As Region = Region
                Region = New Region(path)
                If oldRegion IsNot Nothing Then oldRegion.Dispose()
            End Using
        End If
        If CloseButton IsNot Nothing Then
            CloseButton.Location = New Point(ClientSize.Width - 32, 2)
        End If
    End Sub

    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
        Dim r As New Rectangle(0, 0, Width, Height)
        Using br As New LinearGradientBrush(r, Color.FromArgb(223, 235, 244), Color.FromArgb(246, 250, 252), LinearGradientMode.Vertical)
            e.Graphics.FillRectangle(br, r)
        End Using
        Using header As New LinearGradientBrush(New Rectangle(0, 0, Width, 31), Color.FromArgb(209, 225, 237), Color.FromArgb(235, 243, 248), LinearGradientMode.Vertical)
            e.Graphics.FillRectangle(header, 0, 0, Width, 31)
        End Using
        Using pen As New Pen(Color.FromArgb(190, 205, 218), 1.0F)
            e.Graphics.DrawLine(pen, 0, 30, Width, 30)
        End Using
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        Using path As GraphicsPath = UiDraw.RoundRect(New RectangleF(0.5F, 0.5F, Width - 1.0F, Height - 1.0F), 8)
            Using pen As New Pen(Color.FromArgb(135, 155, 172), 1.0F)
                g.DrawPath(pen, path)
            End Using
        End Using
    End Sub
End Class
