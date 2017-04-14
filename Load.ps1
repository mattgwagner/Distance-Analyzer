$Here = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

$Soldiers = Get-Content Roster.csv | ConvertFrom-Csv

$Endpoint = "http://localhost:59400/Nodes/New"

foreach($Soldier in $Soldiers)
{
	$Node = @{
		Description = $Soldier.Name;
		Address = "$($Soldier.'Home Address'), $($Soldier.'Home City') $($Soldier.'Zip Code')";
		TagsList = (ConvertTo-Json @($Soldier.UIC, $Soldier.Unit, $Soldier.Rank));
	}

	Invoke-RestMethod -Method Post -Uri $Endpoint -Body $Node
}