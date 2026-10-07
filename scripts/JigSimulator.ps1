<#
.SYNOPSIS
    Jig Hardware Simulator for CodeScanner testing station.
.DESCRIPTION
    Simulates the RS-232 serial communication of the factory testing jig.
    Connects to one side of a virtual COM port pair (e.g., COM11), listens for
    the start trigger "#<BARCODE>@", and streams back realistic telemetry frames.
.EXAMPLE
    .\JigSimulator.ps1 -PortName "COM11" -BaudRate 9600 -SimulateResult "PASS"
#>

param(
    [string]$PortName = "COM11",
    [int]$BaudRate = 9600,
    [ValidateSet("PASS", "FAIL")][string]$SimulateResult = "PASS"
)

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "     CodeScanner RS-232 Testing Jig Simulator             " -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "Target Port      : $PortName" -ForegroundColor White
Write-Host "Baud Rate        : $BaudRate" -ForegroundColor White
Write-Host "Simulated Result : $SimulateResult" -ForegroundColor White
Write-Host "==========================================================" -ForegroundColor Cyan

$serialPort = $null

try {
    $serialPort = New-Object System.IO.Ports.SerialPort $PortName, $BaudRate, [System.IO.Ports.Parity]::None, 8, [System.IO.Ports.StopBits]::One
    $serialPort.NewLine = "`r`n"
    $serialPort.ReadTimeout = 2000
    $serialPort.WriteTimeout = 2000
    $serialPort.Open()
    Write-Host "[OK] Connected to $PortName. Listening for barcode scans...`n" -ForegroundColor Green
}
catch {
    Write-Host "[ERROR] Could not open port $PortName." -ForegroundColor Red
    Write-Host "Details: $_" -ForegroundColor Red
    Write-Host "`nTroubleshooting tips:" -ForegroundColor Yellow
    Write-Host "1. Ensure a virtual COM port pair is created (e.g., COM10 <-> COM11) using com0com or VSPE."
    Write-Host "2. Make sure no other program is currently using $PortName."
    exit 1
}

try {
    while ($true) {
        $incomingLine = $null
        try {
            $incomingLine = $serialPort.ReadLine()
        }
        catch [System.TimeoutException] {
            # Idle timeout - keep listening
            continue
        }

        if (-not [string]::IsNullOrWhiteSpace($incomingLine)) {
            Write-Host "[$((Get-Date).ToString('HH:mm:ss'))] << RECEIVED: $incomingLine" -ForegroundColor Magenta

            # Trigger command format from ComPortController: "#<BARCODE>@"
            if ($incomingLine -match "#(.+)@") {
                $barcode = $matches[1].Trim()
                Write-Host "[$((Get-Date).ToString('HH:mm:ss'))] >> Valid trigger for barcode: '$barcode'" -ForegroundColor Cyan

                $okValues = @(
                    "13.1:13.1:OK", "228:230:OK", "69:68:OK", "225:222:OK", "15.0:14.6:OK",
                    "0.0:0.0:OK", "0.0:0.0:OK", "50.2:50.0:OK", "50.0:50.0:OK", "0:0:OK",
                    "0:0:OK", "1.4:1.4:OK", "12.0:12.0:OK", "5.0:5.0:OK", "3.3:3.3:OK",
                    "1.2:1.2:OK", "1.2:1.2:OK", "115:115:PASS", ($SimulateResult -eq "PASS" ? "PASS" : "FAULT"), "FAN:FAN:OK",
                    "368", $barcode, $SimulateResult
                )

                for ($step = 0; $step -le 22; $step++) {
                    Start-Sleep -Milliseconds 1500
                    $tokens = [System.Collections.Generic.List[string]]::new()
                    $tokens.Add("@")
                    $tokens.Add("SPP_0.1")
                    $tokens.Add("SPP_C_LT_1.0")
                    $tokens.Add("VIKRANT_PRO_1250VA_12V")

                    for ($i = 0; $i -lt 23; $i++) {
                        if ($i -le $step) {
                            $tokens.Add($okValues[$i])
                        } else {
                            switch ($i) {
                                18 { $tokens.Add("ONGOING") }
                                19 { $tokens.Add("FAN:FAN:WAIT") }
                                20 { $tokens.Add(($step * 16).ToString()) }
                                21 { $tokens.Add($barcode) }
                                22 { $tokens.Add("ONGOING") }
                                default { $tokens.Add("0.0:0.0:ONGOING") }
                            }
                        }
                    }

                    $tokens.Add("^")
                    $frame = [string]::Join(",", $tokens)
                    $serialPort.WriteLine($frame)
                    
                    if ($step -eq 22) {
                        Write-Host "[$((Get-Date).ToString('HH:mm:ss'))] >> Sent Step 23/23: FINAL RESULT ($SimulateResult)" -ForegroundColor ($SimulateResult -eq "PASS" ? "Green" : "Red")
                    } else {
                        Write-Host "[$((Get-Date).ToString('HH:mm:ss'))] >> Sent Step $($step + 1)/23: Parameter $($step + 1) OK" -ForegroundColor Gray
                    }
                }

                Write-Host "[$((Get-Date).ToString('HH:mm:ss'))] Test cycle complete. Waiting for next trigger...`n" -ForegroundColor Cyan
            }
        }
    }
}
finally {
    if ($serialPort -ne $null -and $serialPort.IsOpen) {
        $serialPort.Close()
        Write-Host "`n[CLOSED] Serial port $PortName closed." -ForegroundColor Yellow
    }
}
