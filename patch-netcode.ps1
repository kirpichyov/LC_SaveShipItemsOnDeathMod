param([String]$path='SaveShipItemsOnDeathMod/bin/Debug/.netstandard2.1/') 

echo "Installing dotnet tool"
dotnet tool install -g Evaisa.NetcodePatcher.Cli

echo "Executing powershell patch script"
echo $path
netcode-patch $path/SaveShipItemsOnDeathMod.dll NetcodePatcher/deps

Write-Host "Press any key to exit..."
$Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")