$Here = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

$Soldiers = Get-Content Roster.csv | ConvertFrom-Csv

$Endpoint = "http://localhost:59400/New"

foreach($Soldier in $Soldiers)
{
	$Node = @{
		Description = $Soldier.Name;
		Address = "$($Soldier.'Home Address'), $($Soldier.'Home City') $($Soldier.'Zip Code')";
	}

	Invoke-RestMethod -Method Post -Uri $Endpoint -Body $Node
}