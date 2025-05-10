# Setting up stuff
$newProjectName = Read-Host "Type the new name for this mod project"
$unityProjectName = Read-Host "Do you have a unity project or plan to use one? If so, type the name of the unity project here. Otherwise, type 'n' to skip this"


$solutionFilePath = "$(Get-Location)\ModName.sln"
$thunderstoreZipCreatorPath = "$(Get-Location)\Thunderstore\manually create thunderstore zip.bat"
$modManifestPath = "$(Get-Location)\Thunderstore\Package Files\manifest.json"
$csprojFilePath = "$(Get-Location)\Code\ModName.csproj"


#Write-Host $solutionFilePath
#Write-Host $thunderstoreZipCreatorPath
#Write-Host $modManifestPath
#Write-Host $csprojFilePath
#Read-Host


$filesToReplaceContents = $solutionFilePath,$thunderstoreZipCreatorPath,$modManifestPath,$csprojFilePath
$filesToRename = $solutionFilePath,$csprojFilePath


if (!(Test-Path $solutionFilePath))
{
	Write-Host "Could not find 'ModName.sln'! The project must have already been renamed."
	Write-Host "Press ENTER to exit."
	$null = Read-Host
	exit
}


# Replacing ModName with the new project name
foreach ($filePath in $filesToReplaceContents)
{
	(Get-Content $filePath).Replace("ModName", $newProjectName) | Set-Content $filePath
}
if ($unityProjectName -ne 'n' -and $unityProjectName -ne '')
{
	(Get-Content $csprojFilePath).Replace("ProjectName", $unityProjectName) | Set-Content $csprojFilePath
}

# Doing it again but for all code folder files (for the namespace & plugin name)
cd "$(Get-Location)/Code"
foreach ($codeFile in (Get-ChildItem *.cs))
{
	(Get-Content $codeFile).Replace("ModName", $newProjectName) | Set-Content $codeFile
}
cd ..

# Rename files with a placeholder name
foreach ($filePath in $filesToRename)
{
	$newPath = $filePath.Replace("ModName", $newProjectName)
	Rename-Item $filePath $newPath
}

Write-Host "Done, but you may still have to rename the folder for the project."
Write-Host "Press ENTER to exit."
$null = Read-Host