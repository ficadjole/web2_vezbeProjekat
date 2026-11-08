Connect-ServiceFabricCluster localhost:19000 | Out-Null
$api     = "http://localhost:8453/api"
$service = "fabric:/ProjekatVezbeWeb/PurchaseOrchestrator"
$idempotencyKey     = [guid]::NewGuid()
$before  = (Invoke-RestMethod "$api/Accounts/user1").balance
Write-Host "IDKEY: $($idempotencyKey) i User1Balance: $($before)"
$body = @{ userId = "user1"; bookId = "978-86-521-0001-0" } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri "$api/Purchases" -ContentType "application/json" `
    -Headers @{ "Idempotency-Key" = $idempotencyKey } -Body $body | Out-Null

Start-Sleep -Seconds 3        # saga je sada u ChargeAsync (Task.Delay)
Get-ServiceFabricPartition -ServiceName $service | ForEach-Object {
    Move-ServiceFabricPrimaryReplica -PartitionId $_.PartitionId -ServiceName $service | Out-Null
}

do { Start-Sleep -Seconds 2; $p = Invoke-RestMethod "$api/Purchases/$idempotencyKey"; Write-Host "Trenutni status sage: $($p.data.status)" }
while ($p.data.status -notin "Completed", "Compensated", "Failed")

$after = (Invoke-RestMethod "$api/Accounts/user1").balance
"Status: $($p.data.status)   cijena: $($p.data.price)   skinuto sa računa: $($before - $after)"
Read-Host
