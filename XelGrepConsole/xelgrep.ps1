## Driver for the xelgrep.exe to handle directory structures

$xelGrep = " .\xelgrep.exe"
$xelGrep = "C:\dev\github\ExtendedEventLogReader32\ExtendedEventLogReader32\bin\Debug\ExtendedEventLogReader32.exe"
$firstDate = [DateTime] "2017-08-31"
$lastDate = [DateTime] "2017-10-05"
$firstDate = $lastDate

<# fancy
for ($currDate = $firstDate; $currDate -le $lastDate; $currDate = $currDate.AddDays(1)) {
   $folder = $currDate.ToString("yyyy-MM-dd")
   echo "Start-Process $xelGrep -ArgumentList $folder -RedirectStandardOutput .\$folder.log -RedirectStandardError .\$folder.err.log"
}
#>

for ($currDate = $firstDate; $currDate -le $lastDate; $currDate = $currDate.AddDays(1)) {
   $folder = $currDate.ToString("yyyy-MM-dd")
   $xelGrep $folder 2>&1 >> .\$folder.log 
}

## C:\dev\github\ExtendedEventLogReader32\ExtendedEventLogReader32\bin\Debug\ExtendedEventLogReader32.exe "C:\dev\github\ExtendedEventLogReader32\temp\2017-10-05" 

