$version = git describe --always --tags

Remove-Item ..\Build\QuickLook-$version.msi -ErrorAction SilentlyContinue
if (!(Test-Path QuickLook-$version.msi)) {
    Rename-Item ..\Build\QuickLook.msi QuickLook-$version.msi
}