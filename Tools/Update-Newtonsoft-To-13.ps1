param(
	[string]$SolutionDir = "$(Split-Path -Parent $MyInvocation.MyCommand.Path)\..",
	[string]$ProjectPath = "CodeScanner\CodeScanner.csproj",
	[string]$PackageVersion = "13.0.3"
)

# Script to install Newtonsoft.Json package and update project reference/hint path
# Usage: powershell -ExecutionPolicy Bypass -File .\Tools\Update-Newtonsoft-To-13.ps1

Write-Host "SolutionDir:" $SolutionDir

$packagesFolder = Join-Path $SolutionDir 'packages'
if (-not (Test-Path $packagesFolder)) {
	New-Item -ItemType Directory -Path $packagesFolder | Out-Null
}

# Ensure NuGet.exe present
$nugetExe = Join-Path $SolutionDir 'nuget.exe'
if (-not (Test-Path $nugetExe)) {
	Write-Host "Downloading nuget.exe..."
	Invoke-WebRequest -Uri https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile $nugetExe
}

# Install Newtonsoft.Json package to packages folder
Write-Host "Installing Newtonsoft.Json $PackageVersion into packages folder..."
& $nugetExe install Newtonsoft.Json -Version $PackageVersion -OutputDirectory $packagesFolder -Source https://api.nuget.org/v3/index.json

# Update packages.config if exists
$pkgConfig = Join-Path $SolutionDir 'CodeScanner\packages.config'
if (Test-Path $pkgConfig) {
	(Get-Content $pkgConfig) -replace 'Newtonsoft.Json\" version=\"[0-9\.]+\"', "Newtonsoft.Json\" version=\"$PackageVersion\"" | Set-Content $pkgConfig
	Write-Host "Updated packages.config to version $PackageVersion"
}

# Update CodeScanner.csproj to refer to new package folder and assembly version
$projFile = Join-Path $SolutionDir $ProjectPath
if (Test-Path $projFile) {
	[xml]$xml = Get-Content $projFile
	$ns = @{msb='http://schemas.microsoft.com/developer/msbuild/2003'}
	$refs = $xml.Project.ItemGroup.Reference | Where-Object { $_.Include -like 'Newtonsoft.Json,*' }
	if ($refs) {
		foreach ($r in $refs) {
			$r.Include = 'Newtonsoft.Json, Version=13.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed, processorArchitecture=MSIL'
			if ($r.HintPath) {
				$r.HintPath = "..\\packages\\Newtonsoft.Json.$PackageVersion\\lib\\net45\\Newtonsoft.Json.dll"
			} else {
				$hp = $xml.CreateElement('HintPath', $xml.Project.NamespaceURI)
				$hp.InnerText = "..\\packages\\Newtonsoft.Json.$PackageVersion\\lib\\net45\\Newtonsoft.Json.dll"
				$r.AppendChild($hp) | Out-Null
			}
		}
		$xml.Save($projFile)
		Write-Host "Updated $ProjectPath references to Newtonsoft.Json $PackageVersion"
	} else {
		Write-Host "No Newtonsoft.Json reference found in project. You may need to add one manually."
	}
} else {
	Write-Host "Project file not found: $projFile"
}

Write-Host "Run 'nuget restore' and rebuild the solution in Visual Studio."
