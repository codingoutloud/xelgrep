$dll = "C:\Windows\Microsoft.NET\assembly\GAC_32\Microsoft.SqlServer.XEvent.Linq\v4.0_14.0.0.0__89845dcd8080cc91\Microsoft.SqlServer.XEvent.Linq.dll"

echo "`n`nTrying to locate Microsoft.SqlServer.XEvent.Linq.dll in your local GAC and copy it into this project."

if (Test-Path $dll) {
  copy $dll .\DoNotRedistribute
  Write-Host "`n`nFound it! Trying to copy it now"
  echo "`n`nIf found and copied, you should see it here:"
  ls .\DoNotRedistribute
} else {
  Write-Warning "`n`nSorry, did not find $dll"
}

