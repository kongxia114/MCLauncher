Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports System.Windows.Forms

Public Class LauncherSettings
    Public Property GameName As String = "123"
    Public Property Password As String = ""
    Public Property OnlineLogin As Boolean
    Public Property AutoMemory As Boolean = True
    Public Property MemoryMb As Integer = 1024
    Public Property RandomSkin As Boolean = True
    Public Property JavaPath As String = "C:\Program Files\Java\jre1.8.0_401\bin\java.exe"
    Public Property SaveToVersion As Boolean
    Public Property DebugMode As Boolean
End Class

''' <summary>游戏启动/更新后端的预留接口。后续接入启动核心时实现此接口并替换 LauncherApi.Backend。</summary>
Public Interface IGameLauncher
    Function GetVersions() As IReadOnlyList(Of String)
    Sub Launch(version As String)
    Sub UpdateGame(kind As String, version As String)
    Sub UpdateSounds()
End Interface

Public NotInheritable Class LauncherApi
    Private Sub New()
    End Sub

    Public Shared Property Backend As IGameLauncher = New PlaceholderGameLauncher()

    Private Shared ReadOnly SettingsFile As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                     "MinecraftLauncher531", "settings.json")

    Public Shared Function LoadSettings() As LauncherSettings
        Try
            If File.Exists(SettingsFile) Then
                Dim loaded = JsonSerializer.Deserialize(Of LauncherSettings)(File.ReadAllText(SettingsFile))
                If loaded IsNot Nothing Then Return loaded
            End If
        Catch
            ' 配置损坏时回退到默认值，不阻止启动器打开。
        End Try
        Return New LauncherSettings()
    End Function

    Public Shared Function SaveSettings(settings As LauncherSettings) As Boolean
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsFile))
            File.WriteAllText(SettingsFile, JsonSerializer.Serialize(settings, New JsonSerializerOptions With {.WriteIndented = True}))
            Return True
        Catch ex As Exception
            MessageBox.Show("保存设置失败：" & ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Public Shared Function DetectJava() As String
        Dim candidates As New List(Of String)
        Dim javaHome As String = Environment.GetEnvironmentVariable("JAVA_HOME")
        If Not String.IsNullOrWhiteSpace(javaHome) Then
            candidates.Add(Path.Combine(javaHome, "bin", "java.exe"))
        End If

        Dim roots = {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Java"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Eclipse Adoptium"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Java")
        }
        For Each root In roots
            Try
                If Directory.Exists(root) Then
                    candidates.AddRange(Directory.GetDirectories(root).Select(Function(d) Path.Combine(d, "bin", "java.exe")))
                End If
            Catch
            End Try
        Next

        Return candidates.FirstOrDefault(Function(p) File.Exists(p))
    End Function

    Public Shared ReadOnly Property MinecraftDirectory As String
        Get
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft")
        End Get
    End Property
End Class

Friend Class PlaceholderGameLauncher
    Implements IGameLauncher

    Private Shared ReadOnly KnownVersions As String() = {"1.6.4", "1.7.10", "1.8.9", "1.10.2", "1.12.2", "1.16.5", "1.18.2", "1.20.1"}

    Public Function GetVersions() As IReadOnlyList(Of String) Implements IGameLauncher.GetVersions
        Return KnownVersions
    End Function

    Public Sub Launch(version As String) Implements IGameLauncher.Launch
        ' TODO: 接入游戏启动核心（Java、参数、账户、Forge 等）。
        MessageBox.Show("启动接口已预留，后续接入启动核心。" & vbCrLf & "当前版本：" & version,
                        "启动游戏", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Public Sub UpdateGame(kind As String, version As String) Implements IGameLauncher.UpdateGame
        ' TODO: 接入版本下载与完整性校验。
        MessageBox.Show("更新接口已预留。" & vbCrLf & "类型：" & kind & vbCrLf & "版本：" & version,
                        "更新游戏", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Public Sub UpdateSounds() Implements IGameLauncher.UpdateSounds
        ' TODO: 接入声音资源下载。
        MessageBox.Show("更新声音文件接口已预留。", "更新声音", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
End Class
