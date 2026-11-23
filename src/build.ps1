$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$fw = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319'
$wpf = Join-Path $fw 'WPF'

$references = @(
    (Join-Path $wpf 'PresentationFramework.dll'),
    (Join-Path $wpf 'PresentationCore.dll'),
    (Join-Path $wpf 'WindowsBase.dll'),
    (Join-Path $fw 'System.Xaml.dll'),
    (Join-Path $fw 'System.Core.dll'),
    (Join-Path $fw 'System.Web.Extensions.dll'),
    (Join-Path $fw 'System.Windows.Forms.dll'),
    (Join-Path $fw 'System.Drawing.dll')
)

$outDir = Join-Path $root 'dist\九月倒计时'
New-Item -ItemType Directory -Path $outDir -Force | Out-Null
$exe = Join-Path $outDir '九月倒计时.exe'

$cscArgs = @(
    '/nologo',
    '/target:winexe',
    '/platform:anycpu',
    '/optimize+',
    '/debug-',
    '/codepage:65001',
    ('/win32icon:' + (Join-Path $root 'Assets\app.ico')),
    ('/win32manifest:' + (Join-Path $root 'app.manifest')),
    ('/out:' + $exe),
    ('/resource:' + (Join-Path $root 'Ui.xaml') + ',SeptemberCountdown.Ui.xaml'),
    ('/resource:' + (Join-Path $root 'Assets\noise.png') + ',SeptemberCountdown.Assets.noise.png'),
    ('/resource:' + (Join-Path $root 'Assets\app.ico') + ',SeptemberCountdown.Assets.app.ico')
)
foreach ($reference in $references) {
    $cscArgs += '/reference:' + $reference
}
$sources = @(
    (Join-Path $root 'Program.cs'),
    (Join-Path $root 'MainWindow.cs'),
    (Join-Path $root 'AppSettings.cs'),
    (Join-Path $root 'UpdateService.cs'),
    (Join-Path $root 'Starfield.cs'),
    (Join-Path $root 'StarWindow.cs'),
    (Join-Path $root 'StartupRegistration.cs'),
    (Join-Path $root 'NativeMethods.cs'),
    (Join-Path $root 'AssemblyInfo.cs')
)
$cscArgs += $sources

& $csc $cscArgs
if ($LASTEXITCODE -ne 0) {
    throw "Compilation failed with exit code $LASTEXITCODE"
}

Copy-Item (Join-Path $root 'README.txt') $outDir -ErrorAction SilentlyContinue
Copy-Item (Join-Path $root 'version.json') $outDir -ErrorAction SilentlyContinue
Write-Host "Built: $exe"
Get-Item $exe | Select-Object FullName, Length, LastWriteTime
