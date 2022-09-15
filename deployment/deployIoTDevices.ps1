$root_path = Split-Path $PSScriptRoot -Parent

#region functions
function New-IoTMockDevices {
    param (
        [string]$resource_group,
        [string]$hub_name,
        [string]$template_file,
        [string]$output_file
    )

    Write-Host
    Write-Host -ForegroundColor Yellow "Looking at devices in IoT hub '$($hub_name)' from resource group '$($resource_group)'"

    $iot_hub = az iot hub show -g $resource_group -n $hub_name | ConvertFrom-Json
    $iot_hub_devices = az iot hub device-identity list -g $resource_group -n $hub_name |ConvertFrom-Json
    $mock_devices = Get-Content -Path $template_file | ConvertFrom-Json -Depth 10

    for ($i = 0; $i -lt $mock_devices.devices.Count; $i++) {
        if ($mock_devices.devices[$i].configuration._kind -eq "hub") {
            $device = $iot_hub_devices | Where-Object { $_.deviceId -eq $mock_devices.devices[$i].configuration.deviceId }
            $device_tags = $mock_devices.devices[$i].configuration.deviceTags.schema

            if (!$device) {
                if ($mock_devices.devices[$i].configuration.deviceType -eq "device") {
                    Write-Host
                    Write-Host -ForegroundColor Yellow "Creating mock iot device '$($mock_devices.devices[$i].configuration.deviceId)' in IoT hub."

                    $device = az iot hub device-identity create `
                    -g $resource_group `
                    -n $hub_name `
                    -d $mock_devices.devices[$i].configuration.deviceId `
                    --edge-enabled $false | ConvertFrom-Json
                }
                elseif ($mock_devices.devices[$i].configuration.deviceType -eq "iotedge") {
                    Write-Host
                    Write-Host -ForegroundColor Yellow "Creating iot edge device '$($mock_devices.devices[$i].configuration.deviceId)' in IoT hub."

                    $device = az iot hub device-identity create `
                    -g $resource_group `
                    -n $hub_name `
                    -d $mock_devices.devices[$i].configuration.deviceId `
                    --edge-enabled $true | ConvertFrom-Json
                }
                else {
                    throw "Missing device type in template file Or check your device's type in template file. Ending script!!!"
                }

                Write-Host
                Write-host -ForegroundColor Yellow "Creating '$($mock_devices.devices[$i].configuration.deviceId)' device-twin tags."
                az iot hub device-twin update `
                    -d $device.deviceId `
                    -g $resource_group `
                    -n $hub_name `
                    --tags "$device_tags"

                if (!!$device.authentication.symmetricKey.primaryKey) {
                    $device_conn_string = "HostName=$($iot_hub.properties.hostName);DeviceId=$($device.deviceId);SharedAccessKey=$($device.authentication.symmetricKey.primaryKey)"
                }
                else {
                    throw "Unable to create connection string for device '$($device.deviceId)'"
                }
            }
            else {
                Write-Host
                Write-Host -ForegroundColor Yellow "Retrieving symmetric key for device '$($mock_devices.devices[$i].configuration.deviceId)' from IoT hub."
                $device_conn_string = az iot hub device-identity connection-string show `
                    -g $resource_group `
                    -n $hub_name `
                    -d $device.deviceId `
                    --query connectionString -o tsv
                Write-host
                Write-host -ForegroundColor Green "Retrieving connection string successfully!!!"

                Write-host
                Write-host -ForegroundColor Yellow "Showing '$($device.deviceId)' device-twin!!!"
                az iot hub device-twin show `
                    -d $device.deviceId `
                    -g $resource_group `
                    -n $hub_name
            }

            $mock_devices.devices[$i].configuration.connectionString = $device_conn_string
        }
    }

    Write-Host
    Write-Host -ForegroundColor Green "Writing mock devices' configuration"
    Set-Content -Path $output_file -Value (ConvertTo-Json $mock_devices -Depth 10)
}
#endregion

function New-Deployment() {
    #region mock devices config file
    $unreal_file = Get-Content (Join-Path $root_path "output" "unreal-plugin-config.json") | ConvertFrom-Json
    
    $script:resource_group_name = $unreal_file.resourceGroupName #"digitaltwins" 
    $script:iot_hub_name = $unreal_file.iotHubName #"digitaltwins" 
    $devices_template = Join-Path $root_path "devices" "mock-devices-template.json"
    $devices_file = Join-Path $root_path "output" "mock-devices.json"
    
    New-IoTMockDevices `
        -resource_group $script:resource_group_name `
        -hub_name $script:iot_hub_name `
        -template_file $devices_template `
        -output_file $devices_file
    #endregion

    #region commit to github
    Set-Location -Path $root_path
    git add .
    git status
    git commit -m "update or retrieving iot device"
    git pull
    git merge
    git push --set-upstream
    #endregion
}

New-Deployment