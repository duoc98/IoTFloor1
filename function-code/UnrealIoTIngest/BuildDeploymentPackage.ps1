dotnet publish --configuration Release --output .\bin\publish
Remove-Item .\funcapp-deploy.zip
Compress-Archive .\bin\publish\* .\funcapp-deploy.zip
Write-Host "Sucessfully build zip deployment package!!!"
Publish-AzWebapp -ResourceGroupName "iotfloor1-rg" -Name "funcapp-07e98e29" -ArchivePath .\funcapp-deploy.zip
Write-Host "Sucessfully deployment package!!!"